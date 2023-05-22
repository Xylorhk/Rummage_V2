using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class ScrapItem : Item
{
    [Range(1, 9999)]
    public int scrapAmount = 0;

    void Update()
    {
        if (itemType != TypeTag.scrap)
        {
            Debug.LogError($"{gameObject.name} is currently of {itemType} type and not Scrap!");
            return;
        }
    }
}
