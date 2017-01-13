using UnityEngine;
using System.Collections;

public class Platform : LevelObject {

	public override void SetDefaultState(){
		kSpriteItem anim = new kSpriteItem ();
		anim.id = (int)sprite.getItemByName (BaseItemData.FLAG_TYPE_FRAME, name).getID();
		m_defaultAnim = anim;
		playOnce (anim.id);

		BoxCollider2D collider = gameObject.AddComponent<BoxCollider2D> ();
		collider.size = new Vector2 ( collider.size.x, collider.size.y / 2);
	}

}
