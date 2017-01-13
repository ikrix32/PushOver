using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor(typeof(kObjectPackage))]
[CanEditMultipleObjects]

public class KObjectPackageEditor : Editor {

	kObjectPackage	_target;
	
	void OnEnable()
	{
		_target = (kObjectPackage)target;
	}

	public override void OnInspectorGUI()
	{
		EditorGUI.BeginChangeCheck ();
		Undo.RecordObject(target, target.name);

		onInspectorGUI();

		if (EditorGUI.EndChangeCheck ()) {
			EditorUtility.SetDirty (target);
		}
	}

	public virtual void onInspectorGUI()
	{
		//EditorGUIUtility.LookLikeInspector();
		DrawDefaultInspector();
		
		EditorGUILayout.BeginHorizontal();
			TextAsset objPckFile = (TextAsset)EditorGUILayout.ObjectField("Object Package Binary",_target.objPackBinary,typeof(TextAsset), true);
			if(objPckFile != _target.objPackBinary) {
				_target.objPackBinary = objPckFile;
				_target.loadObjectPackage();
			}
			EditorGUILayout.EndHorizontal();
		if(GUILayout.Button("Load sprites")) {
			if(_target.objPackBinary != null) {
				_target.loadObjectPackage(true);
			}
		}
	}
}
