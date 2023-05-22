using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DetectableObject : MonoBehaviour
{
    private MeshRenderer meshRenderer;

    private void Awake()
    {
        meshRenderer = GetComponent<MeshRenderer>();
    }

    public void ActivateMeshRenderer(bool shouldActivate)
    {
        meshRenderer.enabled = shouldActivate;
    }
}
