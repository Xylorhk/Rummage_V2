using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyEffectorActions : EffectorActions
{
    public Enemy enemy;

    public override void PowerEffectorAction()
    {
        enemy.SetStun();
    }

    public override void ExplosiveAction(float explosionForce, Vector3 explosionPosition, float explosionRadius)
    {
        enemy.Explode(explosionForce, explosionPosition, explosionRadius);
    }
}
