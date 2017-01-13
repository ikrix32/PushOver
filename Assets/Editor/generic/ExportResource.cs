using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

public class ExportResource
{
	[MenuItem("Assets/Build AssetBundles - iOS")]
	static void ExportResourceTrack()
	{
		/*var path = EditorUtility.SaveFilePanel("Save Resource", "AssetBundles", Selection.activeObject.name, "unity3d");
		if (path.Length != 0)
		{
			//Debug.Log("Building AssetBundle for object " + Selection.activeObject.name);
			var selection = Selection.GetFiltered(typeof(Object), SelectionMode.DeepAssets);
			BuildPipeline.BuildAssetBundle(Selection.activeObject, selection, path, BuildAssetBundleOptions.CollectDependencies | BuildAssetBundleOptions.CompleteAssets, BuildTarget.iOS);
			Selection.objects = selection;
		}*/
		AssetBundleBuild[] buildMap = new AssetBundleBuild[Selection.objects.Length];
		for (int i = 0; i < buildMap.Length; ++i) {
			buildMap[i].assetNames = new string[1];
			buildMap[i].assetNames[0] = AssetDatabase.GetAssetPath(Selection.instanceIDs[i]);
			buildMap[i].assetBundleName = Selection.objects[i].name + ".unity3d";
		}
		BuildPipeline.BuildAssetBundles("AssetBundles", buildMap, BuildAssetBundleOptions.None, BuildTarget.iOS);
	}
/*
	[MenuItem("Assets/Build AssetBundle From Selection - No dependency tracking")]
	static void ExportResourceNoTrack()
	{
		var path = EditorUtility.SaveFilePanel("Save Resource", "AssetBundles", Selection.activeObject.name, "unity3d");
		if (path.Length != 0)
		{
			//Debug.Log("Building AssetBundle for object " + Selection.activeObject.name);
			BuildPipeline.BuildAssetBundle(Selection.activeObject, Selection.objects, path);
		}
	}
	
	[MenuItem("Assets/Export All Downloadable Scenes")]
	static void ExportDownloadableScenes()
	{
		BuildTarget[] targets = new BuildTarget[]{BuildTarget.Android};//, BuildTarget.iPhone};
		string[] scenes = new string[]{"/Scenes/App/kDashboard", "/Scenes/App/kRegistration"};
		foreach (var target in targets)
			foreach (var scene in scenes)
			{
				string exportPath = "AssetBundles/" + target.ToString() + "/";
				if (!System.IO.Directory.Exists(exportPath))
					System.IO.Directory.CreateDirectory(exportPath);
				Debug.Log("Building " + target.ToString() + " AssetBundle for scene " + scene);
				BuildPipeline.BuildPlayer(new string[]{"Assets" + scene + ".unity"}, exportPath + System.IO.Path.GetFileName(scene) + ".unity3d", target, BuildOptions.BuildAdditionalStreamedScenes);
			}
	}
*/
	private static void ExportScene(BuildTarget target)
	{
		var scene = System.IO.Path.GetFileNameWithoutExtension(EditorApplication.currentScene);
		var defaultPath = "AssetBundles/" + target.ToString();
		if (!System.IO.Directory.Exists(defaultPath))
			System.IO.Directory.CreateDirectory(defaultPath);
		var path = EditorUtility.SaveFilePanel("Save Resource", defaultPath, scene, "unity3d");
		if (path.Length != 0)
		{
			Debug.Log("Building " + target.ToString() + " AssetBundle for scene " + scene);
			BuildPipeline.BuildPlayer(new string[]{EditorApplication.currentScene}, path, target, BuildOptions.BuildAdditionalStreamedScenes);
		}
	}
	
	[MenuItem("Assets/Export Current Scene - Android")]
	static void ExportScene_Android()
	{
		ExportScene(BuildTarget.Android);
	}
	
