using UnityEngine;
using System.Collections;

public class kUnitySprite : kObject
{
	protected SpriteRenderer m_spriteRenderer;

	protected override void onInit ()
	{
		base.onInit ();
		m_spriteRenderer = GetComponent<SpriteRenderer> ();
	}

	public override void updateObjectMesh (bool forceClipUpdate = false)
	{
		if (m_spriteRenderer != null) {
			objMesh.size = m_spriteRenderer.bounds.size;
			objMesh.center = transform.localPosition;
		}
		base.updateObjectMesh (forceClipUpdate);

	}

	public override Rect getBounds(){
		Vector2 center = transform.localPosition;
		Vector2 size   = m_spriteRenderer.bounds.size ;
		return new Rect(center.x - size.x / 2, center.y - size.y / 2, size.x, size.y);
	}
	/*
	#if UNITY_EDITOR
	void OnDrawGizmos(){
		if(objMesh != null){
			Rect b = getBoundsWorld();
			Vector3 center = new Vector3(b.center.x,b.center.y,0);
			Gizmos.color = Color.green;
			Gizmos.DrawWireCube(center,new Vector3(b.width,b.height,2));
		}
		IntRect clipRect = m_clipSource != null ? m_clipSource.getClip() : RectUtil.RECT_NO_CLIP;

		if(clipRect.width != 0 && clipRect.height != 0){
			Rect b = new Rect(clipRect.x,clipRect.y,clipRect.width,clipRect.height);
			Vector3 center = new Vector3(b.center.x,b.center.y,0);
			Gizmos.color = Color.cyan;
			Gizmos.DrawWireCube(center,new Vector3(b.width,b.height,2));
		}
	}
	#endif
	*/
}
