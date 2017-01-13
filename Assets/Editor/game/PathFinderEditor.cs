using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(PathFinder))]
[CanEditMultipleObjects]
public class PathFinderEditor : Editor {

	void OnEnable () {
	}

	public override void OnInspectorGUI() {
		DrawDefaultInspector ();

		PathFinder finder = (PathFinder)target;
		if(GUILayout.Button("Scan"))
		{
			finder.Scan();
		}
	}
}