using UnityEngine;
using System.Collections;
using System;

public class LevelMap : kBehaviourScript {
	//4 bits for move dir
	public static int MOVE_LEFT_DIR		= 1<<0;
	public static int MOVE_RIGHT_DIR	= 1<<1;
	public static int MOVE_UP_DIR 		= 1<<2;
	public static int MOVE_DOWN_DIR 	= 1<<3;

	public Vector2 m_gridSize = new Vector2(2,2);
	public Vector2 m_cellSize = new Vector2(10,10);

	protected LevelObject.Type[,] m_grid;

	protected int[,] m_moveMatrix;


	public bool m_debugLevelMap;
	public bool m_debugMoveMatrix;

	public static bool canPassLeft(int access){
		return (access & MOVE_LEFT_DIR) != 0;
	}

	public static bool canPassRight(int access){
		return (access & MOVE_RIGHT_DIR) != 0;
	}

	public static bool canPassUp(int access){
		return (access & MOVE_UP_DIR) != 0;
	}

	public static bool canPassDown(int access){
		return (access & MOVE_DOWN_DIR) != 0;
	}

	public void Scan()
	{
		m_grid = new LevelObject.Type[ (int)m_gridSize.x, (int)m_gridSize.y];

		Platform[] platforms = transform.parent.gameObject.GetComponentsInChildren<Platform>();
		Ladder[] ladders	 = transform.parent.gameObject.GetComponentsInChildren<Ladder>();

		mapObjects (platforms);
		mapObjects (ladders);

		computeMoveMatrix();
	}

	protected void computeMoveMatrix(){
		m_moveMatrix = new int[(int)m_gridSize.x + 10, (int)m_gridSize.y];

		for(int y = (int)m_gridSize.y - 1; y > 0; y--){
			for(int x = 0; x < m_gridSize.x; x++){
				if(m_grid[ x, y] != LevelObject.Type.Platform){
					if(m_grid[ x, y] == LevelObject.Type.Ladder ){
						m_moveMatrix[ x, y] |= MOVE_UP_DIR | MOVE_DOWN_DIR;
					}else if(isWalkableCell(x,y)){
						m_moveMatrix[ x, y] |= MOVE_LEFT_DIR | MOVE_RIGHT_DIR;

						if(m_grid[x , y + 1] == LevelObject.Type.Ladder)
							m_moveMatrix[ x, y] |= MOVE_DOWN_DIR;
					}else if(isFallCell(x,y)){
						m_moveMatrix[ x, y] |= MOVE_DOWN_DIR;
					}
				}
			}
		}
	}

	protected bool isWalkableCell(int x,int y){
		if(y > 0){	
			if(m_grid[x , y - 1] == LevelObject.Type.Platform 
			|| m_grid[x , y - 1] == LevelObject.Type.Ladder)
				return true;

			if(x > 0){
				if(m_grid[x - 1, y - 1] == LevelObject.Type.Platform 
				|| m_grid[x - 1, y - 1] == LevelObject.Type.Ladder)
					return true;
			}

			if(x < m_gridSize.x - 1){
				if(m_grid[x + 1, y - 1] == LevelObject.Type.Platform
				|| m_grid[x + 1, y - 1] == LevelObject.Type.Ladder)
				return true;
			}
		}
		return false;
	}

	protected bool isFallCell(int x,int y){
		return	(y < m_gridSize.y - 1 && (canPassDown(m_moveMatrix[x , y + 1]) && !canPassUp(m_moveMatrix[x , y + 1]))) 
			||  (x > 0 && m_grid[x - 1, y] == LevelObject.Type.Platform)
			|| 	(x < m_gridSize.x - 1 && m_grid[x + 1, y] == LevelObject.Type.Platform);
	}

	protected void mapObjects(LevelObject[] objects){
		Rect gridBounds = new Rect (	transform.position.x - (m_gridSize.x * m_cellSize.x) / 2,
											transform.position.y - m_gridSize.y * m_cellSize.y / 2,
											m_gridSize.x * m_cellSize.x, m_gridSize.y * m_cellSize.y);

		for (int i = 0; i < objects.Length; i++) {
			Rect objectBounds = objects [i].getBoundsWorld ();

			int gridLeftCell	= (int)((objectBounds.x - gridBounds.x) / m_cellSize.x);
			int gridTopCell 	= (int)((objectBounds.y - gridBounds.y) / m_cellSize.y);
			int gridRightCell 	= (int)((objectBounds.x + objectBounds.width - gridBounds.x) / m_cellSize.x);
			int gridBottomCell 	= (int)((objectBounds.y + objectBounds.width - gridBounds.y) / m_cellSize.y);

			Debug.Log ("GRID"+" [ x:"+gridBounds.x+",y:"+gridBounds.y+",w:"+gridBounds.width+",h:"+gridBounds.height);
			Debug.Log (""+objects[i].name+" [x:"+objectBounds.x+",y:"+objectBounds.y+",w:"+objectBounds.width+",h:"+objectBounds.height);
			Debug.Log ("Cells: x:[ "+gridLeftCell+" -> "+gridRightCell+"] y:[ "+gridTopCell+" -> "+gridBottomCell+"]");

			if(	gridLeftCell < m_gridSize.x && gridRightCell >= 0
				&&	gridTopCell  < m_gridSize.y && gridBottomCell >= 0){
				gridLeftCell = gridLeftCell < 0 ? 0 : gridLeftCell;
				gridRightCell = gridRightCell >= m_gridSize.x ? (int)m_gridSize.x - 1 : gridRightCell;

				gridTopCell = gridTopCell < 0 ? 0 : gridTopCell;
				gridBottomCell = gridBottomCell >= m_gridSize.y ? (int)m_gridSize.y - 1 : gridBottomCell;

				for (int x = gridLeftCell; x <= gridRightCell; x++) {
					for (int y = gridTopCell; y <= gridBottomCell; y++) {
						m_grid [ x, y] = objects[i].type;
					}
				}
			}
		}
	}

