using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(LevelCollisionMap))]
[CanEditMultipleObjects]
public class LevelCollisionMapEditor : Editor {

	void OnEnable () {
	}

	public override void OnInspectorGUI() {
		DrawDefaultInspector ();

		LevelCollisionMap finder = (LevelCollisionMap)target;
		if(GUILayout.Button("Scan"))
		{
			finder.Scan();
		}
	}
}