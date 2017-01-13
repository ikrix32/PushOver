using UnityEngine;
using System.Collections;
using System;

public enum kAlign
{
	LEFT 	= 1<<0,
	HCENTER = 1<<1,
	RIGHT	= 1<<2,
	TOP		= 1<<3,
	VCENTER	= 1<<4,
	BOTTOM  = 1<<5,
};


public enum kAlignMode
{
	TOP_LEFT 		= kAlign.TOP|kAlign.LEFT,//2+1 = 3
	TOP_CENTER		= kAlign.TOP|kAlign.HCENTER,//2 + 2*4 =18
	TOP_RIGHT		= kAlign.TOP|kAlign.RIGHT,//2+2*5 = 34
	VCENTER_LEFT	= kAlign.VCENTER|kAlign.LEFT,//8+1= 9
	VCENTER_CENTER	= kAlign.VCENTER|kAlign.HCENTER,//8+2*4=24
	VCENTER_RIGHT	= kAlign.VCENTER|kAlign.RIGHT,//8+2*5=42
	BOTTOM_LEFT		= kAlign.BOTTOM|kAlign.LEFT,//4+1=5
	BOTTOM_CENTER 	= kAlign.BOTTOM|kAlign.HCENTER,//4+8=12
	BOTTOM_RIGHT	= kAlign.BOTTOM|kAlign.RIGHT,//4+2*4=20
};

public class kMesh{
	public Vector3[] 	vertices;
	public int[] 	  	triangles;
	public Vector2[] 	UVs;
	public Color[]	  	colors;
	
	public Vector2		center;
	public Vector2		size;
	public kMesh(){ size = Vector2.one;}
}

public interface ClipSource{
	IntRect getClip();
}

public interface kMeshChangeListener{
	void objectMeshChnaged(kObject obj);
}

public class kObject : kBehaviourScript 
{
#if UNITY_EDITOR	
	[System.NonSerialized]
	public 	bool		debug = false;

	protected kSpriteAsset	lastSourceSprite = null;
#endif
	protected const int SHADER_CLIPLESS = 0;
	protected const int SHADER_CLIP 	= 1;
	protected const int SHADER_CUSTOM 	= 2;
	protected int m_crtShader = SHADER_CLIPLESS;
#if	ENABLE_CREATE_KSPRITE_ASSET
	//DO not use this to assign new sprite in code,use setter!!!
	public  	kSprite sourceSprite;
#endif

	public  kSpriteAsset sprite;

	public  	void setSourceSprite(kSpriteAsset newSprite){
	
		if(sprite != newSprite){
			if(sprite != null)
				sprite.dropReference();
		
			sprite = newSprite;
#if UNITY_EDITOR
			reset();
#endif
			if(sprite != null){
				sprite.grabReference();
				updateTexture();
				updateMesh();
			}
		}
	}

	//Mesh
	protected kMesh objMesh = new kMesh();
	protected kMesh	clippedMesh = new kMesh();
	
	//temporary vertices,used to apply current frame transforms to object
	protected bool	meshChanged   = false;
	protected bool	clipChanged	  = false;		
	protected bool	colorsChanged = false;
	
	//todo kkk [HideInInspector]
	public kView	m_clipSource;
	
	// disables clipping precheck (for rotated objects)
	public bool m_forceRender = false;
	
	[HideInInspector]
	public Transform  m_parentObject = null;
	
	protected MeshFilter	meshFilter = null;
	protected Transform 	m_transform;
	protected Renderer  	m_renderer;
	protected bool	  		m_renderEnabled;

	private static Shader s_shader = null;
	private static Shader s_shader_no_clip = null;

	[SerializeField]
	public Shader m_customShader = null;

	protected virtual Shader shader{
		get{ return s_shader;}
		set{ s_shader = value;}
	}

	protected virtual Shader shader_no_clip{
		get{ return s_shader_no_clip;}
		set{ s_shader_no_clip = value;}
	}

	public Shader customShader{
		get{ return m_customShader;}
		set{ m_customShader = value; clipMesh(false);}
	}

