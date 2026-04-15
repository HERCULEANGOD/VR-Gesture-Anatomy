using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AnatomyLabController : MonoBehaviour
{
    public GameObject bodyGuide;
    public OrganController[] organs;
    public GameObject infoPanel;
    public Text infoTitleText;
    public Text infoBodyText;
    public Text statusText;

    public Toggle isolateToggle;
    public Toggle dissectToggle;
    public Toggle infoToggle;
    public Toggle selectToggle;
    public Toggle dragToggle;
    public Toggle rotateToggle;
    public Toggle zoomToggle;
    public Slider dragSpeedSlider;
    public Slider rotateSpeedSlider;
    public Slider zoomSpeedSlider;

    public AIOrganExpert aiExpert;
    public AIChatController aiChat;
    public AIDissectionGuide aiDissection;
    public GameObject dissectionPanel;

    private readonly Dictionary<OrganController, Vector3> originalScales = new Dictionary<OrganController, Vector3>();
    private readonly Dictionary<OrganController, Vector3> originalPositions = new Dictionary<OrganController, Vector3>();
    private readonly Dictionary<OrganController, Quaternion> originalRotations = new Dictionary<OrganController, Quaternion>();
    
    private EditorInputSimulator editorInput;
    private OrganController selectedOrgan;

    void Start()
    {
        editorInput = FindObjectOfType<EditorInputSimulator>();
        if (editorInput != null)
            editorInput.OnOrganSelected += HandleOrganSelected;

        originalScales.Clear();
        originalPositions.Clear();
        originalRotations.Clear();
        
        if (organs != null)
        {
            foreach (OrganController organ in organs)
            {
                if (organ != null && !originalScales.ContainsKey(organ))
                {
                    originalScales.Add(organ, organ.transform.localScale);
                    originalPositions.Add(organ, organ.transform.localPosition);
                    originalRotations.Add(organ, organ.transform.localRotation);
                }
            }
        }

        InitializeControls();
        SetDissectionView(false);
        ShowOverview();
    }

    void OnDestroy()
    {
        if (editorInput != null)
            editorInput.OnOrganSelected -= HandleOrganSelected;
    }

    void InitializeControls()
    {
        if (editorInput != null)
        {
            if (dragSpeedSlider != null)
            {
                dragSpeedSlider.value = editorInput.dragSpeed;
                dragSpeedSlider.onValueChanged.AddListener(SetDragSpeed);
            }

            if (rotateSpeedSlider != null)
            {
                rotateSpeedSlider.value = editorInput.rotationSpeed;
                rotateSpeedSlider.onValueChanged.AddListener(SetRotateSpeed);
            }

            if (zoomSpeedSlider != null)
            {
                zoomSpeedSlider.value = editorInput.zoomSpeed;
                zoomSpeedSlider.onValueChanged.AddListener(SetZoomSpeed);
            }
        }

        if (isolateToggle != null)
            isolateToggle.onValueChanged.AddListener(delegate { ApplySelectionView(); });

        if (dissectToggle != null)
            dissectToggle.onValueChanged.AddListener(SetDissectionView);

        if (infoToggle != null)
            infoToggle.onValueChanged.AddListener(SetInfoPanelVisible);

        if (selectToggle != null)
            selectToggle.onValueChanged.AddListener(SetSelectionEnabled);

        if (dragToggle != null)
            dragToggle.onValueChanged.AddListener(SetDragEnabled);

        if (rotateToggle != null)
            rotateToggle.onValueChanged.AddListener(SetRotationEnabled);

        if (zoomToggle != null)
            zoomToggle.onValueChanged.AddListener(SetZoomEnabled);

        if (infoToggle != null)
            infoToggle.isOn = true;

        if (dissectToggle != null)
            dissectToggle.isOn = false;

        if (editorInput != null)
        {
            if (selectToggle != null)
                selectToggle.isOn = editorInput.enableSelection;

            if (dragToggle != null)
                dragToggle.isOn = editorInput.enableDrag;

            if (rotateToggle != null)
                rotateToggle.isOn = editorInput.enableRotation;

            if (zoomToggle != null)
                zoomToggle.isOn = editorInput.enableZoom;
        }
    }

    void HandleOrganSelected(OrganController organ)
    {
        if (organ == null)
            return;

        selectedOrgan = organ;
        ApplySelectionView();
        UpdateInfoPanel(organ.organType);

        if (statusText != null)
            statusText.text = "Selected organ: " + organ.organType;

        // ✅ ZOOM CAMERA IN CLOSE to the selected organ for a detailed view
        FocusCameraOnOrgan(organ);

        // ✅ AI ORGAN EXPERT - Fetch intelligence
        if (aiExpert != null)
        {
            aiExpert.infoTitleText = infoTitleText;
            aiExpert.infoBodyText = infoBodyText;
            aiExpert.statusText = statusText;
            aiExpert.OnOrganSelected(organ.organType);
        }

        // ✅ AI CHAT - Set context
        if (aiChat != null)
            aiChat.SetCurrentOrgan(organ.organType.ToString());
    }

    void FocusCameraOnOrgan(OrganController organ)
    {
        if (Camera.main == null || organ == null)
            return;

        // Calculate the organ's bounds to determine how far to stand back
        Renderer[] renderers = organ.GetComponentsInChildren<Renderer>();
        if (renderers.Length == 0)
            return;

        Bounds bounds = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++)
            bounds.Encapsulate(renderers[i].bounds);

        // Position camera in front of the organ, close enough to see detail
        float organSize = Mathf.Max(bounds.size.x, bounds.size.y, bounds.size.z);
        float viewDistance = organSize * 2.5f; // Close enough for detail, far enough to see the whole organ
        
        Vector3 organCenter = bounds.center;
        Camera.main.transform.position = organCenter + new Vector3(0f, 0.05f, -viewDistance);
        Camera.main.transform.LookAt(organCenter);
        
        Debug.Log("[Camera] Focused on " + organ.organType + " at distance " + viewDistance.ToString("F2"));
    }

    // ✅ Called directly by OrganController.OnMouseDown() and keyboard shortcuts
    public void DirectSelectOrgan(OrganController organ)
    {
        Debug.Log("[AnatomyLab] ✅ DirectSelectOrgan called: " + organ.organType);
        HandleOrganSelected(organ);
    }

    void ApplySelectionView()
    {
        if (organs == null)
            return;

        foreach (OrganController organ in organs)
        {
            if (organ == null)
                continue;

            bool isSelected = organ == selectedOrgan;
            organ.gameObject.SetActive(!ShouldIsolate() || isSelected);

            if (originalScales.ContainsKey(organ))
                organ.transform.localScale = isSelected ? originalScales[organ] * 1.08f : originalScales[organ];
        }
    }

    bool ShouldIsolate()
    {
        return isolateToggle != null && isolateToggle.isOn && selectedOrgan != null;
    }

    void SetDissectionView(bool enabled)
    {
        if (dissectionPanel != null)
            dissectionPanel.SetActive(enabled && selectedOrgan != null);

        if (enabled && selectedOrgan != null && aiDissection != null)
            aiDissection.StartDissection(selectedOrgan.organType.ToString());
        else if (!enabled && aiDissection != null)
            aiDissection.StopDissection();

        if (bodyGuide == null)
            return;

        Renderer[] renderers = bodyGuide.GetComponentsInChildren<Renderer>(true);
        Color bodyColor = enabled ? new Color(0.65f, 0.78f, 0.95f, 0.08f) : new Color(0.65f, 0.78f, 0.95f, 0.22f);

        foreach (Renderer renderer in renderers)
        {
            if (renderer == null)
                continue;

            Material material = renderer.material;
            if (material == null)
                continue;

            material.color = bodyColor;
        }
    }

    void SetInfoPanelVisible(bool visible)
    {
        if (infoPanel != null)
            infoPanel.SetActive(visible);
    }

    void SetDragSpeed(float value)
    {
        if (editorInput != null)
            editorInput.dragSpeed = value;
    }

    void SetRotateSpeed(float value)
    {
        if (editorInput != null)
            editorInput.rotationSpeed = value;
    }

    void SetZoomSpeed(float value)
    {
        if (editorInput != null)
            editorInput.zoomSpeed = value;
    }

    void SetSelectionEnabled(bool enabled)
    {
        if (editorInput != null)
            editorInput.enableSelection = enabled;
    }

    void SetDragEnabled(bool enabled)
    {
        if (editorInput != null)
            editorInput.enableDrag = enabled;
    }

    void SetRotationEnabled(bool enabled)
    {
        if (editorInput != null)
            editorInput.enableRotation = enabled;
    }

    void SetZoomEnabled(bool enabled)
    {
        if (editorInput != null)
            editorInput.enableZoom = enabled;
    }

    public void ShowOverview()
    {
        selectedOrgan = null;

        if (organs != null)
        {
            foreach (OrganController organ in organs)
            {
                if (organ == null)
                    continue;

                organ.gameObject.SetActive(true);

                if (originalScales.ContainsKey(organ))
                    organ.transform.localScale = originalScales[organ];
                    
                if (originalPositions.ContainsKey(organ))
                    organ.transform.localPosition = originalPositions[organ];
                    
                if (originalRotations.ContainsKey(organ))
                    organ.transform.localRotation = originalRotations[organ];
            }
        }
        
        // Reset the camera back to its home 1.75m full body overview framing!
        if (Camera.main != null)
        {
            Camera.main.transform.position = new Vector3(0f, 1.35f, -1.8f);
            Camera.main.transform.LookAt(new Vector3(0f, 1.25f, 0f));
        }

        if (statusText != null)
            statusText.text = "Whole-body overview";

        if (infoTitleText != null)
            infoTitleText.text = "Whole Body Overview";

        if (infoBodyText != null)
            infoBodyText.text = OrganInfoCatalog.GetOverview();
    }

    void UpdateInfoPanel(OrganController.OrganType organType)
    {
        if (infoTitleText != null)
            infoTitleText.text = organType.ToString();

        if (infoBodyText != null)
            infoBodyText.text = OrganInfoCatalog.GetInfo(organType);
    }
}
