using BasicTools.ButtonInspector;
using Invector.vCharacterController;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : SingletonMonoBehaviour<Player>, ISaveable
{
    [Header("Third Person Component Variables")]
    public vThirdPersonController vThirdPersonController;
    public vThirdPersonInput vThirdPersonInput;
    public vThirdPersonCamera vThirdPersonCamera;
    [HideInInspector]
    public PlayerInput playerInput;

    [Header("Other Component Variables")]
    public GameObject rootObj;
    public Animator anim;
    public UnityEngine.Animations.Rigging.Rig aimingRig;
    public Rigidbody primaryRigidbody;
    public Collider primaryCollider;
    public Health health;
    public Pickup pickup;
    public ItemDrop itemDropper;
    public PlayerItemHandler itemHandler;

    [Header("Death & Unconscious Variables")]
    public PanelComponentFade panelFade;
    public float deathCamRotSpeed = 2f;
    public float deathTime = 3;

    [Header("Audio Variables")]
    public string leftStepSFX = string.Empty;
    public string rightStepSFX = string.Empty;
    public string deathSFX = string.Empty;

    [Header("Ragdoll Variables")]
    public Ragdoll ragdoll;
    public Transform deathCameraTarget;
    public UnityEngine.AI.NavMeshObstacle mainNavObs;
    public Transform headBone;
    public Transform backBone;
    public Transform footBone;
    public float ragdollRaycastDistance = 0;

    [Header("Juice Variables")]
    public BackpackFill backpackFill;
    public Emoter playerEmoter;

    [Header("Personal Player Variables")]
    public Transform origin = null;

    private bool isAlive = true;
    private bool isUnconscious = false;
    private bool isDeadFalling = false;

    Transform[] childTransforms;
    private bool ragdollCheck = false;
    private bool resetRagdoll = false;
    private bool resetCameraHeight = false;
    private float ragdollTimer = 2f;

    private float originalCameraHeight;

    [Header("Debug Buttons")]
    [Button("Ragdoll Player", "KnockOut")]
    [SerializeField]
    public bool _killBtn;

    public object CaptureState()
    {
        return new SaveData
        {
            backpackFillSizeWeights = backpackFill.GetBlendWeights()
        };
    }

    public void RestoreState(object state)
    {
        var saveData = (SaveData)state;

        backpackFill.SetBlendWeights(saveData.backpackFillSizeWeights);
    }

    [Serializable]
    private struct SaveData
    {
        public List<float> backpackFillSizeWeights;
    }

    new void Awake()
    {
        base.Awake();

        health.OnHealthDepleated.AddListener(Die);
        health.OnHealthRestored.AddListener(Revived);
        health.OnHealthRestored.AddListener(ResetVariables);

        childTransforms = gameObject.GetComponentsInChildren<Transform>();
        ragdoll.GetAllRagdolls(primaryRigidbody, primaryCollider);
        originalCameraHeight = vThirdPersonCamera.height;

        playerEmoter.InitEmoter(anim, true);

        if (origin == null)
        {
            GameObject originPoint = new GameObject("OriginPoint");
            originPoint.transform.parent = transform.root;

            originPoint.transform.position = transform.position;
            originPoint.transform.rotation = transform.rotation;
            origin = originPoint.transform;
        }
    }

    private void Start()
    {
        playerInput = GetComponent<PlayerInput>();
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(backBone.position, backBone.position + backBone.up * ragdollRaycastDistance);

        if (isUnconscious)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(headBone.position, headBone.position - (Vector3.up * ragdollRaycastDistance));
            Gizmos.DrawLine(backBone.position, backBone.position - (Vector3.up * ragdollRaycastDistance));
            Gizmos.DrawLine(footBone.position, footBone.position - (Vector3.up * ragdollRaycastDistance));
        }
    }

    void ToggleRagdoll(bool shouldToggle)
    {
        if (shouldToggle)
        {
            primaryRigidbody.collisionDetectionMode = CollisionDetectionMode.Discrete;
            primaryRigidbody.isKinematic = shouldToggle;
            mainNavObs.enabled = true;
        }
        else
        {
            primaryRigidbody.isKinematic = shouldToggle;
            primaryRigidbody.collisionDetectionMode = CollisionDetectionMode.Continuous;
        }

        primaryCollider.enabled = !shouldToggle;
        anim.enabled = !shouldToggle;


        if ((shouldToggle && !ragdoll.IsRagdolled()) || (!shouldToggle && ragdoll.IsRagdolled()))
        {
            ragdoll.ToggleRagdoll(shouldToggle);
        }
    }

    public void Explode(float explosionForce, Vector3 explosionPosition, float explosionRadius)
    {
        if (Vector3.Distance(transform.position, explosionPosition) > explosionRadius)
        {
            return;
        }

        health.OnHealthDepleated.AddListener(delegate { ragdoll.ExplodeRagdoll(explosionForce, explosionPosition, explosionRadius); });
        health.TakeDamage(health.maxHealth);

        if (itemHandler.GetEquippedItem())
        {
            health.OnHealthDepleated.AddListener(delegate { itemHandler.GetEquippedItem().gameObject.GetComponent<Rigidbody>().AddExplosionForce(explosionForce / 4, explosionPosition, explosionRadius, 0.0f, ForceMode.Impulse); });
        }
    }

    public void FallDeath()
    {
        isDeadFalling = true;
        vThirdPersonCamera.target = null;

        health.OnHealthDepleated.AddListener(delegate { ragdoll.ApplyRagdollForce(primaryRigidbody.velocity, primaryRigidbody.velocity.magnitude); });
        health.TakeDamage(health.maxHealth);
    }

    public void RagdollPlayer()
    {
        if (!ragdoll.IsRagdolled())
        {
            vThirdPersonCamera.SetTarget(deathCameraTarget);
            if (vThirdPersonController.isGrounded)
            {
                vThirdPersonCamera.height = deathCameraTarget.localPosition.y;
            }
            else
            {
                vThirdPersonCamera.height = deathCameraTarget.localPosition.y;
            }

            vThirdPersonInput.ShouldMove(false);

            ToggleRagdoll(true);
        }
    }

    public void KnockOut()
    {
        if (ragdoll.IsRagdolled())
        {
            return;
        }

        health.TakeDamage(1);

        RagdollPlayer();
        itemDropper.DropItems(transform.position, backBone.position.y, 1, 3);

        isUnconscious = true;
    }

    public bool IsAlive()
    {
        return isAlive;
    }

    public bool IsUnconscious()
    {
        return isUnconscious;
    }

    public void SetNewSpawnPoint(Transform spawnPoint)
    {
        origin.position = spawnPoint.position;
        origin.rotation = spawnPoint.rotation;
    }

    public void Spawn(Transform spawnPoint = null)
    {
        if (spawnPoint != null)
        {
            origin.position = spawnPoint.position;
            origin.rotation = spawnPoint.rotation;
        }

        Revived();
    }

    public void Respawn(Transform spawnPoint = null)
    {
        if (spawnPoint != null)
        {
            origin.position = spawnPoint.position;
            origin.rotation = spawnPoint.rotation;
        }

        health.FullHeal();
    }

    public void RegainConsciousness()
    {   
        vThirdPersonCamera.SetTarget(gameObject.transform);
        vThirdPersonInput.ShouldMove(true);
    }

    public void CantUseChassis()
    {
        anim.SetBool("IsActivated", false);
        anim.SetBool("IsStrafing", false);
    }

    public void PlayerStopMove()
    {
        anim.ResetTrigger("PickupTrigger");
        vThirdPersonInput.ShouldMove(false);
    }

    public void PlayerStartMove()
    {
        vThirdPersonInput.ShouldMove(true);
    }

    public void PlayerStoodUp()
    {
        vThirdPersonCamera.height = originalCameraHeight;
        anim.SetLayerWeight(5, 0);
        anim.SetFloat("StandBlend", -1);
        RegainConsciousness();

        StartCoroutine(WaitToMoveAgain());
    }

    private float CheckClosestGroundDist()
    {
        float backCheck = Mathf.Infinity;
        RaycastHit[] backHits = Physics.SphereCastAll(backBone.position, .2f, -Vector3.up, ragdollRaycastDistance);

        if (backHits.Length > 0)
        {
            for (int i = 0; i < backHits.Length; i++)
            {
                bool isPlayer = false;
                for (int j = 0; j < ragdoll.ragdollColliders.Count; j++)
                {
                    if(backHits[i].collider == ragdoll.ragdollColliders[j])
                    {
                        isPlayer = true;
                        break;
                    }

                    if (backHits[i].collider == primaryCollider)
                    {
                        isPlayer = true;
                        break;
                    }
                }

                if (isPlayer)
                {
                    continue;
                }

                if (backHits[i].distance < backCheck)
                {
                    backCheck = backHits[i].distance;
                }
            }
        }

        if (backCheck == Mathf.Infinity)
        {
            Debug.LogError("The ground could not be found!");

            return -1;
        }

        return backCheck;
    }

    private void ResetVariables()
    {
        anim.SetBool("IsActivated", false);
        anim.SetInteger("GripEnum", -1);

        if (itemHandler.GetEquippedItem() != null)
        {
            itemHandler.GetEquippedItem().gameObject.transform.parent = itemHandler.leftHandAttachmentBone;
            itemHandler.GetEquippedItem().gameObject.transform.localPosition = itemHandler.GetEquippedItem().localHandPos;
            itemHandler.GetEquippedItem().gameObject.transform.localRotation = Quaternion.Euler(itemHandler.GetEquippedItem().localHandRot);
            itemHandler.GetEquippedItem().gameObject.GetComponent<Collider>().enabled = false;
            itemHandler.GetEquippedItem().gameObject.GetComponent<Rigidbody>().isKinematic = true;

            //if (exploded)
            //{
            //    itemHandler.UnequipItem(itemHandler.GetEquippedItem());
            //    exploded = false;
            //}   
        }
    }

    private void Revived()
    {
        isAlive = true;
        isDeadFalling = false;
        mainNavObs.enabled = false;
        isUnconscious = false;

        if (!vThirdPersonInput.CanMove())
        {
            vThirdPersonInput.ShouldMove(true);
        }

        vThirdPersonCamera.height = originalCameraHeight;
        vThirdPersonCamera.SetTarget(gameObject.transform);
        vThirdPersonCamera.target = gameObject.transform;
        ToggleRagdoll(false);

        transform.position = origin.position;
        primaryRigidbody.MovePosition(origin.position);
        transform.rotation = origin.rotation;

        vThirdPersonCamera.transform.LookAt(deathCameraTarget);
        vThirdPersonCamera.SetTarget(gameObject.transform);

        transform.parent = rootObj.transform;
    }

    private void Die()
    {
        isAlive = false;

        if (deathSFX != string.Empty)
        {
            //AudioManager.Get().Play(deathSFX);
        }

        vThirdPersonCamera.SetTarget(deathCameraTarget);
        if (vThirdPersonController.isGrounded)
        {
            vThirdPersonCamera.height = deathCameraTarget.localPosition.y;
        }
        else
        {
            vThirdPersonCamera.height = deathCameraTarget.localPosition.y;
        }

        if (itemHandler.GetEquippedItem() != null)
        {
            itemHandler.GetEquippedItem().gameObject.transform.parent = null;
            itemHandler.GetEquippedItem().gameObject.GetComponent<Collider>().enabled = true;
            itemHandler.GetEquippedItem().gameObject.GetComponent<Rigidbody>().isKinematic = false;
        }

        ToggleRagdoll(true);

        health.OnHealthDepleated.RemoveAllListeners();
        health.OnHealthDepleated.AddListener(Die);

        StartCoroutine(StartFade(RespawnTime()));
    }

    IEnumerator StartFade(IEnumerator RespawnType)
    {
        yield return new WaitForSeconds(deathTime);

        panelFade.FadeOutAndIn();
        StartCoroutine(RespawnType);
    }

    IEnumerator RegainConsciousnessTime()
    {
        Vector3 tempHeadBone = new Vector3(headBone.position.x, 0, headBone.position.z),
                tempBackBone = new Vector3(backBone.position.x, 0, backBone.position.z);
        
        if (transform.position.y + (backBone.up * ragdollRaycastDistance).y > backBone.position.y)
        {
            PlayerReferenceAnimator.Instance.SwitchPlayerAnimLayer(0);
            anim.SetFloat("StandBlend", 0);

            PlayerReferenceAnimator.Instance.transform.rotation = Quaternion.LookRotation(tempHeadBone - tempBackBone);
        }
        else
        {
            PlayerReferenceAnimator.Instance.SwitchPlayerAnimLayer(1);
            anim.SetFloat("StandBlend", 1);

            PlayerReferenceAnimator.Instance.transform.rotation = Quaternion.LookRotation(tempBackBone - tempHeadBone);
        }

        yield return new WaitForSeconds(0.5f);

        PlayerReferenceAnimator.Instance.UpdateIdleTransforms();

        vThirdPersonCamera.SetTarget(gameObject.transform);
        ragdoll.ToggleRagdoll(false);
        
        Dictionary<Transform, Vector3> currentTransformWorldPos = new Dictionary<Transform, Vector3>();
        for (int i = 1; i < childTransforms.Length; i++)
        {
            currentTransformWorldPos.Add(childTransforms[i], childTransforms[i].position);
        }

        float floorDist = CheckClosestGroundDist();

        transform.position = new Vector3(ragdoll.ragdollColliders[0].transform.position.x, ragdoll.ragdollColliders[0].transform.position.y - floorDist, ragdoll.ragdollColliders[0].transform.position.z);

        foreach (KeyValuePair<Transform, Vector3> transVect in currentTransformWorldPos)
        {
            if (PlayerReferenceAnimator.Instance.idleTransforms.ContainsKey(transVect.Key.gameObject.name))
            {
                transVect.Key.position = new Vector3(transVect.Value.x, transVect.Value.y - floorDist, transVect.Value.z);
            }
        }

        resetRagdoll = true;
    }

    IEnumerator RespawnTime()
    {
        yield return new WaitForSeconds(panelFade.duration);

        Respawn();
    }

    IEnumerator RagdollWaitRotate()
    {
        yield return new WaitForEndOfFrame();

        Vector3 tempHeadBone = new Vector3(headBone.position.x, 0, headBone.position.z),
                        tempBackBone = new Vector3(backBone.position.x, 0, backBone.position.z);

        if (anim.GetFloat("StandBlend") == 0)
        {
            transform.rotation = Quaternion.LookRotation(tempHeadBone - tempBackBone);
        }
        else
        {
            transform.rotation = Quaternion.LookRotation(tempBackBone - tempHeadBone);
        }

        resetCameraHeight = true;
    }

    IEnumerator WaitToMoveAgain()
    {
        yield return new WaitForSeconds(0.5f);

        isUnconscious = false;
        mainNavObs.enabled = false;
    }

    void Update()
    {
        if (playerInput.actions["Aim"].IsPressed() && vThirdPersonController.isGrounded)
        {
            vThirdPersonController.airSpeed = vThirdPersonController.freeSpeed.runningSpeed;
        }
        else if (vThirdPersonController.isSprinting && vThirdPersonController.isGrounded)
        {
            vThirdPersonController.airSpeed = vThirdPersonController.freeSpeed.sprintSpeed;
        }
        else if (!vThirdPersonController.isSprinting && vThirdPersonController.isGrounded)
        {
            vThirdPersonController.airSpeed = vThirdPersonController.freeSpeed.runningSpeed;
        }


        if (!isAlive)
        {
            if (isDeadFalling)
            {
                Quaternion playerLookRot = Quaternion.LookRotation(transform.position - vThirdPersonCamera.transform.position);
                vThirdPersonCamera.transform.rotation = Quaternion.Slerp(vThirdPersonCamera.transform.rotation, playerLookRot, deathCamRotSpeed * Time.deltaTime); ;
            }
            
            return;
        }

        if (ragdoll.IsRagdolled() && !ragdollCheck && !isDeadFalling)
        {
            ragdollTimer -= Time.deltaTime;
            if (ragdollTimer <= 0)
            {
                if (ragdoll.TotalRigidbodyMagnitude() < 1f)
                {
                    if(CheckClosestGroundDist() != -1)
                    {
                        StartCoroutine(RegainConsciousnessTime());

                        ragdollCheck = true;
                        ragdollTimer = 2f;
                    }
                }
            }
        }

        if (resetRagdoll)
        {
            for (int i = 1; i < childTransforms.Length; i++)
            {
                if (PlayerReferenceAnimator.Instance.idleTransforms.ContainsKey(childTransforms[i].gameObject.name))
                {
                    childTransforms[i].localPosition = Vector3.Lerp(childTransforms[i].localPosition, PlayerReferenceAnimator.Instance.idleTransforms[childTransforms[i].gameObject.name].Key, 2 * Time.deltaTime);
                    childTransforms[i].rotation = Quaternion.Slerp(childTransforms[i].rotation, PlayerReferenceAnimator.Instance.idleTransforms[childTransforms[i].gameObject.name].Value, 2 * Time.deltaTime);
                }
            }

            ragdollTimer -= Time.deltaTime;
            if (ragdollTimer <= 0)
            {
                resetRagdoll = false;
                anim.SetLayerWeight(5, 1);
                ToggleRagdoll(false);
                ragdollCheck = false;
                ragdollTimer = 2f;

                StartCoroutine(RagdollWaitRotate());
            }
        }

        if (resetCameraHeight)
        {
            vThirdPersonCamera.height = Mathf.Lerp(vThirdPersonCamera.height, originalCameraHeight, 2f * Time.deltaTime);

            ragdollTimer -= Time.deltaTime;
            if (ragdollTimer <= 0)
            {
                resetCameraHeight = false;
                ragdollTimer = 2f;
            }
        }

        if (QuestManager.Instance.IsCurrentQuestActive())
        {
            Objective currentObjective = QuestManager.Instance.GetCurrentQuest().GetCurrentObjective();
            if (currentObjective != null)
            {
                currentObjective.CheckLocation(transform.position);
            }
        }
        else
        {
            if (QuestManager.Instance.GetCurrentQuest() != null)
            {
                QuestManager.Instance.TryStartQuest(transform.position);
            }
        }

        if (playerInput.actions["Emote"].WasPressedThisFrame() && vThirdPersonController.inputMagnitude < 0.1f && vThirdPersonInput.CanMove())
        {
            playerEmoter.PlayEmote("EmoteTrigger");
        }

        if (anim.GetInteger("GripEnum") > 0 && !vThirdPersonController.strafeSpeed.rotateWithCamera)
        {
            vThirdPersonController.strafeSpeed.rotateWithCamera = true;
        }
        else if (anim.GetInteger("GripEnum") <= 0 && vThirdPersonController.strafeSpeed.rotateWithCamera)
        {
            vThirdPersonController.strafeSpeed.rotateWithCamera = false;
        }
    }
}
