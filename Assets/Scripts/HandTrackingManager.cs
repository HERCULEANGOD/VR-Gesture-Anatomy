using UnityEngine;

public class HandTrackingManager : MonoBehaviour
{
    public Transform leftHand;
    public Transform rightHand;
    public float pinchThreshold = 0.05f;

#pragma warning disable CS0067 // The event is used by AnatomyUIManager
    public delegate void GestureEvent(Transform hand, bool isPinching);
    public event GestureEvent OnPinchGesture;
#pragma warning restore CS0067

    void Start()
    {
        // Hand tracking support has been removed for a clean base project.
    }

    void Update()
    {
        // Add custom input handling here if needed.
    }
}
