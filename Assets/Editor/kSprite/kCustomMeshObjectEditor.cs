using UnityEngine;
using UnityEditor;
using System.Collections;

#pragma warning disable

[CustomEditor(typeof(kCustomMeshObject))]
public class kCustomMeshObjectEditor : Editor
{
	public kCustomMeshObject _target;
	
	void OnEnable()
	{
		_target = (kCustomMeshObject)target;
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
		//DrawDefaultInspector();
		EditorGUIUtility.LookLikeControls();
		
		EditorGUI.indentLevel = 0;
		/*
		EditorGUILayout.BeginHorizontal();
		_target.texture = (Texture)EditorGUILayout.ObjectField("Texture", _target.texture,typeof(Texture));
		EditorGUILayout.EndHorizontal();
		*/
		EditorGUILayout.BeginHorizontal();
		int vertexCount = EditorGUILayout.IntField("Vertex Count", _target.vertices.Count);
		EditorGUILayout.EndHorizontal();
		
		if (vertexCount > _target.vertices.Count) {
			while (_target.vertices.Count < vertexCount) {
				Vector3 position = _target.vertices.Count > 0 ? _target.vertices[_target.vertices.Count - 1] + Vector3.right * 50 : Vector3.right * 50;
				Color color = _target.vertices.Count > 0 ? _target.vertColors[_target.vertices.Count - 1] : Color.white;
				_target.vertices.Add(position);
				_target.vertColors.Add(color);
			}
		}
		
		// remove node?
		if (vertexCount < _target.vertices.Count) {
			if (EditorUtility.DisplayDialog("Remove vertices?", "Shortening the vertices list will permanently destroy parts of your mesh.", "OK", "Cancel")) {
				int removeCount = _target.vertices.Count - vertexCount;
				_target.vertices.RemoveRange(_target.vertices.Count - removeCount, removeCount);
				_target.vertColors.RemoveRange(_target.vertColors.Count - removeCount, removeCount);
			}
		}
		
		// display nodes
		EditorGUI.indentLevel = 1;
		for (int k = 0; k < _target.vertices.Count; k++) {
			EditorGUILayout.BeginHorizontal();
			_target.vertices[k] = EditorGUILayout.Vector3Field("Vert " + (k + 1), _target.vertices[k]);
			_target.vertColors[k] = EditorGUILayout.ColorField("", _target.vertColors[k], GUILayout.MaxWidth(75));
			EditorGUILayout.EndHorizontal();
		}
		
		EditorGUILayout.Space();
		
		EditorGUILayout.BeginHorizontal();
		_target.triangulatorType = (Triangulator.Type)EditorGUILayout.EnumPopup("Triangulator Type", _target.triangulatorType);
		EditorGUILayout.EndHorizontal();

		EditorGUILayout.BeginHorizontal();
		Color bColor =  EditorGUILayout.ColorField("Blending color",_target.m_blendingColor);
		EditorGUILayout.EndHorizontal();
		if(bColor != _target.m_blendingColor){
			_target.setBlendingColor(bColor);
		}

		EditorGUILayout.BeginHorizontal();
		GUILayoutOption[] emptyOptions = new GUILayoutOption[0];
		Shader sh = (Shader)EditorGUILayout.ObjectField("Custom shader",_target.customShader,typeof(Shader),emptyOptions);
		EditorGUILayout.EndHorizontal();
		if(sh != _target.customShader){
			_target.customShader = sh;
		}

		// update and redraw
		if (GUI.changed) {
			EditorUtility.SetDirty(_target);
		}
	}
	
	void OnSceneGUI()
	{
		// allow waypoint adjustment undo
		Undo.SetSnapshotTarget(_target, "Adjust");

		if (_target.vertices.Count > 0) {
			// node handle display
			for (int j = 0; j < _target.vertices.Count; j++) {
				//_target.paths[i].nodes[j] = Handles.PositionHandle(_target.paths[i].nodes[j], Quaternion.identity);
				_target.vertices[j] = _target.transform.InverseTransformPoint(Handles.PositionHandle(_target.transform.TransformPoint(_target.vertices[j]), Quaternion.identity));
			}
		}
	}
}
