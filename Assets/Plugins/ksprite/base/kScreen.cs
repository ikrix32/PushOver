using UnityEngine;
using System.Collections;
using System.IO;
using System;
#if UNITY_EDITOR
using System.Reflection;
#endif
#if UNITY_IOS
using System.Runtime.InteropServices;
#endif

public enum EOrientationType{
	EOT_PORTRAIT = 0,
	EOT_LANDSCAPE,
	EOT_FREE
}

[ExecuteInEditMode]
public class kScreen : MonoBehaviour
{
	public 	const int 	drawDepth 	= 90;	
	public	const int	zPos		= 1000;
	public	const int	farClipPlane= 1400;
	
	public static float FX_VOLUME = 0.5f;
	
	[HideInInspector]
	public  kCamera m_camera = null;
	
	[HideInInspector]
	public  AudioListener kAudioListener = null;
	
	[HideInInspector]
	public kTouchController kTouchController = null;
	
	
	public Vector2 screenSize;
	
	public bool isInBackground = false;

	private static kScreen m_instance = null;

	public static kScreen instance {
		get {
			return m_instance;
		}
	}
	
	public float m_unloadUnusedResourcesTimer = 0;

	public enum AspectRatio{
		Unknown,
		Aspect3_2,
		Aspect4_3,
		Aspect5_3,
		Aspect16_9,
	}

	void Awake()
	{
		//Delete this if screen object is already loaded

		if(!Application.isPlaying){
			if(instance != null && instance != this){
				DestroyImmediate(instance.gameObject);
				return;
			}
		}else{
			if(instance != null){
				DestroyImmediate(gameObject);
				return;
			}
		}
		if (Application.isPlaying) {
			DontDestroyOnLoad (this);
		}
		Application.targetFrameRate = 30;
		transform.position = new Vector3(0,0,-zPos);
		if(m_camera != null)
			m_camera.camera.farClipPlane  = farClipPlane;
	}
	
	void OnEnable(){} 
	
	void OnApplicationPause (bool pause){
		isInBackground = pause;

		if(!pause)
			kAppManager.enableAllScripts();
	}
	
	void Update () 
	{
#if UNITY_EDITOR
		if(!Application.isPlaying)
		{
			if(gameObject.name.CompareTo("kScreen") != 0)
				gameObject.name = "kScreen";
			
			if(m_camera != null && m_camera.camera != null){
				m_camera.camera.orthographicSize = screenSize.y / 2;
			}		
			checkScreenExistance();	
			if(transform.parent != null)
				transform.parent = null;
		
		}
#endif	
		m_unloadUnusedResourcesTimer += Time.deltaTime;
		//unity won't free memory until loading new level or manually calling UnloadUnusedAssets
		if(!isInBackground && m_unloadUnusedResourcesTimer > 10){
			m_unloadUnusedResourcesTimer = 0;
			Resources.UnloadUnusedAssets();
			System.GC.Collect();
		}
	}
	
#if UNITY_IOS
	[DllImport ("__Internal")]
	private static extern float getiOSMainScreenScale();
#endif
	
	public float getDeviceScaleFactor(){
#if UNITY_EDITOR
		return 1.0f;
#elif UNITY_IOS
		return getiOSMainScreenScale();
#else
		if(Screen.orientation == ScreenOrientation.LandscapeLeft
		|| Screen.orientation == ScreenOrientation.LandscapeRight
		|| Screen.orientation == ScreenOrientation.Landscape)
			return Screen.height * 1.0f/Screen.dpi;
		else
			return Screen.width * 1.0f/Screen.dpi;
#endif
	}
	//hardcode!!!
	public float getKeyboardScaleFactor(){
#if UNITY_EDITOR
		return 1.0f;
#else
		return screenSize.y / Screen.height;//application-device screen height scale factor

/*		AspectRatio aspectRatio = getAspectRatio();
		switch(aspectRatio){
			case AspectRatio.Aspect16_9: return 2.0f;
			case AspectRatio.Aspect3_2: return 2.0f;
		}
		return 1.0f;
*/
#endif
	}
	
	//hardcoded
	public void itweenCallback(){
		kAppManager.transitionComplete();
	}

