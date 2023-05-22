using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemViewer : MonoBehaviour
{
    public Transform handAttachment;
    public Animator UIPlayerAnim;
    public float lerpSpeed = 1;
    private float currentValue = 0;

    public void SwitchPlayerAnimLayer(int index)
    {
        int count = Enum.GetValues(typeof(GripItem.GripType)).Length;
        currentValue = (float)index / (count - 1);
        UIPlayerAnim.SetFloat("Blend", currentValue);
    }

    private void LateUpdate()
    {
        //float currentBlend = UIPlayerAnim.GetFloat("Blend");
        //if ((currentBlend < (currentValue - 0.01f)) || (currentBlend > (currentValue + 0.01f)))
        //{
        //    currentBlend = Mathf.Lerp(currentBlend, currentValue, lerpSpeed * Time.unscaledDeltaTime);
        //    UIPlayerAnim.SetFloat("Blend", currentBlend);
        //}
    }
}
