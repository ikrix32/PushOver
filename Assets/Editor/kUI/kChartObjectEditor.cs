using UnityEngine;
using UnityEditor;
using System.Collections;

#pragma warning disable

[CustomEditor(typeof(kChartObject))]
public class kChartObjectEditor : Editor
{
	kChartObject _target;
	GUIStyle style = new GUIStyle();
	
	void OnEnable()
	{
		style.fontStyle = FontStyle.Bold;
		style.normal.textColor = Color.white;
		_target = (kChartObject)target;
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
		EditorGUIUtility.LookLikeInspector();
		DrawDefaultInspector();
		
		EditorGUILayout.BeginHorizontal();
		kChartObject.ChartType chartType = (kChartObject.ChartType)EditorGUILayout.EnumPopup("Chart Type", _target.m_chartType);
		if (chartType != _target.m_chartType) {
			_target.m_chartType = chartType;
			if (_target.m_chartType == kChartObject.ChartType.LINE) {
				_target.m_numberOfBars = 1; // LINE chart has one single BAR
			}
			else if (_target.m_chartType == kChartObject.ChartType.BAR) {
				_target.m_numberOfBars = 30; // 30 is the default number of bars for BAR
			}
			_target.resetChart();
			_target.loadChartObject();
		}
		EditorGUILayout.EndHorizontal();
		
		EditorGUILayout.BeginHorizontal();
		int numberOfBars = EditorGUILayout.IntField("Number of Bars", _target.m_numberOfBars);
		if (numberOfBars != _target.m_numberOfBars) {
			_target.m_numberOfBars = numberOfBars;
			_target.resetChart();
			_target.loadChartObject();
		}
		EditorGUILayout.EndHorizontal();
		
		EditorGUILayout.BeginHorizontal();
		float barHeight = EditorGUILayout.FloatField("Chart Height", _target.m_barHeight);
		if (barHeight != _target.m_barHeight) {
			_target.m_barHeight = barHeight;
			_target.resetChart();
			_target.loadChartObject();
		}
		EditorGUILayout.EndHorizontal();
		
		EditorGUILayout.BeginHorizontal();
		float chartWidth = EditorGUILayout.FloatField("Chart Width", _target.m_chartWidth);
		if (chartWidth != _target.m_chartWidth) {
			_target.m_chartWidth = chartWidth;
			_target.resetChart();
			_target.loadChartObject();
		}
		EditorGUILayout.EndHorizontal();
		
		EditorGUILayout.BeginHorizontal();
		float barSpacing = EditorGUILayout.FloatField("Bar Spacing", _target.m_barSpacing);
		if (barSpacing != _target.m_barSpacing) {
			_target.m_barSpacing = barSpacing;
			_target.resetChart();
			_target.loadChartObject();
		}
		EditorGUILayout.EndHorizontal();
		
		EditorGUILayout.BeginHorizontal();
		int numberOfDivsForLINE = EditorGUILayout.IntField("Number of Dividers for LINE", _target.m_numOfDivsForLINE);
		if (numberOfDivsForLINE != _target.m_numOfDivsForLINE) {
			_target.m_numOfDivsForLINE = numberOfDivsForLINE;
			_target.resetChart();
			_target.loadChartObject();
		}
		EditorGUILayout.EndHorizontal();
	}
}
