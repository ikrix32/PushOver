using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

public class LevelCollisionMap : kBehaviourScript {
	//4 bits for move dir
	public static int MOVE_LEFT_DIR		= 1 << 0;
	public static int MOVE_RIGHT_DIR	= 1 << 1;
	public static int MOVE_UP_DIR 		= 1 << 2;
	public static int MOVE_DOWN_DIR 	= 1 << 3;

	public IntVector2 m_gridSize = new IntVector2(2,2);
	public Vector2 	  m_cellSize = new Vector2(10,10);

	protected LevelObject.Type[,] m_grid;
	protected int[,] m_moveMatrix;

	private PathFinder m_pathFinder;

	public bool m_debugLevelMap;
	public bool m_debugMoveMatrix;

	public LevelCollisionMap(){
		m_pathFinder = new PathFinder (this);
	}

	public List<XPath> GetPaths(Vector2 from,Vector2 to){
		IntVector2 fromCell = FindReachableCell (from,3,3);
		IntVector2 toCell = FindReachableCell (to,4,3);

		return m_pathFinder.FindPath ( fromCell, toCell);
	}
		
	public void Scan()
	{
		m_grid = new LevelObject.Type[ m_gridSize.x, m_gridSize.y];

		Platform[] platforms = gameObject.GetComponentsInChildren<Platform>();
		Ladder[] ladders	 = gameObject.GetComponentsInChildren<Ladder>();

		MapObjects (platforms);
		MapObjects (ladders);
		ComputeMoveMatrix();
	}

	protected void MapObjects(LevelObject[] objects){
		Rect gridBounds = new Rect (	transform.position.x, transform.position.y - m_gridSize.y * m_cellSize.y,
										m_gridSize.x * m_cellSize.x, m_gridSize.y * m_cellSize.y);

		for (int i = 0; i < objects.Length; i++) {
			Rect objectBounds = objects [i].getBoundsWorld ();

			float gLeft	= (objectBounds.x - gridBounds.x) / m_cellSize.x;
			float gRight = (objectBounds.x + objectBounds.width - gridBounds.x) / m_cellSize.x;

			float gTop 		= (objectBounds.y + objectBounds.height - gridBounds.y) / m_cellSize.y;
			float gBottom 	= (objectBounds.y - gridBounds.y) / m_cellSize.y;

			int gridLeftCell	= Mathf.FloorToInt(gLeft);
			if (gLeft - gridLeftCell >= 0.6f)	gridLeftCell += 1;
			
			int gridRightCell 	= Mathf.FloorToInt(gRight);
			if (gRight - gridRightCell <= 0.4f)	gridRightCell -= 1;

			int gridBottomCell 	= Mathf.FloorToInt(gBottom);
			if (gBottom - gridBottomCell >= 0.6f)gridBottomCell += 1;
			
			int gridTopCell 	= Mathf.FloorToInt(gTop);
			if (gTop - gridTopCell <= 0.4f)	gridTopCell -= 1;

			if(	gridLeftCell < m_gridSize.x && gridRightCell >= 0
				&&	gridBottomCell  < m_gridSize.y && gridTopCell >= 0){
				gridLeftCell = gridLeftCell < 0 ? 0 : gridLeftCell;
				gridRightCell = gridRightCell >= m_gridSize.x ? (int)m_gridSize.x - 1 : gridRightCell;

				gridBottomCell = gridBottomCell < 0 ? 0 : gridBottomCell;
				gridTopCell = gridTopCell >= m_gridSize.y ? (int)m_gridSize.y - 1 : gridTopCell;

				if (objects [i].type == LevelObject.Type.Ladder) {
					int xM = gridLeftCell + (gridRightCell - gridLeftCell) / 2;
					for (int y = gridBottomCell; y <= gridTopCell; y++)
						m_grid [xM, y] = objects [i].type;
				} else {
					for (int x = gridLeftCell; x <= gridRightCell; x++)
						for (int y = gridBottomCell; y <= gridTopCell; y++) 
							m_grid [x, y] = objects [i].type;
				}
			}
		}
	}

	protected void ComputeMoveMatrix(){
		m_moveMatrix = new int[ m_gridSize.x, m_gridSize.y];

		for(int y = (int)m_gridSize.y - 1; y >= 0; y--){
			for(int x = 0; x < m_gridSize.x; x++)
			{
				if(m_grid[ x, y] == LevelObject.Type.Ladder )
				{
					m_moveMatrix[ x, y] |= MOVE_UP_DIR | MOVE_DOWN_DIR;

					if(x > 0 && isWalkableCell( x - 1, y))
						m_moveMatrix[ x, y] |= MOVE_LEFT_DIR;

					if(x < m_gridSize.x - 1 && isWalkableCell( x + 1, y))
						m_moveMatrix[ x, y] |= MOVE_RIGHT_DIR;
				}

				if(isWalkableCell(x,y))
				{
					m_moveMatrix[ x, y] |= MOVE_LEFT_DIR | MOVE_RIGHT_DIR;

					if(y > 0 && m_grid[x , y - 1] == LevelObject.Type.Ladder)
						m_moveMatrix[ x, y] |= MOVE_DOWN_DIR;
					
					if(y < m_gridSize.y - 1 && m_grid[x , y + 1] == LevelObject.Type.Ladder)
						m_moveMatrix[ x, y] |= MOVE_UP_DIR;
				}else if(isFallCell(x,y)){
					m_moveMatrix[ x, y] |= MOVE_DOWN_DIR;
				}
			}
		}
	}

