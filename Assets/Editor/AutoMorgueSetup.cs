using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;

[InitializeOnLoad]
public class AutoMorgueSetup
{
    static AutoMorgueSetup()
    {
        EditorApplication.delayCall += DoSetup;
    }

    static void DoSetup()
    {
        if (File.Exists("Assets/LabEnvironment.prefab")) return;
        string scenePath = "Assets/Morgue Room PBR/morgue scene/morgue.unity";
        if (!File.Exists(scenePath)) return;

        try
        {
            Scene morgueScene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Additive);
            GameObject labEnv = new GameObject("LabEnvironment");
            
            GameObject[] roots = morgueScene.GetRootGameObjects();
            foreach(var root in roots)
            {
                // Ignore cameras and directional lights to avoid conflicts with our scene
                if (root.GetComponent<Camera>() != null) continue;
                if (root.GetComponent<Light>() != null && root.GetComponent<Light>().type == LightType.Directional) continue;
                
                // Parent to our new root container
                root.transform.SetParent(labEnv.transform, true);
            }
            
            // Save it out as a reusable prefab
            PrefabUtility.SaveAsPrefabAsset(labEnv, "Assets/LabEnvironment.prefab");
            Object.DestroyImmediate(labEnv);
            
            // Close the morgue scene so it doesn't clutter their current view
            EditorSceneManager.CloseScene(morgueScene, true);
            
            Debug.Log("Successfully extracted Morgue Scene into LabEnvironment.prefab!");
        }
        catch (System.Exception e)
        {
            Debug.LogError("Failed to extract morgue scene: " + e.Message);
        }
    }
}
