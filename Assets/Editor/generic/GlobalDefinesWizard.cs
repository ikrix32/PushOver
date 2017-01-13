using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System;


public class GlobalDefinesWizard : ScriptableWizard
{
	public enum ConfigType{
		Unity,
		Dev,
		Test,
		Live,
		Minigames,
		Count,
	}

	public class GlobalDefine{
		public string define;
		public bool[] enabled = new bool[(int)ConfigType.Count];
	}

	private static string DEFINES_SAVE_FILE = "GlobalDefines.txt" ;

	public List<GlobalDefine> m_globalDefines = new List<GlobalDefine>();
	
	[MenuItem( "IL Utils/Global Defines" )]
    static void createWizardFromMenu()
	{
		var helper = ScriptableWizard.DisplayWizard<GlobalDefinesWizard>( "Project Global Defines", "Save", "Cancel" );
		helper.minSize = new Vector2( 600, 600 );
		helper.maxSize = new Vector2( 600, 600 );

		helper.m_globalDefines = GetGlobalDefines();
	}

	public static List<GlobalDefine> GetGlobalDefines(){
		if( File.Exists(Path.Combine( Application.dataPath,DEFINES_SAVE_FILE)))
		{
			string data = File.ReadAllText(Path.Combine( Application.dataPath,DEFINES_SAVE_FILE));
		
			List<GlobalDefine> defines = new List<GlobalDefine>();
			defines.AddRange(JsonFx.Json.JsonReader.Deserialize<GlobalDefine[]>(data));

			return defines;
		}
		return new List<GlobalDefine>();
	}
	
	[MenuItem( "IL Utils/Break Prefabs" )]
	public static void BreakPrefabs(){
		GameObject[] obj = (GameObject[])Resources.FindObjectsOfTypeAll(typeof (GameObject));
		foreach (GameObject o in obj)
		{
			if (PrefabUtility.GetPrefabParent(o) != null)
			{
				PrefabUtility.DisconnectPrefabInstance(o);
				//Debug.Log("Reconnect object:"+o.name);
				//PrefabUtility.ReconnectToLastPrefab(o);
			}
		}
	}

	[MenuItem( "IL Utils/Link Sound To All Buttons" )]
	public static void LinkSoundToAllButtons(){
		AudioClip clip = Resources.Load("sounds/UI_Button_Press") as AudioClip;
		Debug.Log("clip = " + clip);
		kButton[] buttons = (kButton[])Resources.FindObjectsOfTypeAll(typeof (kButton));
		foreach (kButton button in buttons)
		{
			kTouchable touch = button.gameObject.GetComponent<kTouchable>();
			if(touch != null)
			{
				touch.m_soundOnPress = null;
				touch.m_soundOnTap = clip;
			}
		}
	}

	[MenuItem( "IL Utils/Set Audio Source to 2D" )]
	public static void SetAudioSourceTo2D(){
		AudioSource[] sources = (AudioSource[])Resources.FindObjectsOfTypeAll(typeof (AudioSource));
		foreach (AudioSource source in sources)
		{
			if(source != null)
			{
				source.spatialBlend = 0;
			}
		}
	}
	
	[MenuItem( "IL Utils/Enable All Objects" )]
	static void EnableAllObjects(){
		Dictionary<int,GameObject> hiddenObject = new Dictionary<int, GameObject>();
		GameObject[] obj = (GameObject[])Resources.FindObjectsOfTypeAll(typeof (GameObject));
		foreach (GameObject o in obj)
		{
			if( o!= null)
			{	
				if(!o.activeSelf){
					o.SetActive(true);
					//Debug.Log("Activate object:"+o.name);
					hiddenObject.Add(o.GetInstanceID(),o);
					//o.SetActive(false);
				}
			}
		}
		
		for(int i = 0; i < hiddenObject.Keys.Count;i++){
			GameObject o = (GameObject)hiddenObject[(int)hiddenObject.Keys.ElementAt(i)];
			o.SetActive(false);
			//Debug.Log("Deactivate object:"+o.name);
		}
		
	}

