using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

[InitializeOnLoad]
public static class SceneHierarchyBootstrap
{
    static SceneHierarchyBootstrap()
    {
        EditorApplication.delayCall += EnsureBootstrapInActiveScene;
        EditorSceneManager.sceneOpened += OnSceneOpened;
        EditorSceneManager.newSceneCreated += OnNewSceneCreated;
    }

    static void OnSceneOpened(Scene scene, OpenSceneMode mode)
    {
        EnsureBootstrapInScene(scene);
    }

    static void OnNewSceneCreated(Scene scene, NewSceneSetup setup, NewSceneMode mode)
    {
        EnsureBootstrapInScene(scene);
    }

    static void EnsureBootstrapInActiveScene()
    {
        EnsureBootstrapInScene(SceneManager.GetActiveScene());
    }

    static void EnsureBootstrapInScene(Scene scene)
    {
        if (!scene.IsValid() || !scene.isLoaded)
            return;

        if (scene.path.StartsWith("Packages/"))
            return;

        if (EditorApplication.isPlayingOrWillChangePlaymode)
            return;

        GameObject existingRoot = GameObject.Find("SceneSetupRoot");
        if (existingRoot == null)
        {
            existingRoot = new GameObject("SceneSetupRoot");
            SceneManager.MoveGameObjectToScene(existingRoot, scene);
        }

        RuntimeSceneBuilder builder = existingRoot.GetComponent<RuntimeSceneBuilder>();
        if (builder == null)
            builder = existingRoot.AddComponent<RuntimeSceneBuilder>();

        builder.EditorBuildScene();

        if (!Application.isPlaying)
            EditorSceneManager.MarkSceneDirty(scene);
    }
}
