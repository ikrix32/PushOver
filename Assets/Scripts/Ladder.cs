using UnityEngine;
using System.Collections;

public class Ladder : LevelObject {
	
	public override void SetDefaultState(){
		kSpriteItem anim = new kSpriteItem ();
		anim.id = (int)sprite.getItemByName (BaseItemData.FLAG_TYPE_FRAME, name).getID();
		m_defaultAnim = anim;
		playOnce (anim.id);
	}
}
