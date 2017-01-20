using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;

public class Level : kBehaviourScript {

	protected LevelCollisionMap m_finder;

	public kSpriteObject m_background;
	public GameObject m_dominosRoot;

	public Door m_enterDoor;
	public Door m_exitDoor;

	private Ant m_ant;

	public void StartLevel(Ant ant){
		m_ant = ant;

		m_ant.transform.position = m_enterDoor.transform.position + Vector3.down * 50;

		m_finder = gameObject.GetComponent<LevelCollisionMap> ();
		m_finder.Scan ();

		kTouchable touch = m_background.GetComponent<kTouchable> ();
		if(touch == null)
			touch = m_background.gameObject.AddComponent<kTouchable> ();

		touch.onTouchEnd = (Touch t)=>{
			Debug.Log("Touch end");
		};

		touch.onTouchPressed = TouchPressed;

		touch.onTouchRelease = TouchReleased;
	}
		
	public void SetupLevel( kSpriteAsset themeSprite, string backgroundFrameName){
		GameObject go = new GameObject ();
		m_background = go.AddComponent<kSpriteObject> ();
		m_background.name = "background";
		m_background.transform.parent = transform;
		m_background.transform.localPosition = Vector3.zero;

		m_background.sprite = themeSprite;
		kSpriteItem defaultAnim = new kSpriteItem ();
		defaultAnim.id = (int)themeSprite.getItemByName (BaseItemData.FLAG_TYPE_FRAME, backgroundFrameName).getID();
		m_background.m_defaultAnim = defaultAnim;
		m_background.playOnce (defaultAnim.id );

		if (m_dominosRoot == null) {
			m_dominosRoot = new GameObject ();
			m_dominosRoot.name = "dominos";
			m_dominosRoot.transform.parent = transform;
			m_dominosRoot.transform.localPosition = Vector3.back;
		}
	}

	protected void TouchPressed(Touch t){
		//Debug.LogError ("Touch pressed at pos:"+t.position);
		m_finder.FindPath ( m_ant.transform.position, t.position);
	}

	protected void TouchReleased(Touch t){
		//Debug.LogError ("Touch released at pos:"+t.position);
	}

	public void SetupPathfinding(Vector2 cellSize){
		m_finder = gameObject.AddComponent<LevelCollisionMap> ();
		m_finder.m_cellSize = cellSize;

		Rect bkgBounds = m_background.getBounds ();
		m_finder.m_gridSize = new IntVector2 ( (int)(bkgBounds.width / cellSize.x), (int)(bkgBounds.height / cellSize.y));
		m_finder.Scan ();
	}

	public void AddObject(LevelObject template, string name,float x,float y)
	{
		LevelObject obj = (LevelObject)GameObject.Instantiate (template);
		obj.name = name;

		if (obj.type == LevelObject.Type.Domino)
			obj.transform.parent = m_dominosRoot.transform;
		else
			obj.transform.parent = transform;
		
		obj.transform.localPosition = new Vector3 ( x, - y, -1 + (obj.type == LevelObject.Type.Ladder ? - 2 : 0));

		if (obj.type == LevelObject.Type.Door) {
			if (name == "Door_0")
				m_enterDoor = (Door)obj;
			else
				m_exitDoor = (Door)obj;
		}

		obj.SetDefaultState();
		obj.gameObject.SetActive (true);
	}
}
