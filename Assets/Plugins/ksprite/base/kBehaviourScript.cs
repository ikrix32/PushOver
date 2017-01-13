using UnityEngine;
using System.Collections;
using System;
using System.Reflection;

public class kBehaviourScript : MonoBehaviour 
{
	protected enum ScriptState{
		UNINITIALIZED = 0,
		INITIALIZED,
		READY
	}
	//Objects created in editor should be initialized by kScene instance
	//this serialized property will tell if the object should call initialization 
	//methods or wait for scene to call these methods
	[SerializeField]//[HideInInspector]
	public	bool shouldAutoInitialize = true;
	
	protected ScriptState scriptState = ScriptState.UNINITIALIZED;
	
	protected virtual void onAppPause(bool pause){
	}
	
	protected virtual void onInit(){
		//Debug.Log("INIT:"+gameObject.name+"."+GetType().Name+"/"+scriptState);
	}
	protected virtual void onStart(){
		//Debug.Log("START:"+gameObject.name+"."+GetType().Name);
	}
	protected virtual void onUpdate(){}
	protected virtual void onFixedUpdate(){}
	protected virtual void onLateUpdate(){}
	protected virtual void onDestroy(){}
	protected virtual void onEnable(){}
	protected virtual void onDisable(){}

	void OnEnable(){
		onEnable();
	}

	void OnDisable(){
		onDisable();
	}

	private bool disabledByUser = false;
	public void OnApplicationPause(bool pause)
	{
#if UNITY_EDITOR
		if (!Application.isPlaying)
			return;
#endif
		onAppPause(pause);
		if (pause) {
			disabledByUser = !enabled;
			enabled = false;
		} else {
			if (!disabledByUser)
				enabled = true;
		}
	}
	
	void Awake(){
		if((/*!Application.isPlaying ||*/ shouldAutoInitialize) && scriptState == ScriptState.UNINITIALIZED){
			initObjectSubTree(transform);
		}
	}
	
	void Start(){
		if((/*!Application.isPlaying ||*/ shouldAutoInitialize) && scriptState == ScriptState.INITIALIZED){
			startObjectSubTree(transform);
		}
	}
	
	// Update is called once per frame
	void Update () {
		if(scriptState != ScriptState.READY){ setupObjectTree(transform); return;}
			
		onUpdate();
	}
	
	void LateUpdate(){
		if(scriptState != ScriptState.READY){ setupObjectTree(transform); return;}
		onLateUpdate();		
	}
	
	void FixedUpdate(){
		if(scriptState != ScriptState.READY){ setupObjectTree(transform); return;}
		onFixedUpdate();
	}
	
	void OnDestroy(){
		onDestroy();
		//cleaup();
	}
	
	/*private void cleaup(){
		Type type = GetType();//typeof(target);   
		FieldInfo[] fields = type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
	//	for(int i = fields.Length - 1; i >= 0;i--)
		for(int i = 0; i < fields.Length;i++)
	   	{
			FieldInfo field = fields[i];
			if(!field.IsStatic && !field.IsLiteral)
		    {
				field.SetValue(this, null);
			}
		}
	}*/
	
	public static void initObjectSubTree(Transform objtransform)
	{
		if (objtransform == null)
			return;

		for ( int i = 0 ; i < objtransform.childCount; i++){
			Transform childTransform = objtransform.GetChild(i);
			initObjectSubTree(childTransform);
		}

		kBehaviourScript[] scripts = objtransform.GetComponents<kBehaviourScript>();
		
		foreach( kBehaviourScript script in scripts){
			if(script != null){
				if(objtransform.gameObject.activeInHierarchy){
					if(script.scriptState == ScriptState.UNINITIALIZED){
						script.scriptState = ScriptState.INITIALIZED;
						script.onInit();
					}
					//else Debug.LogError("Script "+"."+script.GetType().Name+" already initialized");
				}else{
					//Debug.LogError("Script "+script.GetType().Name +" should autoinitialize.");
					script.shouldAutoInitialize = true;
				}
			}
		}
	}
	
	public static void startObjectSubTree(Transform objtransform)
	{
		if (objtransform == null)
			return;

		for ( int i = 0 ; i < objtransform.childCount; i++){
			Transform childTransform = objtransform.GetChild(i);
			startObjectSubTree(childTransform);
		}

		kBehaviourScript[] scripts = objtransform.GetComponents<kBehaviourScript>();
		
		foreach( kBehaviourScript script in scripts){
			if(script != null){
				if(objtransform.gameObject.activeInHierarchy){
					if(script.scriptState == ScriptState.INITIALIZED){
						script.scriptState = ScriptState.READY;
						script.onStart();
					}
				}else
					script.shouldAutoInitialize = true;
			}
		}
	}
	
	private static void setupObjectTree(Transform objT){
		initObjectSubTree(objT);
		startObjectSubTree(objT);
	}
	
	public bool isReady(){
		return scriptState == ScriptState.READY;
	}
}
