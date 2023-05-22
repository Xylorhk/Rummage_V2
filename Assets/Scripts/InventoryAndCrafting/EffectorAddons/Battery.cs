using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Battery : MonoBehaviour
{
    public float rateOfBatteryDrain = 10f;
    public float rateOfBatteryFill = 20f;

    private bool isBatteryDraining = false;
    private float currentBatteryFill;
    private float maxBatteryFill = 100;

    private void Awake()
    {
        currentBatteryFill = maxBatteryFill;
    }

    public void ShouldDrainBattery(bool shouldDrain)
    {
        isBatteryDraining = shouldDrain;
    }

    public bool GetBatteryDrainStatus()
    {
        return isBatteryDraining;
    }

    public float GetCurrentFill()
    {
        return currentBatteryFill;
    }

    private void Update()
    {
        if (currentBatteryFill > 0 && isBatteryDraining)
        {
            currentBatteryFill -= Time.deltaTime * rateOfBatteryDrain;
        }
        else
        {
            if (isBatteryDraining)
            {
                currentBatteryFill = 0;
            }
        }

        if (currentBatteryFill < maxBatteryFill && !isBatteryDraining)
        {
            currentBatteryFill += Time.deltaTime * rateOfBatteryFill;
        }
        else
        {
            if (!isBatteryDraining)
            {
                currentBatteryFill = maxBatteryFill;
            }
        }

        BatteryChargeUI.Instance.UpdateBatteryFill(currentBatteryFill);
    }
}
