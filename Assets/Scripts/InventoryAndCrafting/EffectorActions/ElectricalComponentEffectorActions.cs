using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ElectricalComponentEffectorActions : EffectorActions
{
    public Electrical electricalComponent;

    public override void OnPowerEffectorStartHit()
    {
        electricalComponent.OnPowerStart?.Invoke();
    }

    public override void OnPowerEffectorStopHit()
    {
        electricalComponent.OnPowerStop?.Invoke();
    }

    public override void PowerEffectorAction()
    {
        if (!electricalComponent.IsPowered())
        {
            electricalComponent.SetIsPowered(true);
        }
    }

    public override void EMPEffectorAction()
    {
        if (electricalComponent.IsPowered())
        {
            electricalComponent.SetIsPowered(false);
        }
    }
}
