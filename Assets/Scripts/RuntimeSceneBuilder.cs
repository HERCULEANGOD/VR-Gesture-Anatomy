using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class RuntimeSceneBuilder : MonoBehaviour
{
    public Material defaultOrganMaterial;

    void Start()
    {
        BuildScene();
    }

    public void EditorBuildScene()
    {
#if UNITY_EDITOR
        if (Application.isPlaying)
            return;

        BuildScene();
#endif
    }

    void BuildScene()
    {
        // Prevent old legacy SceneSetup script from running and sabotaging the UI / Organs
        GameObject legacySetup = GameObject.Find("SceneSetupRoot");
        if (legacySetup != null)
            SafeDestroy(legacySetup);
            
        SetupCamera();
        SetupLighting();
        SetupManagers();
        SetupLabEnvironment();
        SetupBodyGuide();
        SetupOrgans();
        SetupUI();
    }

    void SetupCamera()
    {
        if (Camera.main == null)
        {
            GameObject cameraGO = new GameObject("Main Camera");
            cameraGO.tag = "MainCamera";

            Camera camera = cameraGO.AddComponent<Camera>();
            camera.fieldOfView = 34f;
            camera.clearFlags = CameraClearFlags.Skybox;

            cameraGO.transform.position = new Vector3(0f, 1.42f, -0.95f);
            cameraGO.transform.rotation = Quaternion.Euler(2f, 0f, 0f);
            cameraGO.AddComponent<AudioListener>();
        }

        if (Camera.main != null && Camera.main.GetComponent<EditorInputSimulator>() == null)
            Camera.main.gameObject.AddComponent<EditorInputSimulator>();
    }

    void SetupLighting()
    {
        if (FindObjectOfType<Light>() != null)
            return;

        GameObject lightGO = new GameObject("Directional Light");
        Light light = lightGO.AddComponent<Light>();
        light.type = LightType.Directional;
        light.intensity = 1.2f;
        light.color = Color.white;
        lightGO.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
    }

    void SetupLabEnvironment()
    {
        GameObject existingLab = GameObject.Find("LabEnvironment");
        if (existingLab != null)
            SafeDestroy(existingLab);

#if UNITY_EDITOR
        // Auto-load the Morgue prefab if the auto-setup script extracted it
        GameObject labAsset = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/LabEnvironment.prefab");
        if (labAsset != null)
        {
            GameObject instantiatedLab = Instantiate(labAsset);
            instantiatedLab.name = "LabEnvironment";
            instantiatedLab.transform.position = Vector3.zero;
            return;
        }
#endif

        GameObject lab = new GameObject("LabEnvironment");

        Material wallMat = new Material(Shader.Find("Standard"));
        wallMat.color = new Color(0.88f, 0.92f, 0.96f);

        Material floorMat = new Material(Shader.Find("Standard"));
        floorMat.color = new Color(0.45f, 0.50f, 0.55f);

        // Floor
        GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Plane);
        floor.name = "Floor";
        floor.transform.SetParent(lab.transform);
        floor.transform.localPosition = new Vector3(0, 0f, 0);
        floor.transform.localScale = new Vector3(3f, 1f, 3f);
        floor.GetComponent<Renderer>().sharedMaterial = floorMat;

        // Back Wall
        GameObject backWall = GameObject.CreatePrimitive(PrimitiveType.Cube);
        backWall.name = "BackWall";
        backWall.transform.SetParent(lab.transform);
        backWall.transform.localPosition = new Vector3(0, 3f, 4f);
        backWall.transform.localScale = new Vector3(30f, 8f, 0.5f);
        backWall.GetComponent<Renderer>().sharedMaterial = wallMat;
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

        if (FindObjectOfType<MedicalModelDownloader>() == null)
            managers.AddComponent<MedicalModelDownloader>();

        if (FindObjectOfType<DissectionController>() == null)
            managers.AddComponent<DissectionController>();

        if (FindObjectOfType<AnatomyLabController>() == null)
            managers.AddComponent<AnatomyLabController>();

        // ✅ AI Foundational Layer
        if (FindObjectOfType<GeminiService>() == null)
            managers.AddComponent<GeminiService>();

        if (FindObjectOfType<AIOrganExpert>() == null)
            managers.AddComponent<AIOrganExpert>();

        if (FindObjectOfType<AIChatController>() == null)
            managers.AddComponent<AIChatController>();

        if (FindObjectOfType<AIDissectionGuide>() == null)
            managers.AddComponent<AIDissectionGuide>();
    }

    void SetupBodyGuide()
{
    GameObject bodyGuide = GameObject.Find("BodyGuide");

    // ✅ DO NOT destroy existing body
    if (bodyGuide == null)
    {
        bodyGuide = new GameObject("BodyGuide");
        bodyGuide.transform.position = Vector3.zero;
    }

    //  AGGRESSIVELY DESTROY ANY OLD PRIMITIVE BODY PARTS CLUTTERING THE SCENE
    for (int i = bodyGuide.transform.childCount - 1; i >= 0; i--)
    {
        Transform child = bodyGuide.transform.GetChild(i);
        if (child.name == "Torso" || child.name == "Head" || child.name.Contains("Arm") || child.name.Contains("Leg"))
            SafeDestroy(child.gameObject);
    }

    // ✅ If model exists, just use it
    if (TryLoadBodyAsset(bodyGuide))
        return;

    // ✅ Only create primitive body if empty
    if (bodyGuide.transform.childCount > 0)
        return;

    Material bodyMaterial = CreateBodyGuideMaterial();

    CreateBodyPart(bodyGuide.transform, PrimitiveType.Capsule, "Torso", new Vector3(0f, 1.2f, 0f), new Vector3(0.9f, 1.8f, 0.45f), bodyMaterial);
    CreateBodyPart(bodyGuide.transform, PrimitiveType.Sphere, "Head", new Vector3(0f, 2.15f, 0f), new Vector3(0.45f, 0.52f, 0.45f), bodyMaterial);
    CreateBodyPart(bodyGuide.transform, PrimitiveType.Capsule, "LeftArm", new Vector3(-0.6f, 1.35f, 0f), new Vector3(0.2f, 0.9f, 0.2f), bodyMaterial, new Vector3(0f, 0f, 25f));
    CreateBodyPart(bodyGuide.transform, PrimitiveType.Capsule, "RightArm", new Vector3(0.6f, 1.35f, 0f), new Vector3(0.2f, 0.9f, 0.2f), bodyMaterial, new Vector3(0f, 0f, -25f));
    CreateBodyPart(bodyGuide.transform, PrimitiveType.Capsule, "LeftLeg", new Vector3(-0.18f, 0.35f, 0f), new Vector3(0.24f, 1.1f, 0.24f), bodyMaterial);
    CreateBodyPart(bodyGuide.transform, PrimitiveType.Capsule, "RightLeg", new Vector3(0.18f, 0.35f, 0f), new Vector3(0.24f, 1.1f, 0.24f), bodyMaterial);
}

    void SetupOrgans()
    {
        AnatomySceneController anatomyController = FindObjectOfType<AnatomySceneController>();
        if (anatomyController == null)
            return;

        // ✅ AGGRESSIVELY CLEAN UP ALL OLD ORGANS AND ZOMBIE MESHES
        OrganController[] oldControllers = FindObjectsOfType<OrganController>();
        foreach (OrganController oc in oldControllers)
        {
            SafeDestroy(oc.gameObject);
        }
        
        // Also manually kill any "Brain" objects lying around
        GameObject zombieBrain = GameObject.Find("Brain");
        if (zombieBrain != null) SafeDestroy(zombieBrain);

        List<GameObject> organs = new List<GameObject>();

        OrganController.OrganType[] organTypes =
        {
            OrganController.OrganType.Heart,
            OrganController.OrganType.Lung,
            OrganController.OrganType.Liver
        };

        string[] organNames = { "Heart", "Lungs", "Liver" };

        Vector3[] organPositions =
        {
            new Vector3(-0.55f, 1.25f, -0.3f),  // Heart
            new Vector3(0.0f,   1.25f, -0.3f),  // Lungs
            new Vector3(0.55f,  1.25f, -0.3f)   // Liver
        };

        GameObject bodyGuide = GameObject.Find("BodyGuide");
        if (bodyGuide == null)
        {
            Debug.LogError("BodyGuide not found in scene.");
            return;
        }

        for (int i = 0; i < organNames.Length; i++)
        {
            GameObject organRoot = new GameObject(organNames[i]);
            organRoot.transform.SetParent(bodyGuide.transform, false);
            organRoot.transform.localPosition = organPositions[i];
            organRoot.transform.localRotation = Quaternion.identity;
            organRoot.transform.localScale = Vector3.one;

            bool loadedAsset = TryLoadOrganAsset(organRoot, organTypes[i]);

            OrganController organController = organRoot.AddComponent<OrganController>();
            organController.organType = organTypes[i];
            organController.organMaterial = loadedAsset ? null : defaultOrganMaterial;

            if (!loadedAsset)
            {
                ProceduralOrganGenerator generator = organRoot.AddComponent<ProceduralOrganGenerator>();
                generator.organType = organTypes[i];
                generator.overrideMaterial = defaultOrganMaterial;
            }
            else
            {
                // ✅ FORCE FIX WHITE MATERIALS
                Renderer[] childRenderers = organRoot.GetComponentsInChildren<Renderer>(true);
                foreach (Renderer r in childRenderers)
                {
                    Material mat = r.sharedMaterial;
                    if (mat == null || mat.mainTexture == null || mat.name.Contains("White") || mat.name == "Default-Material")
                    {
                        if (defaultOrganMaterial != null)
                        {
                            r.sharedMaterial = defaultOrganMaterial;
                        }
                        else
                        {
                            Material newColorMat = new Material(Shader.Find("Standard"));
                            newColorMat.color = new Color(0.8f, 0.3f, 0.3f);
                            r.sharedMaterial = newColorMat;
                        }
                    }
                }
            }

            organs.Add(organRoot);
        }

        anatomyController.organs = organs.ToArray();
        anatomyController.showAllOnStart = true;
        anatomyController.ShowAllOrgans();
        FocusCameraOnOrgans(organs);

        AnatomyLabController labController = FindObjectOfType<AnatomyLabController>();
        if (labController != null)
        {
            List<OrganController> organControllers = new List<OrganController>();
            foreach (GameObject organ in organs)
            {
                organControllers.Add(organ.GetComponent<OrganController>());
            }
            labController.organs = organControllers.ToArray();
            labController.bodyGuide = GameObject.Find("BodyGuide");
        }
    }

    void SetupUI()
    {
        SetupEventSystem();

        GameObject existingCanvas = GameObject.Find("UI Canvas");
        if (existingCanvas != null)
            SafeDestroy(existingCanvas);

        GameObject canvasGO = new GameObject("UI Canvas");
        Canvas canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);

        canvasGO.AddComponent<GraphicRaycaster>();

        GameObject panelGO = new GameObject("Control Panel");
        panelGO.transform.SetParent(canvasGO.transform, false);

        Image panelImage = panelGO.AddComponent<Image>();
        panelImage.color = new Color(0.08f, 0.08f, 0.08f, 0.8f);
        panelImage.raycastTarget = false; // ✅ Let clicks pass through to 3D organs behind the panel

        RectTransform panelRT = panelGO.GetComponent<RectTransform>();
        panelRT.anchorMin = new Vector2(0.02f, 0.02f);
        panelRT.anchorMax = new Vector2(0.32f, 0.95f); // Used more height to prevent overlapping
        panelRT.offsetMin = Vector2.zero;
        panelRT.offsetMax = Vector2.zero;

        GameObject statusGO = new GameObject("StatusText");
        statusGO.transform.SetParent(panelGO.transform, false);

        Text statusText = statusGO.AddComponent<Text>();
        statusText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        statusText.fontSize = 30;
        statusText.color = Color.white;
        statusText.alignment = TextAnchor.UpperLeft;
        statusText.horizontalOverflow = HorizontalWrapMode.Wrap;
        statusText.verticalOverflow = VerticalWrapMode.Overflow;
        statusText.text = "ANATOMY LAB OVERVIEW\n\n" +
                          "Select an organ in the body to focus it.\n" +
                          "Use the controls below to isolate the organ, fade the body for dissection view, toggle information, and adjust interaction settings.\n" +
                          "If you want richer organ explanations later, we can plug this panel into an LLM provider.";

        RectTransform statusRT = statusGO.GetComponent<RectTransform>();
        statusRT.anchorMin = new Vector2(0f, 0f);
        statusRT.anchorMax = new Vector2(1f, 1f);
        statusRT.offsetMin = new Vector2(20f, 320f);
        statusRT.offsetMax = new Vector2(-12f, -12f);

        AnatomyLabController labController = FindObjectOfType<AnatomyLabController>();
        if (labController != null)
        {
            labController.statusText = statusText;
            labController.aiExpert = FindObjectOfType<AIOrganExpert>();
        }

        CreateControlsPanel(panelGO.transform, labController);
        CreateInfoPanel(canvasGO.transform, labController);
        CreateChatPanel(canvasGO.transform);
        CreateDissectionPanel(canvasGO.transform);

        if (labController != null)
        {
            labController.aiChat = FindObjectOfType<AIChatController>();
            labController.aiDissection = FindObjectOfType<AIDissectionGuide>();
        }
    }

    void SafeDestroy(Object target)
    {
        if (target == null)
            return;

#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            DestroyImmediate(target);
            return;
        }
