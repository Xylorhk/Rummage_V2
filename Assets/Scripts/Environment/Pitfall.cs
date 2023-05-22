using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pitfall : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (Player.Instance.IsAlive())
        {
            Player playerCheck = other.gameObject.GetComponent<Player>();
            if (playerCheck != null)
            {
                playerCheck.FallDeath();
                return;
            }
        }

        JunkerBot junkerCheck = other.gameObject.GetComponent<JunkerBot>();
        if (junkerCheck != null)
        {
            if (junkerCheck.IsAlive())
            {
                junkerCheck.KillJunker();
                return;
            }
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (Player.Instance.IsAlive())
        {
            Player playerCheck = other.gameObject.GetComponent<Player>();
            if (playerCheck != null)
            {
                playerCheck.FallDeath();
                return;
            }
        }

        JunkerBot junkerCheck = other.gameObject.GetComponent<JunkerBot>();
        if (junkerCheck != null)
        {
            if (junkerCheck.IsAlive())
            {
                junkerCheck.KillJunker();
                return;
            }
        }
    }
}
