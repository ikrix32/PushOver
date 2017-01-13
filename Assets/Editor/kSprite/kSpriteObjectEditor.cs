
using UnityEngine;
using UnityEditor;
using System.Collections;
using System;
using System.Reflection;

[CustomEditor(typeof(kSpriteObject))]

[CanEditMultipleObjects]
public class kSpriteObjectEditor : BaseEditor
{
	protected override void FieldValueChanged(FieldInfo field,System.Object target){
		base.FieldValueChanged(field,target);
		if(field.Name == "m_defaultAnim"  && !Application.isPlaying){
			//kSpriteObject obj = (kSpriteObject)target;
			//obj.play(obj.m_defaultAnim.id,PlaybackMode.ANIM_PLAY_LOOP,PlaybackDir.ANIM_PLAY_FW);
			// force refresh for copied objects
			((kSpriteObject)target).EditorRefresh();
		}
	}
}
