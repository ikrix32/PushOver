using UnityEngine;
using System.Collections;

public class Ant : kSpriteObject {
	public kSpriteItem m_idleAnim;
	public kSpriteItem m_walkLeftAnim;
	public kSpriteItem m_walkRightAnim;
	public kSpriteItem m_falltAnim;
	public kSpriteItem m_upAnim;
	public kSpriteItem m_downAnim;

	public Rigidbody2D m_rigidBody;
	public float impulseForce;

	protected override void onUpdate(){
		base.onUpdate ();

		if (Input.GetKey(KeyCode.LeftArrow))
			m_rigidBody.AddForce(Vector2.left * impulseForce);

		if (Input.GetKey(KeyCode.RightArrow))
			m_rigidBody.AddForce(Vector2.right * impulseForce);
	}
}
