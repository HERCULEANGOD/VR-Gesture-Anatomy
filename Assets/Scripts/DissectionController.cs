using UnityEngine;
using System.Collections.Generic;

public class DissectionController : MonoBehaviour
{
    public GameObject[] dissectionLayers; // Layer 0: outer, Layer 1: muscle, etc.
    private int currentLayer = 0;

    void Start()
    {
        EnsureLayersAssigned();

        if (dissectionLayers == null || dissectionLayers.Length == 0)
            return;

        currentLayer = Mathf.Clamp(currentLayer, 0, dissectionLayers.Length - 1);
        UpdateLayers();
    }

    public void NextLayer()
    {
        if (dissectionLayers == null || dissectionLayers.Length == 0)
            return;

        if (currentLayer < dissectionLayers.Length - 1)
        {
            currentLayer++;
            UpdateLayers();
        }
    }

    public void PreviousLayer()
    {
        if (dissectionLayers == null || dissectionLayers.Length == 0)
            return;

        if (currentLayer > 0)
        {
            currentLayer--;
            UpdateLayers();
        }
    }

    void EnsureLayersAssigned()
    {
        if (dissectionLayers != null && dissectionLayers.Length > 0)
            return;

        if (transform.childCount == 0)
            return;

        List<GameObject> childLayers = new List<GameObject>();
        for (int i = 0; i < transform.childCount; i++)
        {
            GameObject child = transform.GetChild(i).gameObject;
            if (child != null)
                childLayers.Add(child);
        }

        if (childLayers.Count > 0)
            dissectionLayers = childLayers.ToArray();
    }

    void UpdateLayers()
    {
        if (dissectionLayers == null || dissectionLayers.Length == 0)
            return;

        for (int i = 0; i < dissectionLayers.Length; i++)
        {
            if (dissectionLayers[i] != null)
            {
                dissectionLayers[i].SetActive(i <= currentLayer);
                if (i == currentLayer)
                {
                    // Fade outer layers
                    MeshRenderer mr = dissectionLayers[i].GetComponent<MeshRenderer>();
                    if (mr != null)
                        mr.material.color = new Color(1, 1, 1, 0.3f); // Semi-transparent
                }
            }
        }
    }
}
