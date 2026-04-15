using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SceneSetup : MonoBehaviour
{
    public Material defaultOrganMaterial;
    public Vector3[] organPositions = new Vector3[]
    {
        new Vector3(-1.2f, 1.0f, 1.5f),
        new Vector3(0.0f, 1.0f, 1.5f),
        new Vector3(1.2f, 1.0f, 1.5f),
        new Vector3(2.4f, 1.0f, 1.5f)
    };

    void Start()
    {
        SetupScene();
    }

    void SetupScene()
    {
        SetupCamera();
        SetupLighting();
        SetupManagers();
        SetupUI();
        SetupOrgans();
        SetupEditorSimulator();
        SetupXR();
    }

    void SetupCamera()
    {
        if (Camera.main == null)
        {
            GameObject cameraGO = new GameObject("Main Camera");
            Camera camera = cameraGO.AddComponent<Camera>();
            cameraGO.tag = "MainCamera";
            cameraGO.transform.position = new Vector3(0, 1.6f, -3.0f);
            cameraGO.transform.rotation = Quaternion.Euler(10f, 0f, 0f);
            cameraGO.AddComponent<AudioListener>();
        }

        if (Camera.main != null && Camera.main.GetComponent<EditorInputSimulator>() == null)
            Camera.main.gameObject.AddComponent<EditorInputSimulator>();
    }

    void SetupLighting()
    {
        if (FindObjectOfType<Light>() == null)
        {
            GameObject lightGO = new GameObject("Directional Light");
            Light light = lightGO.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.2f;
            light.color = Color.white;
            lightGO.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
        }
    }

    void SetupManagers()
    {
        GameObject managers = GameObject.Find("Managers");
        if (managers == null)
            managers = new GameObject("Managers");

        if (FindObjectOfType<HandTrackingManager>() == null)
            managers.AddComponent<HandTrackingManager>();

        if (FindObjectOfType<AnatomySceneController>() == null)
            managers.AddComponent<AnatomySceneController>();

        if (FindObjectOfType<AnatomyUIManager>() == null)
            managers.AddComponent<AnatomyUIManager>();

        if (FindObjectOfType<MedicalModelDownloader>() == null)
            managers.AddComponent<MedicalModelDownloader>();

        if (FindObjectOfType<DissectionController>() == null)
            managers.AddComponent<DissectionController>();
    }

    void SetupUI()
    {
        AnatomyUIManager uiManager = FindObjectOfType<AnatomyUIManager>();
        if (uiManager == null)
            return;

        GameObject canvasGO = new GameObject("UI Canvas");
        Canvas canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasGO.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        canvasGO.AddComponent<GraphicRaycaster>();

        GameObject panelGO = new GameObject("Control Panel");
        panelGO.transform.SetParent(canvasGO.transform, false);
        Image panelImage = panelGO.AddComponent<Image>();
        panelImage.color = new Color(0.08f, 0.08f, 0.08f, 0.88f);

        RectTransform panelRT = panelGO.GetComponent<RectTransform>();
        panelRT.anchorMin = new Vector2(0.02f, 0.02f);
        panelRT.anchorMax = new Vector2(0.34f, 0.42f);
        panelRT.offsetMin = Vector2.zero;
        panelRT.offsetMax = Vector2.zero;

        GameObject statusGO = new GameObject("StatusText");
        statusGO.transform.SetParent(panelGO.transform, false);
        Text statusText = statusGO.AddComponent<Text>();
        statusText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        statusText.fontSize = 20;
        statusText.color = Color.white;
        statusText.alignment = TextAnchor.UpperLeft;
        statusText.horizontalOverflow = HorizontalWrapMode.Wrap;
        statusText.verticalOverflow = VerticalWrapMode.Overflow;
        statusText.text = "ANATOMY CONTROLS\n\n" +
            "👆 Point to Select\n" +
            "✊ Grab to Move\n" +
            "🔄 Rotate to Spin\n" +
            "🤏 Pinch to Zoom In\n" +
            "🖐️ Spread to Zoom Out\n" +
            "✂️ Swipe to Dissect\n" +
            "📍 Pin in Place\n" +
            "🔙 Reset View";

        RectTransform statusRT = statusGO.GetComponent<RectTransform>();
        statusRT.anchorMin = new Vector2(0, 0);
        statusRT.anchorMax = new Vector2(1, 1);
        statusRT.offsetMin = new Vector2(12, 12);
        statusRT.offsetMax = new Vector2(-12, -12);

        uiManager.uiPanel = panelGO;
        uiManager.statusText = statusText;
    }

    void SetupOrgans()
    {
        AnatomySceneController anatomyController = FindObjectOfType<AnatomySceneController>();
        if (anatomyController == null)
            return;

        GameObject organsRoot = GameObject.Find("Organs");
        if (organsRoot == null)
            organsRoot = new GameObject("Organs");

        List<GameObject> organs = new List<GameObject>();
        OrganController.OrganType[] organTypes = new OrganController.OrganType[]
        {
            OrganController.OrganType.Heart,
            OrganController.OrganType.Brain,
            OrganController.OrganType.Lung,
            OrganController.OrganType.Liver
        };

        string[] organNames = new string[] { "Heart", "Brain", "Lungs", "Liver" };

        for (int i = 0; i < organNames.Length; i++)
        {
            GameObject organRoot = GameObject.Find(organNames[i]);
            if (organRoot == null)
            {
                organRoot = new GameObject(organNames[i]);
                organRoot.transform.SetParent(organsRoot.transform);
                organRoot.transform.position = organPositions[i % organPositions.Length];

                var generator = organRoot.AddComponent<ProceduralOrganGenerator>();
                generator.organType = organTypes[i];
                generator.overrideMaterial = defaultOrganMaterial;

                var organController = organRoot.AddComponent<OrganController>();
                organController.organType = organTypes[i];
                organController.organMaterial = defaultOrganMaterial;
            }
            else
            {
                if (organRoot.GetComponent<ProceduralOrganGenerator>() == null)
                {
                    var generator = organRoot.AddComponent<ProceduralOrganGenerator>();
                    generator.organType = organTypes[i];
                    generator.overrideMaterial = defaultOrganMaterial;
                }

                if (organRoot.GetComponent<OrganController>() == null)
                {
                    var organController = organRoot.AddComponent<OrganController>();
                    organController.organType = organTypes[i];
                    organController.organMaterial = defaultOrganMaterial;
                }
            }

            organs.Add(organRoot);
        }

        anatomyController.organs = organs.ToArray();
        anatomyController.LoadOrgan(0);
        FocusCameraOnOrgan(organs[0]);
    }

    void SetupEditorSimulator()
    {
        Camera cam = Camera.main;
        if (cam != null && cam.GetComponent<EditorInputSimulator>() == null)
            cam.gameObject.AddComponent<EditorInputSimulator>();
    }

    void SetupXR()
    {
        // XR support removed for a clean project setup.
    }

    void FocusCameraOnOrgan(GameObject organ)
    {
        if (organ == null || Camera.main == null)
            return;

        Transform cameraTransform = Camera.main.transform;
        Vector3 targetPosition = organ.transform.position + new Vector3(0f, 0.3f, 0f);
        cameraTransform.position = targetPosition + new Vector3(0f, 0.2f, -4.5f);
        cameraTransform.LookAt(targetPosition);
    }
}
