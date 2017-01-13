using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class kPicture : kPickerItem 
{
	[SerializeField]
	protected bool 	  m_useCustomSize;

	[SerializeField]
	protected Vector2 m_size = new Vector2(100,100);

	[SerializeField]
	protected Texture m_texture;

	protected virtual Texture texture{
		get{ return m_texture;}
		set{ m_texture = value;}
	}

	[SerializeField]
	protected kAlignMode m_alignMode = kAlignMode.VCENTER_CENTER;

	public bool useCustomSize{
		get{ return m_useCustomSize;}
		set{
			m_useCustomSize = value;
			updateMesh();
		}
	}

	public Vector2 size{
		get{ return m_size;}
		set{
			m_size = value;
			updateMesh();
		}
	}

	public kAlignMode alignment{
		get{ return m_alignMode;}
		set{
			if(m_alignMode != value){
				m_alignMode = value;
				updateMesh();
			}
		}
	}

	/** Textures downloaded using LoadFromUrl should be destroyed */
	protected bool m_dinamicTexture;
	public virtual void setTexture(Texture tex,bool dinamic){
		unloadTexture();
		m_dinamicTexture = dinamic;
		texture = tex;
		if(texture != null){
			texture.filterMode = FilterMode.Bilinear;
			texture.mipMapBias = -0.7f;
		}

		if(m_material != null)
			m_material.mainTexture = texture;
			
		updateMesh();
	}

	public Texture getTexture(){
		return texture;
	}

	protected override void onInit() 
	{
		base.onInit();
		updateMesh();
		if(m_material != null)
			m_material.mainTexture = texture;
	}
	
	protected override void onUpdate() {
		base.onUpdate();
		#if UNITY_EDITOR
		if (!Application.isPlaying) {
			updateMesh();
			updateObjectMesh (true); 
		}
		#endif
	}

	protected override void updateMesh() 
	{
		Vector2 _size = useCustomSize ? m_size : (texture != null ? new Vector2(texture.width,texture.height) : Vector2.one * 10);

		if(objMesh.triangles == null)
			objMesh.triangles = new int[]{ 0, 3, 2, 0, 2, 1 };

		bool setUVs = (objMesh.UVs == null || objMesh.UVs.Length != 4);
		bool setColors = true;//(objMesh.colors == null || objMesh.colors.Length != 4);

		Color vertColor = texture != null ? Color.white : Color.clear;

		if (setUVs)		objMesh.UVs = new Vector2[]{ new Vector2(0, 0), new Vector2(1, 0), new Vector2(1, 1), new Vector2(0, 1) };
		if(setColors) 	objMesh.colors = new Color[]{ vertColor, vertColor,vertColor, vertColor};

		if (objMesh.vertices == null || objMesh.vertices.Length != 4) {
			objMesh.vertices = new Vector3[4];
		}
		float xOff = 0;
		float yOff = 0;

		if (((int)m_alignMode & (int)kAlign.TOP) != 0)
			yOff = -_size.y;
		else if (((int)m_alignMode & (int)kAlign.VCENTER) != 0)
			yOff = -_size.y / 2;
	
		if (((int)m_alignMode & (int)kAlign.RIGHT) != 0)
			xOff = -_size.x;
		else if (((int)m_alignMode & (int)kAlign.HCENTER) != 0)
			xOff = -_size.x / 2;

		objMesh.vertices = new Vector3[]{	new Vector3(xOff, yOff, 0), new Vector3(xOff + _size.x, yOff, 0),
			new Vector3(xOff + _size.x, yOff + _size.y, 0), new Vector3(xOff, yOff + _size.y , 0) };

		objMesh.center.x = 0;
		objMesh.center.y = 0;
		objMesh.size = _size;

		meshChanged = true;
	}

	/*public Color m_blendColor = Color.white;
	public override void setBlendingColor(Color color) {
		if(name == "logo")
			Debug.Log("Picture blendColor:"+color);
		m_blendingColor = color;
		if(objMesh.colors != null){
			for (int i = 0; i < objMesh.colors.Length; i++) 
				objMesh.colors[i] = color;
		}
		colorsChanged = true;
	}*/
	/** Path to texture relative to Resources folder, ex: avatars/texBody */
	public bool loadFromResources(string path){
		Texture tex = (Texture)Resources.Load(path);
		setTexture(tex,false);
		return tex != null;
	}

	public virtual void unloadTexture(){
		if(texture != null && m_dinamicTexture){
			DestroyImmediate(texture);
		}
		texture = null;
		if(m_material != null)
			m_material.mainTexture = null;
		m_dinamicTexture = false;
		clearObjectMesh();
	}

	protected override void onDestroy(){
		unloadTexture();
		base.onDestroy();
	}

#if UNITY_EDITOR	
	void OnDrawGizmos(){
		Vector2 vSize = useCustomSize ? m_size : (texture != null ? new Vector2(texture.width,texture.height) : Vector2.one * 10);
		vSize = new Vector2(vSize.x * transform.localScale.x, vSize.y * transform.localScale.y);

		float xOff = 0;
		float yOff = 0;
		
		if (((int)m_alignMode & (int)kAlign.TOP) != 0)
			yOff = -vSize.y;
		else if (((int)m_alignMode & (int)kAlign.VCENTER) != 0)
			yOff = -vSize.y / 2;
		
		if (((int)m_alignMode & (int)kAlign.RIGHT) != 0)
			xOff = -vSize.x;
		else if (((int)m_alignMode & (int)kAlign.HCENTER) != 0)
			xOff = -vSize.x / 2;

		Rect viewPortWorldCoord = RectUtil.newRectangle(transform.position.x + xOff, transform.position.y + yOff,vSize.x ,vSize.y);
		
		if(viewPortWorldCoord.width != 0 && viewPortWorldCoord.height != 0){
			Vector3 center = new Vector3(viewPortWorldCoord.x + viewPortWorldCoord.width/2,viewPortWorldCoord.y + viewPortWorldCoord.height/2,0);
			Gizmos.color = Color.yellow;
			Gizmos.DrawWireCube(center,new Vector3(viewPortWorldCoord.width,viewPortWorldCoord.height,2));
		}
	}
#endif
}