	private static AspectRatio m_aspectRatio = AspectRatio.Unknown;
	public static AspectRatio getAspectRatio(){
		if(m_aspectRatio == AspectRatio.Unknown){
			decimal screenHeight= 1;
			decimal screenWidth = 1;
#if UNITY_EDITOR
			screenHeight= (decimal)(instance.screenSize.y > instance.screenSize.x ? instance.screenSize.y : instance.screenSize.x);
			screenWidth = (decimal)(instance.screenSize.y < instance.screenSize.x ? instance.screenSize.y : instance.screenSize.x);	
#else
			screenHeight= Screen.height > Screen.width ? Screen.height : Screen.width;
			screenWidth = Screen.height < Screen.width ? Screen.height : Screen.width;
#endif
			decimal	ratio = decimal.Round(screenHeight/screenWidth,2);

			m_aspectRatio = AspectRatio.Aspect3_2;
			if(ratio <= 1.33m){
				m_aspectRatio = AspectRatio.Aspect4_3;
			}else if(ratio <= 1.5m){
				m_aspectRatio = ratio - 1.33m < 1.5m - ratio ? AspectRatio.Aspect4_3 : AspectRatio.Aspect3_2;
			}else if(ratio <= 1.66m){
				m_aspectRatio = ratio - 1.5m< 1.66m - ratio ? AspectRatio.Aspect3_2 : AspectRatio.Aspect5_3 ;
			}else if(ratio <= 1.77m){
				m_aspectRatio = ratio - 1.66m < 1.77m - ratio ? AspectRatio.Aspect5_3  : AspectRatio.Aspect16_9;
			}else
				m_aspectRatio = AspectRatio.Aspect16_9;
		}
		return m_aspectRatio;
	}

	public void setScreenSize(Vector2 size){
		screenSize = size;
		m_camera.camera.orthographicSize = screenSize.y / 2;
	}
#if UNITY_EDITOR
	private EOrientationType m_editorOrientation = EOrientationType.EOT_PORTRAIT;
#endif
	public EOrientationType GetScreenOrientation()
	{
#if UNITY_EDITOR
		return m_editorOrientation;
#else
		switch (Screen.orientation) {
		case ScreenOrientation.Portrait:
		case ScreenOrientation.PortraitUpsideDown:
			return EOrientationType.EOT_PORTRAIT;
		case ScreenOrientation.LandscapeLeft:
		case ScreenOrientation.LandscapeRight:
			return EOrientationType.EOT_LANDSCAPE;
		}
		return EOrientationType.EOT_FREE;
#endif
	}

	public void SetScreenOrientation(EOrientationType type, bool orthoResize = true)
	{
#if UNITY_EDITOR
		m_editorOrientation = type;
#endif
		/*switch ( type )
		{
			case EOrientationType.EOT_PORTRAIT:
				Screen.orientation = ( Input.deviceOrientation == DeviceOrientation.PortraitUpsideDown ) ? ScreenOrientation.PortraitUpsideDown : ScreenOrientation.Portrait;
				break;
			case EOrientationType.EOT_LANDSCAPE:
				Screen.orientation = ( Input.deviceOrientation == DeviceOrientation.LandscapeRight ) ? ScreenOrientation.LandscapeRight : ScreenOrientation.LandscapeLeft;
				
				break;
			case EOrientationType.EOT_FREE:
				//no need to rotate the screen, we can direcly activate the autorotation
				Screen.orientation = ScreenOrientation.AutoRotation;
				break;
			default:
				break;
		}*/
		Screen.autorotateToLandscapeLeft = type != EOrientationType.EOT_PORTRAIT;
		Screen.autorotateToLandscapeRight = type != EOrientationType.EOT_PORTRAIT;;
		Screen.autorotateToPortrait = type != EOrientationType.EOT_LANDSCAPE;
		Screen.autorotateToPortraitUpsideDown = type != EOrientationType.EOT_LANDSCAPE;
		Screen.orientation = type != EOrientationType.EOT_PORTRAIT ? ScreenOrientation.LandscapeLeft : ScreenOrientation.Portrait;

		if ( orthoResize ){
			if ( type == EOrientationType.EOT_PORTRAIT )
				instance.m_camera.camera.orthographicSize = instance.screenSize.y / 2;
			else
				instance.m_camera.camera.orthographicSize = instance.screenSize.x / 2;
		}

		if(type != EOrientationType.EOT_FREE){
			//StartCoroutine(SetOrientationRoutine(type,orthoResize));
		}
		StartCoroutine(EnableAutoRotation());
	}
	private IEnumerator EnableAutoRotation()
	{
		yield return new WaitForSeconds(0.5f);
		Screen.orientation = ScreenOrientation.AutoRotation;
	}
	/*
	private IEnumerator SetOrientationRoutine(EOrientationType type, bool orthoResize)
	{
		Debug.Log ("Device orientation portrait:" + type + " " + Input.deviceOrientation);
		if ( orthoResize ){
			if ( type == EOrientationType.EOT_PORTRAIT )
				kScreen.instance.kCamera.orthographicSize = kScreen.instance.screenSize.y / 2;
			else
				kScreen.instance.kCamera.orthographicSize = kScreen.instance.screenSize.x / 2;
		}

		#if UNITY_EDITOR
		#elif UNITY_IOS
		//set the rotation for iOS as well, unity please fix :) 
		//setiOSOrientation(!inPortrait);
		#endif
		
		//wait one frame before enabling autorotate to allow unity to force desired orientation 
		yield return new WaitForSeconds (5);

		Debug.Log("Enable auto rotation!!!");
		Screen.autorotateToLandscapeLeft = type != EOrientationType.EOT_PORTRAIT;
		Screen.autorotateToLandscapeRight = type != EOrientationType.EOT_PORTRAIT;;
		Screen.autorotateToPortrait = type != EOrientationType.EOT_LANDSCAPE;
		Screen.autorotateToPortraitUpsideDown = type != EOrientationType.EOT_LANDSCAPE;
		Screen.orientation = ScreenOrientation.AutoRotation;

	}*/
	
