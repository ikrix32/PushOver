using UnityEngine;
using System.Collections;

public class LevelMap : kBehaviourScript {
	public class CellAcc
	{
		public static int MOVE_ANY_DIR = 1;
		public static int MOVE_LEFT_DIR= 2;
		public static int MOVE_RIGHT_DIR= 4;
		public static int MOVE_UP_DIR = 8;
		public static int MOVE_DOWN_DIR = 16;

	}

	public Vector2 m_gridSize = new Vector2(2,2);
	public Vector2 m_cellSize = new Vector2(10,10);

	protected LevelObject.Type[,] m_grid;


	public bool m_debug;

	public void Scan()
	{
		m_grid = new LevelObject.Type[ (int)m_gridSize.x, (int)m_gridSize.y];

		Platform[] platforms = transform.parent.gameObject.GetComponentsInChildren<Platform>();
		Ladder[] ladders	 = transform.parent.gameObject.GetComponentsInChildren<Ladder>();

		mapObjects (platforms);
		mapObjects (ladders);
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
						m_grid [x, y] = objects[i].type;
					}
				}
			}
		}
	}

	#if UNITY_EDITOR	
	static Color red = new Color(1,0,0,0.3f);
	static Color blue = new Color(0,0,1,0.3f);
	static Color green = new Color(0,1,0,0.1f);
	void OnDrawGizmos(){
		if(m_debug){
			Gizmos.color = Color.white;
			Gizmos.DrawWireCube(transform.position, new Vector3(m_gridSize.x * m_cellSize.x, m_gridSize.y * m_cellSize.y, 2));

			if (m_grid != null) {
				float width = m_gridSize.x * m_cellSize.x; 
				float height= m_gridSize.y * m_cellSize.y; 

				for (int i = 0; i < m_gridSize.x; i++) {
					for (int j = 0; j < m_gridSize.y; j++) {
						Vector3 pos = new Vector3 (	transform.position.x - width / 2 + i * m_cellSize.x + m_cellSize.x/2, 
													transform.position.y - height / 2 + j * m_cellSize.y + + m_cellSize.y/2, 
													transform.position.z);

						if (m_grid [i, j] == LevelObject.Type.Platform) {
							Gizmos.color = red;
							Gizmos.DrawCube (pos, new Vector3 (m_cellSize.x, m_cellSize.y, 2));
						} else if (m_grid [i, j] == LevelObject.Type.Ladder) {
							Gizmos.color = blue;
							Gizmos.DrawCube (pos, new Vector3 (m_cellSize.x, m_cellSize.y, 2));
						} else {
							Gizmos.color = green;
							Gizmos.DrawWireCube (pos, new Vector3 (m_cellSize.x, m_cellSize.y, 2));
						}
					}
				}
			}
		}
	}
	#endif
}