	public Material 	m_material{
		get{
			if(GetComponent<Renderer>() == null)
				return null;
			if(Application.isEditor)
				return GetComponent<Renderer>().sharedMaterial;
			else
				return GetComponent<Renderer>().material;
		}
		set{
			if(GetComponent<Renderer>() != null){
				if(Application.isEditor)
					GetComponent<Renderer>().sharedMaterial = value;
				else
					GetComponent<Renderer>().material = value;
			}
		}
	}
	#if UNITY_EDITOR
	private Color m_lastBlendColor = Color.white;
	#endif
	public Color m_blendingColor = Color.white;

	public virtual void setBlendingColor(Color c){
		m_blendingColor = c;
		if(m_material != null){
			m_material.color = m_blendingColor;
		}
	}

	public virtual Color getBlendingColor(){
		return m_blendingColor;
	}

	public virtual void setBlendingColor(float r, float g, float b)
	{
		Color x = getBlendingColor();
		x.r = r;
		x.g = g;
		x.b = b;
		setBlendingColor(x);
	}

	public virtual void setAlpha(float alpha){
		Color x = getBlendingColor();
		x.a = alpha;
		setBlendingColor(x);
	}

#if UNITY_EDITOR
	public void EditorRefresh(){
		if (meshFilter != null) {
			meshFilter.sharedMesh = new Mesh();
		}
		onInit();
	}
#endif
	protected override void onInit(){
		base.onInit(); 
#if	ENABLE_CREATE_KSPRITE_ASSET
		if(!Application.isPlaying && sourceSprite != null/* && !sourceSprite.isLoaded()*/){
			sprite = kSpriteAssetHelper.getSpriteAsset(sourceSprite);
		}
#endif
		if(sprite != null/* && !sourceSprite.isLoaded()*/ && !Application.isPlaying){
			//Debug.LogError(gameObject.name+" error.Source sprite not loaded:" + sourceSprite.gameObject.name);
			sprite.loadSprite();
		}
		//rem - this cause the sprite to be referenced twice by avatar components
		//if(sourceSprite != null) sourceSprite.grabReference();

		if(meshFilter == null && (meshFilter = GetComponent<MeshFilter>()) == null){
			//Debug.Log("Add mesh filter on object "+gameObject.name);
			if(!(this is kTextMesh) && !(this is kUnitySprite)){
				gameObject.layer = kSpriteAsset.kSpriteLayer;
				meshFilter = gameObject.AddComponent<MeshFilter>();
			}else{
				if(gameObject.layer == 0)
					gameObject.layer = kSpriteAsset.kSpriteLayer;
			}
		}
		
		m_renderer = GetComponent<Renderer>();
		if(m_renderer == null)
			m_renderer = gameObject.AddComponent<MeshRenderer>();
#if UNITY_EDITOR
		lastSourceSprite = sprite;
#endif

		if(!Application.isPlaying){
			if(meshFilter != null && meshFilter.sharedMesh == null)
				meshFilter.sharedMesh = new Mesh();
		}else if(meshFilter != null && meshFilter.mesh == null)
			meshFilter.mesh = new Mesh();

		if(s_shader == null){
			//#if UNITY_EDITOR
			//s_shader = Shader.Find("kSprite/Sprite Shader Unity Only");
			//#else
			s_shader = Shader.Find("kSprite/Sprite Shader");
			//#endif
		}

		if(s_shader_no_clip == null)
			s_shader_no_clip = Shader.Find("kSprite/Sprite ShaderN");

		m_material = new Material(s_shader);
		m_crtShader =  SHADER_CLIP;

		m_material.color = m_blendingColor;

		if(sprite != null&& sprite.isLoaded())
			m_material.mainTexture = sprite.sourceTexture;

		m_renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
		m_renderer.receiveShadows = false;
		m_transform = transform;
	}
	
	protected override void onStart(){
		base.onStart();
		
		if(m_clipSource == null){
			m_parentObject = m_transform.parent;
			//Debug.Log("On init Find clip source for "+name);
			findClipSourceFor(this,m_transform.parent);
		}
	}

