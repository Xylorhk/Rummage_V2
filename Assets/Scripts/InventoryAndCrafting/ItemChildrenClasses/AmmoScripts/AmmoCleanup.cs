using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AmmoCleanup : MonoBehaviour
{
    private void OnDisable()
    {
        this.gameObject.GetComponent<Rigidbody>().velocity = Vector3.zero;
        this.gameObject.GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
        this.gameObject.GetComponent<Rigidbody>().isKinematic = true;
        this.gameObject.GetComponent<Rigidbody>().isKinematic = false;
    }
}
