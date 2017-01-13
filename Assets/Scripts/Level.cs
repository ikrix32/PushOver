using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;

public class Level : kBehaviourScript {
	public GameObject m_dominosRoot;

	public Door m_enterDoor;
	public Door m_exitDoor;


	public void SetupLevel( kSpriteAsset themeSprite, string backgroundFrameName){
		GameObject go = new GameObject ();
		kSpriteObject background = go.AddComponent<kSpriteObject> ();
		background.name = "background";
		background.transform.parent = transform;
		background.transform.localPosition = Vector3.zero;

		background.sprite = themeSprite;
		kSpriteItem defaultAnim = new kSpriteItem ();
		defaultAnim.id = (int)themeSprite.getItemByName (BaseItemData.FLAG_TYPE_FRAME, backgroundFrameName).getID();
		background.m_defaultAnim = defaultAnim;
		background.playOnce (defaultAnim.id );

		if (m_dominosRoot == null) {
			m_dominosRoot = new GameObject ();
			m_dominosRoot.name = "dominos";
			m_dominosRoot.transform.parent = transform;
			m_dominosRoot.transform.localPosition = Vector3.back;
		}
	}

	public void AddObject(LevelObject template, string name,float x,float y)
	{
		LevelObject obj = (LevelObject)GameObject.Instantiate (template);
		obj.gameObject.SetActive (true);

		obj.name = name;

		if (obj.type == LevelObject.Type.Domino) {
			obj.transform.parent = m_dominosRoot.transform;
		} else {
			obj.transform.parent = transform;
		}
		obj.transform.localPosition = new Vector3 ( x, - y, -1);

		if (obj.type == LevelObject.Type.Door) {
			if (name == "Door_0")
				m_enterDoor = (Door)obj;
			else
				m_exitDoor = (Door)obj;
		}

		obj.SetDefaultState();
	}

//	private void SetDefaultObjectState(LevelObject obj){
//		string defaultFrameName = obj.name;
//		if(obj.type == LevelObject.Type.Domino){
//			defaultFrameName += name == "DominoExploder" ? "_6" : "_7";
//		}
//
//		kSpriteItem defaultAnim = new kSpriteItem ();
//		defaultAnim.id = (int)obj.sprite.getItemByName (BaseItemData.FLAG_TYPE_FRAME, defaultFrameName).getID();
//		obj.m_defaultAnim = defaultAnim;
//		obj.playOnce (defaultAnim.id );
//	}
}
