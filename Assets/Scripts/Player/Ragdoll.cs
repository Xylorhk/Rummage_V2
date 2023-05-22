using BasicTools.ButtonInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ragdoll : MonoBehaviour
{
    private List<Rigidbody> ragdollRigidbodies = new List<Rigidbody>();
    [HideInInspector] public List<Collider> ragdollColliders = new List<Collider>();
    private bool isRagdolled = false;

    public void GetAllRagdolls(Rigidbody primaryRigidbody, Collider primaryCollider)
    {
        foreach (Rigidbody rb in gameObject.GetComponentsInChildren<Rigidbody>())
        {
            if (rb == primaryRigidbody)
            {
                continue;
            }

            rb.isKinematic = true;
            ragdollRigidbodies.Add(rb);
        }

        foreach (Collider collider in gameObject.GetComponentsInChildren<Collider>())
        {
            if (collider == primaryCollider)
            {
                continue;
            }

            collider.enabled = false;
            ragdollColliders.Add(collider);
        }
    }

    public void ToggleRagdoll(bool shouldToggle)
    {
        for (int i = 0; i < ragdollColliders.Count; i++)
        {
            ragdollColliders[i].enabled = shouldToggle;
        }

        for (int i = 0; i < ragdollRigidbodies.Count; i++)
        {
            ragdollRigidbodies[i].isKinematic = !shouldToggle;
        }

        isRagdolled = shouldToggle;
    }

    public void UnwrapRagdoll()
    {
        for (int i = 0; i < ragdollColliders.Count; i++)
        {
            ragdollColliders[i].enabled = false;
        }

        for (int i = 0; i < ragdollRigidbodies.Count; i++)
        {
            ragdollRigidbodies[i].isKinematic = true;
        }
    }

    public bool IsRagdolled()
    {
        return isRagdolled;
    }

    public void ExplodeRagdoll(float explosionForce, Vector3 explosionPosition, float explosionRadius)
    {
        if (ragdollRigidbodies[0] != null)
        {
            if (ragdollRigidbodies[0].isKinematic)
            {
                Debug.Log("Ragdoll Rigidbodies are kinematic");
                return;
            }
        }
        else
        {
            return;
        }

        for (int i = 0; i < ragdollRigidbodies.Count; i++)
        {
            ragdollRigidbodies[i].AddExplosionForce(explosionForce, explosionPosition, explosionRadius, 5f, ForceMode.Impulse);
        }
    }

    public void ApplyRagdollForce(Vector3 velocityDir, float velocityMagnitude)
    {
        velocityDir = velocityDir.normalized;
        
        if (ragdollRigidbodies[0] != null)
        {
            if (ragdollRigidbodies[0].isKinematic)
            {
                Debug.Log("Ragdoll Rigidbodies are kinematic");
                return;
            }
        }
        else
        {
            return;
        }

        for (int i = 0; i < ragdollRigidbodies.Count; i++)
        {
            ragdollRigidbodies[i].velocity = velocityDir * velocityMagnitude;
        }
    }

    public float TotalRigidbodyMagnitude()
    {
        float totalMag = 0;

        for (int i = 0; i < ragdollRigidbodies.Count; i++)
        {
            totalMag += Mathf.Abs(ragdollRigidbodies[0].velocity.magnitude);
        }

        return totalMag;
    }
}
