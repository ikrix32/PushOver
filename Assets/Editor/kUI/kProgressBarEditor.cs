using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor(typeof(kProgressBar))]
[CanEditMultipleObjects]
public class kProgressBarEditor : BaseEditor
{
	protected kProgressBar _target;
	
	void OnEnable()
	{
		_target = (kProgressBar)target;
	}
	
	public override void onInspectorGUI()
	{
		//DrawDefaultInspector();
		//EditorGUIUtility.LookLikeInspector();

		EditorGUILayout.BeginHorizontal();
		_target.m_text = (kTextField)EditorGUILayout.ObjectField("Progress Bar Text", _target.m_text, typeof(kTextField), true);
		EditorGUILayout.EndHorizontal();

		EditorGUILayout.BeginHorizontal();
		_target.m_type = (kProgressBarType)EditorGUILayout.EnumPopup("Progress Bar Type", _target.m_type);
		EditorGUILayout.EndHorizontal();

		switch (_target.m_type)
		{
		case kProgressBarType.Mesh:
		case kProgressBarType.CircularMesh:
			EditorGUILayout.BeginHorizontal();
			_target.m_barWidth = EditorGUILayout.FloatField("Bar Width", _target.m_barWidth);
			EditorGUILayout.EndHorizontal();

			EditorGUILayout.BeginHorizontal();
			_target.m_roundEndPoints = EditorGUILayout.IntField("Round End Points", _target.m_roundEndPoints);
			EditorGUILayout.EndHorizontal();

			EditorGUILayout.BeginHorizontal();
			_target.m_startColor = EditorGUILayout.ColorField("Start Color", _target.m_startColor);
			EditorGUILayout.EndHorizontal();

			EditorGUILayout.BeginHorizontal();
			_target.m_endColor = EditorGUILayout.ColorField("End Color", _target.m_endColor);
			EditorGUILayout.EndHorizontal();

			if (_target.m_type == kProgressBarType.Mesh) {
				EditorGUILayout.BeginHorizontal();
				_target.m_startPos = EditorGUILayout.Vector3Field("Start Pos", _target.m_startPos);
				EditorGUILayout.EndHorizontal();

				EditorGUILayout.BeginHorizontal();
				_target.m_endPos = EditorGUILayout.Vector3Field("End Pos", _target.m_endPos);
				EditorGUILayout.EndHorizontal();
			} else {
				EditorGUILayout.BeginHorizontal();
				_target.m_barRadius = EditorGUILayout.FloatField("Bar Radius", _target.m_barRadius);
				EditorGUILayout.EndHorizontal();

				EditorGUILayout.BeginHorizontal();
				_target.m_startAngle = EditorGUILayout.FloatField("Start Angle", _target.m_startAngle);
				EditorGUILayout.EndHorizontal();

				EditorGUILayout.BeginHorizontal();
				_target.m_endAngle = EditorGUILayout.FloatField("End Angle", _target.m_endAngle);
				EditorGUILayout.EndHorizontal();

				EditorGUILayout.BeginHorizontal();
				_target.m_angleStep = EditorGUILayout.FloatField("Angle Step", _target.m_angleStep);
				EditorGUILayout.EndHorizontal();
			}
			break;
		case kProgressBarType.Sprite:
			EditorGUILayout.BeginHorizontal();
			_target.m_progressFrame = (kSpriteObject)EditorGUILayout.ObjectField("Progress Frame", _target.m_progressFrame, typeof(kSpriteObject), true);
			EditorGUILayout.EndHorizontal();
			break;
		}

		if (GUI.changed)
			EditorUtility.SetDirty(_target);
	}
}