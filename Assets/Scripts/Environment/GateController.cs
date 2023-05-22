using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GateController : MonoBehaviour
{
    public bool openOnAwake = false;
    public Animator gateAnimator;
    public string openParameter;

    void Awake()
    {
        if (openOnAwake)
        {
            OpenGate();
        }
    }

    public void OpenGate()
    {
        gateAnimator.SetBool(openParameter, true);
    }

    public void CloseGate()
    {
        
        gateAnimator.SetBool(openParameter, false);
    }
}
