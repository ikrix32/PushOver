using UnityEngine;
using System.Collections;

public class PathFinder : kBehaviourScript {
	public Vector2 m_gridSize = new Vector2(2,2);
	public Vector2 m_cellSize = new Vector2(10,10);
	//public float m_scanRaycastLength = 10;

	protected int[,] m_grid;


	public bool m_debug;

	public void Scan(){
		m_grid = new int[ (int)m_gridSize.x, (int)m_gridSize.y];

		float width = m_gridSize.x * m_cellSize.x; 
		float height= m_gridSize.y * m_cellSize.y; 
		for (int i = 0; i < m_gridSize.x; i++) {
			for (int j = 0; j < m_gridSize.y; j++) {
				Vector3 pos = new Vector3 (	transform.position.x - width / 2 + i * m_cellSize.x + m_cellSize.x/2, 
											transform.position.y - height / 2 + j * m_cellSize.y + + m_cellSize.y/2, 
											transform.position.z);
				RaycastHit2D hit = Physics2D.Raycast (pos, Vector3.forward);// * m_scanRaycastLength);

				m_grid [i , j] = (hit != null && hit.collider != null) ? 1 : 0;
			}
		}
	}

	#if UNITY_EDITOR	
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
						
						if (m_grid [i, j] != 0) {
							Gizmos.color = Color.red;
							Gizmos.DrawCube (pos, new Vector3 (m_cellSize.x, m_cellSize.y, 2));
						} else {
							Gizmos.color = Color.green;
							Gizmos.DrawWireCube (pos, new Vector3 (m_cellSize.x, m_cellSize.y, 2));
						}
					}
				}
			}
		}
	}
	#endif
}