	public Texture m_bluredBackgroundTexture;
	public Vector2 m_scale = Vector2.one;
	public Vector2 m_offset = Vector2.zero;

	public void SetBlurBackgroundProperties(Vector2 scale,Vector2 offset){
		m_scale = scale;
		m_offset = offset;
	}

	public void SetBlurBackgroundTexture(Texture tex){
		m_bluredBackgroundTexture = tex;
		SetBlurBackgroundProperties( Vector2.one, Vector2.zero);
	}

	public Texture2D TakeScreenShot()
	{
		Texture2D tex = new Texture2D(Screen.width, Screen.height);
		tex.ReadPixels(new Rect(0,0,Screen.width,Screen.height),0,0);
		tex.Apply();
		
		return tex;
	}

	public Texture2D TakeBlurredScreenShot()
	{
		RenderTexture currentRT = m_camera.camera.targetTexture;

		RenderTexture	renderTexture = RenderTexture.GetTemporary (Screen.width, Screen.height);
		renderTexture.filterMode = FilterMode.Bilinear;
		m_camera.camera.targetTexture = renderTexture;
		m_camera.camera.Render();
		
		m_camera.camera.targetTexture = currentRT;
		Texture2D tex = (Texture2D)TextureProcessor.ApplyBlur(renderTexture,1);

		RenderTexture.ReleaseTemporary (renderTexture);
		
		return tex;
	}

	public Texture2D TakeBlurredScreenShot(int width,int height,float cameraOrthographicSize)
	{
		RenderTexture currentRT = m_camera.camera.targetTexture;

		RenderTexture	renderTexture = RenderTexture.GetTemporary (width,height);
		renderTexture.filterMode = FilterMode.Bilinear;

		float crtOrthoSize = m_camera.camera.orthographicSize;
		m_camera.camera.orthographicSize = cameraOrthographicSize;
		m_camera.camera.targetTexture = renderTexture;

		m_camera.camera.Render();

		m_camera.camera.orthographicSize = crtOrthoSize;
		m_camera.camera.targetTexture = currentRT;

		Texture2D tex = (Texture2D)TextureProcessor.ApplyBlur(renderTexture,1);

		RenderTexture.ReleaseTemporary (renderTexture);

		return tex;
	}