	protected bool isWalkableCell(int x,int y){
		if (m_grid [x, y] == LevelObject.Type.Platform)
			return false;
		
		if(y > 0 ){	
			if(m_grid[ x , y] != LevelObject.Type.Ladder 
			&&(m_grid[ x , y - 1] == LevelObject.Type.Platform 
			|| m_grid[ x , y - 1] == LevelObject.Type.Ladder))
				return true;
		}
		return false;
	}

	protected bool isFallCell(int x,int y){
		if (m_grid [x, y] == LevelObject.Type.Platform)
			return false;
		
		IntVector2 topCell = new IntVector2 (x, y + 1);
		if((y < m_gridSize.y - 1 && (CanPassCellDown(topCell) && !CanPassCellUp(topCell))) 
		|| (x > 0 && m_grid[x - 1, y] == LevelObject.Type.Platform)
		|| (x < m_gridSize.x - 1 && m_grid[x + 1, y] == LevelObject.Type.Platform))
			return true;

		if(x > 0 && y > 0){
			if(m_grid[ x - 1, y - 1] == LevelObject.Type.Platform)
			//|| m_grid[ x - 1, y - 1] == LevelObject.Type.Ladder)
				return true;
		}

		if(y > 0 && x < m_gridSize.x - 1){
			if(m_grid[ x + 1, y - 1] == LevelObject.Type.Platform)
			//|| m_grid[ x + 1, y - 1] == LevelObject.Type.Ladder)
				return true;
		}

		return false;
	}

	public bool CanPassCell( IntVector2 cell){
		return m_moveMatrix[cell.x,cell.y] != 0;
	}

	public bool CanPassCellLeft( IntVector2 cell){
		return cell.x > 0 && (m_moveMatrix[cell.x,cell.y] & MOVE_LEFT_DIR) != 0;
	}

	public bool CanPassCellRight( IntVector2 cell){
		return cell.x < m_gridSize.x - 1 && (m_moveMatrix[cell.x,cell.y] & MOVE_RIGHT_DIR) != 0;
	}

	public bool CanPassCellUp( IntVector2 cell){
		return cell.y < m_gridSize.x - 1 && (m_moveMatrix[cell.x,cell.y] & MOVE_UP_DIR) != 0;
	}

	public bool CanPassCellDown( IntVector2 cell){
		return cell.y > 0 && (m_moveMatrix[cell.x,cell.y] & MOVE_DOWN_DIR) != 0;
	}

	private IntVector2 FindReachableCell(Vector2 pos,int vTollerance,int hTollerance){
		IntVector2 cell = PosToCell (pos);
		for (int i = 0; i < vTollerance; i++) {
			if(m_moveMatrix[cell.x,cell.y - i] != 0){
				cell.y -= i;
				return cell;
			}
			if(m_moveMatrix[cell.x,cell.y + i] != 0){
				cell.y += i;
				return cell;
			}
		}

		for (int i = 0; i < hTollerance; i++) {
			if(m_moveMatrix[cell.x - i,cell.y] != 0){
				cell.x -= i;
				return cell;
			}
			if(m_moveMatrix[cell.x + i,cell.y] != 0){
				cell.x += i;
				return cell;
			}
		}
		return cell;
	}

	public IntVector2 PosToCell(Vector2 pos){
		Vector2 gridPos = new Vector2 (transform.position.x, transform.position.y - m_gridSize.y * m_cellSize.y);

		float cellX	= (pos.x - gridPos.x) / m_cellSize.x;
		float cellY = (pos.y - gridPos.y) / m_cellSize.y;

		return new IntVector2 (Mathf.RoundToInt (cellX), Mathf.RoundToInt (cellY));
	}

	public Vector3 CellToPos(IntVector2 cell){
		float gridHeight = m_gridSize.y * m_cellSize.y;
		return new Vector3 ( 	transform.position.x + cell.x * m_cellSize.x + m_cellSize.x / 2, 
								transform.position.y - gridHeight + cell.y * m_cellSize.y + m_cellSize.y / 2, 
								transform.position.z);
	}

	#if UNITY_EDITOR	
	public Color platformColor = new Color(1,0,0,0.3f);
	public Color ladderColor = new Color(0,0,1,0.3f);
	public Color emptyCell = new Color(0.7f,0.7f,0.7f,0.1f);

	public Color moveCellse = new Color(0,1,0,0.5f);

	public Color[] m_pathColors;

