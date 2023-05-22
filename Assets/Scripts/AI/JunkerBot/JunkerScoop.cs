using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JunkerScoop : MonoBehaviour
{
    public Collider scoopCollider;
    private bool playerInRange = false;

    public bool IsPlayerInRange()
    {
        return playerInRange;
    }

    public void SetPlayerInRange(bool shouldPlayerBeInRange)
    {
        playerInRange = shouldPlayerBeInRange;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (playerInRange)
        {
            return;
        }

        Player playerCheck = null;
        playerCheck = other.transform.root.GetComponentInChildren<Player>();

        if (playerCheck != null)
        {
            if (Player.Instance.vThirdPersonInput.CanMove() && !Player.Instance.IsUnconscious())
            {
                playerInRange = true;
            }
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (playerInRange)
        {
            return;
        }

        Player playerCheck = null;
        playerCheck = other.transform.root.GetComponentInChildren<Player>();

        if (playerCheck != null)
        {
            if (Player.Instance.vThirdPersonInput.CanMove() && !Player.Instance.IsUnconscious())
            {
                playerInRange = true;
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!playerInRange)
        {
            return;
        }

        Player playerCheck = null;
        playerCheck = other.transform.root.GetComponentInChildren<Player>();

        if (playerCheck != null)
        {
            playerInRange = false;
        }
    }
}
