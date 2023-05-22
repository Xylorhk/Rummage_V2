using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    public bool hasTriggered;
    public Transform spawnPoint;

    private void OnDrawGizmos()
    {
        if (spawnPoint == null)
        {
            return;
        }

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(spawnPoint.position, 0.2f);
        Gizmos.DrawLine(spawnPoint.position, spawnPoint.position + Vector3.up * 2f);
    }

    private void Awake()
    {
        BoxCollider collider = GetComponent<BoxCollider>();
        collider.isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        Player playerCheck = other.gameObject.GetComponent<Player>();

        if (playerCheck != null)
        {
            if (Player.Instance.IsAlive() && !Player.Instance.ragdoll.IsRagdolled())
            {
                if (!hasTriggered)
                {
                    Player.Instance.SetNewSpawnPoint(spawnPoint);
                    hasTriggered = true;
                }
            }
        }
    }

    private void OnTriggerStay(Collider other)
    {
        Player playerCheck = other.gameObject.GetComponent<Player>();

        if (playerCheck != null)
        {
            if (Player.Instance.IsAlive() && !Player.Instance.ragdoll.IsRagdolled())
            {
                if (!hasTriggered)
                {
                    Player.Instance.SetNewSpawnPoint(spawnPoint);
                    hasTriggered = true;
                }
            }
        }
    }
}
