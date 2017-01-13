using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using System.Reflection;
using System;
using System.Linq;
using System.IO;

public interface CustomBuildProcessor{
	void OnGUI(ScriptableWizard wizard);
	void OnPreProcess(string buildPathDir);
	void OnPostProcess(string buildPathDir);
	
	void OnPreProcessAssetBundle(BuildProcessor.ABundle bundle);
	void OnPostProcessAssetBundle(BuildProcessor.ABundle bundle);
}

public class BuildProcessor : ScriptableWizard {

	[MenuItem ("IL Utils/Build Processor")]
    static void BuildSettings() {
		var helper = ScriptableWizard.DisplayWizard<BuildProcessor>( "Build Processor", "Save", "Cancel" );
		helper.minSize = new Vector2( 1100, 650 );
		helper.maxSize = new Vector2( 1100, 650 );
    }
	
	List<CustomBuildProcessor> m_userScripts = new List<CustomBuildProcessor>();
	Config m_config;
	
	public BuildProcessor()
	{
		m_config = Config.load();
		instantiateUserScripts();
	}
	
	private void instantiateUserScripts(){
		IEnumerable<Type> types = Assembly.GetExecutingAssembly().GetTypes().Where(type => (type.GetInterfaces().Where(interf => interf == typeof(CustomBuildProcessor)).Count()) > 0);
        foreach (Type type in types){
            m_userScripts.Add((CustomBuildProcessor)Activator.CreateInstance(type));
        }
	}
	
	private Vector2 sceneViewScrollPos = Vector2.zero;
	private Vector2 excludeViewScrollPos = Vector2.zero;
	private Vector2 assetsViewScrollPos = Vector2.zero;
	
	private int tmpSelScene = 0;
	
