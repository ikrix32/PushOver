#define DEBUG_PATH_FINDING
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PathFinder {
	LevelCollisionMap m_map;

	private XPath 		m_path;
	private List<XPath> m_paths = new List<XPath> ();

	IntVector2 m_pStart;
	IntVector2 m_pEnd;

	public PathFinder(LevelCollisionMap map){
		m_map = map;
		m_path = new XPath(map);
	}

	public List<XPath> FindPath(IntVector2 fromCell,IntVector2 toCell){
		m_pStart = fromCell;
		m_pEnd = toCell;

		FindPathNew (fromCell, toCell);
		return m_paths;
	}

	public void FindPathNew( IntVector2 from, IntVector2 to) 
	{
		m_path.Clear ();
		m_pStart = from;
		m_pEnd = to;

		#if DEBUG_PATH_FINDING
		m_map.StartCoroutine (recursePath(from,to));
		#else
		recursePath(from,to);
		#endif

		/*List<int> toRemove = new List<int> ();

		//remove dangerous paths
		for(int i = 0; i < m_paths.Count; i++){
			int noFallCells = 0;
			for (int j = 0; j < m_paths [i].Size; j++) {
				IntVector2 cell = m_paths [i].Get (j);
				if (canPassDown (cell) && !canPassUp (cell)) {
					noFallCells++;
					if (noFallCells > 7) {
						toRemove.Add (i);
						j = m_paths [i].Size;//go to next path
					}
				} else
					noFallCells = 0;
			}
		}

		for (int i = toRemove.Count - 1; i >= 0; i--)
			m_paths.RemoveAt (toRemove[i]);

		Debug.LogError ("Remaining paths "+m_paths.Count);*/
	}

	#if DEBUG_PATH_FINDING
	private IEnumerator recursePath(IntVector2 current, IntVector2 to) {
	yield return null;
	#else
	private void recursePath(IntVector2 current, IntVector2 to) {
	#endif
		m_path.Push (current);

		if(current.x == to.x && current.y == to.y) {
			// arrived at destination
			m_paths.Add (new XPath(m_path));
		}else {
			if (m_map.CanPassCellLeft (current)) {
				IntVector2 left = new IntVector2 (current.x - 1, current.y);
				if (!m_path.Contains (left)) {
					#if DEBUG_PATH_FINDING
					yield return m_map.StartCoroutine(recursePath (left, to));
					#else
					recursePath ( left, to);
					#endif
				}// else cannot go this way
			}

			if (m_map.CanPassCellRight (current)) {
				IntVector2 right = new IntVector2 (current.x + 1, current.y);
				if (!m_path.Contains (right)) {
					#if DEBUG_PATH_FINDING
					yield return  m_map.StartCoroutine(recursePath (right, to));
					#else
					recursePath ( right, to);
					#endif
				}// else cannot go this way
			}

			if (m_map.CanPassCellDown (current)) {
				IntVector2 down = new IntVector2 (current.x, current.y - 1);
				if (!m_path.Contains (down)) {
					#if DEBUG_PATH_FINDING
					yield return  m_map.StartCoroutine(recursePath (down, to));
					#else
					recursePath (down, to);
					#endif
				}//else cannot go this way
			}

			if (m_map.CanPassCellUp (current)) {
				IntVector2 up = new IntVector2 (current.x, current.y + 1);
				if (!m_path.Contains (up)) {
					#if DEBUG_PATH_FINDING
					yield return  m_map.StartCoroutine(recursePath (up, to));
					#else
					recursePath (up, to);
					#endif
				}//else cannot go this way
			}
		}

		m_path.Pop();
	}

	#if UNITY_EDITOR
	public void DrawPaths(Color[] pathColors){
		Vector3 start = m_map.CellToPos (m_pStart); 
		Gizmos.color = Color.red;
		Gizmos.DrawCube (start, new Vector3 (m_map.m_cellSize.x, m_map.m_cellSize.y, 2));

		Vector3 end = m_map.CellToPos (m_pEnd);
		Gizmos.color = Color.red;
		Gizmos.DrawCube (end, new Vector3 (m_map.m_cellSize.x, m_map.m_cellSize.y, 2));

		for (int i = 0; i < m_paths.Count; i++) {
			if(m_paths[i] != null)
				m_paths[i].Draw(pathColors[i]);
		}

		m_path.Draw(new Color(1,1,0,0.8f));
	}


	#endif
}
