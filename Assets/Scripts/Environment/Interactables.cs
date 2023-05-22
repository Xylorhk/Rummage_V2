using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Interactables : MonoBehaviour
{
    public bool shouldPulse = false;

    public Color rimColor;
    [Range(0, 4)]
    public float maxRimSize = 1.25f;
    [Range(4, 5)]
    public float minRimSize = 5;
    public float pausePulseTimer = 2f;
    public float pulseSpeed = 2;

    public Renderer mainRenderer;
    private Material material;
    private float currentRimSize = 0;
    private float maxPausePulseTimer = 0f;
    private bool pulseDown = true;

    private void Awake()
    {
        currentRimSize = maxRimSize;
        maxPausePulseTimer = pausePulseTimer;
        pulseDown = true;

        if (GetComponent<Renderer>())
        {
            material = GetComponent<Renderer>().material;
        }
        else
        {
            Debug.Log("in here");
            material = mainRenderer.material;
        }
        
        Color transparentColor = new Color(rimColor.r, rimColor.g, rimColor.b, 0);
        material.SetColor("_RimColor", transparentColor);
    }

    public void ShouldPulse(bool nShouldPulse)
    {
        shouldPulse = nShouldPulse;
    }

    void SwitchPulse()
    {
        pausePulseTimer -= Time.deltaTime;
        if (pausePulseTimer <= 0)
        {
            pulseDown = !pulseDown;
            pausePulseTimer = maxPausePulseTimer;
        }
    }

    private void Update()
    {
        
        Color transparentColor = new Color(rimColor.r, rimColor.g, rimColor.b, 0);
        if (shouldPulse)
        {
            if (material.GetColor("_RimColor") != rimColor)
            {
                material.SetColor("_RimColor", rimColor);
            }

            if (pulseDown)
            {
                if (currentRimSize < minRimSize - 0.05f)
                {
                    currentRimSize = Mathf.Lerp(currentRimSize, minRimSize, pulseSpeed * Time.deltaTime);
                    material.SetFloat("_RimSize", currentRimSize);
                }
                else
                {

                    SwitchPulse();
                }
            }
            else
            {
                if (currentRimSize > maxRimSize + 0.05f)
                {
                    currentRimSize = Mathf.Lerp(currentRimSize, maxRimSize, pulseSpeed * Time.deltaTime);
                    material.SetFloat("_RimSize", currentRimSize);
                }
                else
                {
                    SwitchPulse();
                }
            }
        }
        else
        {
            if (material.GetColor("_RimColor") != transparentColor)
            {
                material.SetColor("_RimColor", transparentColor);
            }

            currentRimSize = maxRimSize;
            pulseDown = true;
        }
    }
}
