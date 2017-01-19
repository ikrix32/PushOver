using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class XPath{
	protected List<IntVector2> m_waypoints = new List<IntVector2>();

	public XPath(){
	}

	public XPath(XPath  p){
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
}
