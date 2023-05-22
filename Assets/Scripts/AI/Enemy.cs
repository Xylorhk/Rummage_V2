using BasicTools.ButtonInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityStandardAssets.Characters.ThirdPerson;

public class Enemy : MonoBehaviour
{
    [Header("Movement Component Variables")]
    public NavMeshAgent nav;
    public ThirdPersonCharacter thirdPersonCharacter;

    [Header("Behavior Component Variables")]
    public EnemyStateMachine enemyStateMachine;
    public EnemyBehavior enemyBehavior;
    public FieldOfView enemyFOV;

    [Header("Other Component Variables")]
    public LayerMask enemyMask;
    public Animator anim;
    public Rigidbody primaryRigidbody;
    public Collider primaryCollider;
    public Health health;

    [Header("Juice Variables")]
    public Ragdoll ragdoll;
    public float deathTimer = 0;
    public float disintigrateSpeed = 1;

    [Header("Debugging")]
    [Button("Kill Enemy", "KillEnemy")]
    [SerializeField] bool _killBtn;
    public bool showAttackRadius = true;
    public bool showChaseRadius = true;
    public bool showFOV = true;

    private bool isAlive = true;
    private bool isDead = false;

    private float lowActivationLerp = 0;
    private float maxActivationLerp = 1;

    private List<Material> enemyMats = new List<Material>();

    private void Awake()
    {
        nav.updateRotation = false;
        
        health.OnHealthDepleated.AddListener(Die);
        ragdoll.GetAllRagdolls(primaryRigidbody, primaryCollider);

        GetAllMaterials();
    }

    public bool IsAlive()
    {
        return isAlive;
    }

    public void StopAttack()
    {
        anim.SetBool("IsAttacking", false);
        anim.ResetTrigger("Hit");
    }

    void ToggleRagdoll(bool shouldToggle)
    {
        primaryRigidbody.isKinematic = shouldToggle;
        primaryCollider.enabled = !shouldToggle;
        anim.enabled = !shouldToggle;
        
        nav.isStopped = shouldToggle;
        nav.enabled = !shouldToggle;

        ragdoll.ToggleRagdoll(shouldToggle);
    }

    /// <summary>
    /// Purely for debugging.
    /// </summary>
    private void KillEnemy()
    {
        health.TakeDamage(100);
    }

    public void Explode(float explosionForce, Vector3 explosionPosition, float explosionRadius)
    {
        if (Vector3.Distance(transform.position, explosionPosition) > explosionRadius)
        {
            return;
        }

        health.OnHealthDepleated.AddListener(delegate { ragdoll.ExplodeRagdoll(explosionForce, explosionPosition, explosionRadius); });
        health.TakeDamage(100);
    }

    public void SetStun()
    {
        enemyStateMachine.switchState(EnemyStateMachine.StateType.Stunned);
    }

    void GetAllMaterials()
    {
        foreach (Renderer rend in GetComponentsInChildren<Renderer>())
        {
            if (!enemyMats.Contains(rend.material))
            {
                enemyMats.Add(rend.material);
            }
        }
    }

    private void Disintigrate()
    {
        if (lowActivationLerp < maxActivationLerp - 0.05f)
        {
            lowActivationLerp = Mathf.Lerp(lowActivationLerp, maxActivationLerp, disintigrateSpeed * Time.deltaTime);

            for (int i = 0; i < enemyMats.Count; i++)
            {
                enemyMats[i].SetFloat("_DissolveAmount", lowActivationLerp);
            }
            return;
        }

        lowActivationLerp = maxActivationLerp;
        for (int i = 0; i < enemyMats.Count; i++)
        {
            enemyMats[i].SetFloat("_DissolveAmount", 1);
        }

        gameObject.SetActive(false);
    }

    private void Die()
    {
        isAlive = false;
        isDead = true;
        ToggleRagdoll(true);

        health.OnHealthDepleated.RemoveAllListeners();
        health.OnHealthDepleated.AddListener(Die);
    }

    void Update()
    {
        if (isDead)
        {
            deathTimer -= Time.deltaTime;
            if (deathTimer <= 0)
            {
                if (lowActivationLerp == 0)
                {
                    ragdoll.UnwrapRagdoll();
                }

                Disintigrate();
            }
        }
    }
}
