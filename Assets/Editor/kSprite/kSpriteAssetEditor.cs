using UnityEngine;
using UnityEditor;
using System.IO;
using System;
using System.Reflection;

[CustomEditor(typeof(kSpriteAsset))]

[CanEditMultipleObjects]
public class kSpriteAssetEditor : BaseEditor
{
	[MenuItem("Assets/Create/kSprite")]
	public static void CreateMyAsset()
	{

		kSpriteAsset asset = CreateSpriteAsset(GetCurrentPath() +"/NewSprite.kSprite.asset");
		//AssetDatabase.SaveAsset(asset);
		//EditorUtility.FocusProjectWindow();
		Selection.activeObject = asset;        
	}

	public static kSpriteAsset CreateSpriteAsset(string filePath){
		kSpriteAsset asset = ScriptableObject.CreateInstance<kSpriteAsset>();//new kSpriteNew(Shader.Find("Mobile/Particles/Alpha Blended"));  //scriptable object
		Debug.LogWarning("Creating sprite:"+filePath);
		AssetDatabase.CreateAsset(asset,filePath);
		EditorUtility.SetDirty(asset);
		return asset;
	}

	public static string GetCurrentPath(){
		string path = "Assets";
		foreach (UnityEngine.Object obj in Selection.GetFiltered(typeof(UnityEngine.Object), SelectionMode.Assets)){
			path = AssetDatabase.GetAssetPath(obj);
			if (File.Exists(path)) {
				path = Path.GetDirectoryName(path);
			}
			break;
		}
		return path;
	}

	protected override void DrawInspectorStyleGUI(){
		base.DrawInspectorStyleGUI();

		if(GUILayout.Button("Reload")) {
			((kSpriteAsset)targetObj).loadSprite();
			return;
		}
	}

	protected override void FieldValueChanged(FieldInfo field, System.Object target)
	{
		base.FieldValueChanged(field,target);
		if (field.Name == "spriteBinary")
		{
			kSpriteAsset obj = (kSpriteAsset)target;
			obj.loadSprite();
			EditorUtility.SetDirty(obj);
		}
	}
}
/*
[CustomEditor(typeof(kSpriteAsset))]
[CanEditMultipleObjects]
public class kSpriteAssetEditor : Editor{
	kSpriteAsset	_target;
	
	void OnEnable(){
		_target = (kSpriteAsset)target;
	}
	
	
    [MenuItem("Assets/Create/kSprite")]
    public static void CreateMyAsset()
 	{

		kSpriteAsset asset = ScriptableObject.CreateInstance<kSpriteAsset>();//new kSpriteNew(Shader.Find("Mobile/Particles/Alpha Blended"));  //scriptable object
		AssetDatabase.CreateAsset(asset, GetCurrentPath() +"/NewSprite.kSprite.asset");
        //AssetDatabase.SaveAsset(asset);
		//EditorUtility.FocusProjectWindow();
        Selection.activeObject = asset;        
    }
	
	public static string GetCurrentPath(){
		string path = "Assets";
		foreach (UnityEngine.Object obj in Selection.GetFiltered(typeof(UnityEngine.Object), SelectionMode.Assets)){
		    path = AssetDatabase.GetAssetPath(obj);
    		if (File.Exists(path)) {
                path = Path.GetDirectoryName(path);
            }
            break;
        }
		return path;
	}
	
	public override void OnInspectorGUI()
	{
		EditorGUIUtility.LookLikeInspector();
	
		EditorGUILayout.BeginHorizontal(); 
		EditorGUILayout.Toggle("Loaded",_target.loaded);
		
		EditorGUILayout.TextField(_target.m_spriteFile);
		EditorGUILayout.EndHorizontal();
		
		
		EditorGUILayout.BeginHorizontal();
		if(GUILayout.Button("Open File"))
		{
			_target.m_spriteFile = EditorUtility.OpenFilePanel("Choose sprite file",GetCurrentPath(),"bytes");
			
            if (_target.m_spriteFile.Length != 0) {
				int index = _target.m_spriteFile.LastIndexOf("Resources");
				if(index >= 0){
					index += 10;
					_target.m_spriteFile = _target.m_spriteFile.Substring(index, _target.m_spriteFile.Length - index);
					if(_target.m_spriteFile.Contains(".bytes")) _target.m_spriteFile = _target.m_spriteFile.Substring(0,_target.m_spriteFile.IndexOf(".bytes"));
					_target.loadSprite();
				}else{
					_target.m_spriteFile = "";
					Debug.LogError("Sprite file not in folder named 'Resources' ");
				}
			}
        
        }
		EditorGUILayout.EndHorizontal();
		
		EditorGUILayout.BeginHorizontal();
		_target.m_material = (Material)EditorGUILayout.ObjectField("Material",_target.m_material,typeof(Material));
		EditorGUILayout.EndHorizontal();
	}

}*/