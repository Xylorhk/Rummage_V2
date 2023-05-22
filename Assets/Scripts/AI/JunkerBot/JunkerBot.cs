using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class JunkerBot : MonoBehaviour
{
    [Header("Movement Component Variables")]
    public NavMeshAgent nav;

    [Header("Behavior Component Variables")]
    public JunkerStateMachine stateMachine;
    public JunkerBehavior behavior;
    public JunkerFOV junkerFOV;

    [Header("Other Component Variables")]
    public LayerMask junkerMask;
    public Animator anim;
    public JunkerScoop junkerScoop;
    public Rigidbody primaryRigidbody;
    public Collider primaryCollider;
    public Health health;

    [Header("Disabled Variables")]
    public GameObject rootObject;
    public float disabledTimer = 5f;
    private float maxDisabledTimer = 0f;

    [Header("Juice Variables")]
    [Range(5, 100)]
    public float playerScoopingForce = 30;
    public float deathTimer = 2f;
    public GameObject squirmVFX;
    public Renderer eyeRenderer;
    public Color neutralEyeColor = Color.blue;
    public Color aggresiveEyeColor = Color.red;
    public Color disabledEyeColor = Color.black;

    private Material currentMaterial;

    [Header("Debugging")]
    public bool showActRadius = true;
    public bool showChaseRadius = true;
    public bool showFOV = true;

    private bool isAlive = true;
    private bool isDead = false;
    private bool isDisabled = false;
    private bool isGrabbed = false;

    [HideInInspector] public bool shouldScoop = true;

    private void Awake()
    {
        maxDisabledTimer = disabledTimer;

        currentMaterial = eyeRenderer.material;

        health.OnHealthDepleated.AddListener(Die);
    }

    public bool IsAlive()
    {
        return isAlive;
    }

    public void StopAction()
    {
        anim.ResetTrigger("StartAction");
        nav.isStopped = false;

        
        shouldScoop = false;
        stateMachine.switchState(JunkerStateMachine.StateType.Patrol);

        junkerScoop.SetPlayerInRange(false);
    }

    public void ResetDisabledTimer()
    {
        disabledTimer = maxDisabledTimer;
    }

    public void GrabToggle(bool gotGrabbed)
    {
        isGrabbed = gotGrabbed;
        ResetDisabledTimer();

        anim.SetBool("IsPanicking", true);
        squirmVFX.SetActive(true);
    }

    public void ScoopPlayer()
    {
        Player.Instance.ragdoll.ExplodeRagdoll(playerScoopingForce, Player.Instance.transform.position, 2f);
    }

    public void ToggleActive(bool isActive)
    {
        primaryRigidbody.isKinematic = isActive;

        if (isActive)
        {
            nav.enabled = isActive;
            nav.isStopped = !isActive;
        }
        else
        {
            nav.isStopped = !isActive;
            nav.enabled = isActive;
        }

        isDisabled = !isActive;
        anim.SetBool("IsDisabled", !isActive);

        if (isActive)
        {
            gameObject.layer = transform.GetChild(0).gameObject.layer;
        }
        else
        {
            gameObject.layer = 0;
        }
    }

    public void KillJunker()
    {
        health.TakeDamage(100);
    }

    private void Die()
    {
        isAlive = false;
        isDead = true;

        health.OnHealthDepleated.RemoveAllListeners();
        health.OnHealthDepleated.AddListener(Die);
    }

    public void ChangeEmissionColor(Color targetColor)
    {
        currentMaterial.SetColor("_EmissionColor", targetColor);
    }

    void Update()
    {
        if (isDead)
        {
            deathTimer -= Time.deltaTime;
            if (deathTimer <= 0)
            {
                primaryCollider.enabled = false;
                gameObject.SetActive(false);
            }
        }

        if (junkerScoop.IsPlayerInRange())
        {
            if (stateMachine.GetCurrentState() != JunkerStateMachine.StateType.Act && !isDisabled && !Player.Instance.IsUnconscious() && !Player.Instance.ragdoll.IsRagdolled())
            {
                stateMachine.switchState(JunkerStateMachine.StateType.Act);
                Player.Instance.KnockOut();
            }
        }

        if (Player.Instance.vThirdPersonInput.CanMove() && !Player.Instance.IsUnconscious() && !shouldScoop)
        {
            shouldScoop = true;
        }

        if (isDisabled && !isGrabbed)
        {

            disabledTimer -= Time.deltaTime;
            if (disabledTimer <= 0)
            {
                ResetDisabledTimer();
                junkerScoop.scoopCollider.isTrigger = true;
                ToggleActive(true);

                anim.SetBool("IsPanicking", false);
                squirmVFX.SetActive(false);

                if (stateMachine.GetCurrentState() == JunkerStateMachine.StateType.Disabled)
                {
                    stateMachine.switchState(JunkerStateMachine.StateType.Patrol);
                }

                return;
            }

            if(junkerScoop.scoopCollider.isTrigger)
            {
                junkerScoop.scoopCollider.isTrigger = false;
            }
        }
    }
}
