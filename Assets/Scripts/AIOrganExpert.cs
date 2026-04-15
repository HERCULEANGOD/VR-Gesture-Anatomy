using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// AI Organ Expert — Queries Gemini for detailed 
/// educational medical info whenever a student selects an organ.
/// </summary>
public class AIOrganExpert : MonoBehaviour
{
    public Text infoTitleText;
    public Text infoBodyText;
    public Text statusText;

    private OrganController.OrganType currentOrgan;
    private string[] cache = new string[4]; // Heart, Brain, Lung, Liver
    private bool isQuerying = false;

    public void OnOrganSelected(OrganController.OrganType organType)
    {
        currentOrgan = organType;
        if (isQuerying) return; // Prevent spamming!

        if (infoTitleText != null)
            infoTitleText.text = "🔬 " + organType.ToString() + " (AI Analysis)";

        if (infoBodyText != null)
            infoBodyText.text = "🔄 Connecting to medical AI... please wait.";

        // Check local cache first (saves API tokens)
        int idx = (int)organType;
        if (idx < cache.Length && !string.IsNullOrEmpty(cache[idx]))
        {
            DisplayInfo(organType, cache[idx]);
            return;
        }

        if (GeminiService.Instance == null || !GeminiService.Instance.IsConfigured())
        {
            DisplayInfo(organType, "AI Connection Error. Check Key.");
            return;
        }

        string prompt = "You are a medical anatomy expert. The student selected the " + organType.ToString() + ". " +
                        "Briefly explain its: 1. Location, 2. Primary Function, 3. One critical clinical condition " +
                        "related to it. Keep it under 120 words. Use clear medical terms. No markdown formatting.";

        isQuerying = true;
        GeminiService.Instance.SendPrompt(prompt,
            (response) => {
                isQuerying = false;
                if (idx < cache.Length) cache[idx] = response;
                DisplayInfo(organType, response);
            },
            (error) => {
                isQuerying = false;
                DisplayInfo(organType, "⚠️ AI Connection Error: " + error);
            }
        );
    }

    void DisplayInfo(OrganController.OrganType organType, string text)
    {
        if (infoBodyText != null) infoBodyText.text = text;
        if (statusText != null) statusText.text = "Spatial Info: " + organType.ToString() + " analyzed.";
    }

    public void LearnMore()
    {
        string url = "";
        
        // 🔬 Switch to professional medical resources instead of basic Wikipedia
        switch (currentOrgan)
        {
            case OrganController.OrganType.Heart:
                url = "https://teachmeanatomy.info/thorax/organs/heart/";
                break;
            case OrganController.OrganType.Brain:
                url = "https://teachmeanatomy.info/neuroanatomy/structures/cerebrum/";
                break;
            case OrganController.OrganType.Lung:
                url = "https://teachmeanatomy.info/thorax/organs/lungs/";
                break;
            case OrganController.OrganType.Liver:
                url = "https://teachmeanatomy.info/abdomen/viscera/liver/";
                break;
            default:
                url = "https://medlineplus.gov/anatomy.html";
                break;
        }

        Application.OpenURL(url);
        Debug.Log("[AI Expert] Redirecting to premium medical resource: " + url);
    }
}