	protected void updateTexture(){
		if(m_material != null){
			if(sprite != null && sprite.isLoaded())
				m_material.mainTexture = sprite.sourceTexture;
			else
				m_material.mainTexture = null;
		}
	}

	protected override void onUpdate(){
		base.onUpdate();

#if UNITY_EDITOR
		if(!Application.isPlaying)
		{
			//source sprite changed stop animation
			if(lastSourceSprite != sprite){
				if(lastSourceSprite != null)
					lastSourceSprite.dropReference();

				lastSourceSprite = sprite;
			
				reset();

				if(sprite != null){
					sprite.grabReference();
					updateTexture();
					updateMesh();
				}
			}

			if(m_material.mainTexture == null && ( sprite != null && sprite.isLoaded() )){
				m_material.mainTexture = sprite.sourceTexture;
			}

			if(m_blendingColor != m_lastBlendColor){
				setBlendingColor(m_blendingColor);
			}
			m_lastBlendColor = m_blendingColor;
		}
#endif
		if(m_transform.hasChanged || hasChangedOnLastFrame){
			if(m_parentObject != m_transform.parent ){
				onParentChange();
			}
		}
		updateSoundPlayback();
	}
	
	public virtual void onParentChange(){
		m_parentObject = transform.parent;
		//Debug.Log("Find clip source for "+name);
		findClipSourceFor(this,m_parentObject);
	}
	
	public virtual void childMeshChanged(kObject child){
		//TODO - remove this update using containter reference
		/*if(child != this && GetComponent<kScrollableContainer>() != null){
			GetComponent<kScrollableContainer>().updateContentBounds();
		}else if(transform.parent != null){
			kObject parent = transform.parent.GetComponent<kObject>();
			if(parent != null) parent.childMeshChanged(child);
		}*/	
	}
	int steps = 0;
	public virtual void findClipSourceFor(kObject obj,Transform parent){
		if(parent == null){
			obj.setClipSource(null);
			return;
		}
		obj.steps++;
		kObject pObject = parent.GetComponent<kObject>();
		if(pObject != null){
			//Debug.Log("Found clip for "+obj.name+" in "+obj.steps+" steps");
			obj.setClipSource(pObject.getClipSource());
		}else 
			findClipSourceFor(obj,parent.parent);
	}

	protected virtual void setClipSource(ClipSource source){
		//if(source == null)
		//	Debug.Log("Set null clip source on "+name);
		if(m_clipSource != source){
			m_clipSource = (kView)source;
			clipChanged = true;
			//clipMesh(false);//todo kkk
			updateClipSourceOnSubTree(gameObject,source);
		}
	}
	
	protected virtual ClipSource getClipSource(){
		return m_clipSource;
	}
	
	protected void updateClipSourceOnSubTree(GameObject obj,ClipSource source){
		foreach (Transform child in obj.transform){
			kObject kObj = child.GetComponent<kObject>();
			if (kObj != null) kObj.setClipSource(source);
			else updateClipSourceOnSubTree(child.gameObject,source);
		}
	}
	
	public virtual Rect getBoundsWorld(){
		Vector2 center = Vector2.Scale(objMesh.center,transform.lossyScale);
		Vector2 size   = Vector2.Scale(objMesh.size,transform.lossyScale);
		
		center = center + (Vector2)transform.position;
		return new Rect(center.x - size.x / 2, center.y - size.y / 2, size.x, size.y);
	}
	
	public virtual Rect getClippedBoundsWorld(){
		Vector2 center = Vector2.Scale(clippedMesh.center,transform.lossyScale);
		Vector2 size   = Vector2.Scale(clippedMesh.size,transform.lossyScale);
		
		center = center + (Vector2)transform.position;
		return new Rect(center.x - size.x/2 ,center.y - size.y /2,size.x,size.y);
	}
	
