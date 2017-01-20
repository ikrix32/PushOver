using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class XPath{
	LevelCollisionMap m_collisionMap;

	protected List<IntVector2> m_waypoints = new List<IntVector2>();

	public XPath(LevelCollisionMap collisionMap){
		m_collisionMap = collisionMap;
	}

	public XPath(XPath  p){
		m_collisionMap = p.m_collisionMap;
		SetWaypoints (p.m_waypoints.ToArray());
	}

	public void SetWaypoints(IntVector2[]  waypoints){
		m_waypoints.AddRange( waypoints);
	}

	public IntVector2 Get(int i){
		return m_waypoints [i];
	}

	public bool Contains(IntVector2 waipoint){
		return m_waypoints.Contains (waipoint);
	}

	public void Push(IntVector2 waipoint){
		m_waypoints.Add (waipoint);
	}

	public void Pop(){
		m_waypoints.RemoveAt (m_waypoints.Count - 1);
	}

	public void Clear(){
		m_waypoints.Clear ();
	}

	public int Size{
		get{ return m_waypoints == null ? 0 : m_waypoints.Count; }
	}

	#if UNITY_EDITOR
	public void Draw(Color col)
	{
		for ( int i = 0; i < m_waypoints.Count; i++) {
			IntVector2 waipoint = m_waypoints[i];
			Vector3 pos = m_collisionMap.CellToPos (waipoint);
			Gizmos.color = col;
			Gizmos.DrawCube ( pos, new Vector3 (m_collisionMap.m_cellSize.x, m_collisionMap.m_cellSize.y, 2));
		}
	}
	#endif
}