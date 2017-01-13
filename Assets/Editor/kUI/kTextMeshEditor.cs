using UnityEngine;
using UnityEditor;
using System.Collections;
using System;
using System.Reflection;

[CustomEditor(typeof(kTextMesh))]
[CanEditMultipleObjects]
public class kTextMeshEditor : BaseEditor
{
	protected override void FieldValueChanged(FieldInfo field, System.Object target)
	{
		base.FieldValueChanged(field, target);
		if (field.Name == "m_text" || field.Name == "m_blendColor" ||
			field.Name == "m_textWarpWidth" || field.Name == "m_linesNumber") {
			((kTextMesh)target).updateTextMesh();
		}
	}

	public override void onInspectorGUI()
	{
		base.onInspectorGUI();

		EditorGUILayout.BeginHorizontal();
		EditorGUILayout.LabelField("Text", GUILayout.MinWidth(120));
		((kTextMesh)target).m_text = EditorGUILayout.TextArea(((kTextMesh)target).m_text, GUILayout.MinWidth(240), GUILayout.MaxWidth(480), GUILayout.Height(45));
		EditorGUILayout.EndHorizontal();

		if (GUI.changed) {
			EditorUtility.SetDirty(target);
			((kTextMesh)target).updateTextMesh();
		}
	}
}