	public virtual Rect getBounds(){
		Vector2 center = Vector2.Scale(objMesh.center,transform.localScale);
		Vector2 size   = Vector2.Scale(objMesh.size,transform.localScale);
		return new Rect(center.x - size.x / 2, center.y - size.y / 2, size.x, size.y);
	}
	
	public virtual Rect getClippedBounds(){
		Vector2 center = Vector2.Scale(clippedMesh.center,transform.localScale); 
		Vector2 size   = Vector2.Scale(clippedMesh.size,transform.localScale);
		
		return new Rect(center.x - size.x/2 ,center.y - size.y /2,size.x,size.y);
	}

	public virtual int getSizeIncreaseForTouch(){
		return 0;
	}

	public virtual bool isReceivingTouchEventsAfterLoosingFocus(){
		//fix scroll issue when objects
		if(m_clipSource != null && (m_clipSource.allowHorizontalScroll || m_clipSource.allowVerticalScroll))
			return true;
		return false;
	}
	
	public kMesh getObjectMesh(){
		return objMesh;
	}
	
	protected virtual void updateMesh(){
		Debug.Log("Update unknown mesh");
	}
	
#if UNITY_EDITOR		
	protected virtual void reset(){}
#endif
	
	protected bool hasChangedOnLastFrame = false;
	protected override void onLateUpdate(){
		base.onLateUpdate();
		updateObjectMesh();

		hasChangedOnLastFrame = transform.hasChanged;
		transform.hasChanged = false;
	}	
	
	protected IntRect clipLocal = new IntRect();
	protected IntRect clipInters= new IntRect();	
	
	protected void clipMesh(bool newMesh)
	{
		if(m_renderer == null || m_material == null) return;
		
		IntRect clipRect = m_clipSource != null ? m_clipSource.getClip() : RectUtil.RECT_NO_CLIP;
			
		float clipCenterX = clippedMesh.center.x;
		float clipCenterY = clippedMesh.center.y;
		float clipSizeX = clippedMesh.size.x;
		float clipSizeY = clippedMesh.size.y;
		
		if(clipRect.width != RectUtil.RECT_NO_CLIP.width 
		&& clipRect.height!= RectUtil.RECT_NO_CLIP.height){
			RectToLocalCoord(ref clipRect, ref clipLocal);
			RectUtil.intersect(ref clipLocal,ref objMesh.center,ref objMesh.size,ref clipInters);
			clipInters.getCenter(ref clippedMesh.center);
			clipInters.getSize(ref clippedMesh.size);
		}else{
			clippedMesh.center	 = objMesh.center;
			clippedMesh.size	 = objMesh.size; 
		}
				
		bool changed = clipCenterX != clippedMesh.center.x || clipCenterY != clippedMesh.center.y || clipSizeX != clippedMesh.size.x || clipSizeY != clippedMesh.size.y;
		clipChanged = clipChanged || changed;
		
		//if(!changed) return;
	
		if(!m_forceRender && (clippedMesh.size.x <= 0 || clippedMesh.size.y <= 0)){
			m_renderer.enabled = false;
			return;
		}
		m_renderer.enabled = true;

		//if(!(this is kTextMesh))
		{
			//Color blend = m_material.color;
			if(clipRect.width != RectUtil.RECT_NO_CLIP.width || clipRect.height != RectUtil.RECT_NO_CLIP.height){
				bool isClipped = objMesh.size.x - clippedMesh.size.x > 1  || objMesh.size.y - clippedMesh.size.y > 1;
				if(isClipped){
					if(m_crtShader != SHADER_CLIP){
						m_crtShader = SHADER_CLIP;
						m_material.shader = shader;
					}
					m_material.SetVector("_Clip",new Vector4(clipRect.x,clipRect.y,clipRect.width,clipRect.height));
				}else{
					setDefaulShader();
				}
			}else{
				setDefaulShader();
				//m_material.SetVector("_Clip",new Vector4(-2000,-2000,4000,4000));//RectUtil.RECT_NO_CLIP.x,RectUtil.RECT_NO_CLIP.y,RectUtil.RECT_NO_CLIP.width,RectUtil.RECT_NO_CLIP.height));
			}
			//m_material.color = blend;
		}/*else{
			if(clipRect.width != RectUtil.RECT_NO_CLIP.width || clipRect.height != RectUtil.RECT_NO_CLIP.height){
				m_material.SetVector("_Clip",new Vector4(clipRect.x,clipRect.y,clipRect.width,clipRect.height));
			}else{
				m_material.SetVector("_Clip",new Vector4(-2000,-2000,4000,4000));//RectUtil.RECT_NO_CLIP.x,RectUtil.RECT_NO_CLIP.y,RectUtil.RECT_NO_CLIP.width,RectUtil.RECT_NO_CLIP.height));
			}
		}*/
	}

