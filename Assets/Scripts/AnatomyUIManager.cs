using UnityEngine;
using UnityEngine.UI;

public class AnatomyUIManager : MonoBehaviour
{
    public GameObject uiPanel;
    public Text statusText;
    public Button[] controlButtons;

    private HandTrackingManager handTracker;

    void Start()
    {
        handTracker = FindObjectOfType<HandTrackingManager>();
        if (handTracker != null)
            handTracker.OnPinchGesture += OnPinchGesture;

        UpdateUI();
    }

    void UpdateUI()
    {
        if (statusText == null)
        {
            Debug.Log("AnatomyUIManager: statusText is not assigned.");
            return;
        }

        statusText.text = "ANATOMY CONTROLS\n\n" +
                         "👆 Point to Select\n" +
                         "✊ Grab to Move\n" +
                         "🔄 Rotate Hand to Spin\n" +
                         "🤏 Pinch to Zoom In\n" +
                         "🖐️ Spread to Zoom Out\n" +
                         "✂️ Swipe to Dissect\n" +
                         "📍 Pin in Place\n" +
                         "🔙 Reset View";
    }

    void OnPinchGesture(Transform hand, bool isPinching)
    {
        if (statusText != null)
            statusText.text = isPinching ? "Pinching detected!" : "Pinch released";
    }

    public void OnSelectOrgan()
    {
        if (statusText != null)
            statusText.text = "Organ selected";
    }

    public void OnDissect()
    {
        if (statusText != null)
            statusText.text = "Dissecting...";
    }

    public void OnReset()
    {
        if (statusText != null)
            statusText.text = "View reset";
    }
}