	void OnGUI()
	{
		GUILayout.BeginHorizontal();
		GUILayout.BeginArea(new Rect(0,0,450,650));
		EditorGUI.indentLevel++;
		EditorGUI.DropShadowLabel(new Rect(0,0,60,15),"Scenes");
		EditorGUI.DrawRect(new Rect(0,20,500,250),Color.white);
		
		GUILayout.Space(20);
		sceneViewScrollPos = EditorGUILayout.BeginScrollView(sceneViewScrollPos,GUILayout.Height(250));
		for(int i=0;i<m_config.scenes.Count;++i) {
			GUILayout.BeginHorizontal();
			string path = m_config.scenes[i].path;
        	bool enabled = EditorGUILayout.BeginToggleGroup(path,m_config.scenes[i].enabled);
			EditorGUILayout.EndToggleGroup();
			if(enabled != m_config.scenes[i].enabled){
				m_config.scenes[i].enabled = enabled;
				m_config.hasChanges = true;
			}
			if(GUILayout.Button("-", GUILayout.Width(20), GUILayout.Height(15))) {
				m_config.removeSceneAt(i);
				m_config.hasChanges = true;
				return;
			}
			GUILayout.EndHorizontal();
			EditorGUILayout.Separator();
        };
		EditorGUILayout.EndScrollView();
		EditorGUILayout.Separator();
		if(GUILayout.Button("Add Current Scene", GUILayout.Width(150), GUILayout.Height(25))) {
			if(!m_config.containsScene(EditorApplication.currentScene)){
				m_config.addScene(new EditorBuildSettingsScene(EditorApplication.currentScene,true));
			}
		}
		EditorGUI.indentLevel--;
		
		GUILayout.Space(20);
		m_config.activeBuildTarget= (BuildTarget)EditorGUILayout.EnumPopup("Build Target",m_config.activeBuildTarget,GUILayout.Width(300));
		GUILayout.Space(15);
		GUILayout.BeginHorizontal();
		EditorGUILayout.LabelField("Current Code Version ");
		GUILayout.EndHorizontal();

		//GUILayout.BeginHorizontal();
		//PlayerSettings.bundleVersion = EditorGUILayout.TextField("Build Version",PlayerSettings.bundleVersion);
		//GUILayout.EndHorizontal();
		
		
		GUILayout.Space(15);
		GUILayout.BeginHorizontal();
		GUILayout.Label(" Build location: "+ EditorUserBuildSettings.GetBuildLocation(m_config.activeBuildTarget));
		if(GUILayout.Button("Change")) {
			changeBuildLocation(m_config.activeBuildTarget);
			return;
		}
		GUILayout.EndHorizontal();
		m_config.buildStreamingAssets = EditorGUILayout.Toggle("Build Streaming Assets",m_config.buildStreamingAssets);
		
		GUILayout.Space(10);
		GUILayout.BeginHorizontal();
		GUILayout.BeginVertical();
		EditorGUI.indentLevel++;
		EditorUserBuildSettings.development = EditorGUILayout.BeginToggleGroup("Development Build", EditorUserBuildSettings.development);
		EditorUserBuildSettings.connectProfiler = EditorGUILayout.Toggle("Autoconnect Profiler",EditorUserBuildSettings.connectProfiler);
		EditorUserBuildSettings.allowDebugging = EditorGUILayout.Toggle("Script Debugging",EditorUserBuildSettings.allowDebugging);
		EditorGUILayout.EndToggleGroup();
		EditorUserBuildSettings.symlinkLibraries = EditorGUILayout.Toggle("Symlink Unity Libraries",EditorUserBuildSettings.symlinkLibraries);
		EditorGUI.indentLevel--;
		GUILayout.EndVertical();
		
		GUILayout.BeginVertical();
		GUILayout.Space(10);
		GUILayout.BeginHorizontal();
		if(GUILayout.Button("Build AssetBundles", GUILayout.Width(150), GUILayout.Height(50))) {
			buildAssetsBundles();
			return;
		}
		
		/*if(GUILayout.Button("Build", GUILayout.Width(100), GUILayout.Height(50))) {
			build();
			return;
		}*/
		
		GUILayout.EndHorizontal();
		GUILayout.EndVertical();
		GUILayout.EndHorizontal();
		//GUILayout.EndArea();
		
		GUILayout.Space(8);
		
		EditorGUI.DropShadowLabel(new Rect(0,495,450,15),"Build Exclude List");
		EditorGUI.DrawRect(new Rect(0,515,450,100),Color.grey);
		
		//GUILayout.BeginArea(new Rect(0,520,450,82));
		excludeViewScrollPos = EditorGUILayout.BeginScrollView(excludeViewScrollPos,GUILayout.Height(100));
		for(int i=0;i<m_config.exclusionList.Count;++i) {
			GUILayout.BeginHorizontal();
			bool enabled =  EditorGUILayout.BeginToggleGroup(m_config.exclusionList[i].path, m_config.exclusionList[i].enabled);//EditorGUILayout.Toggle(m_config.exclusionList[i].path,m_config.exclusionList[i].enabled);
			EditorGUILayout.EndToggleGroup();
			if(enabled != m_config.exclusionList[i].enabled){
				m_config.exclusionList[i].enabled = enabled;
				m_config.hasChanges = true;
			}
			if(GUILayout.Button("-", GUILayout.Width(20), GUILayout.Height(15))) {
				m_config.removeExclusionAt(i);
				return;
			}
			GUILayout.EndHorizontal();
			EditorGUILayout.Separator();
        };
		EditorGUILayout.EndScrollView();
		
		if(GUILayout.Button("Add Folder", GUILayout.Width(150), GUILayout.Height(25))) {
			/*string path = EditorUtility.SaveFolderPanel("Exclude Folder ",Application.dataPath + "/Resources","");
	   
			if(!string.IsNullOrEmpty(path) && path.Contains(Application.dataPath)){
				m_config.addExclusion(path.Replace(Application.dataPath,""));
			}*/
		}
		GUILayout.EndArea();
		
		
		EditorGUI.DrawRect(new Rect(450,0,650,650),new Color(49.0f/255,77.0f/255,121.0f/255,1));
		EditorGUI.DropShadowLabel(new Rect(450,2,100,15),"Asset Bundles");
		EditorGUI.DrawRect(new Rect(452,20,646,250),Color.grey);
		GUILayout.BeginArea(new Rect(452,0,646,650));
		GUILayout.BeginVertical();
		GUILayout.Space(20);
		assetsViewScrollPos = EditorGUILayout.BeginScrollView(assetsViewScrollPos,GUILayout.Height(250));
		
		string[] bScenes = Directory.GetFiles(Application.dataPath + "/Scenes", "*.unity", SearchOption.AllDirectories);
		for (int i = 0; i < bScenes.Length; i++) {
			bScenes[i] = bScenes[i].Substring(bScenes[i].IndexOf("Assets/Scenes"));
			bScenes[i] = bScenes[i].Replace("/", "\\");
		}
		
		for(int i = 0;i < m_config.m_assets.Count;++i) {
			GUILayout.BeginHorizontal();
			
			bool export = EditorGUILayout.BeginToggleGroup(Path.GetFileName(m_config.m_assets[i].exportPath),m_config.m_assets[i].export);
			if(export != m_config.m_assets[i].export){
				m_config.m_assets[i].export = export;
				m_config.hasChanges = true;
			}
			EditorGUILayout.EndToggleGroup();
			
			if(GUILayout.Button("-", GUILayout.Width(20), GUILayout.Height(15))) {
				m_config.m_assets.RemoveAt(i);
				m_config.hasChanges = true;
				return;
			}
			GUILayout.EndHorizontal();
			
			EditorGUI.indentLevel++;
			EditorGUILayout.LabelField("Export location: " + m_config.m_assets[i].exportPath);
			m_config.m_assets[i].foldout = EditorGUILayout.Foldout(m_config.m_assets[i].foldout,"Scenes");
			
			if(m_config.m_assets[i].foldout){
				EditorGUI.indentLevel++;
				for(int j = 0; j < m_config.m_assets[i].scenes.Count;j++){
					GUILayout.BeginHorizontal();
					EditorGUILayout.LabelField(m_config.m_assets[i].scenes[j]);
					
					if(GUILayout.Button("-", GUILayout.Width(20), GUILayout.Height(15))) {
						m_config.m_assets[i].scenes.RemoveAt(j);
						m_config.hasChanges = true;
						return;
					}
					GUILayout.EndHorizontal();
				}
				EditorGUILayout.Separator();
				GUILayout.BeginHorizontal();
				tmpSelScene = EditorGUILayout.Popup(tmpSelScene, bScenes);
				if (GUILayout.Button("+", GUILayout.Width(20), GUILayout.Height(17))) {
					m_config.m_assets[i].scenes.Add(bScenes[tmpSelScene].Replace("\\", "/"));
					m_config.hasChanges = true;
					return;
				}
				GUILayout.EndHorizontal();
				EditorGUI.indentLevel--;
			}
			EditorGUI.indentLevel--;
			EditorGUILayout.Separator();
        };
		EditorGUILayout.EndScrollView();
		
		if(GUILayout.Button("Add Bundle", GUILayout.Width(100), GUILayout.Height(30))) {
			string path = EditorUtility.SaveFilePanel("Export Folder ",Application.streamingAssetsPath,"new","unity3d");
			string projRoot = Application.dataPath.Replace("Assets","");
			if(!string.IsNullOrEmpty(path) && path.Contains(projRoot)){
				ABundle bundle = new ABundle();
				bundle.exportPath = path.Replace(projRoot,"");
				m_config.m_assets.Add(bundle);
				m_config.hasChanges = true;
			}
		}
		GUILayout.EndVertical();
		GUILayout.EndArea();
		GUILayout.EndHorizontal();
		
		
		for(int i = 0; i < m_userScripts.Count;i++){
			m_userScripts[i].OnGUI(this);
		}
		if(m_config.hasChanges)
			m_config.save();
	}
	
