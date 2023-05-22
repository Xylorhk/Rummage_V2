using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MultiGenerator : Electrical
{
    public List<ElectricalBox> generators;

    void TryPower()
    {
        if (generators.Count == 0)
        {
            if (IsPowered())
            {
                SetIsPowered(false);
            }
            
            return;
        }

        bool allGensOn = true;

        for (int i = 0; i < generators.Count; i++)
        {
            if(!generators[i].IsPowered())
            {
                allGensOn = false;
                break;
            }
        }

        if (allGensOn != IsPowered())
        {
            SetIsPowered(allGensOn);
        }
    }

    private void Update()
    {
        TryPower();
    }
}
