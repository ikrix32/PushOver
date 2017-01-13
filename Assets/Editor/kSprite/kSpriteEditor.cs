using UnityEngine;
using UnityEditor;
using System.Collections;
using System;
using System.Reflection;
#if	ENABLE_CREATE_KSPRITE_ASSET
[CustomEditor(typeof(kSprite))]

[CanEditMultipleObjects]
public class kSpriteEditor : BaseEditor
{
	protected override void FieldValueChanged(FieldInfo field, System.Object target)
	{
		base.FieldValueChanged(field,target);
		if (field.Name == "spriteBinary")
		{
			kSprite obj = (kSprite)target;
			obj.loadSprite();
			obj.updateGraphicNames();
		}
	}
}
#endif
