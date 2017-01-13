using UnityEditor;
using UnityEngine;
using System.Collections;

// Custom Editor using SerializedProperties.
// Automatic handling of multi-object editing, undo, and prefab overrides.
[CustomEditor(typeof(PushOverScene))]
[CanEditMultipleObjects]
public class PushOverSceneEditor : Editor {

	void OnEnable () {
	}

	public override void OnInspectorGUI() {
		DrawDefaultInspector ();

		PushOverScene scene = (PushOverScene)target;
		if(GUILayout.Button("Import levels"))
		{
			scene.LoadScene();
		}
	}
}