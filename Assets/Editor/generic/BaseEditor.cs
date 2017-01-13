using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System;
using System.ComponentModel;
using System.Text.RegularExpressions;

#pragma warning disable

public class BaseEditor : Editor 
{
	protected object targetObj;

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
		Undo.RecordObject(target, target.name);

		targetObj = (object) target;
	 	
		//DrawDefaultInspector ();
		
	   	EditorGUIUtility.LookLikeInspector ();
		
		DrawCustomInspectors(targetObj);

		DrawInspectorStyleGUI();

		if(GUI.changed)
			EditorUtility.SetDirty(target);
	}

	protected virtual void DrawInspectorStyleGUI(){}

	protected virtual void DrawCustomInspectors(object target)
	{
		Type type = target.GetType();//typeof(target);   
		FieldInfo[] fields = type.GetFields();
		//for(int i = fields.Length - 1; i >= 0; i--)
		for(int i = 0; i < fields.Length; i++)
	   	{
			FieldInfo field = fields[i];
			bool hideInInspector = false;
			object[] myAttributes = field.GetCustomAttributes(true);

			for(int j = 0; j < myAttributes.Length && !hideInInspector; j++)
				if(myAttributes[j].GetType().Name == "HideInInspector")
					hideInInspector = true;
			
			if(field.IsPublic/*&& !field.IsNotSerialized*/ && !field.IsLiteral && !hideInInspector)
		    {
				EditorGUIFor(field,target);
			}
		}
	}
	
	protected virtual void FieldValueChanged(FieldInfo field,System.Object target){}
	
	//should be overriden for custom field editor
	protected virtual void EditorGUIFor(FieldInfo field,System.Object target) //where T : new()
	{
		GUILayoutOption[] emptyOptions = new GUILayoutOption[0];
		
		if(field.FieldType != typeof(System.Action) && target != null){
			object oldValue = field.GetValue(target);
			object newValue = EditorGUIFor(MakeLabel(field),oldValue,field.FieldType,target,this);
			field.SetValue(target, newValue);
			if(!Equals(oldValue,newValue,field.FieldType)) FieldValueChanged(field,target);
		}
	}
	
	public static object EditorGUIFor(string label,object obj,Type objType,System.Object targetObject = null,BaseEditor caller = null) //where T : new()
	{
		GUILayoutOption[] emptyOptions = new GUILayoutOption[0];
		
		if(objType == typeof(bool)){
			return EditorGUILayout.Toggle(label,(bool)obj);
		}else 
		if(objType == typeof(int)){
			return EditorGUILayout.IntField(label, (int)obj);
	  	}else
		if(objType == typeof(uint)){
			return (uint)EditorGUILayout.IntField(label, (int)obj);
		}else
		if(objType == typeof(float)){
			return EditorGUILayout.FloatField(label,(float)obj);
		}else 
		if(objType == typeof(string)){
			return EditorGUILayout.TextField(label,(string)obj);
	  	}else
		if(objType == typeof(Enum) || objType.IsSubclassOf(typeof(Enum))){
			return EditorGUILayout.EnumPopup(label,(Enum)obj);
		}else
		if(objType == typeof(Color)){
			return CustomColorField(label, (Color)obj);
		}else
		if(objType == typeof(Vector2)){
			return EditorGUILayout.Vector2Field(label,(Vector2)obj,emptyOptions);
		}else
		if(objType == typeof(Vector3)){
			return EditorGUILayout.Vector3Field(label,(Vector3)obj,emptyOptions);
		}else
		if(objType == typeof(Vector4)){
			return EditorGUILayout.Vector4Field(label,(Vector4)obj,emptyOptions);
		}else
		if(objType == typeof(Rect)){
			return EditorGUILayout.RectField(label,(Rect)obj,emptyOptions);
		}///etc. for other primitive types
 		else if(objType.IsSubclassOf(typeof(UnityEngine.Object))){
			return EditorGUILayout.ObjectField(label,(UnityEngine.Object)obj,objType,emptyOptions);
		}else if(objType == typeof(System.Action)){
       	/*{	Type[] parmTypes = new Type[]{ field.FieldType};
 			string methodName = "DrawDefaultInspector";
 			MethodInfo drawMethod = field.FieldType.GetMethod(methodName);
 			if(drawMethod == null)
               Debug.LogError("No method found: " + methodName +" on filed type: " + field.FieldType);
            bool foldOut = true;
 	       drawMethod.MakeGenericMethod(parmTypes).Invoke(null,new object[]{MakeLabel(field),field.GetValue(target)});}*/ 
		}else if(objType == typeof(kSpriteItem)){
			kSpriteItem gRes = (kSpriteItem)obj;
			
			kSpriteAsset sourceSprite = null;
			//search for source sprite
			if(targetObject != null){
				Type type = targetObject.GetType(); 
				FieldInfo[] fields = type.GetFields();
			   	for(int i = 0; i < fields.Length && sourceSprite == null;i++){
					FieldInfo field1 = fields[i];
					if(field1.FieldType == typeof(kSpriteAsset))
						sourceSprite = (kSpriteAsset)field1.GetValue(targetObject);
				}
			}

			return SpriteItemField(label, gRes, sourceSprite, targetObject);

			/*if(sourceSprite != null && sourceSprite.isLoaded() && gRes != null){
				validateSpriteItem(label,sourceSprite,gRes,(MonoBehaviour)targetObject);
			
				//todo check is the graphic id is the same object as the one detected by name and type and update item id if not
				EditorGUILayout.BeginHorizontal();
				int gTypeIndex 	= BaseItemData.dispatchType((uint)gRes.id) == BaseItemData.FLAG_TYPE_SEQUENCE ? 1 : 0;
				int gIndex 		= sourceSprite.getNameIndex(gRes.id);
							
				string[] types 	= null;
				if(sourceSprite.getGraphicResNames((int)BaseItemData.FLAG_TYPE_SEQUENCE).Length > 0)
					types = new string[]{"Frame","Sequence"};
				else 
					types = new string[]{"Frame"};
				int newTypeIndex= EditorGUILayout.Popup(label,gTypeIndex, types);
				//type changed reset graphic index
				if(newTypeIndex != gTypeIndex)	
					gIndex = 0;
				gRes.type =(int)(newTypeIndex == 0 ? BaseItemData.FLAG_TYPE_FRAME : BaseItemData.FLAG_TYPE_SEQUENCE);
							
				gIndex = EditorGUILayout.Popup(gIndex, sourceSprite.getGraphicResNames(gRes.type));
				gRes.name = sourceSprite.getGraphicResNames(gRes.type)[gIndex];
						
				int gID = sourceSprite.getGraphicResID(gRes.type,gIndex);
						
				if(gRes.id != gID){
					Debug.Log("graphics changed " + label +" ,old id:"+gRes.id.ToString("x")+" "+gID.ToString("x"));
					gRes = new kSpriteItem(gID,gRes.type,gRes.name);
				}
						
				EditorGUILayout.EndHorizontal();
			}else if(sourceSprite == null || !sourceSprite.isLoaded()){
				EditorGUILayout.LabelField(" WARNING: Sprite "+(sourceSprite != null ? sourceSprite.name +" not loaded." : " is null"));
			}
			return gRes;*/
		}else if(objType.IsArray){
			//EditorGUI.indentLevel--;
			if(!arraysFold.ContainsKey(label))arraysFold.Add(label,false);
			arraysFold[label] = EditorGUILayout.Foldout(arraysFold[label],label);
			
			Array array = (Array)obj;
			if(arraysFold[label]){
				EditorGUI.indentLevel++;
				Type elemType = objType.GetElementType();
				
				if (array == null)
					array = Array.CreateInstance(elemType, 0);
				else {
					int size = EditorGUILayout.IntField("Size", array.Length);
					if (size != array.Length) {
						Array nArray = Array.CreateInstance(elemType, size);
						int length = Math.Min(array.Length, size);
	      				Array.ConstrainedCopy(array, 0, nArray, 0, length);
						array = nArray;
					}
				}
				//EditorGUI.indentLevel++;
				for (int i = 0; i < array.Length; i++) {
					object element = array.GetValue(i);
					array.SetValue(EditorGUIFor("Element " + i, element, elemType, targetObject,caller), i);
				}
				//EditorGUI.indentLevel--;
				EditorGUI.indentLevel--;
			}
			//EditorGUI.indentLevel++;
			return array;
		}else if(objType.IsGenericType 
		&& objType.GetGenericTypeDefinition() == typeof(List<>)){
			//EditorGUI.indentLevel--;
			
			Type elemType = objType.GetGenericArguments()[0];
			IList list = (IList)obj;
			
			if(!arraysFold.ContainsKey(label))arraysFold.Add(label,false);
			arraysFold[label] = EditorGUILayout.Foldout(arraysFold[label],label + (list != null ? "  Size: "+list.Count : ""));
			
			if(arraysFold[label]){
				EditorGUI.indentLevel++;
				
				if (list == null)
					list = new List<object>();
				
				//EditorGUI.indentLevel++;
				for (int i = 0; i < list.Count; i++) {
					object element = list[i];
					EditorGUILayout.BeginHorizontal();
					list[i] = EditorGUIFor("Element " + i, element, elemType, targetObject);
					if(GUILayout.Button("Remove", GUILayout.MaxWidth(80))) {
			        	list.RemoveAt(i);
						return list;
			        }
					EditorGUILayout.EndHorizontal();
				}
				GUILayout.BeginHorizontal();
				GUILayout.FlexibleSpace();
				if(GUILayout.Button("Add", GUILayout.MaxWidth(160))) {
			    	list.Add(Activator.CreateInstance(elemType));
			  	}
				GUILayout.FlexibleSpace();
				GUILayout.EndHorizontal();
				//EditorGUI.indentLevel--;
				EditorGUI.indentLevel--;
			}
			//EditorGUI.indentLevel++;
			return list;
		}else
		if(objType.IsSubclassOf(typeof(System.Object))){
			if(objType.IsSerializable){
				if(!arraysFold.ContainsKey(label))arraysFold.Add(label,false);
				arraysFold[label] = EditorGUILayout.Foldout(arraysFold[label],label);				
				if(arraysFold[label]){
					EditorGUI.indentLevel++;

					FieldInfo[] fields = objType.GetFields();
					for (int i = 0; i < fields.Length; i++)
					{
						FieldInfo field = fields[i];
						bool hideInInspector = false;
						object[] myAttributes = field.GetCustomAttributes(true);
						
						for(int j = 0; j < myAttributes.Length && !hideInInspector; j++){
							if(myAttributes[j].GetType().Name == "HideInInspector")
								hideInInspector = true;
						}
						
						if (field.IsPublic/*&& !field.IsNotSerialized*/ && !field.IsLiteral && !hideInInspector)
						{
							caller.EditorGUIFor(field,obj);
						}
					}
					EditorGUI.indentLevel--;
				}
			}
			return obj;
		}else
		{
			//Debug.LogError(
			//	"DrawDefaultInspectors does not support fields of type " + objType);
		}
		return obj;
	}
	static Dictionary<string, bool> arraysFold = new Dictionary<string, bool>();
	
	protected bool Equals(object oldVal,object newVal,Type objType) //where T : new()
	{
		if(objType == typeof(bool)){
			return (bool)oldVal == (bool)newVal;
		}else 
		if(objType == typeof(int)){
			return (int)oldVal == (int)newVal;
	  	}else
		if(objType == typeof(uint)){
			return (uint)oldVal == (uint)newVal;
		}else
		if(objType == typeof(float)){
			return (float)oldVal == (float)newVal;
		}else 
		if(objType == typeof(string)){
			return (string)oldVal == (string)newVal;
	  	}else
		if(objType == typeof(Enum) || objType.IsSubclassOf(typeof(Enum))){
			return (Enum)oldVal == (Enum)newVal;
		}else
		if(objType == typeof(Color)){
			return (Color)oldVal == (Color)newVal;
		}else
		if(objType == typeof(Vector2)){
			return (Vector2)oldVal == (Vector2)newVal;
		}else
		if(objType == typeof(Vector3)){
			return (Vector3)oldVal == (Vector3)newVal;
		}else
		if(objType == typeof(Vector4)){
			return (Vector4)oldVal == (Vector4)newVal;
		}else
		if(objType == typeof(Rect)){
			return (Rect)oldVal == (Rect)newVal;
		}///etc. for other primitive types
 		else if(objType.IsSubclassOf(typeof(object))){
			return oldVal == newVal;
		}else if(objType == typeof(System.Action)){
      		 return false;
		}else if(objType.IsArray)
		{
			return false;
		}
		return true;
	}
	
	public static bool validateSpriteItem(string field,kSpriteAsset sourceSprite,kSpriteItem gRes,MonoBehaviour targetObject = null)
	{
		if(Application.isPlaying)
			return false;

		if(targetObject == null) return false;
		
		if(gRes.name == null || gRes.name.Length == 0){
			Debug.Log("Null res name:"+targetObject.gameObject);
			return false;
		}
		//todo: optimize
		BaseItemData gData = sourceSprite.getItemByName((uint)gRes.type,gRes.name);
		BaseItemData gData1 = sourceSprite.get((uint)gRes.id);

		if( gData != gData1)
		{
			if(gData == null){
				if(gData1 == null)
					gData1 = sourceSprite.get(0x0300000);

				if(gData1 != null)
				{
					Debug.LogWarning(	"Can't find "+(gRes.type == BaseItemData.FLAG_TYPE_FRAME? "frame ":"sequence ")+ gRes.name + "("+gRes.id.ToString("x")+")"+
					               " referenced by "+ targetObject.gameObject.name+"."+field+".\n Reference updated to use "+
					               (gData1.getType() == BaseItemData.FLAG_TYPE_FRAME ? "frame ": "sequence ")+gData1.m_name);
					gRes.type = (int)gData1.getType();
					gRes.id = (int)gData1.getID();
					gRes.name = gData1.m_name;
					return true;
				}
			}else{
				if(gData1 == null){
					Debug.LogWarning("Updated object "+targetObject.name+" graphic id for "+(gRes.type == BaseItemData.FLAG_TYPE_FRAME? "frame ":"sequence ")+ gRes.name + "(oldId:"+gRes.id.ToString("x")+",newId:"+gData.getID()+")");
					gRes.id = (int)gData.getID();
					return true;
				}else{
					Debug.LogWarning("Updated "+targetObject.name+" graphic id? for "+(gRes.type == BaseItemData.FLAG_TYPE_FRAME? "frame ":"sequence ")+ gRes.name + "(oldId:"+gRes.id.ToString("x")+",newId:"+gData.getID()+")");
					gRes.id = (int)gData.getID();
					return true;
				}
			}
		}
		return false;
	}
	
	protected static string MakeLabel(FieldInfo field)
	{
   		GUIContent guiContent = new GUIContent();      
		guiContent.text = SplitCamelCase(field.Name);      
	   
		object[] descriptions = 
	      field.GetCustomAttributes(typeof(DescriptionAttribute), true);
	 
	   	if(descriptions.Length > 0)
	   	{	//just use the first one.
	    	guiContent.tooltip = (descriptions[0] as DescriptionAttribute).Description;
	   	}
	   	return guiContent.text;
	}
	
	public static string SplitCamelCase( string str )
	{
		if(str.StartsWith("m_"))str = str.Substring(2,str.Length - 2);
		string newStr 	= Regex.Replace( Regex.Replace( str, @"(\P{Ll})(\P{Ll}\p{Ll})", "$1 $2" ), @"(\p{Ll})(\P{Ll})", "$1 $2" );
		char[] array = newStr.ToCharArray();
		// Handle the first letter in the string.
		if (array.Length >= 1)
		{
		    if (char.IsLower(array[0]))
		    {
				array[0] = char.ToUpper(array[0]);
		    }
		}
		// Scan through the letters, checking for spaces.
		// ... Uppercase the lowercase letters following spaces.
		for (int i = 1; i < array.Length; i++)
		{
		    if (array[i - 1] == ' ')
		    {
				if (char.IsLower(array[i]))
				{
				    array[i] = char.ToUpper(array[i]);
				}
		    }
		}
		return new string(array);
	}
	
	public static kSpriteItem SpriteItemField(string label, kSpriteItem gRes, kSpriteAsset sourceSprite, System.Object targetObject)
	{
		Color outColor;
		return SpriteItemFieldWithColor(label, gRes, sourceSprite, targetObject, Color.white, out outColor, false);
	}
	
	public static kSpriteItem SpriteItemFieldWithColor(string label, kSpriteItem gRes, kSpriteAsset sourceSprite, System.Object targetObject, Color inColor, out Color outColor, bool useColor)
	{
		outColor = Color.white;
		if(sourceSprite != null && sourceSprite.isLoaded() && gRes != null){
			bool autoCorrected= validateSpriteItem(label,sourceSprite,gRes,(MonoBehaviour)targetObject);
			
			//todo check is the graphic id is the same object as the one detected by name and type and update item id if not
			EditorGUILayout.BeginHorizontal();
			int gTypeIndex 	= BaseItemData.dispatchType((uint)gRes.id) == BaseItemData.FLAG_TYPE_SEQUENCE ? 1 : 0;
			int gIndex 		= sourceSprite.getNameIndex(gRes.id);
			
			string[] types 	= null;
			if(sourceSprite.getGraphicResNames((int)BaseItemData.FLAG_TYPE_SEQUENCE).Length > 0)
				types = new string[]{"Frame","Sequence"};
			else 
				types = new string[]{"Frame"};
			int newTypeIndex= EditorGUILayout.Popup(label,gTypeIndex, types);
			//type changed reset graphic index
			if(newTypeIndex != gTypeIndex)	
				gIndex = 0;
			gRes.type =(int)(newTypeIndex == 0 ? BaseItemData.FLAG_TYPE_FRAME : BaseItemData.FLAG_TYPE_SEQUENCE);
			
			gIndex = EditorGUILayout.Popup(gIndex, sourceSprite.getGraphicResNames(gRes.type));
			gRes.name = sourceSprite.getGraphicResNames(gRes.type)[gIndex];
			
			int gID = sourceSprite.getGraphicResID(gRes.type,gIndex);
			
			if(gRes.id != gID || autoCorrected){
				Debug.Log("graphics changed for: " + label + ", old id: " + gRes.id.ToString("x") + ", new id: " + gID.ToString("x"));
				gRes = new kSpriteItem(gID,gRes.type,gRes.name);
			}
			
			if (useColor) {
				outColor = EditorGUILayout.ColorField(inColor, GUILayout.Width(75));
			}
			
			EditorGUILayout.EndHorizontal();
		}else if(sourceSprite == null || !sourceSprite.isLoaded()){
			EditorGUILayout.LabelField(" WARNING: Sprite "+(sourceSprite != null ? sourceSprite.name +" not loaded." : " is null"));
		}
		return gRes;
	}
	
	public static Color CustomColorField(string label, Color color)
	{
		EditorGUILayout.BeginHorizontal();
		color = EditorGUILayout.ColorField(label, color);
		string previousHex = ColorToHex(color);
		string hex = EditorGUILayout.TextField(previousHex, GUILayout.Width(65));
		if (hex != previousHex) {
			color = HexToColor(hex);
		}
		EditorGUILayout.EndHorizontal();
		return color;
	}
	
	private static string ColorToHex(Color32 color)
	{
		return color.r.ToString("X2") + color.g.ToString("X2") + color.b.ToString("X2");
	}
	
	private static Color32 HexToColor(string hex)
	{
		if (!string.IsNullOrEmpty(hex) && hex.Length == 6) {
			byte r, g, b;
			if (byte.TryParse(hex.Substring(0, 2), System.Globalization.NumberStyles.HexNumber, System.Globalization.CultureInfo.CurrentCulture, out r) &&
			    byte.TryParse(hex.Substring(2, 2), System.Globalization.NumberStyles.HexNumber, System.Globalization.CultureInfo.CurrentCulture, out g) &&
			    byte.TryParse(hex.Substring(4, 2), System.Globalization.NumberStyles.HexNumber, System.Globalization.CultureInfo.CurrentCulture, out b)) {
				return new Color32(r, g, b, 255);
			}
		}
		return Color.black;
	}
}
