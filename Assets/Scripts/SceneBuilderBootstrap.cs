using UnityEngine;

public class SceneBuilderBootstrap : MonoBehaviour
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void BuildScene()
    {
        if (FindObjectOfType<RuntimeSceneBuilder>() == null)
        {
            GameObject bootstrap = new GameObject("SceneSetupBootstrap");
            bootstrap.AddComponent<RuntimeSceneBuilder>();
        }
    }
}
