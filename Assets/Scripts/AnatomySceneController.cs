using UnityEngine;
using UnityEngine.SceneManagement;

public class AnatomySceneController : MonoBehaviour
{
    public GameObject[] organs;
    public bool showAllOnStart = true;
    private int currentOrganIndex = 0;

    void Start()
    {
        if (showAllOnStart)
        {
            ShowAllOrgans();
            return;
        }

        LoadOrgan(currentOrganIndex);
    }

    public void ShowAllOrgans()
    {
        if (organs == null || organs.Length == 0)
        {
            Debug.LogWarning("AnatomySceneController: No organs assigned.");
            return;
        }

        foreach (GameObject organ in organs)
        {
            if (organ != null)
                organ.SetActive(true);
        }
    }

    public void LoadOrgan(int index)
    {
        if (organs == null || organs.Length == 0)
        {
            Debug.LogWarning("AnatomySceneController: No organs assigned.");
            return;
        }

        // Hide all organs
        foreach (GameObject organ in organs)
        {
            if (organ != null)
                organ.SetActive(false);
        }

        // Show selected organ
        if (index >= 0 && index < organs.Length)
        {
            if (organs[index] != null)
                organs[index].SetActive(true);
        }

        currentOrganIndex = index;
    }

    public void NextOrgan()
    {
        if (organs == null || organs.Length == 0)
            return;

        LoadOrgan((currentOrganIndex + 1) % organs.Length);
    }

    public void PreviousOrgan()
    {
        if (organs == null || organs.Length == 0)
            return;

        LoadOrgan((currentOrganIndex - 1 + organs.Length) % organs.Length);
    }
}