	[MenuItem("Assets/Export Current Scene - iOS")]
	static void ExportScene_iOS()
	{
		ExportScene(BuildTarget.iOS);
	}
/*
	private static void ExportMission1Bundle(BuildTarget target)
	{
		string[] scenes = new string[]{
			"Assets/Scenes/Game/mission1/m1_scene1.unity", "Assets/Scenes/Game/mission1/m1_scene2.unity",
			"Assets/Scenes/Game/mission1/m1_scene3.unity", "Assets/Scenes/Game/mission1/m1_scene4.unity",
			"Assets/Scenes/Game/mission1/m1_scene5.unity", "Assets/Scenes/Game/mission1/m1_scene6.unity",
			"Assets/Scenes/Game/mission1/m1_scene7.unity", "Assets/Scenes/Game/mission1/m1_scene8.unity",
			"Assets/Scenes/Game/mission1/map.unity"
		};
		var exportPath = "AssetBundles/" + target.ToString() + "/";
		if (!System.IO.Directory.Exists(exportPath))
			System.IO.Directory.CreateDirectory(exportPath);
		Debug.Log("Building " + target.ToString() + " AssetBundle for mission1...");
		BuildPipeline.BuildStreamedSceneAssetBundle(scenes, exportPath + "map2test.unity3d", target);
	}
	
	[MenuItem("Assets/Export Map 2 Test Bundle - iOS")]
	public static void ExportMission1_iOS()
	{
		ExportMission1Bundle(BuildTarget.iPhone);
	}
	
	[MenuItem("Build/Build Mission 2 Resource Bundle")]
	static void ExportMission2ResBundle()
	{
		string mainPath = "Assets/Resources/events/egypt/";
		string[] resources = new string[] { "C08A.png", "C08B.png", "C08C.png", "C09A.png", "C09B.png", "C09C.png", "C10A.png", "C10B.png", "C10C.png",
			"C11A.png", "C11B.png", "C11C.png", "C12A.png", "C12B.png", "C12C.png", "C13A.png", "C13B.png", "C13C.png", "C14.png" };
		Texture2D[] textures = new Texture2D[resources.Length];
		for (int i = 0; i < resources.Length; ++i) {
			textures[i] = (Texture2D)Resources.LoadAssetAtPath(mainPath + resources[i], typeof(Texture2D));
			if (textures[i] == null) {
				Debug.LogWarning(" * " + mainPath + resources[i] + " not found!");
			}
		}
		BuildPipeline.BuildAssetBundle(textures[0], textures, "AssetBundles/m2resBundle.unity3d", BuildAssetBundleOptions.CompleteAssets, BuildTarget.iPhone);
	}
*/

	//[MenuItem("IL Utils/Clean Animations")]
	static void CleanAnimations()
	{
		foreach (Object obj in Selection.objects) {
			if (obj is AnimationClip) {
				CleanAnim(obj as AnimationClip);
			}
		}
	}

	static void CleanAnim(AnimationClip clip)
	{
		Debug.LogWarning(" * clean anim: " + clip.name);
		foreach (EditorCurveBinding curveBinding in AnimationUtility.GetObjectReferenceCurveBindings(clip)) {
			string name = curveBinding.propertyName.ToLower();
			if (name.Contains("sprite")) {
				ObjectReferenceKeyframe[] keyFrames = AnimationUtility.GetObjectReferenceCurve(clip, curveBinding);
				List<ObjectReferenceKeyframe> newKeyFrames = new List<ObjectReferenceKeyframe>();
				newKeyFrames.Add(keyFrames[0]);
				for (int i = 1; i < keyFrames.Length; ++i) {
					if (keyFrames[i].value == null && keyFrames[i - 1].value == null)
						continue;
					if (keyFrames[i].value == null || keyFrames[i - 1].value == null ||
						keyFrames[i].value.ToString() != keyFrames[i - 1].value.ToString()) {
						newKeyFrames.Add(keyFrames[i]);
					}
				}
				AnimationUtility.SetObjectReferenceCurve(clip, curveBinding, newKeyFrames.ToArray());
			}
		}
		foreach (EditorCurveBinding curveBinding in AnimationUtility.GetCurveBindings(clip)) {
			string name = curveBinding.propertyName.ToLower();
			if (name.Contains("position")) {
				AnimationCurve curve = AnimationUtility.GetEditorCurve(clip, curveBinding);
				AnimationCurve newCurve = new AnimationCurve();
				for (int i = 0; i < curve.keys.Length; ++i) {
					if (i == 0 || i == curve.keys.Length - 1 ||
					    (name != "m_localposition.z" && KeyframeHasSignificantChanges(curve, i, 0.01f))) {
						newCurve.AddKey(new Keyframe(curve.keys[i].time, curve.keys[i].value));
					}
				}
				AnimationUtility.SetEditorCurve(clip, curveBinding, newCurve);
			} else if (name.Contains("rotation")) {
				/*AnimationCurve curve = AnimationUtility.GetEditorCurve(clip, curveBinding);
				AnimationCurve newCurve = new AnimationCurve();
				for (int i = 0; i < curve.keys.Length; ++i) {
					if (i == 0 || i == curve.keys.Length - 1 ||
					    (name == "m_localrotation.z" && KeyframeHasSignificantChanges(curve, i, 0.0001f))) {
						newCurve.AddKey(new Keyframe(curve.keys[i].time, curve.keys[i].value));
					}
				}
				AnimationUtility.SetEditorCurve(clip, curveBinding, newCurve);*/
			} else if (name.Contains("scale") || name.Contains("isactive")) {
				AnimationUtility.SetEditorCurve(clip, curveBinding, null);
			}
		}
	}

	static bool KeyframeHasSignificantChanges(AnimationCurve curve, int keyIndex, float epsilon)
	{
		return Mathf.Abs(curve.keys[keyIndex].value - curve.keys[keyIndex - 1].value) > epsilon ||
			Mathf.Abs(curve.keys[keyIndex].value - curve.keys[keyIndex + 1].value) > epsilon;
	}
}
