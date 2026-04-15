using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// MULTI-AI SWITCHER: Automatically selects the best available Brain.
/// Priority: Local Ollama (0 latency) -> Google Gemini (Cloud) -> Hardcoded Fallback.
/// </summary>
public class GeminiService : MonoBehaviour
{
    public enum AIEngine { Auto, Ollama, Gemini, LocalFallback }

    [Header("Configuration")]
    public AIEngine selectedEngine = AIEngine.Auto;
    public string ollamaIP = "localhost"; // Change to your PC's IP (e.g. 192.168.1.5) if using a Headset
    public string ollamaModel = "llama3";
    public string geminiApiKey = ""; 
    
    private string OLLAMA_URL => $"http://{ollamaIP}:11434/api/generate";
    private string OLLAMA_PROBE => $"http://{ollamaIP}:11434/api/tags";
    private const string GEMINI_URL = "https://generativelanguage.googleapis.com/v1beta/models/gemini-1.5-flash:generateContent";
    
    private static GeminiService _instance;
    public static GeminiService Instance => _instance ??= FindObjectOfType<GeminiService>();

    private bool _isOllamaAvailable = false;
    public string CurrentStatus { get; private set; } = "Initializing...";

    void Awake() 
    { 
        _instance = this; 
        LoadAPIKeyFromConfig();
        StartCoroutine(ProbeOllama());
    }

    void LoadAPIKeyFromConfig()
    {
        string configPath = System.IO.Path.Combine(Application.dataPath, "gemini_config.txt");
        if (System.IO.File.Exists(configPath))
            geminiApiKey = System.IO.File.ReadAllText(configPath).Trim();
    }

    IEnumerator ProbeOllama()
    {
        CurrentStatus = "🔍 Searching for Local AI (Ollama)...";
        using (UnityWebRequest request = UnityWebRequest.Get(OLLAMA_PROBE))
        {
            request.timeout = 2;
            yield return request.SendWebRequest();
            _isOllamaAvailable = request.result == UnityWebRequest.Result.Success;
            
            if (_isOllamaAvailable) {
                CurrentStatus = "✅ Local AI Active (" + ollamaModel + ")";
                Debug.Log("🚀 [AI Brain] Ollama detected at " + ollamaIP + "! Running in Local-First mode.");
            } else {
                CurrentStatus = IsGeminiReady() ? "☁️ Cloud AI Active (Gemini)" : "📂 Safe-Mode (Offline)";
                Debug.LogWarning("⚠️ [AI Brain] Ollama not found at " + ollamaIP + ". Falling back to Cloud/Local Knowledge.");
            }
        }
    }

    private bool IsGeminiReady() => !string.IsNullOrEmpty(geminiApiKey) && geminiApiKey.Length > 20;
    public bool IsConfigured() => _isOllamaAvailable || IsGeminiReady();

    public void SendPrompt(string prompt, Action<string> onSuccess, Action<string> onError = null)
    {
        if (selectedEngine == AIEngine.Ollama || (selectedEngine == AIEngine.Auto && _isOllamaAvailable))
        {
            StartCoroutine(ExecuteOllamaRequest(prompt, onSuccess, (err) => {
                Debug.LogWarning("🕒 [AI Brain] Ollama timed out or failed. Trying Gemini Cloud...");
                StartCoroutine(ExecuteGeminiRequest(prompt, onSuccess, onError));
            }));
        }
        else
        {
            StartCoroutine(ExecuteGeminiRequest(prompt, onSuccess, onError));
        }
    }

    IEnumerator ExecuteOllamaRequest(string prompt, Action<string> onSuccess, Action<string> onFail)
    {
        string json = "{\"model\":\"" + ollamaModel + "\",\"prompt\":\"" + prompt.Replace("\"", "'") + "\",\"stream\":false}";
        
        using (UnityWebRequest request = new UnityWebRequest(OLLAMA_URL, "POST"))
        {
            byte[] body = Encoding.UTF8.GetBytes(json);
            request.uploadHandler = new UploadHandlerRaw(body);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            request.timeout = 15;

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                string response = request.downloadHandler.text;
                string extractedBody = ExtractPropertyValue(response, "response");
                if (string.IsNullOrEmpty(extractedBody)) extractedBody = response;
                
                onSuccess?.Invoke(extractedBody.Replace("\\n", "\n").Replace("\\\"", "\""));
            }
            else
            {
                onFail?.Invoke(request.error);
            }
        }
    }

    /// <summary>
    /// Robust property extraction for simple JSON without requiring external libraries.
    /// Handles escaped quotes and nested objects better than index-skipping.
    /// </summary>
    string ExtractPropertyValue(string json, string propertyName)
    {
        string search = "\"" + propertyName + "\":\"";
        int start = json.IndexOf(search);
        if (start == -1) return null;
        
        start += search.Length;
        // Find the closing quote, but skip escaped ones \"
        int end = -1;
        for (int i = start; i < json.Length; i++)
        {
            if (json[i] == '\"' && (i == 0 || json[i - 1] != '\\'))
            {
                end = i;
                break;
            }
        }
        
        if (end == -1) return null;
        return json.Substring(start, end - start);
    }

    IEnumerator ExecuteGeminiRequest(string prompt, Action<string> onSuccess, Action<string> onError)
    {
        if (string.IsNullOrEmpty(geminiApiKey))
        {
            onSuccess?.Invoke(GetLocalFallback(prompt));
            yield break;
        }

        string url = $"{GEMINI_URL}?key={geminiApiKey}";
        string cleanPrompt = prompt.Replace("\"", "'").Replace("\n", " ");
        string json = "{\"contents\":[{\"parts\":[{\"text\":\"" + cleanPrompt + "\"}]}]}";

        using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
        {
            byte[] body = Encoding.UTF8.GetBytes(json);
            request.uploadHandler = new UploadHandlerRaw(body);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            request.timeout = 10;

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                onSuccess?.Invoke(ParseGeminiResponse(request.downloadHandler.text));
            }
            else
            {
                onSuccess?.Invoke(GetLocalFallback(prompt));
            }
        }
    }

    string ParseGeminiResponse(string json)
    {
        try {
            int start = json.IndexOf("\"text\":") + 8;
            int end = json.IndexOf("\"", start);
            string result = json.Substring(start, end - start);
            return result.Replace("\\n", "\n").Replace("\\\"", "\"").Replace("\\\\", "\\").Replace("**", "");
        } catch { return GetLocalFallback("general"); }
    }

    string GetLocalFallback(string prompt)
    {
        if (prompt.Contains("Heart")) return "The HEART is a muscular organ that pumps blood. Function: Circulation.";
        if (prompt.Contains("Lung")) return "The LUNGS are for respiration. Location: Thoracic cavity.";
        return "Dissection Step: Isolate the target organ and identify key anatomical landmarks.";
    }
}