	public static void checkScreenExistance()
	{
		if(instance == null){
			GameObject screenHolder;
			if((screenHolder = GameObject.Find("kScreen")) == null){
				screenHolder = new GameObject();
				screenHolder.transform.position = new Vector3(0,0,-zPos);
				screenHolder.name = "kScreen";
			}
			if((m_instance = screenHolder.GetComponent<kScreen>()) == null){
				m_instance = screenHolder.AddComponent<kScreen>();
				//kScreenInstance.transform.parent = gameObject.transform;
				//set default screen size
				instance.screenSize = new Vector2(640,1136);
			}
		}
		
		if(instance.m_camera == null && (instance.m_camera = instance.GetComponent<kCamera>()) == null)
		{	//Create the camera
			Camera camera = instance.GetComponent<Camera>();
			if(camera == null)
				camera = instance.gameObject.AddComponent<Camera>();
			instance.m_camera = instance.gameObject.AddComponent<kCamera>();
			instance.m_camera.camera = camera;
			camera.clearFlags = CameraClearFlags.Depth;
			camera.nearClipPlane = 0.3f;
			camera.farClipPlane  = farClipPlane;
			camera.depth 		  = drawDepth;
			camera.rect = new Rect( 0.0f, 0.0f, 2.0f, 2.0f );
			camera.orthographic = true;
			camera.orthographicSize = instance.screenSize.y / 2;
			camera.transform.position = Vector3.back * zPos;
			
			camera.cullingMask = 1 << kSpriteAsset.kSpriteLayer;
	
			GameObject cameraHolder = GameObject.FindWithTag("MainCamera");
			if(cameraHolder != null){
				if(cameraHolder.GetComponent("Camera") != null){
					Camera mainCamera = (Camera)cameraHolder.GetComponent("Camera");
					uint layerMask = 0xFFFFFFFF;
					mainCamera.cullingMask &= (int)(layerMask ^ (1<< kSpriteAsset.kSpriteLayer));
				}
			}
		}
		
		if (instance.kAudioListener == null && (instance.kAudioListener = instance.GetComponent<AudioListener>()) == null)
		{
			instance.kAudioListener = instance.gameObject.AddComponent<AudioListener>();
		}
		
		if(instance.kTouchController == null 
		&&(instance.kTouchController = instance.GetComponent<kTouchController>()) == null){
			instance.kTouchController = instance.gameObject.AddComponent<kTouchController>();
			instance.kTouchController.m_camera = instance.m_camera;
		}
#if UNITY_EDITOR
		if(GetTypeForName("kScene") != null && GetTypeForName("kAppManager") != null)
		{//check if there is any kscene object in scene
			kScene.checkSceneExistence();
		}
#endif
	}
#if UNITY_EDITOR
	void OnRenderObject(){
		if(!Application.isPlaying && m_camera != null && m_camera.camera != null && screenSize != Vector2.zero){
			m_camera.camera.orthographicSize = screenSize.y / 2;
			m_camera.camera.farClipPlane  = farClipPlane;
			m_camera.camera.transform.position = Vector3.back * zPos;
		}
	}
#if UNITY_EDITOR
	void OnDrawGizmos(){
		if(m_camera != null && m_camera.camera != null && screenSize != Vector2.zero){
			Gizmos.DrawWireCube(m_camera.transform.position,new Vector3(screenSize.x,screenSize.y,m_camera.camera.orthographicSize));
			Gizmos.DrawWireSphere(transform.position,1);
		}
	}
#endif
	/*protected void OnDestroy(){
		if(kCamera != null)
			DestroyImmediate(kCamera);
		kCamera = null;
	}*/
	
	/** C# equivalent for Java's classForName,use reflection to access a script file */
	public static Type GetTypeForName( string TypeName )
	{
	 
	    // Try Type.GetType() first. This will work with types defined
	    // by the Mono runtime, in the same assembly as the caller, etc.
	    var type = Type.GetType( TypeName );
	 
	    // If it worked, then we're done here
	    if( type != null )
	       return type;
	 	
		//TODO - check this on iphone
		/*var assemblyC = Assembly.LoadWithPartialName("Assembly-CSharp");
		if( assemblyC != null )// Ask that assembly to return the proper Type
			return assemblyC.GetType( TypeName );*/

		
	    // If the TypeName is a full name, then we can try loading the defining assembly directly
	    if( TypeName.Contains( "." ) )
	    {
	 
	       // Get the name of the assembly (Assumption is that we are using 
	       // fully-qualified type names)
	       var assemblyName = TypeName.Substring( 0, TypeName.IndexOf( '.' ) );
	 
	       // Attempt to load the indicated Assembly
	       var assembly = Assembly.Load( assemblyName );
	       if( assembly == null )
	         return null;
	 
	       // Ask that assembly to return the proper Type
	       type = assembly.GetType( TypeName );
	       if( type != null )
	         return type;
	 
	    }
	 
	    // If we still haven't found the proper type, we can enumerate all of the 
	    // loaded assemblies and see if any of them define the type
	    var currentAssembly = Assembly.GetExecutingAssembly();
	    var referencedAssemblies = currentAssembly.GetReferencedAssemblies();
	    foreach( var assemblyName in referencedAssemblies )
	    {
	 
	       // Load the referenced assembly
	       var assembly = Assembly.Load( assemblyName );
	       if( assembly != null )
	       {
	         // See if that assembly defines the named type
	         type = assembly.GetType( TypeName );
	         if( type != null )
	          return type;
	       }
	    }
		
		// The type just couldn't be found...
	    return null;
	}
#endif
}
