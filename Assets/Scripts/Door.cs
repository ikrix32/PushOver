using UnityEngine;
using System.Collections;

public class Door : LevelObject {
	public kSpriteItem m_openAnim;
	public kSpriteItem m_closeAnim; 

	public void open(){
		playOnce (m_openAnim.id);
	}

	public void close(){
		playOnce (m_closeAnim.id);
	}

	public override void SetDefaultState(){
		close ();
	}
}