	Vector2 scroll;
	void OnGUI()
	{
		scroll = EditorGUILayout.BeginScrollView(scroll);		
		var toRemove = new List<GlobalDefine>();

		EditorGUILayout.BeginHorizontal();
		EditorGUILayout.LabelField("Define Name", GUILayout.Width(220));
		EditorGUILayout.LabelField("Unity", GUILayout.Width(50));
		EditorGUILayout.LabelField("Dev",GUILayout.Width(50));
		EditorGUILayout.LabelField("Test",GUILayout.Width(50));
		EditorGUILayout.LabelField("Live",GUILayout.Width(50));
		EditorGUILayout.LabelField("Minigames",GUILayout.Width(50));
		EditorGUILayout.EndHorizontal();

		foreach( GlobalDefine define in m_globalDefines )
		{
			if( defineEditor( define ) )
				toRemove.Add( define );
		}

		foreach( GlobalDefine define in toRemove )
			m_globalDefines.Remove( define );

		if( GUILayout.Button( "Add Define" ) )
		{
			var d = new GlobalDefine();
			d.define = "NEW_DEFINE";
			//d.enabled = false;
			m_globalDefines.Add( d );
		}
		GUILayout.Space( 40 );

		if( GUILayout.Button( "Save" ) )
		{
			Save();
			Close();
		}
		EditorGUILayout.EndScrollView();
	}

	private void Save()
	{
		deleteFiles();

		string data = JsonFx.Json.JsonWriter.Serialize(m_globalDefines);
		File.WriteAllText(Path.Combine( Application.dataPath, DEFINES_SAVE_FILE), data );

		//apply Unity config
		ApplyGlobalDefines( BuildTargetGroup.iOS, ConfigType.Unity, m_globalDefines);
	}

	public static void ApplyGlobalDefines( BuildTargetGroup targetGroup,ConfigType config, List<GlobalDefine> globalDefines = null){
		if(globalDefines == null)
			globalDefines = GetGlobalDefines();

		var toDisk = globalDefines.Where( d => d.enabled[(int)config] ).Select( d => d.define + string.Empty).ToArray();
		string defines = "";
		if( toDisk.Length > 0 )
		{
			for( int i = 0; i < toDisk.Length; i++ )
				defines += toDisk[i] + ";";
		}

		PlayerSettings.SetScriptingDefineSymbolsForGroup(targetGroup,defines);
	}

	private void deleteFiles()
	{
		var smcsFile = Path.Combine( Application.dataPath, "smcs.rsp" );
		var gmcsFile = Path.Combine( Application.dataPath, "gmcs.rsp" );

		if( File.Exists( smcsFile ) )
			File.Delete( smcsFile );

		if( File.Exists( gmcsFile ) )
			File.Delete( gmcsFile );
	}


	/*private void writeFiles( string data )
	{
		var smcsFile = Path.Combine( Application.dataPath, "smcs.rsp" );
		var gmcsFile = Path.Combine( Application.dataPath, "gmcs.rsp" );

		// -define:debug;poop
		File.WriteAllText( smcsFile, data );
		File.WriteAllText( gmcsFile, data );
	}*/


	private bool defineEditor( GlobalDefine define )
	{
		EditorGUILayout.BeginHorizontal();

		define.define = EditorGUILayout.TextField( define.define );

		for(int i = 0; i < (int)ConfigType.Count;i++){
			define.enabled[i] = EditorGUILayout.Toggle( define.enabled[i] , GUILayout.Width(50));
		}
		//define.value = EditorGUILayout.TextField( define.value );

		var remove = false;
		if( GUILayout.Button( "Remove" ) )
			remove = true;

		EditorGUILayout.EndHorizontal();

		return remove;
	}


	// Called when the 'save' button is pressed
    void OnWizardCreate()
    {
		// .Net 2.0 Subset: smcs.rsp
		// .Net 2.0: gmcs.rsp
		// -define:debug;poop
    }


    void OnWizardOtherButton()
    {
    	this.Close();
    }
}