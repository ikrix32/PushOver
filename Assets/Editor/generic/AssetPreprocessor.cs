using UnityEngine;
using UnityEditor;

public class AssetPreprocessor : UnityEditor.AssetModificationProcessor
{
    public static string[] OnWillSaveAssets(string[] paths)
    {
        // Get the name of the scene to save.
       // string scenePath = string.Empty;
        //string sceneName = string.Empty;
 	
        foreach (string path in paths)
        {
            if (path.Contains(".unity"))
            {
				//scenePath = System.IO.Path.GetDirectoryName(path);
                //sceneName = System.IO.Path.GetFileNameWithoutExtension(path);
				GlobalDefinesWizard.BreakPrefabs();
            }
        }
        return paths;
    }
}
