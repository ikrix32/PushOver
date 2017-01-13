using UnityEngine;
using UnityEditor;
using System.Collections;

#pragma warning disable

[CustomEditor(typeof(kPicture))]
public class kPictureEditor : Editor
{
	public kPicture _target;
	
	void OnEnable()
	{
		_target = (kPicture)target;
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
		GUILayoutOption[] emptyOptions = new GUILayoutOption[0];
		//DrawDefaultInspector();
		EditorGUIUtility.LookLikeControls();
		
		EditorGUI.indentLevel = 0;

		bool customSize = EditorGUILayout.Toggle("Use Custom Size",(bool)_target.useCustomSize);
		if(customSize != _target.useCustomSize)
			_target.useCustomSize = customSize;

		if(_target.useCustomSize){
			Vector2 size = EditorGUILayout.Vector2Field("Size",(Vector2)_target.size,emptyOptions);
			if(!size.Equals(_target.size)){
				_target.size = size;
			}
		}

		kAlignMode align = (kAlignMode)EditorGUILayout.EnumPopup("Alignment",_target.alignment);
		if(align != _target.alignment){
			_target.alignment = align;
		}

		Texture texture = (Texture)EditorGUILayout.ObjectField("Texture",(Texture)_target.getTexture(),typeof(Texture),emptyOptions);
		if(texture != _target.getTexture()){
			_target.setTexture(texture,false);
			_target.updateObjectMesh();
		}

		Color color =  EditorGUILayout.ColorField("Blending color",_target.m_blendingColor);
		if(color != _target.m_blendingColor){
			_target.setBlendingColor(color);
		}

		EditorGUILayout.BeginHorizontal();
		Shader sh = (Shader)EditorGUILayout.ObjectField("Custom shader",_target.customShader,typeof(Shader),emptyOptions);
		EditorGUILayout.EndHorizontal();

		if(sh != _target.customShader){
			_target.customShader = sh;
		}
	}
}
