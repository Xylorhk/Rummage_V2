using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LandmineEffectorActions : EffectorActions
{
    public LandmineNEW landmine;

    public override void PowerEffectorAction()
    {
        if (!landmine.hasExploded)
        {
            landmine.ActivateExplosion();
        }
    }

    public override void ExplosiveAction(float explosionForce, Vector3 explosionPosition, float explosionRadius)
    {
        if (!landmine.hasExploded)
        {
            landmine.DoDelayExplosion(landmine.explosionDelay);
        }
    }
}
