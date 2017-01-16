using UnityEngine;
using System.Collections;

public class Platform : LevelObject {

	public override void SetDefaultState(){
		kSpriteItem anim = new kSpriteItem ();
		anim.id = (int)sprite.getItemByName (BaseItemData.FLAG_TYPE_FRAME, name).getID();
		m_defaultAnim = anim;
		playOnce (anim.id);

		BoxCollider2D collider = gameObject.AddComponent<BoxCollider2D> ();
		collider.offset = new Vector2 (getBounds().width / 2, - getBounds().height / 2);
		collider.size = new Vector2 ( getBounds().width, getBounds().height / 2);
	}
}
