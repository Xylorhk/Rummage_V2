using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Trailer : MonoBehaviour
{
    public Transform parent;
    public Vector3 position;
    public Vector3 rotation;
    void Update()
    {
        gameObject.transform.parent = parent;
        gameObject.transform.localPosition = position;
        gameObject.transform.localRotation = Quaternion.Euler(rotation);
    }
}
