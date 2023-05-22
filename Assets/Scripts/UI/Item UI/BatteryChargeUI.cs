using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BatteryChargeUI : SingletonMonoBehaviour<BatteryChargeUI>
{
    public GameObject batteryUIObj;
    public Image batteryFillImage;
    public float maxBatteryFill = 0.5f;

    new void Awake()
    {
        base.Awake();

        UpdateBatteryFill(100);
    }

    public void UpdateBatteryFill(float fill)
    {
        if (fill < 0)
        {
            Debug.Log("FILL ERROR, UNDER 0");
        }

        batteryFillImage.fillAmount = ExtensionMethods.Remap(fill, 0, 100, 0, maxBatteryFill);
    }

    public void ShowBatteryCharge(bool shouldShow)
    {
        batteryUIObj.SetActive(shouldShow);
    }
}