	private void changeBuildLocation(BuildTarget target){
		string path = EditorUtility.SaveFolderPanel("Build "+target,"",""+target);
	   
		if(!string.IsNullOrEmpty(path) && path != EditorUserBuildSettings.GetBuildLocation(target))
			EditorUserBuildSettings.SetBuildLocation(target,path);
	}
	
	private void build(){
		string path = EditorUserBuildSettings.GetBuildLocation(m_config.activeBuildTarget);
		
		if(m_config.buildStreamingAssets){
			buildStreamingAssetsBundles();
		}
		
		string[] scenes = m_config.getScenes(false);
		for(int i = 0; i < m_userScripts.Count;i++)
			m_userScripts[i].OnPreProcess(path);
		
		
		string tempDir = Application.dataPath+"/../TempBuild";
		if(!Directory.Exists(tempDir)){
			Directory.CreateDirectory(tempDir);
		}
		
		for(int i = 0; i < m_config.exclusionList.Count;i++){
			if(m_config.exclusionList[i].enabled){
				string dPath = Application.dataPath + m_config.exclusionList[i].path;
				if(Directory.Exists(dPath)){
					string parent = Path.GetDirectoryName(tempDir + m_config.exclusionList[i].path);
					if(!Directory.Exists(parent))
						Directory.CreateDirectory(parent);
					Directory.Move(dPath,tempDir + m_config.exclusionList[i].path);
				}
			}
		}
		
		BuildPipeline.BuildPlayer(scenes,EditorUserBuildSettings.GetBuildLocation(m_config.activeBuildTarget),m_config.activeBuildTarget,BuildOptions.None);
       
		for(int i = 0; i < m_config.exclusionList.Count;i++){
			if(m_config.exclusionList[i].enabled){
				string dPath = Application.dataPath + m_config.exclusionList[i].path;
				if(Directory.Exists(tempDir + m_config.exclusionList[i].path))
					Directory.Move(tempDir + m_config.exclusionList[i].path,dPath);
			}
		}
		Directory.Delete(tempDir,true);
		
		for(int i = 0; i < m_userScripts.Count;i++)
			m_userScripts[i].OnPostProcess(path);  
		
	}
	