	private void setDefaulShader(){
		if(customShader != null){
			if(m_crtShader != SHADER_CUSTOM){
				m_crtShader = SHADER_CUSTOM;
				m_material.shader = customShader;
			}
		}else{
			if(m_crtShader != SHADER_CLIPLESS){
				m_crtShader = SHADER_CLIPLESS;
				m_material.shader = shader_no_clip;
			}
		}
	}

	public virtual void updateObjectMesh (bool forceClipUpdate = false) 
	{
		if(!isReady()) return;
		
		if (meshChanged || m_transform.hasChanged || hasChangedOnLastFrame
		|| 	clipChanged || forceClipUpdate){
			clipMesh(meshChanged);
	    }
		
		bool notifyParent = meshChanged;
		
		if(meshChanged || clipChanged || forceClipUpdate){
			clipChanged = false;
			
			/*if (forceClipUpdate){
				foreach (Transform child in transform){
					kObject kObj = child.GetComponent<kObject>();
					if (kObj != null)
						kObj.updateObjectMesh(true);
				}
			}*/
			kTouchable touch = GetComponent<kTouchable>();
			if(touch != null)
				touch.updateTouchArea();
		}
		
		if(meshChanged || colorsChanged)
		{
			Mesh 	 animMesh 	 = null;
			
			if(!Application.isPlaying)
				animMesh 	 = meshFilter.sharedMesh;
			else
				animMesh 	 = meshFilter.mesh;
	
			if(meshChanged && animMesh != null){
				//Debug.Log("Update real mesh:"+gameObject.name);
				animMesh.Clear();
       			animMesh.vertices 	= objMesh.vertices;
       			animMesh.uv 		= objMesh.UVs;
       			animMesh.triangles 	= objMesh.triangles;
				animMesh.colors 	= objMesh.colors;
				//animMesh.RecalculateNormals();
				//animMesh.RecalculateBounds();
			}else if(colorsChanged && animMesh != null){
				animMesh.colors 	= objMesh.colors;
			}
		}
		meshChanged = false;
		colorsChanged = false;
		
		if(notifyParent){
			sendMeshChangedNotif();//childMeshChanged(this);
		}
	}
	
	public virtual void sendMeshChangedNotif(){
		//if (Application.isPlaying)
			//SendMessageUpwards("childMeshChanged", this, SendMessageOptions.DontRequireReceiver);
	}
	
	public void clearObjectMesh(){
		Mesh 	 animMesh 	 = null;
		
		if(!Application.isPlaying)//todo debug this
			animMesh 	 = meshFilter != null ? meshFilter.sharedMesh : null;
		else
			animMesh 	 = meshFilter != null ? meshFilter.mesh : null;

		if(animMesh != null) animMesh.Clear();
	}
	
	//helper methods
	protected Rect updateCorners(kMesh mesh,Rect corners,int indexStart,int length){
		for(int i= indexStart;i < indexStart + length;i++){
			if(mesh.vertices[i].x < corners.x) corners.x = mesh.vertices[i].x;
			if(mesh.vertices[i].y < corners.y) corners.y = mesh.vertices[i].y;
			if(mesh.vertices[i].x > corners.width) corners.width = mesh.vertices[i].x;
			if(mesh.vertices[i].y > corners.height) corners.height = mesh.vertices[i].y;
		}
		return corners;
	}

