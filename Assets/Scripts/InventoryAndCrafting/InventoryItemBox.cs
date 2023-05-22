using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventoryItemBox : MonoBehaviour
{
    public Image inventoryItemBackground;
    public Image inventoryItemIcon;

    public Sprite restoredBackground;
    public Sprite brokenBackground;

    public void SetInventoryIcon(Sprite itemIcon, bool isRestored)
    {
        if (isRestored)
        {
            inventoryItemBackground.sprite = restoredBackground;
        }
        else
        {
            inventoryItemBackground.sprite = brokenBackground;
        }

        inventoryItemIcon.sprite = itemIcon;
    }
}