	private void buildAssetBundle(ABundle bundle)
	{
		for (int i = 0; i < m_userScripts.Count; i++) {
			m_userScripts[i].OnPreProcessAssetBundle(bundle);
		}
		string fileName = Path.GetFileName(bundle.exportPath);
		string path = Application.dataPath + "/../" + bundle.exportPath;
		if (!path.Contains("StreamingAssets")) {
			path = path.Replace(fileName, m_config.activeBuildTarget.ToString() + "/" + fileName);
		}
		string folder = Path.GetDirectoryName(path);
		if (!Directory.Exists(folder))
			Directory.CreateDirectory(folder);
/*
		uint CRC = 0;
		BuildPipeline.BuildStreamedSceneAssetBundle(bundle.scenes.ToArray(), path, m_config.activeBuildTarget, out CRC);

		string info = "\n Bundle Name: " + fileName;
		info += "\n Bundle Size: " + (new System.IO.FileInfo(path)).Length;
		info += "\n Bundle CRC: " + CRC.ToString("x");

		string ext = Path.GetExtension(path);
		File.WriteAllText(path.Replace(ext, "_info.txt"), info);
*/
		AssetBundleBuild[] buildMap = new AssetBundleBuild[1];
		buildMap[0].assetBundleName = fileName;//Path.GetFileName(bundle.exportPath);
		buildMap[0].assetNames = bundle.scenes.ToArray();
		BuildPipeline.BuildAssetBundles(folder, buildMap, BuildAssetBundleOptions.None, m_config.activeBuildTarget);

		for (int i = 0; i < m_userScripts.Count; i++) {
			m_userScripts[i].OnPostProcessAssetBundle(bundle);
		}
	}
	