	protected override void onDestroy(){
		if(m_material != null){
			if(Application.isEditor)
				DestroyImmediate(m_material);
			else
				Destroy(m_material);
			m_material = null;
		}

		if(sprite != null)
			sprite.dropReference();
		
		base.onDestroy();
	}

#if UNITY_EDITOR
	void OnDrawGizmos(){
		if(debug)
		{
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
	}
#endif
	
	Vector2 tmpPos = new Vector2();
	Vector2 tmpSize= new Vector2();
	public void RectToLocalCoord(ref IntRect r, ref IntRect o){
		Vector3 lossyScale = m_transform.lossyScale;
		Vector3 position = m_transform.position;
		//Quaternion invRotation = Quaternion.Inverse(m_transform.rotation);
		
		float invScaleX = 1/lossyScale.x;
		float invScaleY = 1/lossyScale.y;
		//Vector2 invScale= new Vector2(,1/lossyScale.y);
		tmpPos.x = r.x - position.x; tmpPos.y = r.y - position.y;//Vector2 pos = new Vector2(r.x  - position.x,r.y - position.y);
		tmpPos.x  *= invScaleX;//Vector2.Scale(pos,invScale);
		tmpPos.y  *= invScaleY;
		//pos 	= invRotation * pos;
		
		//Vector2 size = new Vector2(r.width,r.height);
		tmpSize.x = r.width; tmpSize.y = r.height;
		tmpSize.x *= invScaleX;//Vector2.Scale(size,invScale);
		tmpSize.y *= invScaleY;
		//size 	= invRotation * size;*/
		/*tmpPos.x = r.x; tmpPos.y = r.y;
		tmpSize.x = r.width; tmpSize.y = r.height;
		tmpPos = m_transform.InverseTransformPoint(tmpPos);*/
		//tmpSize = m_transform.InverseTransformPoint(tmpSize);
		
		o.x = (int)(tmpSize.x >= 0 ? tmpPos.x : (tmpPos.x + tmpSize.x)); 
		o.y = (int)(tmpSize.y >= 0 ? tmpPos.y : tmpPos.y + tmpSize.y);
		o.width = (int)(tmpSize.x >= 0 ? tmpSize.x : -tmpSize.x);
		o.height= (int)(tmpSize.y >= 0 ? tmpSize.y : -tmpSize.y);
		//o.x = (int)pos.x; o.y = (int)pos.y; o.width = (int)size.x; o.height = (int)size.y;
	}
	
	public void playSound(AudioClip clip, float volume = -1f, bool loop = false)
	{
		m_audioClip = clip;
		m_volume = volume;
		m_loop = loop;
	}
	private AudioClip m_audioClip = null;
	private float m_volume = -1;
	private bool  m_loop = false;

	private void updateSoundPlayback(){
		if(m_audioClip != null)
		{
			AudioClip clip = m_audioClip;
			m_audioClip = null;

			AudioSource audioSource = GetComponent<AudioSource>();
			if ( audioSource == null) {
				audioSource = gameObject.AddComponent<AudioSource>();
			}
			audioSource.spatialBlend = 0;
			if (m_loop) {
				audioSource.loop = true;
				audioSource.playOnAwake = false;
				audioSource.volume = m_volume >= 0 ? m_volume : kScreen.FX_VOLUME;
				audioSource.clip = clip;
				audioSource.Play();
			} else {
				audioSource.PlayOneShot(clip, m_volume >= 0 ? m_volume : kScreen.FX_VOLUME);
			}
		}
	}
	
	public void stopSound()
	{
		m_audioClip = null;
		AudioSource audioSource = GetComponent<AudioSource>();
		if (audioSource != null) {
			audioSource.loop = false;
			audioSource.Stop();
		}
	}
	
	static float lastTime = 0;
	
	public static void LOG_TIME(string label)
	{
		float a = Time.realtimeSinceStartup;
		if(lastTime == 0)
			lastTime = a;
		
		
		double diff =  a - lastTime;
		Debug.Log(label+" : "+a + " - " + lastTime+" = " + diff);
		lastTime = a;
	}
}
