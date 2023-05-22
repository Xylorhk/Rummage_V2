using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LandmineNEW : Explosive
{
    public bool turnOnDetectorMat = false;
    public Material detectorMat;

    private void Awake()
    {
        if (turnOnDetectorMat)
        {
            gameObject.GetComponent<Renderer>().material = detectorMat;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other != this.gameObject)
        {
            if (!hasExploded)
            {
                ActivateExplosion();
            }
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other != this.gameObject)
        {
            if (!hasExploded)
            {
                ActivateExplosion();
            }
        }
    }

    public void ActivateExplosion()
    {
        hasExploded = true;
        Explode();
    }
}
