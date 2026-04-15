using UnityEngine;
using UnityEngine.EventSystems;
using System;

[RequireComponent(typeof(Camera))]
public class EditorInputSimulator : MonoBehaviour
{
    public LayerMask interactableLayer = ~0;
    public float dragSpeed = 10f;
    public float zoomSpeed = 2f;
    public float rotationSpeed = 100f;
    public bool enableSelection = true;
    public bool enableDrag = true;
    public bool enableZoom = true;
    public bool enableRotation = true;

    public event Action<OrganController> OnOrganSelected;

    private Transform selectedObject;
    private Plane dragPlane;
    private Vector3 dragOffset;
    private Camera attachedCamera;
    private Vector3 lastMousePosition;

    void Awake()
    {
        attachedCamera = GetComponent<Camera>();
    }

    void Update()
    {
        HandleZoom();
        HandleKeyboardSelection();
        HandleDrag();
        HandleRotation();
        lastMousePosition = Input.mousePosition;
    }

    // ✅ Called directly by OrganController.OnMouseDown() — guaranteed to work
    public void NotifyOrganClicked(OrganController organ)
    {
        if (organ == null) return;
        
        Debug.Log("[EditorInput] ✅ Organ clicked via OnMouseDown: " + organ.organType);
        selectedObject = organ.transform;
        
        // Set up drag plane for dragging
        Vector3 organScreenPos = attachedCamera.WorldToScreenPoint(organ.transform.position);
        dragPlane = new Plane(attachedCamera.transform.forward * -1f, organ.transform.position);
        dragOffset = Vector3.zero;
        
        OnOrganSelected?.Invoke(organ);
    }

    // ✅ KEYBOARD FALLBACK: Press 1=Heart, 2=Lungs, 3=Liver
    void HandleKeyboardSelection()
    {
        OrganController[] allOrgans = FindObjectsOfType<OrganController>();
        
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            foreach (var o in allOrgans)
                if (o.organType == OrganController.OrganType.Heart) { SelectOrgan(o); break; }
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            foreach (var o in allOrgans)
                if (o.organType == OrganController.OrganType.Lung) { SelectOrgan(o); break; }
        }
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            foreach (var o in allOrgans)
                if (o.organType == OrganController.OrganType.Liver) { SelectOrgan(o); break; }
        }
    }
    
    void SelectOrgan(OrganController organ)
    {
        Debug.Log("[EditorInput] ✅ Keyboard selected: " + organ.organType);
        selectedObject = organ.transform;
        OnOrganSelected?.Invoke(organ);
        organ.OnGrabbed();
        
        // Also directly notify the lab controller
        AnatomyLabController lab = FindObjectOfType<AnatomyLabController>();
        if (lab != null)
            lab.DirectSelectOrgan(organ);
    }

    void HandleZoom()
    {
        if (!enableZoom)
            return;

        float scroll = Input.mouseScrollDelta.y;
        if (Mathf.Abs(scroll) > 0.01f)
        {
            attachedCamera.transform.position += attachedCamera.transform.forward * scroll * zoomSpeed;
        }
    }

    void HandleDrag()
    {
        if (!enableDrag)
            return;

        if (Input.GetMouseButton(0) && selectedObject != null)
        {
            Ray ray = attachedCamera.ScreenPointToRay(Input.mousePosition);
            if (dragPlane.Raycast(ray, out float enter))
            {
                Vector3 hitPoint = ray.GetPoint(enter);
                selectedObject.position = Vector3.Lerp(selectedObject.position, hitPoint + dragOffset, Time.deltaTime * dragSpeed);
            }
        }
        
        if (Input.GetMouseButtonUp(0) && selectedObject != null)
        {
            var organ = selectedObject.GetComponent<OrganController>();
            if (organ != null)
                organ.OnReleased();
            selectedObject = null;
        }
    }

    void HandleRotation()
    {
        if (!enableRotation)
            return;

        if (Input.GetMouseButton(1) && selectedObject != null)
        {
            Vector3 delta = Input.mousePosition - lastMousePosition;
            selectedObject.Rotate(attachedCamera.transform.up, delta.x * rotationSpeed * Time.deltaTime, Space.World);
            selectedObject.Rotate(attachedCamera.transform.right, -delta.y * rotationSpeed * Time.deltaTime, Space.World);
        }
    }

    bool IsPointerOverUI()
    {
        return EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
    }
}