#endif

        Destroy(target);
    }

    void FocusCameraOnOrgans(List<GameObject> organs)
    {
        if (organs == null || organs.Count == 0 || Camera.main == null)
            return;

        Vector3 min = organs[0].transform.position;
        Vector3 max = organs[0].transform.position;

        foreach (GameObject organ in organs)
        {
            if (organ == null)
                continue;

            min = Vector3.Min(min, organ.transform.position);
            max = Vector3.Max(max, organ.transform.position);
        }

        Vector3 targetPosition = new Vector3(0f, 1.25f, 0f);
        // Move camera to realistic viewing distance for a 1.75m tall body
        Camera.main.transform.position = new Vector3(0f, 1.35f, -1.8f);
        Camera.main.transform.LookAt(targetPosition);
    }

    void SetupEventSystem()
    {
        if (FindObjectOfType<EventSystem>() != null)
            return;

        GameObject eventSystem = new GameObject("EventSystem");
        eventSystem.AddComponent<EventSystem>();
        eventSystem.AddComponent<StandaloneInputModule>();
    }

    void CreateControlsPanel(Transform parent, AnatomyLabController labController)
    {
        GameObject controlsRoot = new GameObject("InteractiveControls");
        controlsRoot.transform.SetParent(parent, false);

        RectTransform controlsRT = controlsRoot.AddComponent<RectTransform>();
        controlsRT.anchorMin = new Vector2(0f, 0f);
        controlsRT.anchorMax = new Vector2(1f, 0f);
        controlsRT.pivot = new Vector2(0.5f, 0f);
        controlsRT.sizeDelta = new Vector2(0f, 290f);
        controlsRT.anchoredPosition = new Vector2(0f, 16f);

        CreateButton(controlsRoot.transform, "OverviewButton", "Show Overview", new Vector2(0.5f, 250f), new Vector2(260f, 36f), delegate
        {
            if (labController != null)
                labController.ShowOverview();
        });

        Toggle isolateToggle = CreateToggle(controlsRoot.transform, "IsolateToggle", "Isolate selected organ", new Vector2(16f, 206f));
        Toggle dissectToggle = CreateToggle(controlsRoot.transform, "DissectToggle", "Dissection view", new Vector2(16f, 174f));
        Toggle infoToggle = CreateToggle(controlsRoot.transform, "InfoToggle", "Show information panel", new Vector2(16f, 142f));
        Toggle selectToggle = CreateToggle(controlsRoot.transform, "SelectToggle", "Enable selection", new Vector2(16f, 110f));
        Toggle dragToggle = CreateToggle(controlsRoot.transform, "DragToggle", "Enable drag", new Vector2(16f, 78f));
        Toggle rotateToggle = CreateToggle(controlsRoot.transform, "RotateToggle", "Enable rotation", new Vector2(16f, 46f));
        Toggle zoomToggle = CreateToggle(controlsRoot.transform, "ZoomToggle", "Enable zoom", new Vector2(16f, 14f));

        Slider dragSlider = CreateSlider(controlsRoot.transform, "DragSlider", "Drag speed", new Vector2(16f, -24f), 1f, 20f, 10f);
        Slider rotateSlider = CreateSlider(controlsRoot.transform, "RotateSlider", "Rotate speed", new Vector2(16f, -58f), 30f, 240f, 100f);
        Slider zoomSlider = CreateSlider(controlsRoot.transform, "ZoomSlider", "Zoom speed", new Vector2(16f, -92f), 0.5f, 6f, 2f);

        if (labController != null)
        {
            labController.isolateToggle = isolateToggle;
            labController.dissectToggle = dissectToggle;
            labController.infoToggle = infoToggle;
            labController.selectToggle = selectToggle;
            labController.dragToggle = dragToggle;
            labController.rotateToggle = rotateToggle;
            labController.zoomToggle = zoomToggle;
            labController.dragSpeedSlider = dragSlider;
            labController.rotateSpeedSlider = rotateSlider;
            labController.zoomSpeedSlider = zoomSlider;
        }
    }

    void CreateInfoPanel(Transform canvasRoot, AnatomyLabController labController)
    {
        GameObject infoPanel = new GameObject("Info Panel");
        infoPanel.transform.SetParent(canvasRoot, false);

        Image infoBackground = infoPanel.AddComponent<Image>();
        infoBackground.color = new Color(0.07f, 0.09f, 0.14f, 0.84f);
        infoBackground.raycastTarget = false; // ✅ Let clicks pass through to 3D organs

        RectTransform infoRT = infoPanel.GetComponent<RectTransform>();
        infoRT.anchorMin = new Vector2(0.62f, 0.05f);
        infoRT.anchorMax = new Vector2(0.97f, 0.78f);
        infoRT.offsetMin = Vector2.zero;
        infoRT.offsetMax = Vector2.zero;

        Text titleText = CreateText(infoPanel.transform, "InfoTitle", 30, TextAnchor.UpperLeft);
        RectTransform titleRT = titleText.GetComponent<RectTransform>();
        titleRT.anchorMin = new Vector2(0f, 1f);
        titleRT.anchorMax = new Vector2(1f, 1f);
        titleRT.pivot = new Vector2(0.5f, 1f);
        titleRT.sizeDelta = new Vector2(0f, 36f);
        titleRT.anchoredPosition = new Vector2(0f, -12f);
        titleRT.offsetMin = new Vector2(14f, titleRT.offsetMin.y);
        titleRT.offsetMax = new Vector2(-14f, titleRT.offsetMax.y);

        Text bodyText = CreateText(infoPanel.transform, "InfoBody", 22, TextAnchor.UpperLeft);
        RectTransform bodyRT = bodyText.GetComponent<RectTransform>();
        bodyRT.anchorMin = new Vector2(0f, 0f);
        bodyRT.anchorMax = new Vector2(1f, 1f);
        bodyRT.offsetMin = new Vector2(14f, 14f);
        bodyRT.offsetMax = new Vector2(-14f, -56f);
        bodyText.verticalOverflow = VerticalWrapMode.Overflow;

        if (labController != null)
        {
            labController.infoPanel = infoPanel;
            labController.infoTitleText = titleText;
            labController.infoBodyText = bodyText;
        }

        // ✅ Add "Learn More" Link Button
        CreateButton(infoPanel.transform, "LearnMoreBtn", "🌐 View Medical Record", new Vector2(0f, 10f), new Vector2(240f, 32f), () =>
        {
            AIOrganExpert expert = FindObjectOfType<AIOrganExpert>();
            if (expert != null) expert.LearnMore();
        });
    }

    Text CreateText(Transform parent, string name, int fontSize, TextAnchor alignment)
    {
        GameObject textGO = new GameObject(name);
        textGO.transform.SetParent(parent, false);

        Text text = textGO.AddComponent<Text>();
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.fontSize = fontSize;
        text.color = Color.white;
        text.alignment = alignment;
        text.horizontalOverflow = HorizontalWrapMode.Wrap;
        text.verticalOverflow = VerticalWrapMode.Overflow;
        text.raycastTarget = false; // ✅ Don't block 3D organ clicks
        return text;
    }

    Button CreateButton(Transform parent, string name, string label, Vector2 anchoredPosition, Vector2 size, UnityEngine.Events.UnityAction action)
    {
        GameObject buttonGO = new GameObject(name);
        buttonGO.transform.SetParent(parent, false);

        Image image = buttonGO.AddComponent<Image>();
        image.color = new Color(0.18f, 0.42f, 0.7f, 0.92f);

        Button button = buttonGO.AddComponent<Button>();
        button.onClick.AddListener(action);

        RectTransform rt = buttonGO.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0f);
        rt.anchorMax = new Vector2(0.5f, 0f);
        rt.pivot = new Vector2(0.5f, 0f);
        rt.sizeDelta = size;
        rt.anchoredPosition = anchoredPosition;

        Text labelText = CreateText(buttonGO.transform, "Label", 15, TextAnchor.MiddleCenter);
        RectTransform labelRT = labelText.GetComponent<RectTransform>();
        labelRT.anchorMin = Vector2.zero;
        labelRT.anchorMax = Vector2.one;
        labelRT.offsetMin = Vector2.zero;
        labelRT.offsetMax = Vector2.zero;
        labelText.text = label;

        return button;
    }

    Toggle CreateToggle(Transform parent, string name, string label, Vector2 anchoredPosition)
    {
        GameObject toggleGO = new GameObject(name);
        toggleGO.transform.SetParent(parent, false);

        RectTransform rt = toggleGO.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0f, 0f);
        rt.anchorMax = new Vector2(1f, 0f);
        rt.pivot = new Vector2(0f, 0f);
        rt.sizeDelta = new Vector2(-20f, 22f);
        rt.anchoredPosition = anchoredPosition;

        Toggle toggle = toggleGO.AddComponent<Toggle>();

        GameObject backgroundGO = new GameObject("Background");
        backgroundGO.transform.SetParent(toggleGO.transform, false);
        Image background = backgroundGO.AddComponent<Image>();
        background.color = new Color(0.85f, 0.85f, 0.85f, 0.9f);
        RectTransform backgroundRT = backgroundGO.GetComponent<RectTransform>();
        backgroundRT.anchorMin = new Vector2(0f, 0.5f);
        backgroundRT.anchorMax = new Vector2(0f, 0.5f);
        backgroundRT.sizeDelta = new Vector2(18f, 18f);
        backgroundRT.anchoredPosition = new Vector2(10f, 0f);

        GameObject checkmarkGO = new GameObject("Checkmark");
        checkmarkGO.transform.SetParent(backgroundGO.transform, false);
        Image checkmark = checkmarkGO.AddComponent<Image>();
        checkmark.color = new Color(0.12f, 0.6f, 0.25f, 1f);
        RectTransform checkmarkRT = checkmarkGO.GetComponent<RectTransform>();
        checkmarkRT.anchorMin = new Vector2(0.5f, 0.5f);
        checkmarkRT.anchorMax = new Vector2(0.5f, 0.5f);
        checkmarkRT.sizeDelta = new Vector2(10f, 10f);
        checkmarkRT.anchoredPosition = Vector2.zero;

        Text labelText = CreateText(toggleGO.transform, "Label", 14, TextAnchor.MiddleLeft);
        labelText.text = label;
        RectTransform labelRT = labelText.GetComponent<RectTransform>();
        labelRT.anchorMin = new Vector2(0f, 0f);
        labelRT.anchorMax = new Vector2(1f, 1f);
        labelRT.offsetMin = new Vector2(28f, 0f);
        labelRT.offsetMax = new Vector2(0f, 0f);

        toggle.graphic = checkmark;
        toggle.targetGraphic = background;
        return toggle;
    }

    Slider CreateSlider(Transform parent, string name, string label, Vector2 anchoredPosition, float minValue, float maxValue, float defaultValue)
    {
        GameObject sliderRoot = new GameObject(name);
        sliderRoot.transform.SetParent(parent, false);

        RectTransform rootRT = sliderRoot.AddComponent<RectTransform>();
        rootRT.anchorMin = new Vector2(0f, 0f);
        rootRT.anchorMax = new Vector2(1f, 0f);
        rootRT.pivot = new Vector2(0f, 0f);
        rootRT.sizeDelta = new Vector2(-20f, 22f);
        rootRT.anchoredPosition = anchoredPosition;

        Text labelText = CreateText(sliderRoot.transform, "Label", 13, TextAnchor.MiddleLeft);
        labelText.text = label;
        RectTransform labelRT = labelText.GetComponent<RectTransform>();
        labelRT.anchorMin = new Vector2(0f, 0f);
        labelRT.anchorMax = new Vector2(0.42f, 1f);
        labelRT.offsetMin = Vector2.zero;
        labelRT.offsetMax = Vector2.zero;

        GameObject sliderGO = new GameObject("Slider");
        sliderGO.transform.SetParent(sliderRoot.transform, false);
        RectTransform sliderRT = sliderGO.AddComponent<RectTransform>();
        sliderRT.anchorMin = new Vector2(0.46f, 0.5f);
        sliderRT.anchorMax = new Vector2(1f, 0.5f);
        sliderRT.sizeDelta = new Vector2(0f, 18f);
        sliderRT.anchoredPosition = Vector2.zero;

        Image background = sliderGO.AddComponent<Image>();
        background.color = new Color(0.24f, 0.24f, 0.24f, 0.95f);

        Slider slider = sliderGO.AddComponent<Slider>();
        slider.minValue = minValue;
        slider.maxValue = maxValue;
        slider.value = defaultValue;

        GameObject fillArea = new GameObject("Fill Area");
        fillArea.transform.SetParent(sliderGO.transform, false);
        RectTransform fillAreaRT = fillArea.AddComponent<RectTransform>();
        fillAreaRT.anchorMin = new Vector2(0f, 0f);
        fillAreaRT.anchorMax = new Vector2(1f, 1f);
        fillAreaRT.offsetMin = new Vector2(6f, 6f);
        fillAreaRT.offsetMax = new Vector2(-16f, -6f);

        GameObject fillGO = new GameObject("Fill");
        fillGO.transform.SetParent(fillArea.transform, false);
        Image fill = fillGO.AddComponent<Image>();
        fill.color = new Color(0.22f, 0.62f, 0.95f, 1f);
        RectTransform fillRT = fillGO.GetComponent<RectTransform>();
        fillRT.anchorMin = new Vector2(0f, 0f);
        fillRT.anchorMax = new Vector2(1f, 1f);
        fillRT.offsetMin = Vector2.zero;
        fillRT.offsetMax = Vector2.zero;

        GameObject handleSlideArea = new GameObject("Handle Slide Area");
        handleSlideArea.transform.SetParent(sliderGO.transform, false);
        RectTransform handleSlideAreaRT = handleSlideArea.AddComponent<RectTransform>();
        handleSlideAreaRT.anchorMin = new Vector2(0f, 0f);
        handleSlideAreaRT.anchorMax = new Vector2(1f, 1f);
        handleSlideAreaRT.offsetMin = new Vector2(10f, 0f);
        handleSlideAreaRT.offsetMax = new Vector2(-10f, 0f);

        GameObject handleGO = new GameObject("Handle");
        handleGO.transform.SetParent(handleSlideArea.transform, false);
        Image handle = handleGO.AddComponent<Image>();
        handle.color = new Color(0.95f, 0.95f, 0.95f, 1f);
        RectTransform handleRT = handleGO.GetComponent<RectTransform>();
        handleRT.sizeDelta = new Vector2(12f, 22f);

        slider.fillRect = fillRT;
        slider.handleRect = handleRT;
        slider.targetGraphic = handle;
        slider.direction = Slider.Direction.LeftToRight;

        return slider;
    }

    Material CreateBodyGuideMaterial()
    {
        Shader shader = Shader.Find("Standard");
        if (shader == null)
            shader = Shader.Find("Sprites/Default");

        Material material = new Material(shader);
        material.color = new Color(0.65f, 0.78f, 0.95f, 0.22f);
        return material;
    }

    void CreateBodyPart(Transform parent, PrimitiveType primitiveType, string name, Vector3 localPosition, Vector3 localScale, Material material)
    {
        CreateBodyPart(parent, primitiveType, name, localPosition, localScale, material, Vector3.zero);
    }

    void CreateBodyPart(Transform parent, PrimitiveType primitiveType, string name, Vector3 localPosition, Vector3 localScale, Material material, Vector3 localEulerAngles)
    {
        GameObject part = GameObject.CreatePrimitive(primitiveType);
        part.name = name;
        part.transform.SetParent(parent, false);
        part.transform.localPosition = localPosition;
        part.transform.localEulerAngles = localEulerAngles;
        part.transform.localScale = localScale;

        Renderer renderer = part.GetComponent<Renderer>();
        if (renderer != null)
            renderer.material = new Material(material);

        Collider collider = part.GetComponent<Collider>();
        if (collider != null)
            Destroy(collider);
    }

    bool TryLoadOrganAsset(GameObject organRoot, OrganController.OrganType organType)
    {
#if UNITY_EDITOR
        string assetPath = GetAssetPath(organType);
        if (string.IsNullOrEmpty(assetPath))
            return false;

        GameObject organAsset = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
        if (organAsset == null)
            return false;

        ClearChildren(organRoot.transform);
        RemoveProceduralComponents(organRoot);

        GameObject visual = Instantiate(organAsset, organRoot.transform);
        visual.name = organAsset.name;
        visual.transform.localPosition = Vector3.zero;
        visual.transform.localRotation = Quaternion.identity;
        visual.transform.localScale = Vector3.one;

        FitVisualToTargetSize(organRoot, visual.transform, GetTargetSize(organType));
        AddRootCollider(organRoot);
        return true;
#else
        return false;
#endif
    }

    bool TryLoadBodyAsset(GameObject bodyRoot)
    {
#if UNITY_EDITOR
        const string assetPath = "Assets/Models/anatomical-planes-study/source/anatomical_planes_SketchfabFBX.fbx";
        GameObject bodyAsset = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
        if (bodyAsset == null)
            return false;

        GameObject visual = null;

        // Clean up any duplicated bodies and find one if it exists
        // Iterate backwards because we might be destroying
        for (int i = bodyRoot.transform.childCount - 1; i >= 0; i--)
        {
            Transform child = bodyRoot.transform.GetChild(i);
            if (child.name == bodyAsset.name || child.name.Contains("anatomical_planes"))
            {
                if (visual == null)
                    visual = child.gameObject; // Keep the first one we find
                else
                    SafeDestroy(child.gameObject); // Destroy duplicates from past runs
            }
        }

        if (visual == null)
        {
            visual = Instantiate(bodyAsset, bodyRoot.transform);
            visual.name = bodyAsset.name;
        }

        visual.transform.localPosition = Vector3.zero;
        visual.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);
        visual.transform.localScale = Vector3.one;

        FitVisualToTargetSize(bodyRoot, visual.transform, 1.75f);
        
        // ALIGN BODY PROPERLY: Bottom of bounds should be exactly at local Y = 0
        Bounds bounds = CalculateRenderBounds(visual.transform);
        Vector3 bottomCenterWorld = new Vector3(bounds.center.x, bounds.min.y, bounds.center.z);
        Vector3 bottomCenterLocal = bodyRoot.transform.InverseTransformPoint(bottomCenterWorld);
        visual.transform.localPosition -= bottomCenterLocal;
        
        return true;
