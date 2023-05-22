using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MagneticForce : MonoBehaviour
{
    public GameObject Magnet;
    public float forceFactor;
    public bool magnetize = true;

    //Attach this script to the 'object' you would like to magnetize. Then attach the "Magnet" gameobject to what you want the 'object' to magnetize to.
    void Update()
    {
        if (magnetize)
        {
            Attract();
        }
        else
        {
            Repel();
        }
    }

    private void Attract()
    {
        GetComponent<Rigidbody>().AddForce((Magnet.transform.position - transform.position) * forceFactor * Time.smoothDeltaTime);
    }

    private void Repel()
    {
        GetComponent<Rigidbody>().AddForce((Magnet.transform.position + transform.position) * forceFactor * Time.smoothDeltaTime);
    }
}