	private void buildAssetsBundles(){
		for(int i = 0; i < m_config.m_assets.Count;i++){
			if(m_config.m_assets[i].export)
				buildAssetBundle(m_config.m_assets[i]);
		}
	}
	private void buildStreamingAssetsBundles(){
		for(int i = 0; i < m_config.m_assets.Count;i++){
			if(m_config.m_assets[i].exportPath.Contains("StreamingAssets"))//m_config.m_assets[i].export)
				buildAssetBundle(m_config.m_assets[i]);
		}
	}
	
	
 /*
    [MenuItem ("Build/Custom Build",false,0)]
    static void CustomBuildOnly() {
        CustomBuild(false);
    }
    [MenuItem ("Build/Custom Build - Run",false,1)]
    static void CustomBuildRun() {
        CustomBuild(true);
    }
     
    static void CustomBuild(bool runbuild) {
        /*BuildOptions customBuildOptions=EditorUserBuildSettings;
        if (runbuild) {
            //set EditorUserBuildSettings.architectureFlags enum flags set buildrun bit on
            if ((customBuildOptions & BuildOptions.AutoRunPlayer) != BuildOptions.AutoRunPlayer) {
                customBuildOptions = customBuildOptions | BuildOptions.AutoRunPlayer;
            }
        } else {
            //set EditorUserBuildSettings.architectureFlags enum flags set showfolder bit on
            if ((customBuildOptions & BuildOptions.ShowBuiltPlayer) != BuildOptions.ShowBuiltPlayer) {
                customBuildOptions = customBuildOptions | BuildOptions.ShowBuiltPlayer;
            }
        }*/
        //BuildTarget mycustombuildtarget=EditorUserBuildSettings.activeBuildTarget;
        //string extn="";
		/*
        switch (EditorUserBuildSettings.activeBuildTarget) {
            case BuildTargetGroup.Standalone:
                switch (EditorUserBuildSettings.selectedStandaloneTarget) {
                    case BuildTarget.DashboardWidget:
                        mycustombuildtarget=BuildTarget.DashboardWidget;
                        extn="wdgt";
                        break;
                    case BuildTarget.StandaloneWindows:
                        mycustombuildtarget=BuildTarget.StandaloneWindows;
                        extn="exe";
                        break;
                //    case BuildTarget.StandaloneWindows64:
                //        mycustombuildtarget=BuildTarget.StandaloneWindows64;
                //        extn="exe";
                //        break;
                    case BuildTarget.StandaloneOSXUniversal:
                        mycustombuildtarget=BuildTarget.StandaloneOSXUniversal;
                        extn="app";
                        break;
                    case BuildTarget.StandaloneOSXPPC:
                        mycustombuildtarget=BuildTarget.StandaloneOSXPPC;
                        extn="app";
                        break;
                    case BuildTarget. StandaloneOSXIntel:
                        mycustombuildtarget=BuildTarget. StandaloneOSXIntel;
                        extn="app";
                        break;
                }        
                break;
            case BuildTargetGroup.WebPlayer:
                if (EditorUserBuildSettings.webPlayerStreamed) {
                    mycustombuildtarget=BuildTarget.WebPlayerStreamed;
                    extn="unity3d";
                } else {
                    mycustombuildtarget=BuildTarget.WebPlayer;
                    extn="unity3d";
                }
                break;
            case BuildTargetGroup.Wii:
                mycustombuildtarget=BuildTarget.Wii;
                //extn="???"
                break;
            case BuildTargetGroup.iPhone:
                mycustombuildtarget=BuildTarget.iPhone;
                extn="xcode";
                break;
            case BuildTargetGroup.PS3:
                mycustombuildtarget=BuildTarget.PS3;
                //extn="???"
                break;
            case BuildTargetGroup.XBOX360:
                mycustombuildtarget=BuildTarget.XBOX360;
                //extn="???"
                break;
            case BuildTargetGroup.Android:
                mycustombuildtarget=BuildTarget.Android;
                extn="apk";
                break;
            case BuildTargetGroup.Broadcom:
                mycustombuildtarget=BuildTarget.StandaloneBroadcom;
                //extn="???"
                break;
            case BuildTargetGroup.GLESEmu:
                mycustombuildtarget=BuildTarget.StandaloneGLESEmu;
                //extn="???"
                break;
            case BuildTargetGroup.NaCl:
                mycustombuildtarget=BuildTarget.NaCl;
                //extn="???"
                break;
        }* /
 
        string savepath = EditorUtility.SaveFilePanel("Build "+EditorUserBuildSettings.activeBuildTarget,
        EditorUserBuildSettings.GetBuildLocation(mycustombuildtarget),"",extn);
        if(savepath.Length != 0) {
            string dir=System.IO.Path.GetDirectoryName(savepath); //get build directory
            string[] scenes=new string[EditorBuildSettings.scenes.Length];
            for(int i=0;i<EditorBuildSettings.scenes.Length;++i) {
                scenes[i]=EditorBuildSettings.scenes[i].path.ToString();
            };
            PreProcess(dir);
            BuildPipeline.BuildPlayer(scenes,savepath,mycustombuildtarget,BuildOptions.None);
            EditorUserBuildSettings.SetBuildLocation(mycustombuildtarget,dir); //store new location for this type of build
            PostProcess(dir); // * warning, if set to build and run in build settings, this may fire after application execution.
        }  
        Debug.Log(mycustombuildtarget.ToString() );
    }
 
    static void PreProcess(string BuildPathDir) {
        //Your pre-process here, copy Assets/Folders to Directory, edit html pages etc
        //or you could execute a shell, cgi-perl or bat command by doing something like below:-
        //Application.OpenURL("yourfile.bat "+BuildPathDir); //etc
    }
     
    static void PostProcess(string BuildPathDir) {
        //Your post-process here, copy Assets/Folders to Directory, edit html pages etc
        //*warning, if set to run after build, these commands may execute after the built application is run.
        //Application.OpenURL("yourfile.bat "+BuildPathDir); //etc
    }*/
	
