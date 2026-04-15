using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

/// <summary>
/// Interaction controller for the medical AMA (Ask Me Anything) chatbox.
/// </summary>
public class AIChatController : MonoBehaviour
{
    public InputField chatInput;
    public Text chatHistoryText;
    public Text chatTitleText;
    public Button sendButton;

    private string currentOrganContext = "Human Body";
    private List<string> history = new List<string>();

    public void SetCurrentOrgan(string organName)
    {
        currentOrganContext = organName;
        if (chatTitleText != null)
            chatTitleText.text = "💬 Ask about " + organName;
        
        // Brief history reset for new organ context
        history.Clear();
        if (chatHistoryText != null)
            chatHistoryText.text = "I am ready. Ask me anything about the " + organName + ".";
    }

    public void SendQuestion()
    {
        if (chatInput == null || string.IsNullOrEmpty(chatInput.text)) return;
        if (!GeminiService.Instance.IsConfigured()) return;

        string question = chatInput.text;
        chatInput.text = "";
        
        AddToHistory("You", question);
        AddToHistory("AI", "Thinking...");

        string prompt = "Context: The student is looking at the " + currentOrganContext + " in a 3D XR anatomy lab. " +
                        "Question: " + question + "\n" +
                        "Respond as a medical professor. Keep it under 100 words. No markdown.";

        GeminiService.Instance.SendPrompt(prompt,
            (response) => {
                UpdateLastAIEntry(response);
            },
            (error) => {
                UpdateLastAIEntry("⚠️ Sorry, I'm having trouble connecting: " + error);
            }
        );
    }

    void AddToHistory(string sender, string text)
    {
        history.Add("<b>" + sender + ":</b> " + text);
        UpdateDisplay();
    }

    void UpdateLastAIEntry(string text)
    {
        if (history.Count > 0)
            history[history.Count - 1] = "<b>AI:</b> " + text;
        UpdateDisplay();
    }

    void UpdateDisplay()
    {
        if (chatHistoryText != null)
            chatHistoryText.text = string.Join("\n\n", history);
    }
}