	#if UNITY_EDITOR	
	static Color red = new Color(1,0,0,0.3f);
	static Color yellow = new Color(0,1,0,0.5f);
	static Color gray = new Color(0,0,0,0.3f);
	static Color blue = new Color(0,0,1,0.3f);

	void OnDrawGizmos(){
		if(m_debugLevelMap){
			Gizmos.color = Color.white;
			Gizmos.DrawWireCube(transform.position, new Vector3(m_gridSize.x * m_cellSize.x, m_gridSize.y * m_cellSize.y, 2));

			if (m_grid != null) {
				float width = m_gridSize.x * m_cellSize.x; 
				float height= m_gridSize.y * m_cellSize.y; 

				for (int y = 0; y < m_gridSize.y; y++) {
					for (int x = 0; x < m_gridSize.x; x++) {
						Vector3 pos = new Vector3 (	transform.position.x -  width / 2 + x * m_cellSize.x + m_cellSize.x/2, 
													transform.position.y - height / 2 + y * m_cellSize.y + m_cellSize.y/2, 
													transform.position.z);

						if (m_grid [x, y] == LevelObject.Type.Platform) {
							Gizmos.color = red;
							Gizmos.DrawCube (pos, new Vector3 (m_cellSize.x, m_cellSize.y, 2));
						} else if (m_grid [x, y] == LevelObject.Type.Ladder) {
							Gizmos.color = blue;
							Gizmos.DrawCube (pos, new Vector3 (m_cellSize.x, m_cellSize.y, 2));
						} else {
							Gizmos.color = gray;
							Gizmos.DrawWireCube (pos, new Vector3 (m_cellSize.x, m_cellSize.y, 2));
						}
					}
				}
			}
		}

		if(m_debugMoveMatrix){
			if (m_moveMatrix != null) {
				float width = m_gridSize.x * m_cellSize.x; 
				float height= m_gridSize.y * m_cellSize.y; 

				for (int y = 0; y < m_gridSize.y; y++) {
					for (int x = 0; x < m_gridSize.x; x++) {
						Vector3 pos = new Vector3 (	transform.position.x -  width / 2 + x * m_cellSize.x + m_cellSize.x/2, 
													transform.position.y - height / 2 + y * m_cellSize.y + m_cellSize.y/2, 
													transform.position.z);

						if (m_moveMatrix [ x, y] != 0) {
							Gizmos.color = yellow;
							Gizmos.DrawCube (pos, new Vector3 (m_cellSize.x, m_cellSize.y, 2));
							drawArrows( pos, m_moveMatrix [ x, y]);
						} else {
							//Gizmos.color = red;
							//Gizmos.DrawCube (pos, new Vector3 (m_cellSize.x, m_cellSize.y, 2));
						}
					}
				}
			}
		}
	}

	void drawArrows(Vector3 pos,int move){
		Gizmos.color = Color.black;
		if(canPassLeft(move)){
			float arrSize = 4;
			float arrHalf = arrSize / 2;
			Vector3 p1 = pos + Vector3.left * arrSize;
			Gizmos.DrawLine( pos, p1);

			Gizmos.DrawLine( p1 + Vector3.up * arrHalf , p1 + Vector3.left * arrHalf);
			Gizmos.DrawLine( p1 + Vector3.down * arrHalf, p1 + Vector3.left * arrHalf);
			Gizmos.DrawLine( p1 + Vector3.up * arrHalf, p1 + Vector3.down * arrHalf);
		}

		if(canPassRight(move)){
			float arrSize = 4;
			float arrHalf = arrSize / 2;
			Vector3 p1 = pos + Vector3.right * arrSize;
			Gizmos.DrawLine( pos, p1);

			Gizmos.DrawLine( p1 + Vector3.up * arrHalf , p1 + Vector3.right * arrHalf);
			Gizmos.DrawLine( p1 + Vector3.down * arrHalf, p1 + Vector3.right * arrHalf);
			Gizmos.DrawLine( p1 + Vector3.up * arrHalf, p1 + Vector3.down * arrHalf);
		}

		if(canPassUp(move)){
			float arrSize = 4;
			float arrHalf = arrSize / 2;
			Vector3 p1 = pos + Vector3.up * arrSize;
			Gizmos.DrawLine( pos, p1);

			Gizmos.DrawLine( p1 + Vector3.left * arrHalf , p1 + Vector3.up * arrHalf);
			Gizmos.DrawLine( p1 + Vector3.right * arrHalf, p1 + Vector3.up * arrHalf);
			Gizmos.DrawLine( p1 + Vector3.left * arrHalf, p1 + Vector3.right * arrHalf);
		}


		if(canPassUp(move)){
			float arrSize = 4;
			float arrHalf = arrSize / 2;
			Vector3 p1 = pos + Vector3.down * arrSize;
			Gizmos.DrawLine( pos, p1);

			Gizmos.DrawLine( p1 + Vector3.left * arrHalf , p1 + Vector3.down * arrHalf);
			Gizmos.DrawLine( p1 + Vector3.right * arrHalf, p1 + Vector3.down * arrHalf);
			Gizmos.DrawLine( p1 + Vector3.left * arrHalf, p1 + Vector3.right * arrHalf);
		}
	}
	#endif
}
