using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EffectorActions : MonoBehaviour
{
    public virtual void PowerEffectorAction() { }

    public virtual void OnPowerEffectorStartHit() { }

    public virtual void OnPowerEffectorStopHit() { }

    public virtual void GrabberEffectorAction() { }

    public virtual void ExplosiveAction(float explosionForce, Vector3 explosionPosition, float explosionRadius) { }

    public virtual void EMPEffectorAction() { }
}