	void OnDrawGizmos(){
		float width = m_gridSize.x * m_cellSize.x; 
		float height= m_gridSize.y * m_cellSize.y; 

		if(m_debugLevelMap){
			Gizmos.color = Color.white;
			Gizmos.DrawWireCube(	transform.position + Vector3.right * width / 2 + Vector3.down * height / 2,
									new Vector3(m_gridSize.x * m_cellSize.x, m_gridSize.y * m_cellSize.y, 2));

			if (m_grid != null)
			{
				for (int y = 0; y < m_gridSize.y; y++) {
					for (int x = 0; x < m_gridSize.x; x++) {
						Vector3 pos = new Vector3 (	transform.position.x + x * m_cellSize.x + m_cellSize.x/2, 
													transform.position.y - height + y * m_cellSize.y + m_cellSize.y/2, 
													transform.position.z);

						if (m_grid [x, y] == LevelObject.Type.Platform) {
							Gizmos.color = platformColor;
							Gizmos.DrawCube (pos, new Vector3 (m_cellSize.x, m_cellSize.y, 2));
						} else if (m_grid [x, y] == LevelObject.Type.Ladder) {
							Gizmos.color = ladderColor;
							Gizmos.DrawCube (pos, new Vector3 (m_cellSize.x, m_cellSize.y, 2));
						} else {
							Gizmos.color = emptyCell;
							Gizmos.DrawWireCube (pos, new Vector3 (m_cellSize.x, m_cellSize.y, 2));
						}
					}
				}
			}
		}

		if(m_debugMoveMatrix){
			if (m_moveMatrix != null) {
				IntVector2 cell = new IntVector2 (0, 0);
				for (int y = 0; y < m_gridSize.y; y++) {
					for (int x = 0; x < m_gridSize.x; x++) {
						Vector3 pos = new Vector3 (	transform.position.x + x * m_cellSize.x + m_cellSize.x/2, 
													transform.position.y - height + y * m_cellSize.y + m_cellSize.y/2, 
													transform.position.z);
						cell.x = x;
						cell.y = y;
						if (CanPassCell(cell)) {
							Gizmos.color = moveCellse;
							Gizmos.DrawCube (pos, new Vector3 (m_cellSize.x, m_cellSize.y, 2));

							DrawArrows( pos, cell);
						} else {
							//Gizmos.color = red;
							//Gizmos.DrawCube (pos, new Vector3 (m_cellSize.x, m_cellSize.y, 2));
						}
					}
				}
			}
		}

		m_pathFinder.DrawPaths (m_pathColors);
	}

	void DrawArrows(Vector3 pos,IntVector2 cell){
		float arrowSize = m_cellSize.x / 2;
		float arrowTail = arrowSize * 0.75f;
		float arrowHead = arrowSize * 0.25f;

		if(CanPassCell(cell)){
			Gizmos.color = Color.white;

			Vector3 p1 = pos + Vector3.left * arrowTail;
			Gizmos.DrawLine( pos, p1);

			Gizmos.DrawLine( p1 + Vector3.up * arrowHead , p1 + Vector3.left * arrowHead);
			Gizmos.DrawLine( p1 + Vector3.down * arrowHead, p1 + Vector3.left * arrowHead);
			Gizmos.DrawLine( p1 + Vector3.up * arrowHead, p1 + Vector3.down * arrowHead);
		}

		if(CanPassCellRight(cell)){
			Gizmos.color = Color.yellow;
			Vector3 p1 = pos + Vector3.right * arrowTail;
			Gizmos.DrawLine( pos, p1);

			Gizmos.DrawLine( p1 + Vector3.up * arrowHead , p1 + Vector3.right * arrowHead);
			Gizmos.DrawLine( p1 + Vector3.down * arrowHead, p1 + Vector3.right * arrowHead);
			Gizmos.DrawLine( p1 + Vector3.up * arrowHead, p1 + Vector3.down * arrowHead);
		}

		if(CanPassCellUp(cell)){
			Gizmos.color = Color.green;
			Vector3 p1 = pos + Vector3.up * arrowTail;
			Gizmos.DrawLine( pos, p1);

			Gizmos.DrawLine( p1 + Vector3.left * arrowHead , p1 + Vector3.up * arrowHead);
			Gizmos.DrawLine( p1 + Vector3.right * arrowHead, p1 + Vector3.up * arrowHead);
			Gizmos.DrawLine( p1 + Vector3.left * arrowHead, p1 + Vector3.right * arrowHead);
		}

		if(CanPassCellDown(cell)){
			Gizmos.color = Color.red;
			Vector3 p1 = pos + Vector3.down * arrowTail;
			Gizmos.DrawLine( pos, p1);

			Gizmos.DrawLine( p1 + Vector3.left * arrowHead , p1 + Vector3.down * arrowHead);
			Gizmos.DrawLine( p1 + Vector3.right * arrowHead, p1 + Vector3.down * arrowHead);
			Gizmos.DrawLine( p1 + Vector3.left * arrowHead, p1 + Vector3.right * arrowHead);
		}
	}

	#endif
}
