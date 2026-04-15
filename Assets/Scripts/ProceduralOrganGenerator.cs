using UnityEngine;

public class ProceduralOrganGenerator : MonoBehaviour
{
    public OrganController.OrganType organType;
    public Material overrideMaterial;

    void Start()
    {
        GenerateOrgan();
    }

    void GenerateOrgan()
    {
        MeshFilter meshFilter = GetComponent<MeshFilter>();
        if (meshFilter == null)
            meshFilter = gameObject.AddComponent<MeshFilter>();

        MeshRenderer meshRenderer = GetComponent<MeshRenderer>();
        if (meshRenderer == null)
            meshRenderer = gameObject.AddComponent<MeshRenderer>();

        Mesh mesh = CreateOrganMesh();

        meshFilter.mesh = mesh;
        meshRenderer.material = CreateMaterial();

        ApplyOrganScale();
        ConfigureCollider();
    }

    Material CreateMaterial()
    {
        if (overrideMaterial != null)
            return new Material(overrideMaterial);

        Shader shader = Shader.Find("Standard");
        if (shader == null)
            shader = Shader.Find("Sprites/Default");

        Material material = new Material(shader);
        material.color = new Color(0.85f, 0.2f, 0.2f, 1f);
        return material;
    }

    Mesh CreateOrganMesh()
    {
        switch (organType)
        {
            case OrganController.OrganType.Heart:
                return ClonePrimitiveMesh(PrimitiveType.Sphere);
            case OrganController.OrganType.Brain:
                return ClonePrimitiveMesh(PrimitiveType.Sphere);
            case OrganController.OrganType.Lung:
                return ClonePrimitiveMesh(PrimitiveType.Capsule);
            case OrganController.OrganType.Liver:
                return ClonePrimitiveMesh(PrimitiveType.Cube);
            default:
                return ClonePrimitiveMesh(PrimitiveType.Sphere);
        }
    }

    Mesh ClonePrimitiveMesh(PrimitiveType primitiveType)
    {
        GameObject primitive = GameObject.CreatePrimitive(primitiveType);
        Mesh sourceMesh = primitive.GetComponent<MeshFilter>().sharedMesh;
        Mesh clonedMesh = Instantiate(sourceMesh);
        Destroy(primitive);
        return clonedMesh;
    }

    void ApplyOrganScale()
    {
        switch (organType)
        {
            case OrganController.OrganType.Heart:
                transform.localScale = new Vector3(0.7f, 0.85f, 0.7f);
                break;
            case OrganController.OrganType.Brain:
                transform.localScale = new Vector3(0.95f, 0.75f, 0.85f);
                break;
            case OrganController.OrganType.Lung:
                transform.localScale = new Vector3(0.8f, 1.15f, 0.6f);
                break;
            case OrganController.OrganType.Liver:
                transform.localScale = new Vector3(1.15f, 0.55f, 0.8f);
                break;
        }
    }

    void ConfigureCollider()
    {
        MeshCollider meshCollider = GetComponent<MeshCollider>();
        if (meshCollider != null)
            Destroy(meshCollider);

        SphereCollider sphereCollider = GetComponent<SphereCollider>();
        if (sphereCollider != null)
            Destroy(sphereCollider);

        CapsuleCollider capsuleCollider = GetComponent<CapsuleCollider>();
        if (capsuleCollider != null)
            Destroy(capsuleCollider);

        BoxCollider boxCollider = GetComponent<BoxCollider>();
        if (boxCollider != null)
            Destroy(boxCollider);

        switch (organType)
        {
            case OrganController.OrganType.Heart:
            case OrganController.OrganType.Brain:
                sphereCollider = gameObject.AddComponent<SphereCollider>();
                sphereCollider.radius = 0.5f;
                sphereCollider.center = Vector3.zero;
                break;

            case OrganController.OrganType.Lung:
                capsuleCollider = gameObject.AddComponent<CapsuleCollider>();
                capsuleCollider.radius = 0.35f;
                capsuleCollider.height = 1.2f;
                capsuleCollider.direction = 1;
                capsuleCollider.center = Vector3.zero;
                break;

            case OrganController.OrganType.Liver:
                boxCollider = gameObject.AddComponent<BoxCollider>();
                boxCollider.size = Vector3.one;
                boxCollider.center = Vector3.zero;
                break;
        }
    }
}
