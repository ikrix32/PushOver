using UnityEngine;
using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteInEditMode]
public class kScene : kBehaviourScript 
{
	[SerializeField][HideInInspector]
	protected List<kViewController> viewControllers = new List<kViewController>();
	public bool 	defaultHidden = false;
	public bool 	dontDestroyOnLoad;
	public kView	activeView;
	public Camera 	mainCamera;
	
	//Root object for all sprites/fonts and other resource types
	[SerializeField][HideInInspector]
	private GameObject rootSprites = null;
	[SerializeField][HideInInspector]
	private GameObject rootFonts = null;
	
	private int referenceCount = 0;
	
	public kViewController  getControllerForView(string viewName){
		for(int i = 0; i < viewControllers.Count;i++){
			if((viewControllers[i] != null) && (viewControllers[i].getView(viewName) != null))
				return viewControllers[i];
		}
		return null;
	}
	
	public void disableAllViews(){
		for(int i = 0; i < viewControllers.Count;i++){
			if(viewControllers[i] != null)
				viewControllers[i].disableAllViews();
		}
	}
	
	public void destroyMainCamera(){
		if(mainCamera != null)
			DestroyImmediate(mainCamera.gameObject);
	}
	
	public void unload(){
		Destroy(gameObject);
		Resources.UnloadUnusedAssets();
	}
	
	void Awake(){
		if(dontDestroyOnLoad){
			if(Application.isPlaying){ // unity 5.3 fix
				DontDestroyOnLoad(gameObject);
			}
		}
		if(rootSprites != null && rootSprites.transform != null)
			initObjectSubTree(rootSprites.transform);
		if(rootFonts != null && rootFonts.transform != null)
			initObjectSubTree(rootFonts.transform);
		initObjectSubTree(transform);
		//if(mainCamera != null)
		//	mainCamera.gameObject.SetActive(false);
		kScreen.checkScreenExistance();
	}
	
	void Start(){
		if(rootSprites != null && rootSprites.transform != null)
			startObjectSubTree(rootSprites.transform);
		if(rootFonts != null && rootFonts.transform != null)
			startObjectSubTree(rootFonts.transform);
		startObjectSubTree(transform);
	}
	
	
	// Use this for initialization
	protected override void onStart () {
		base.onStart();
		
		if(!Application.isPlaying) return;
		
		disableAllViews();
		kAppManager.sceneLoaded(this);
	}
	
	// Update is called once per frame
	protected override void onUpdate () {
		base.onUpdate();
#if UNITY_EDITOR
		if(!Application.isPlaying){
			checkUnitySceneName();
			updateRoots();
			updateObjectHierarchy();
			updateBehaviourScripts();
		}
#endif
	}
	
	public void grabReference(){
		referenceCount ++;
	}
	public void dropReference(){
		referenceCount--;
	}
	
	public int getReferenceCont(){
		return referenceCount;
	}
	
#if UNITY_EDITOR	
	private void updateRoots(){
		foreach (GameObject obj in UnityEngine.Object.FindObjectsOfType(typeof(GameObject))) {
			Camera cam = obj.GetComponent<Camera>();
			kScreen screen = obj.GetComponent<kScreen>();
			if (obj.transform.parent == null && screen == null && cam == null && obj.name != "AWSPrefab") {
				obj.transform.parent = transform;
			}
			if (cam != null && (cam.name == "Main Camera" || screen != null) && obj.transform.parent != null) {
				obj.transform.parent = null;
			}
		}
	}
	
	private void updateObjectHierarchy(){
		viewControllers.Clear();
		foreach (kViewController vController in UnityEngine.Object.FindObjectsOfType(typeof(kViewController))){
			viewControllers.Add(vController);
		}
		
		if(rootFonts == null){
			rootFonts = new GameObject();
			rootFonts.transform.parent = transform.parent;
			rootFonts.name = "xFonts";
		}
			
		foreach (kFont font in UnityEngine.Object.FindObjectsOfType(typeof(kFont)))
			if(font.transform.parent != rootFonts.transform)
				font.transform.parent = rootFonts.transform;
	}
	
	public void checkUnitySceneName(){
		string[] path = EditorApplication.currentScene.Split('/');
        string sceneName = path[path.Length - 1];// Application.loadedLevelName;
		sceneName = sceneName.Replace(".unity","");
			
		if(gameObject.name.CompareTo(sceneName) != 0)
			gameObject.name = sceneName;
	}
	
	public void updateBehaviourScripts(){
		foreach (kBehaviourScript script in UnityEngine.Object.FindObjectsOfType(typeof(kBehaviourScript))){//Resources.FindObjectsOfTypeAll(typeof(kBehaviourScript))){
			script.shouldAutoInitialize = false;
		}
	}
	
	public static void checkSceneExistence(){
		var scenes = UnityEngine.Object.FindObjectsOfType(typeof(kScene));
		
		if(scenes == null || scenes.Length == 0)
		{
			GameObject sceneObj = new GameObject();
			sceneObj.AddComponent<kScene>();
		}
	}
#endif
}