	public class ABundle{
		private bool m_fold;
		private bool m_export = true;
		
		private string m_exportPath;
		private List<string> m_scenes = new List<string>();
		
		internal int tmpSelScene = 0;
		
		public string exportPath{
			get{
				return m_exportPath;
			}
			set{
				m_exportPath = value;
			}
		}
		public List<string> scenes{
			get{
				return m_scenes;
			}
			set{
				m_scenes = value;
			}
		}
		
		public bool export{
			get{ return m_export;}
			set{ m_export = value;}
		}
		public bool foldout{
			get{ return m_fold;}
			set{ m_fold = value;}
		}
	}
	
	
	public class Config
	{
		public static string fileName = Application.dataPath+"/../ProjectSettings/_build_config.txt";
		public bool hasChanges = false;
		private BuildTarget m_activeBuildTarget = EditorUserBuildSettings.activeBuildTarget;
		internal List<EditorBuildSettingsScene> scenes = new List<EditorBuildSettingsScene>();//not serialized
		public	List<ExclusionPath> exclusionList = new List<ExclusionPath>();
		
		private bool m_buildStreamingAssets;
		
		public List<ABundle> m_assets = new List<ABundle>();
		
		public BuildTarget activeBuildTarget{
			get{
				return m_activeBuildTarget;
			}
			set{
				if(value != m_activeBuildTarget){
					hasChanges = true;
					m_activeBuildTarget = value;
				}
			}
		}
		
		public bool buildStreamingAssets{
			get{
				return m_buildStreamingAssets;
			}
			set{
				m_buildStreamingAssets = value;
				hasChanges = true;
			}
		}
		
		
		public void addScene(EditorBuildSettingsScene scene){
			scenes.Add(scene);
			hasChanges = true;
		}
		
		public bool containsScene(string path){
			for(int i = 0; i < scenes.Count;i++)
				if(scenes[i].path == path)
					return true;
			return false;
		}
		
		public void removeSceneAt(int index){
			scenes.RemoveAt(index);
			hasChanges = true;
		}
		
		public string[] getScenes(bool all){
			List<string> result = new List<string>();
			for(int i = 0; i < scenes.Count;i++)
				if(all || scenes[i].enabled)
					result.Add(scenes[i].path);
			return result.ToArray();
		}
		
		public void addExclusion(string path){
			exclusionList.Add(new ExclusionPath(path,true));
			hasChanges = true;
		}
		
		public void removeExclusionAt(int index){
			exclusionList.RemoveAt(index);
			hasChanges = true;
		}
		
		public static Config load(){
			Config cfg = null;
			if (File.Exists(Config.fileName)){
				string content = File.ReadAllText(Config.fileName);
				cfg = JsonFx.Json.JsonReader.Deserialize<Config>(content);
			}else
				cfg = new Config();
			
			for(int i = 0; i < EditorBuildSettings.scenes.Length;i++)
				cfg.scenes.Add(EditorBuildSettings.scenes[i]);
			
			return cfg;
		}
		
		public void save(){
			File.WriteAllText(Config.fileName,JsonFx.Json.JsonWriter.Serialize(this));
			EditorBuildSettings.scenes = scenes.ToArray();
			hasChanges = false;
		}
		
		public class ExclusionPath{
			public string path;
			public bool enabled;
			public ExclusionPath(){}
			public ExclusionPath(string path,bool enabled){this.path = path;this.enabled = enabled;}
		}
	}
}
