using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(LevelMap))]
[CanEditMultipleObjects]
public class LevelMapEditor : Editor {

	void OnEnable () {
	}

	public override void OnInspectorGUI() {
		DrawDefaultInspector ();

		LevelMap finder = (LevelMap)target;
		if(GUILayout.Button("Scan"))
		{
			finder.Scan();
		}
	}
}