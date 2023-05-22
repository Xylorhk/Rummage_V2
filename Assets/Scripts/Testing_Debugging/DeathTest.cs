using BasicTools.ButtonInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeathTest : MonoBehaviour
{
    public float explosionForce;
    public float explosionRadius;

    public Transform spawnPoint;

    [Button("Kill Player", "KillPlayer")]
    [SerializeField]
    public bool _killBtn;

    [Button("Explode Player", "ExplodePlayer")]
    [SerializeField]
    public bool _explodeBtn;

    [Button("Revive Player", "RevivePlayer")]
    [SerializeField]
    public bool _reviveBtn;

    [Button("Set New Spawn Point", "NewSpawnPoint")]
    [SerializeField]
    public bool _spawnBtn;

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }

    public void KillPlayer()
    {
        Player.Instance.health.TakeDamage(100);
    }

    public void ExplodePlayer()
    {
        Player.Instance.Explode(explosionForce, transform.position, explosionRadius);
    }

    public void RevivePlayer()
    {
        Player.Instance.Respawn();
    }

    public void NewSpawnPoint()
    {
        Debug.Log("called");
        Player.Instance.SetNewSpawnPoint(spawnPoint);
    }
}
