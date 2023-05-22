using System.Collections;
using UnityEngine;

public class Explosive : MonoBehaviour
{
    [Header("Explosion Variables")]
    public float explosionRadius = 3;
    public float explosionForce = 30;
    public float explosionDelay = 0.1f;
    public bool hasExploded = false;
    public string bombTag = "bomb";

    private float destroyDelay = 1f;
    private bool hasPlayedJuice = false;

    ObjectPooler.Key explosionParticleKey = ObjectPooler.Key.ExplosionParticle;

    protected void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }

    //Searches for nearby object in a defined radius and applies a force to those objects
    protected void Explode()
    {
        hasExploded = true;

        MeshRenderer meshRenderer = GetComponent<MeshRenderer>();
        if (meshRenderer != null)
        {
            meshRenderer.enabled = false;
        }

        PlayJuice();
        hasPlayedJuice = true;

        Collider[] colliders = Physics.OverlapSphere(transform.position, explosionRadius);
        foreach (Collider nearbyObject in colliders)
        {
            if (nearbyObject == Player.Instance.primaryCollider)
            {
                Player.Instance.Explode(explosionForce, transform.position, explosionRadius);
                continue;
            }

            EffectorActions effectorActions = nearbyObject.GetComponent<EffectorActions>();
            if (effectorActions != null)
            {
                effectorActions.ExplosiveAction(explosionForce, transform.position, explosionRadius);
                continue;
            }

            Rigidbody rb = nearbyObject.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.AddExplosionForce(explosionForce, transform.position, explosionRadius);
            }

            Explosive explosive = nearbyObject.GetComponent<Explosive>();
            if (explosive != null)
            {
                if (!explosive.hasExploded)
                {
                    explosive.DoDelayExplosion(explosionDelay);
                }
            }
        }

        DisableAfterTime(gameObject, destroyDelay);
    }

    private void PlayJuice()
    {
        if (hasPlayedJuice)
        {
            return;
        }

        int randNumb = Random.Range(1, 4);
        string exlosionSFX = $"Explosion{randNumb}";
        AudioManager.Get().PlayFromPool(exlosionSFX, transform.position, "Sound");
        GameObject spawnedParticle = ObjectPooler.GetPooler(explosionParticleKey).GetPooledObject();
        spawnedParticle.transform.position = transform.position;
        spawnedParticle.transform.rotation = transform.rotation;
        spawnedParticle.SetActive(true);

        DisableAfterTime(spawnedParticle, 1);
    }

    protected void ResetExplosive()
    {
        GetComponent<MeshRenderer>().enabled = true;
        hasExploded = false;
        hasPlayedJuice = false;
    }

    void DisableAfterTime(GameObject objectToDisable, float time = 0)
    {
        StartCoroutine(DisableEnum(time, objectToDisable));
    }

    IEnumerator DisableEnum(float disableTime, GameObject objectToDisable)
    {
        yield return new WaitForSeconds(disableTime);

        objectToDisable.SetActive(false);
    }

    public void DoDelayExplosion(float delayTime)
    {
        StartCoroutine(DelayExplosion(explosionDelay));
    }

    IEnumerator DelayExplosion(float delay)
    {
        yield return new WaitForSeconds(delay);

        Explode();
    }
}