#else
        return false;
#endif
    }

    string GetAssetPath(OrganController.OrganType organType)
    {
        switch (organType)
        {
            case OrganController.OrganType.Heart:
                return "Assets/Models/realistic-human-heart/source/Heart.fbx";
            case OrganController.OrganType.Lung:
                return "Assets/Models/realistic-human-lungs/source/Lungs With Texture.fbx";
            case OrganController.OrganType.Liver:
                return "Assets/Models/human-liver/source/human liver.fbx";
            default:
                return string.Empty;
        }
    }

    float GetTargetSize(OrganController.OrganType organType)
    {
        switch (organType)
        {
            case OrganController.OrganType.Heart:
                return 0.16f; // Realistic heart ~16cm
            case OrganController.OrganType.Brain:
                return 0.20f;
            case OrganController.OrganType.Lung:
                return 0.35f; // Realistic lung span ~35cm
            case OrganController.OrganType.Liver:
                return 0.25f; // Realistic liver ~25cm
            default:
                return 0.5f;
        }
    }

    void ClearChildren(Transform parent)
    {
        for (int i = parent.childCount - 1; i >= 0; i--)
            SafeDestroy(parent.GetChild(i).gameObject);
    }

    void RemoveProceduralComponents(GameObject organRoot)
    {
        ProceduralOrganGenerator generator = organRoot.GetComponent<ProceduralOrganGenerator>();
        if (generator != null)
            SafeDestroy(generator);

        MeshFilter meshFilter = organRoot.GetComponent<MeshFilter>();
        if (meshFilter != null)
            SafeDestroy(meshFilter);

        MeshRenderer meshRenderer = organRoot.GetComponent<MeshRenderer>();
        if (meshRenderer != null)
            SafeDestroy(meshRenderer);

        MeshCollider meshCollider = organRoot.GetComponent<MeshCollider>();
        if (meshCollider != null)
            SafeDestroy(meshCollider);

        SphereCollider sphereCollider = organRoot.GetComponent<SphereCollider>();
        if (sphereCollider != null)
            SafeDestroy(sphereCollider);

        CapsuleCollider capsuleCollider = organRoot.GetComponent<CapsuleCollider>();
        if (capsuleCollider != null)
            SafeDestroy(capsuleCollider);

        BoxCollider boxCollider = organRoot.GetComponent<BoxCollider>();
        if (boxCollider != null)
            SafeDestroy(boxCollider);
    }

    void FitVisualToTargetSize(GameObject organRoot, Transform visual, float targetSize)
    {
        Bounds bounds = CalculateRenderBounds(visual);
        if (bounds.size == Vector3.zero)
            return;

        float currentMaxDimension = Mathf.Max(bounds.size.x, bounds.size.y, bounds.size.z);
        if (currentMaxDimension > 0.0001f)
        {
            float scaleFactor = targetSize / currentMaxDimension;
            visual.localScale = Vector3.one * scaleFactor;
        }

        Bounds newBounds = CalculateRenderBounds(visual);
        Vector3 offset = organRoot.transform.InverseTransformPoint(newBounds.center);
        visual.localPosition -= offset;
    }

    void AddRootCollider(GameObject organRoot)
    {
        // ✅ Strip ALL Rigidbodies from children to prevent physics conflicts
        Rigidbody[] rigidbodies = organRoot.GetComponentsInChildren<Rigidbody>(true);
        foreach (Rigidbody rb in rigidbodies)
        {
            rb.isKinematic = true;
            SafeDestroy(rb);
        }

        // ✅ Strip any existing colliders from children that might interfere
        Collider[] existingColliders = organRoot.GetComponentsInChildren<Collider>(true);
        foreach (Collider c in existingColliders)
        {
            SafeDestroy(c);
        }

        // ✅ SIMPLE & GUARANTEED: Add a single BoxCollider on the ROOT organ object
        // This is the object that has OrganController, so GetComponentInParent will always find it
        Bounds bounds = CalculateRenderBounds(organRoot.transform);
        if (bounds.size == Vector3.zero)
        {
            // Fallback: small collider so at least something is clickable
            BoxCollider fallback = organRoot.AddComponent<BoxCollider>();
            fallback.size = Vector3.one * 0.15f;
            Debug.Log("[AddRootCollider] ⚠️ No renderers found on " + organRoot.name + ", using fallback collider");
            return;
        }

        BoxCollider collider = organRoot.AddComponent<BoxCollider>();
        collider.center = organRoot.transform.InverseTransformPoint(bounds.center);
        collider.size = bounds.size;
        Debug.Log("[AddRootCollider] ✅ Added BoxCollider to " + organRoot.name + " center=" + collider.center + " size=" + collider.size);
    }

    Bounds CalculateRenderBounds(Transform root)
    {
        Renderer[] renderers = root.GetComponentsInChildren<Renderer>();
        if (renderers.Length == 0)
            return new Bounds(root.position, Vector3.zero);

        Bounds combinedBounds = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++)
            combinedBounds.Encapsulate(renderers[i].bounds);

        return combinedBounds;
    }

    void CreateChatPanel(Transform canvasRoot)
    {
        GameObject chatPanel = new GameObject("Chat Panel");
        chatPanel.transform.SetParent(canvasRoot, false);

        Image chatBg = chatPanel.AddComponent<Image>();
        chatBg.color = new Color(0.05f, 0.08f, 0.12f, 0.9f);
        chatBg.raycastTarget = false;

        RectTransform chatRT = chatPanel.GetComponent<RectTransform>();
        chatRT.anchorMin = new Vector2(0.33f, 0.02f);
        chatRT.anchorMax = new Vector2(0.67f, 0.42f);
        chatRT.offsetMin = Vector2.zero;
        chatRT.offsetMax = Vector2.zero;

        // Title
        Text chatTitle = CreateText(chatPanel.transform, "ChatTitle", 20, TextAnchor.UpperCenter);
        chatTitle.text = "💬 Ask Me Anything";
        chatTitle.color = new Color(0.4f, 0.85f, 1f);
        RectTransform titleRT = chatTitle.GetComponent<RectTransform>();
        titleRT.anchorMin = new Vector2(0f, 1f);
        titleRT.anchorMax = new Vector2(1f, 1f);
        titleRT.pivot = new Vector2(0.5f, 1f);
        titleRT.sizeDelta = new Vector2(0f, 28f);
        titleRT.anchoredPosition = new Vector2(0f, -4f);

        // Chat history display
        Text chatHistory = CreateText(chatPanel.transform, "ChatHistory", 14, TextAnchor.UpperLeft);
        chatHistory.text = "Select an organ, then ask any question!\n\nExamples:\n• What diseases affect this organ?\n• How does blood flow through it?";
        chatHistory.color = new Color(0.85f, 0.9f, 0.95f);
        RectTransform histRT = chatHistory.GetComponent<RectTransform>();
        histRT.anchorMin = new Vector2(0f, 0f);
        histRT.anchorMax = new Vector2(1f, 1f);
        histRT.offsetMin = new Vector2(10f, 50f);
        histRT.offsetMax = new Vector2(-10f, -34f);

        // Input field
        InputField inputField = CreateInputField(chatPanel.transform, "ChatInput", new Vector2(10f, 10f), new Vector2(-100f, 36f));

        // Send button
        AIChatController chatController = FindObjectOfType<AIChatController>();
        Button sendBtn = CreateButton(chatPanel.transform, "SendBtn", "Ask AI", new Vector2(0f, 0f), new Vector2(80f, 36f), delegate
        {
            if (chatController != null)
                chatController.SendQuestion();
        });
        RectTransform sendRT = sendBtn.GetComponent<RectTransform>();
        sendRT.anchorMin = new Vector2(1f, 0f);
        sendRT.anchorMax = new Vector2(1f, 0f);
        sendRT.pivot = new Vector2(1f, 0f);
        sendRT.anchoredPosition = new Vector2(-8f, 8f);

        // Wire up chat controller
        if (chatController != null)
        {
            chatController.chatInput = inputField;
            chatController.chatHistoryText = chatHistory;
            chatController.chatTitleText = chatTitle;
            chatController.sendButton = sendBtn;
        }
    }

    InputField CreateInputField(Transform parent, string name, Vector2 offsetMin, Vector2 offsetMax)
    {
        GameObject inputGO = new GameObject(name);
        inputGO.transform.SetParent(parent, false);

        Image bg = inputGO.AddComponent<Image>();
        bg.color = new Color(0.1f, 0.12f, 0.15f, 1f);

        RectTransform rt = inputGO.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0f, 0f);
        rt.anchorMax = new Vector2(1f, 0f);
        rt.pivot = new Vector2(0f, 0f);
        rt.offsetMin = new Vector2(8f, 8f);
        rt.offsetMax = new Vector2(-95f, 44f);

        GameObject textGO = new GameObject("Text");
        textGO.transform.SetParent(inputGO.transform, false);
        Text text = textGO.AddComponent<Text>();
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.fontSize = 14;
        text.color = Color.white;
        text.alignment = TextAnchor.MiddleLeft;

        RectTransform textRT = textGO.GetComponent<RectTransform>();
        textRT.anchorMin = Vector2.zero;
        textRT.anchorMax = Vector2.one;
        textRT.offsetMin = new Vector2(5f, 0f);
        textRT.offsetMax = new Vector2(-5f, 0f);

        InputField inputField = inputGO.AddComponent<InputField>();
        inputField.textComponent = text;
        return inputField;
    }

    void CreateDissectionPanel(Transform canvasRoot)
    {
        GameObject dissectPanel = new GameObject("Dissection Panel");
        dissectPanel.transform.SetParent(canvasRoot, false);
        dissectPanel.SetActive(false); // Hidden by default

        Image dissectBg = dissectPanel.AddComponent<Image>();
        dissectBg.color = new Color(0.12f, 0.05f, 0.05f, 0.9f);
        dissectBg.raycastTarget = false;

        RectTransform dissectRT = dissectPanel.GetComponent<RectTransform>();
        dissectRT.anchorMin = new Vector2(0.33f, 0.45f);
        dissectRT.anchorMax = new Vector2(0.67f, 0.98f);
        dissectRT.offsetMin = Vector2.zero;
        dissectRT.offsetMax = Vector2.zero;

        // Title
        Text dissectTitle = CreateText(dissectPanel.transform, "DissectTitle", 20, TextAnchor.UpperCenter);
        dissectTitle.text = "🔪 Dissection Guide";
        dissectTitle.color = new Color(1f, 0.6f, 0.6f);
        RectTransform dTitleRT = dissectTitle.GetComponent<RectTransform>();
        dTitleRT.anchorMin = new Vector2(0f, 1f);
        dTitleRT.anchorMax = new Vector2(1f, 1f);
        dTitleRT.pivot = new Vector2(0.5f, 1f);
        dTitleRT.sizeDelta = new Vector2(0f, 28f);
        dTitleRT.anchoredPosition = new Vector2(0f, -4f);

        // Guide body text
        Text guideBody = CreateText(dissectPanel.transform, "GuideBody", 15, TextAnchor.UpperLeft);
        guideBody.text = "Select an organ and enable Dissection View to begin.";
        guideBody.color = new Color(0.9f, 0.85f, 0.85f);
        RectTransform guideRT = guideBody.GetComponent<RectTransform>();
        guideRT.anchorMin = new Vector2(0f, 0f);
        guideRT.anchorMax = new Vector2(1f, 1f);
        guideRT.offsetMin = new Vector2(12f, 50f);
        guideRT.offsetMax = new Vector2(-12f, -36f);

        // Navigation buttons
        AIDissectionGuide guide = FindObjectOfType<AIDissectionGuide>();

        CreateButton(dissectPanel.transform, "PrevStep", "← Prev", new Vector2(-100f, 10f), new Vector2(80f, 32f), () => {
            if (guide != null) guide.PreviousStep();
        });

        CreateButton(dissectPanel.transform, "NextStep", "Next →", new Vector2(100f, 10f), new Vector2(80f, 32f), () => {
            if (guide != null) guide.NextStep();
        });

        // Wire up dissection guide
        if (guide != null)
        {
            guide.guideTitleText = dissectTitle;
            guide.guideBodyText = guideBody;
        }

        // Store reference in Lab Controller
        AnatomyLabController lab = FindObjectOfType<AnatomyLabController>();
        if (lab != null)
            lab.dissectionPanel = dissectPanel;
    }
}
