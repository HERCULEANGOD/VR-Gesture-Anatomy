using UnityEngine;

public class OrganController : MonoBehaviour
{
    public enum OrganType { Heart, Brain, Lung, Liver }
    public OrganType organType;
    public Material organMaterial;
    public float mass = 1.0f;

    private Rigidbody rb;
    private MeshRenderer meshRenderer;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        meshRenderer = GetComponent<MeshRenderer>();

        if (rb == null) rb = gameObject.AddComponent<Rigidbody>();
        rb.useGravity = false;
        rb.isKinematic = true;
        rb.mass = mass;

        // Only apply procedural color if there's no real FBX model loaded as a child
        bool hasLoadedModel = transform.childCount > 0;

        if (meshRenderer != null && !hasLoadedModel)
        {
            if (organMaterial != null)
                meshRenderer.material = new Material(organMaterial);

            organMaterial = meshRenderer.material;
        }

        switch (organType)
        {
            case OrganType.Heart: rb.mass = 0.3f; break;
            case OrganType.Brain: rb.mass = 1.4f; break;
            case OrganType.Lung: rb.mass = 1.2f; break;
            case OrganType.Liver: rb.mass = 1.6f; break;
        }

        if (organMaterial != null && !hasLoadedModel)
        {
            organMaterial.EnableKeyword("_SUBSURFACE_SCATTERING");
            organMaterial.SetFloat("_Translucency", 0.5f);
            organMaterial.color = GetOrganColor(organType);
            organMaterial.SetColor("_Color", GetOrganColor(organType));

            if (organMaterial.HasProperty("_BaseColor"))
                organMaterial.SetColor("_BaseColor", GetOrganColor(organType));
        }
    }

    // ✅ GUARANTEED CLICK DETECTION — Unity calls this automatically when 
    // the user clicks on any GameObject that has a Collider. No raycasting needed.
    void OnMouseDown()
    {
        Debug.Log("🖱️ [OrganController] CLICKED on: " + organType);
        
        // Notify EditorInputSimulator
        EditorInputSimulator sim = FindObjectOfType<EditorInputSimulator>();
        if (sim != null)
            sim.NotifyOrganClicked(this);
        
        // Directly notify AnatomyLabController as backup
        AnatomyLabController lab = FindObjectOfType<AnatomyLabController>();
        if (lab != null)
            lab.DirectSelectOrgan(this);
            
        OnGrabbed();
    }
    
    void OnMouseUp()
    {
        OnReleased();
    }

    Color GetOrganColor(OrganType type)
    {
        switch (type)
        {
            case OrganType.Heart: return new Color(0.8f, 0.1f, 0.1f);
            case OrganType.Brain: return new Color(0.9f, 0.7f, 0.5f);
            case OrganType.Lung: return new Color(0.8f, 0.8f, 0.8f);
            case OrganType.Liver: return new Color(0.6f, 0.3f, 0.1f);
            default: return Color.white;
        }
    }

    public void OnGrabbed()
    {
        if (organMaterial != null)
            organMaterial.SetColor("_EmissionColor", Color.yellow * 0.5f);
    }

    public void OnReleased()
    {
        if (organMaterial != null)
            organMaterial.SetColor("_EmissionColor", Color.black);
    }
}
