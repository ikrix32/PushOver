using UnityEngine;
using System.Collections;

public class Domino : LevelObject {

	public override void SetDefaultState(){
		kSpriteItem anim = new kSpriteItem ();
		string defaultFrame = name + (name != "DominoExploder" ? "_7" : "_6");
		anim.id = (int)sprite.getItemByName (BaseItemData.FLAG_TYPE_FRAME, defaultFrame).getID();
		m_defaultAnim = anim;
		playOnce (anim.id);
	}
}
