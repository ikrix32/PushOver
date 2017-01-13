using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public abstract class kAppManager{
	
	public const int TOP_VIEW_OFFSET = 60;
	//protected static List<kScene> loadedScenes = new List<kScene>();
	protected static Hashtable loadedScenes = new Hashtable();
	protected static Hashtable loadCallback = new Hashtable();
	
	protected static List<kViewController> 	controllerStack = new List<kViewController>();
	protected static List<kView> 			viewStack = new List<kView>();
	
	public static System.Action<string> showViewCallback = null;

	public const float UP_DOWN_TRANSITION_DURATION = 0.75f;
	public const float LEF_RIGTH_TRANSITION_DURATION = 0.5f;
	public const float FADE_TRANSITION_DURATION = 1f;

	
	public enum ViewTransitionType{
		NONE = 0,
		FLIP,
		FADE,
		SLIDE_LEFT,
		SLIDE_RIGHT,
		SLIDE_UP,
		SLIDE_DOWN,
		SLIDE_OVER_LEFT,
		SLIDE_OVER_RIGHT,
		SLIDE_OVER_UP,
		SLIDE_OVER_DOWN,
	};
	
	//private static System.Action sceneLoadCallback = null;
	
	public static void loadScene(string sceneName,System.Action onLoadComplete = null){
		//sceneLoadCallback = onLoadComplete;
		sceneName = getSceneNameForCurrAspectRatio(sceneName);
		if(loadCallback.ContainsKey(sceneName)){//avoid loading twice same scene
			if(onLoadComplete != null){
				System.Action tmp = onLoadComplete;
				if(loadCallback[sceneName] != null)
					tmp += (System.Action)loadCallback[sceneName];
				loadCallback[sceneName] = tmp;
			}
		}else{
			loadCallback.Add(sceneName,onLoadComplete);
		}
	
		//loadedScenes.Clear();
		Hashtable scenes = new Hashtable();
		foreach(kScene scene in loadedScenes.Values){
			if(scene != null && scene.dontDestroyOnLoad)
				scenes.Add(scene.name,scene);
		}
		loadedScenes = scenes;
		controllerStack.Clear();
		viewStack.Clear();
#if DEBUG_APP_MANAGER
		Debug.Log("Load scene:"+sceneName);
#endif
		Application.LoadLevel(sceneName);
	}
	
	/** Returns true if scene was found and load started */
	public static void loadSceneAdditive(string sceneName,System.Action onLoadComplete = null){
		sceneName = getSceneNameForCurrAspectRatio(sceneName);
#if DEBUG_APP_MANAGER
		Debug.Log("Load scene additive:"+sceneName+" already loading :"+(loadCallback[sceneName] != null));
#endif
		if(getScene(sceneName) != null){
			getScene(sceneName).grabReference();
			if(onLoadComplete != null)	onLoadComplete();
			return;
		}
		if(loadCallback.ContainsKey(sceneName)){//avoid loading twice same scene
			if(onLoadComplete != null){
				System.Action tmp = onLoadComplete;
				if(loadCallback[sceneName] != null)
					tmp += (System.Action)loadCallback[sceneName];
				loadCallback[sceneName] = tmp;
			}
			return;
		}
		loadCallback.Add(sceneName,onLoadComplete);
		//sceneLoadCallback = onLoadComplete;
		Application.LoadLevelAdditiveAsync(sceneName);
	}
	
	public static kScene getScene(string sceneName){
		sceneName = getSceneNameForCurrAspectRatio(sceneName);
		if(loadedScenes.ContainsKey(sceneName)){
			return (kScene)loadedScenes[sceneName];	
		}
		return null;
	}
	
	public static void unloadScene(string sceneName){
		sceneName = getSceneNameForCurrAspectRatio(sceneName);
		kScene scene = getScene(sceneName);
		if(scene != null){
			scene.dropReference();
			if(scene.getReferenceCont() <= 0){
				loadedScenes.Remove(sceneName);
				scene.unload();
			}
		}
	}
	
	private enum TransType{
		SHOW = 0,
		PUSH,
		POP
	}
	
	public static bool containsView(kView view){
		return viewStack.Contains(view);
	}
	
	public static kView getCurrentView(){
		if(viewStack.Count == 0) return null;
		return viewStack[viewStack.Count - 1];
	}

	public static int getViewCount(){
		return viewStack.Count;
	}
	
	public static kView getPreviousView(){
		if(viewStack.Count < 2) return null;
		return viewStack[viewStack.Count - 2];
	}
	
	public static kViewController getCurrentViewController(){
		if(controllerStack.Count == 0) return null;
		return controllerStack[controllerStack.Count - 1];
	}
	
	public static void pushView(string viewName,ViewTransitionType transition,System.Action onComplete = null,bool hideOldView = true){
		showView(viewName,transition,onComplete,hideOldView,true);
	}
	
	public static void showView(string viewName,ViewTransitionType transition,System.Action onComplete = null,bool hideOldView = true,bool pushView = false){
		if (viewInTransition != null) {
#if DEBUG_APP_MANAGER
			Debug.LogError("showView: " + viewName + " ignored! viewInTransition: " + viewInTransition.name);
#endif
			return;
		}
		kViewController vController = null;
		kView view = null;
		
		hidePrevViewAfterTransition = hideOldView;
		
		foreach(kScene scene in loadedScenes.Values){
			vController = scene.getControllerForView(viewName);
			if(vController != null) break;
		}
		
		view = vController != null ? vController.getView(viewName) : null;
		
		if(view != null){
			viewAnimType =  transition;
			transitionType = pushView ? TransType.PUSH : TransType.SHOW;
			transitionCallback = onComplete;
			
			if(viewStack.Count > 0)
				controllerStack[controllerStack.Count - 1 ].viewStartTransition(viewStack[viewStack.Count - 1], viewAnimType, false);
			
			startViewTransition(vController,view,transition);
		}
#if DEBUG_APP_MANAGER
		else
			Debug.LogError("Can't find view: " + viewName);
#endif

	}
	
	public static void popCurentView(ViewTransitionType viewTransition,System.Action onComplete= null){
		if(viewInTransition != null){
			iTween.Stop();
			transitionComplete();
		}
		if(viewStack.Count > 1){
			viewAnimType = viewTransition;
			transitionType = TransType.POP;
			transitionCallback = onComplete;
			controllerStack[controllerStack.Count - 1 ].viewStartTransition(viewStack[viewStack.Count - 1], viewAnimType, false);
			//controllerStack[controllerStack.Count - 2 ].viewWillShow(viewStack[viewStack.Count -2]);
			startViewTransition(controllerStack[controllerStack.Count - 2 ],viewStack[viewStack.Count - 2],viewTransition);
		}
#if DEBUG_APP_MANAGER
		else
			Debug.LogError("Error popView");
#endif
	}

	public static void popPrevView(){
		if(viewStack.Count > 1){
			kView prevView = viewStack [viewStack.Count - 2];
			if(prevView.IsFaded()){
				prevView.setIsFaded(false);
				iTween.FadeUpdate(prevView.gameObject, iTween.Hash("alpha", 1f, "time", 0f, "includechildren", true));
			}

			prevView.gameObject.SetActive(false);
			controllerStack.RemoveAt(viewStack.Count - 2);
			viewStack.RemoveAt(viewStack.Count - 2);
		}
#if DEBUG_APP_MANAGER
		else
			Debug.LogError("Error popPrevView");
#endif
	}

	public static void popView(string viewName){
		int crtView = viewStack.Count - 1;
		while(crtView >= 0){
			if(viewStack[crtView] != null && viewStack[crtView].name == viewName){
				viewStack[crtView].gameObject.SetActive(false);
				controllerStack.RemoveAt(crtView);
				viewStack.RemoveAt(crtView);
				return;
			}
			crtView--;
		}
#if DEBUG_APP_MANAGER
		Debug.LogError("popView: No view found for view name:"+viewName);
#endif
	}

	public static bool isInTransition(){
		return loadCallback.Count > 0 || viewInTransition != null;
	}
	
	public static void transitionComplete(){
		if(controllerInTransition != null && viewInTransition != null)
		{
			if(viewStack.Count > 0 )
			{
				bool overlapping = (viewAnimType == ViewTransitionType.SLIDE_OVER_LEFT || viewAnimType == ViewTransitionType.SLIDE_OVER_RIGHT
								|| viewAnimType == ViewTransitionType.SLIDE_OVER_UP || viewAnimType == ViewTransitionType.SLIDE_OVER_DOWN
								|| viewAnimType == ViewTransitionType.FADE);
				
				if(hidePrevViewAfterTransition || transitionType != TransType.PUSH)// || !overlapping)
				{
					viewStack[viewStack.Count - 1].gameObject.SetActive(false);
					controllerStack[controllerStack.Count - 1].viewEndTransition(viewStack[viewStack.Count - 1],viewAnimType, false);
				}
				
				if(transitionType == TransType.POP || transitionType == TransType.SHOW )
				{
					controllerStack.RemoveAt(viewStack.Count - 1);
					viewStack.RemoveAt(viewStack.Count - 1);
				}
			}
			if (transitionType == TransType.PUSH || transitionType == TransType.SHOW){
				int viewIndex = -1;
				for (int i = 0; i < viewStack.Count && viewIndex < 0; ++i)
					if (viewStack[i] == viewInTransition)
						viewIndex = i;
				if (viewIndex >= 0)
				{
					while (viewStack.Count > viewIndex)
					{
						controllerStack.RemoveAt(viewStack.Count - 1);
						viewStack.RemoveAt(viewStack.Count - 1);
					}
				}
				controllerStack.Add(controllerInTransition);
				viewStack.Add(viewInTransition);
				controllerInTransition.viewEndTransition(viewInTransition, viewAnimType ,true);
			}else{
				controllerInTransition.viewEndTransition(viewInTransition, viewAnimType ,true);;
			}
			if(transitionCallback != null) transitionCallback();

			/*foreach(kScene scene in loadedScenes.Values){
				if(scene.getControllerForView(viewInTransition.name) == null)
					scene.gameObject.SetActive(false);
			}*/

			viewInTransition = null;
			controllerInTransition=null;
		}
	}
	
	private static kViewController 		controllerInTransition;
	private static kView 				viewInTransition;
	private static ViewTransitionType	viewAnimType;
	private static System.Action 	transitionCallback;
	private static TransType 		transitionType;
	private static bool 			hidePrevViewAfterTransition;
	
	
	private static void startViewTransition(kViewController controller,kView view,ViewTransitionType viewTransition)
	{
		/*foreach(kScene scene in loadedScenes.Values){
			if(scene.getControllerForView(view.name) != null)
			{
				scene.gameObject.SetActive(true);
				break;
			}
		}*/
		if (showViewCallback != null) {
			showViewCallback(view.name);
		}
		view.gameObject.SetActive(true);
		controller.viewStartTransition(view, viewTransition,true);
			
		controllerInTransition = controller;
		viewInTransition 	= view;
		
		kScreen screen = kScreen.instance;
		Vector3 screenPos = screen.transform.position;
		Vector2 screenSize= screen.screenSize;
		
		Vector3 screenTopLeft  = new Vector3(screenPos.x - screenSize.x/2,screenPos.y + screenSize.y/2,0);
		Vector3 screenTopRight = new Vector3(screenPos.x + screenSize.x/2,screenPos.y + screenSize.y/2,0);
		Vector3 screenBottomLeft  = new Vector3(screenPos.x - screenSize.x/2,screenPos.y - screenSize.y/2,0);
		//Vector3 screenBottomRight = new Vector3(screenPos.x + screenSize.x/2,screenPos.y - screenSize.y/2,0);
		
		bool overlapping	= (viewTransition == ViewTransitionType.SLIDE_OVER_LEFT || viewTransition == ViewTransitionType.SLIDE_OVER_RIGHT
							|| viewTransition == ViewTransitionType.SLIDE_OVER_UP || viewTransition == ViewTransitionType.SLIDE_OVER_DOWN 
							|| viewTransition == ViewTransitionType.FADE);
			
		Vector3 zOffset = Vector3.zero;
		
		if(viewStack.Count > 0 && (overlapping || viewTransition == ViewTransitionType.NONE))
			zOffset = new Vector3(0,0,viewStack[viewStack.Count - 1].transform.position.z - TOP_VIEW_OFFSET);
		else if(viewStack.Count > 0)
			zOffset = new Vector3(0,0,viewStack[viewStack.Count - 1].transform.position.z);
			
		Vector3 newViewPos = Vector3.zero;
		Vector3 newViewTargetOffset = Vector3.zero;
		Vector3 oldViewTargetOffset = Vector3.zero;
		
		switch(viewTransition){
			case ViewTransitionType.NONE:{
				newViewPos = screenTopLeft + zOffset;
			}break;

			case ViewTransitionType.FADE:{
				if(transitionType != TransType.POP){
					newViewPos = screenTopLeft + zOffset;
				}else{
					newViewPos = screenTopLeft + Vector3.forward * view.transform.position.z;
				}
			}break;
			
			case ViewTransitionType.FLIP:
			{
				/*vc.transform.position = viewController.transform.position;
				vc.transform.RotateAround(Vector3.zero,Vector3.up,360);
				viewController.transform.RotateAround(Vector3.zero,Vector3.up,180);
				viewController.gameObject.SetActive(false);
				vc.gameObject.SetActive(true);
				//iTween.RotateTo(vc.gameObject,new Vector3(0,-180,0), 10);
				//iTween.RotateTo(viewController.gameObject,new Vector3(0,180,0), 10);*/
			}break;
			
			case ViewTransitionType.SLIDE_OVER_LEFT:
			case ViewTransitionType.SLIDE_LEFT:
			{
				if(transitionType != TransType.POP){
					newViewPos = screenTopRight + zOffset;
					newViewTargetOffset = Vector3.left * view.viewSize.x;
					oldViewTargetOffset = overlapping? Vector3.zero : Vector3.left * view.viewSize.x;
				}else{
					newViewPos = overlapping? view.transform.position : screenTopRight + Vector3.forward * view.transform.position.z;
					newViewTargetOffset = overlapping? Vector3.zero : Vector3.left * view.viewSize.x;
					oldViewTargetOffset = Vector3.left * view.viewSize.x;
				}
			}break;
			
			case ViewTransitionType.SLIDE_OVER_RIGHT:
			case ViewTransitionType.SLIDE_RIGHT:
			{
				if(transitionType != TransType.POP){
					newViewPos = screenTopLeft + Vector3.left * view.viewSize.x + zOffset;
					newViewTargetOffset = Vector3.right * view.viewSize.x;
					oldViewTargetOffset = overlapping ? Vector3.zero : Vector3.right * view.viewSize.x;
				}else{
					newViewPos = overlapping ? view.transform.position : screenTopLeft + Vector3.left * view.viewSize.x + Vector3.forward * view.transform.position.z;
					newViewTargetOffset = overlapping? Vector3.zero : Vector3.right * view.viewSize.x;
					oldViewTargetOffset = Vector3.right * view.viewSize.x;
				}
			}break;
			
			case ViewTransitionType.SLIDE_OVER_UP:
			case ViewTransitionType.SLIDE_UP:
			{
				if(transitionType != TransType.POP){
					newViewPos = screenBottomLeft + zOffset;
					newViewTargetOffset = Vector3.up * view.viewSize.y;
					oldViewTargetOffset = overlapping ? Vector3.zero : Vector3.up * view.viewSize.y;
				}else{
					newViewPos = overlapping ? view.transform.position :  screenBottomLeft + Vector3.forward * view.transform.position.z;
					newViewTargetOffset = overlapping ? Vector3.zero : Vector3.up * view.viewSize.y;
					oldViewTargetOffset = Vector3.up * view.viewSize.y;
				}
				if (view.IsFaded()) {
					view.setIsFaded(false);
					iTween.FadeTo(view.gameObject, iTween.Hash("alpha", 1f, "time", getTransitionDuration(ViewTransitionType.FADE), "includechildren", true, "easeType", iTween.EaseType.linear));
				}
			}break;
			
			case ViewTransitionType.SLIDE_OVER_DOWN:
			case ViewTransitionType.SLIDE_DOWN:
			{
				if(transitionType != TransType.POP){
					newViewPos = screenTopLeft + Vector3.up * view.viewSize.y + zOffset;
					newViewTargetOffset = Vector3.down * view.viewSize.y;
					oldViewTargetOffset = overlapping ? Vector3.zero : Vector3.down * view.viewSize.y;
				}else{
					newViewPos = overlapping? view.transform.position : screenTopLeft + Vector3.up * view.viewSize.y + Vector3.forward * view.transform.position.z;
					newViewTargetOffset = overlapping? Vector3.zero : Vector3.down * view.viewSize.y;
					oldViewTargetOffset = Vector3.down * view.viewSize.y;
				}
				if (view.IsFaded()) {
					view.setIsFaded(false);
					iTween.FadeTo(view.gameObject, iTween.Hash("alpha", 1f, "time", getTransitionDuration(ViewTransitionType.FADE), "includechildren", true, "easeType", iTween.EaseType.linear));
				}
			}break;
		}

		if(viewTransition == ViewTransitionType.NONE){
			view.transform.position = newViewPos;
			transitionComplete();
		}else{
			if(transitionType != TransType.POP){
				view.transform.position = newViewPos;

				if(viewStack.Count > 0){
					if(viewTransition != ViewTransitionType.FADE){
						if(oldViewTargetOffset != Vector3.zero){
							iTween.MoveTo(viewStack[viewStack.Count - 1].gameObject, iTween.Hash("position",viewStack[viewStack.Count - 1].transform.position + oldViewTargetOffset,
								"time", getTransitionDuration(viewTransition), "easeType", iTween.EaseType.easeOutCubic));
						}else if(overlapping){
							if(hidePrevViewAfterTransition){
								viewStack[viewStack.Count - 1].setIsFaded(true);
								iTween.FadeTo(	viewStack[viewStack.Count - 1].gameObject, iTween.Hash("alpha", 0f, "time", getTransitionDuration(ViewTransitionType.FADE), "includechildren", true, "easeType", iTween.EaseType.easeInQuint));
							}
						}
					}else{
						if(hidePrevViewAfterTransition){
							viewStack[viewStack.Count - 1].setIsFaded(true);
							iTween.FadeTo(	viewStack[viewStack.Count - 1].gameObject, iTween.Hash("alpha", 0f, "time", getTransitionDuration(ViewTransitionType.FADE), "includechildren", true, "easeType", iTween.EaseType.easeInQuint));
						}
					}
				}
				if(viewTransition != ViewTransitionType.FADE){
					if(newViewTargetOffset != Vector3.zero){
						iTween.MoveTo(	view.gameObject, iTween.Hash("position", view.transform.position + newViewTargetOffset,
										"time", getTransitionDuration(viewTransition), "easeType", iTween.EaseType.easeOutCubic,
										"oncomplete", "itweenCallback", "oncompletetarget", screen.gameObject));
					}
				}else{
					//if(view.IsFaded()){
					view.setIsFaded(false);
					iTween.FadeUpdate(view.gameObject, iTween.Hash("alpha", 1f, "time", 0f, "includechildren", true));
					iTween.FadeFrom( view.gameObject, iTween.Hash("alpha", 0f, "time", 1.5 * getTransitionDuration(ViewTransitionType.FADE), "includechildren", true, "easeType", iTween.EaseType.easeOutCubic,
				 					"oncomplete", "itweenCallback", "oncompletetarget", screen.gameObject));
				}
			}else{
				view.transform.position = newViewPos;
				if(viewStack.Count > 0){
					if(viewTransition != ViewTransitionType.FADE){
						if(oldViewTargetOffset != Vector3.zero ){
							iTween.MoveTo(	viewStack[viewStack.Count - 1].gameObject, iTween.Hash("position",viewStack[viewStack.Count - 1].transform.position + oldViewTargetOffset,
								"time", getTransitionDuration(viewTransition), "easeType", iTween.EaseType.easeInOutCubic,"oncomplete", "itweenCallback", "oncompletetarget", screen.gameObject));
						}
					}else{
						viewStack[viewStack.Count - 1].setIsFaded(true);
						iTween.FadeTo(	viewStack[viewStack.Count - 1].gameObject, iTween.Hash("alpha", 0f, "time", getTransitionDuration(ViewTransitionType.FADE), "includechildren", true, "easeType", iTween.EaseType.linear,
										"oncomplete", "itweenCallback", "oncompletetarget", screen.gameObject));
					}
				}
				
				if(viewTransition != ViewTransitionType.FADE){
					if(newViewTargetOffset != Vector3.zero)
						iTween.MoveTo(	view.gameObject, iTween.Hash("position", view.transform.position + newViewTargetOffset,
										"time", getTransitionDuration(viewTransition), "easeType", iTween.EaseType.easeInOutCubic));
					if(view.IsFaded()){
						view.setIsFaded(false);
						iTween.FadeUpdate(view.gameObject, iTween.Hash("alpha", 1f, "time", 0f, "includechildren", true));
						iTween.FadeFrom(view.gameObject, iTween.Hash("alpha", 0f, "time", getTransitionDuration(ViewTransitionType.FADE), "includechildren", true, "easeType", iTween.EaseType.linear));
					}
				}else{
					if(view.IsFaded()){
						view.setIsFaded(false);
						iTween.FadeUpdate(view.gameObject, iTween.Hash("alpha", 1f, "time", 0f, "includechildren", true));
						iTween.FadeFrom(view.gameObject, iTween.Hash("alpha", 0f, "time", getTransitionDuration(ViewTransitionType.FADE), "includechildren", true, "easeType", iTween.EaseType.linear));
					}
				}
			}
		}
	}

	private static float getTransitionDuration(ViewTransitionType type){
		switch(type){
			case ViewTransitionType.FADE: 
				return FADE_TRANSITION_DURATION;

			case ViewTransitionType.SLIDE_OVER_LEFT:
			case ViewTransitionType.SLIDE_LEFT:
			case ViewTransitionType.SLIDE_OVER_RIGHT:
			case ViewTransitionType.SLIDE_RIGHT:
				return LEF_RIGTH_TRANSITION_DURATION;

			case ViewTransitionType.SLIDE_OVER_UP:
			case ViewTransitionType.SLIDE_UP:
			case ViewTransitionType.SLIDE_OVER_DOWN:
			case ViewTransitionType.SLIDE_DOWN:
				return UP_DOWN_TRANSITION_DURATION;
		}
		return 0;
	}
	
	//This will be called from scene when a new scene is loaded
	public static void sceneLoaded(kScene scene)
	{
#if DEBUG_APP_MANAGER
		Debug.Log("New scene loaded: "+scene.name+" stack count:"+viewStack.Count);
#endif
		if(loadedScenes.Count > 0){
			scene.destroyMainCamera();
		}
		//if(scene.defaultHidden)
		//	scene.gameObject.SetActive(false);

		if(!loadedScenes.Contains(scene.name))
			loadedScenes.Add(scene.name,scene);
		else
			loadedScenes[scene.name] = scene;
		scene.grabReference();
		
		//if(sceneLoadCallback != null) 
		//	sceneLoadCallback();
		//sceneLoadCallback = null;

		if(viewStack.Count == 0){
			if(scene.activeView != null){
#if DEBUG_APP_MANAGER
				Debug.Log("Show active view:"+scene.activeView.name);
#endif
				showView(scene.activeView.name,ViewTransitionType.NONE);
			}
#if DEBUG_APP_MANAGER
			else
				Debug.LogError("Error: Active view not set for scene "+scene.gameObject.name);
#endif
		}	
		
		if(loadCallback.ContainsKey(scene.name)){
			if(loadCallback[scene.name] != null) ((System.Action)loadCallback[scene.name])();
			loadCallback.Remove(scene.name);
		}
	}

	private static string getSceneNameForCurrAspectRatio(string sceneName)
	{
		string suffix = "";
#if DEBUG_APP_MANAGER
		if(kScreen.instance == null)
			Debug.LogError("kScreen instance is missing");
#endif
	
		switch(kScreen.getAspectRatio()){
			case kScreen.AspectRatio.Aspect4_3: suffix = "4_3";break;
			case kScreen.AspectRatio.Aspect5_3: suffix = "5_3";break;
			case kScreen.AspectRatio.Aspect16_9: suffix = "16_9";break;
		}

		if(Application.CanStreamedLevelBeLoaded(sceneName + suffix))
			return sceneName + suffix;
		else if(Application.CanStreamedLevelBeLoaded(sceneName + "16_9"))
			return sceneName + "16_9";
		else
			return sceneName;
	}

	private static Vector2 SCREEN_SIZE_3_2 = new Vector2(640,960);
	private static Vector2 SCREEN_SIZE_4_3 = new Vector2(768,1024);
	private static Vector2 SCREEN_SIZE_5_3 = new Vector2(681,1136);
	private static Vector2 SCREEN_SIZE_16_9= new Vector2(640,1136);

	public static Vector2 getResScreenSize(kScreen.AspectRatio aspectRatio){
#if DEBUG_APP_MANAGER
		if(kScreen.instance == null)
			Debug.LogError("kScreen instance is missing");
#endif

		switch(aspectRatio){
			case kScreen.AspectRatio.Aspect3_2: return SCREEN_SIZE_3_2;
			case kScreen.AspectRatio.Aspect4_3: return SCREEN_SIZE_4_3;
			case kScreen.AspectRatio.Aspect5_3: return SCREEN_SIZE_5_3;
			case kScreen.AspectRatio.Aspect16_9: return SCREEN_SIZE_16_9;
		}
		return SCREEN_SIZE_3_2;
	}
	
	public static bool showList = false;
		
	public static void listLoadedScripts(){
		kBehaviourScript[] scripts = (kBehaviourScript[])Resources.FindObjectsOfTypeAll(typeof(kBehaviourScript));
		string remainingScripts = "\n Remaining scripts:";
		for(int i = 0; i < scripts.Length;i++){
			remainingScripts += "\n\t" +scripts[i].name;
		}
#if DEBUG_APP_MANAGER
		Debug.Log(remainingScripts);
#endif
		showList = false;
	}

	private static string[] PERSISTENT_OBJECTS = new string[]{
		"kScreen",
		"ILGameController",
		"synchManager",
		"device_simulator",
		"ILNotificationCenter",
		"UnityFacebookSDKPlugin",
		"AppleAppStoreCallbackMonoBehaviour",
		"UnityThreadHelper",
		"AWSPrefab",
	};

	private static bool canDestroyObject(string objectName){
		for(int i = 0;i < PERSISTENT_OBJECTS.Length;i++){
			if(objectName == PERSISTENT_OBJECTS[i])
				return false;
		}
		return true;
	}
#if UNITY_EDITOR
	public static void PauseApp(bool pause){
		Debug.LogWarning("" +(pause ? "Pause App.":"Resume App."));
		kBehaviourScript[] scripts = (kBehaviourScript[])Resources.FindObjectsOfTypeAll(typeof(kBehaviourScript));
		for(int i = 0;i < scripts.Length;i++){
			scripts[i].OnApplicationPause(pause);
		}
	}
#endif
	public static void unloadEverything(){
		foreach(kScene scene in loadedScenes.Values){
#if DEBUG_APP_MANAGER
			Debug.Log("Unload scene " + scene.name);
#endif
			scene.unload();
		}
		loadCallback.Clear();
		controllerStack.Clear();
		viewStack.Clear();
		loadedScenes.Clear();
		controllerInTransition = null;
		viewInTransition = null;

		GameObject[] objects = (GameObject[])Resources.FindObjectsOfTypeAll(typeof(GameObject));
		for(int i = 0;i < objects.Length;i++){
			bool canBeDestroyed = true;
			if(objects[i] != null && canDestroyObject(objects[i].name))
				GameObject.Destroy(objects[i]);
		}

		Texture[] texures = (Texture[])Resources.FindObjectsOfTypeAll(typeof(Texture));
		for(int i = 0;i < texures.Length;i++){
			if(texures[i].name != null && texures[i].name.Length > 0 && !texures[i].name.Contains("Font")
			   && !texures[i].name.Contains("Unity")
			   && texures[i].name != "Soft")
			{
				if(texures[i].GetType() is UnityEngine.Cubemap)
				{
					continue;
				}
				Resources.UnloadAsset(texures[i]);
			}
		}
		Resources.UnloadUnusedAssets();
		System.GC.Collect();
	}

	public static void unloadAllScenes(){
		foreach(kScene scene in loadedScenes.Values){
#if DEBUG_APP_MANAGER
			Debug.Log("Unload scene " + scene.name);
#endif
			scene.unload();
		}

		unloadTextures();
		loadCallback.Clear();
		controllerStack.Clear();
		viewStack.Clear();
	
		loadedScenes.Clear();
		System.GC.Collect();
		System.GC.Collect();
		System.GC.Collect();
		System.GC.Collect();
		Resources.UnloadUnusedAssets();
	}
	
	public static void unloadTextures(){
		GameObject[] objects = (GameObject[])Resources.FindObjectsOfTypeAll(typeof(GameObject));
		for(int i = 0;i < objects.Length;i++){
			if(objects[i] != null && objects[i].name != "kScreen"
			&& objects[i].name != "ILGameController" && objects[i].name != "synchManager")
				GameObject.Destroy(objects[i]);
		}
		
		Texture[] texures = (Texture[])Resources.FindObjectsOfTypeAll(typeof(Texture));
		for(int i = 0;i < texures.Length;i++){
			if(texures[i].name != null && texures[i].name.Length > 0 && !texures[i].name.Contains("Font"))
				Resources.UnloadAsset(texures[i]);
		}
		Resources.UnloadUnusedAssets();
		System.GC.Collect();
	}

	public static void enableAllScripts(){
		kBehaviourScript[] scripts = (kBehaviourScript[])Resources.FindObjectsOfTypeAll(typeof(kBehaviourScript));

		for(int i = 0;i < scripts.Length;i++){
			if(!scripts[i].enabled)
				scripts[i].OnApplicationPause(false);
		}
	}
	
	public static void suspendAppState(){
		/*Debug.Log("Suspend");
		foreach(kScene scene in loadedScenes.Values){
			Debug.Log("Unload scene " + scene.name);
			//suspendedScenes.Add(JSONLevelSerializer.SaveObjectTree(scene.gameObject));
			//scene.freeTextures();
			//scene.unload();
			scene.gameObject.SetActive(false);
		}
		Texture[] texures = (Texture[])Resources.FindObjectsOfTypeAll(typeof(Texture));
		for(int i = 0;i < texures.Length;i++){
			Debug.Log("unload texture "+texures[i].name);
			if(texures[i].name != null && texures[i].name.Length > 0 && !texures[i].name.Contains("Font"))
				Resources.UnloadAsset(texures[i]);
		}
		kScreen.instance.camera.enabled = false;
		System.GC.AddMemoryPressure(30*1024);
		System.GC.Collect();
		//kAppManager.loadScene("empty_scene");
		
		//Resources.UnloadUnusedAssets();

		//System.GC.Collect();
		/*Application.LoadLevel("empty_scene");*/
	}
	
	public static void resumeAppState(){
		//for(int i = 0; i < suspendedScenes.Count;i++){
		//	Debug.Log("Deserialize scene " + i);
			//JSONLevelSerializer.LoadObjectTree(suspendedScenes[i]);
		//}
	}
	
#if DEBUG_LOG_SPRITES_USAGE
	public class xSprite{
		public string name;
		//public List<string> 	modules = new List<string>();
		public List<string> 	frames	= new List<string>();
		public List<string> 	sequences= new List<string>();
		
		public void xAddModule(string name){
			/*int idx = -1;
			for(int i = 0; i < modules.Count && idx < 0;i++)
				if(modules[i].CompareTo(name) == 0)
					idx = i;
			if(idx < 0)
				modules.Add(name);*/
		}
		public void xAddFrame(string name){
			int idx = -1;
			for(int i = 0; i < frames.Count && idx < 0;i++)
				if(frames[i].CompareTo(name) == 0)
					idx = i;
			if(idx < 0)
				frames.Add(name);
		}
		public void xAddSequence(string name){
			int idx = -1;
			for(int i = 0; i < sequences.Count && idx < 0;i++)
				if(sequences[i].CompareTo(name) == 0)
					idx = i;
			if(idx < 0)
				sequences.Add(name);
		}
		
		public string serialize(string ident){
			string txt = "\n"+ident+ name+":\n"+ident+"{";
			txt += "\n"+ident+"\t"+"frames:\n"+ident+"\t{";
			for(int i = 0; i < frames.Count;i++){
				txt += "\n" + ident+"\t\t" + frames[i];
			}
			txt += "\n"+ident +"\t}";
			txt += "\n"+ident+"\t"+"sequences:\n"+ident+"\t{";
			for(int i = 0; i < sequences.Count;i++){
				txt += "\n" + ident+"\t\t" + sequences[i];
			}
			txt += "\n"+ident +"\t}";
			return txt;
		}
	}
	
	public class xScene{
		public string name;
		public List<xSprite> sprites = new List<xSprite>();
		
		public void xAdd(string spriteName,BaseItemData item){
			if(spriteName.Contains("registrationFonts")
			|| spriteName.Contains("avatar1") || spriteName.Contains("avatar2"))
				return;
			
			xSprite sprite = null;
			for(int i = 0; i < sprites.Count && sprite == null;i++)
				if(sprites[i].name.CompareTo(spriteName) == 0)
					sprite = sprites[i];
			if(sprite == null){
				sprite = new xSprite();
				sprite.name = spriteName;
				sprites.Add(sprite);
			}
			
			if(item is SequenceData){
				sprite.xAddSequence(item.m_name);
			}else if(item is FrameData){
				sprite.xAddFrame(item.m_name);
			}else if(item is ModuleData){
				sprite.xAddModule(item.m_name);
			}else{
				Debug.LogError("Uncknown item type");
			}
		}
		
		public string serialize(string ident){
			string txt = "\n"+ident + name+":\n"+ident+"{";
			for(int i = 0; i < sprites.Count;i++){
				txt += sprites[i].serialize(ident + "\t");
			}
			txt += "\n"+ident+"\t}";
			return txt;
		}
	}
	
	public static List<xScene> x_scenes = new List<xScene>();
	
	public class kItem{
		public string 	name;
		public uint 	type;
		public string 	spriteFile;
		public List<string> scenes=new List<string>();
		
		public void addScene(string sceneName){
			for(int i = 0; i < scenes.Count;i++){
				if(scenes[i].CompareTo(sceneName) == 0)
					return;
			}
			scenes.Add(sceneName);
		}
	}
	
	public static List<kItem> items = new List<kItem>();
	
	public static void usedItem(string sceneName,string spriteName,BaseItemData item)
	{
		if(false)
		{
			xScene x_scene = null;
			for(int i = 0; i < x_scenes.Count && x_scene == null;i++)
				if(x_scenes[i].name.CompareTo(sceneName) == 0)
					x_scene = x_scenes[i];
			if(x_scene == null){
				x_scene = new xScene();
				x_scenes.Add(x_scene);
				x_scene.name = sceneName;
			}
			x_scene.xAdd(spriteName,item);
		}else{
			kItem it = getItemIndex(item.m_name,item.getType());
			if(it == null){
				it = new kItem();
				it.name = item.m_name;
				it.type = item.getType();
				it.spriteFile = spriteName;
				it.addScene(sceneName);
				items.Add(it);
			}else{
				it.addScene(sceneName);
			}
		}
	}
	
	public static kItem getItemIndex(string name,uint type){
		for(int i = 0; i < items.Count;i++){
			if(items[i].name.CompareTo(name) == 0 && items[i].type == type)
				return items[i];
		}
		return null;
	}
	
	public static void printSpriteUsage()
	{
		
		string txt = "";
		//txt = JsonFx.Json.JsonWriter.Serialize(x_scenes);
		txt = JsonFx.Json.JsonWriter.Serialize(items);
		Debug.Log("Save sprite usage: "+txt);
		File.WriteAllText(Path.Combine( Application.dataPath,Path.Combine("Resources","sprite_usage_json.txt")), txt );
	}
#endif
	
}
