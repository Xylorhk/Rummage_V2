using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShaderTest : MonoBehaviour
{
    public List<Material> shaderMats;
    [Range(0, 5)]
    public float radius = 0;

    private void Awake()
    {
        for (int i = 0; i < shaderMats.Count; i++) {
            shaderMats[i].SetFloat("_StealthRadius", radius);
            shaderMats[i].SetVector("_StealthCenter", transform.position);
            shaderMats[i].renderQueue = 3000;
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, radius);
    }

    private void Update()
    {
        for (int i = 0; i < shaderMats.Count; i++)
        {
            shaderMats[i].SetFloat("_StealthRadius", radius);
            shaderMats[i].SetVector("_StealthCenter", transform.position);
        }
    }
}
