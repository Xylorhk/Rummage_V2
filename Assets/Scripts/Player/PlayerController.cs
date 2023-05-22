using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [SerializeField] private Transform debugHitPointTransform;

    public Transform sceneCamera;

    public float walkSpeed = 6.0f;
    public float turnSmoothTime = 0.1f;
    public float explosionForce = 11.0f;
    public float ragdollStunDuration = 5.0f;
    public Animator animator;

    // cached components
    CapsuleCollider capsule;
    CharacterController characterController;
    
    //transient data
    float turnSmoothVelocity;
    bool isThrowing = false;
    bool isRagdolling = false;
    float yVelocity;
    Vector3 moveDirection = Vector3.zero;
    float ragdollStunTimer = 0.0f;
    private bool canMove = true;
    public float maxGrappleDistance = 200f;

    public LayerMask GrappleMask;
    private Vector3 grapplePosition;
    private State state;

    private enum State
    {
        Normal,
        Grappling,

    }

    void Start()
    {
        capsule = GetComponent<CapsuleCollider>();
        characterController = GetComponent<CharacterController>();

        //https://answers.unity.com/questions/741074/why-character-controller-floats-5-cm-above-ground.html
        // calculate the correct vertical position:
        float correctHeight = characterController.center.y + characterController.skinWidth;
        // set the controller center vector:
        characterController.center = new Vector3(0, correctHeight, 0);

        //AttachHeldGrenade();

        state = State.Normal;
    }

    void Update()
    {
        switch (state)
        {
            default:
            case State.Normal:
                ManageInput();
                //ManageHookshotStart();
                break;
            case State.Grappling:
                ManageHookshotMovement();
                break;
        }
    }

    public bool CanMove()
    {
        return canMove;
    }

    public void ShouldMove(bool shouldMove)
    {
        canMove = shouldMove;
    }

    public void ManageInput()
    {
        if (Time.timeScale < 0.1f || !canMove)
        {
            return;
        }

        if (isRagdolling)
        {
            if (transform.position.y < 0)
            {
                Vector3 pos = transform.position;
                pos.y = 0;
                yVelocity = 0;
            }

            ragdollStunTimer -= Time.deltaTime;

            if (ragdollStunTimer <= 0.0f)
            {
                ExitRagdoll();
            }
        }
        else
        {
            //if (attachedHeldGrenade != null && characterController.enabled && characterController.isGrounded && Input.GetMouseButtonDown(0))
            //{
            //    animator.SetTrigger("requestThrow");
            //    isThrowing = true;
            //}

            if (isThrowing)
            {
                return;
            }

            Vector2 moveDirection = Player.Instance.playerInput.actions["Move"].ReadValue<Vector2>();
            float h = moveDirection.x;
            float v = moveDirection.y;
            Vector3 desiredDirection = new Vector3(h, 0.0f, v).normalized;

            if (!characterController.isGrounded)
            {
                desiredDirection = Vector3.zero;
            }

            if (desiredDirection.magnitude > 0 && !isThrowing)
            {
                animator.SetInteger("state", 1);
                float targetAngle = Mathf.Atan2(desiredDirection.x, desiredDirection.z) * Mathf.Rad2Deg + sceneCamera.eulerAngles.y;
                float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, turnSmoothTime);

                transform.rotation = Quaternion.Euler(0.0f, angle, 0.0f);

                moveDirection = Quaternion.Euler(0.0f, targetAngle, 0.0f) * Vector3.forward;
            }
            else
            {
                animator.SetInteger("state", 0);

                if (characterController.isGrounded)
                {
                    moveDirection = Vector3.zero;
                }
            }
        }

        yVelocity -= 9.81f * Time.deltaTime;
        characterController.Move(((moveDirection * walkSpeed) + new Vector3(0, yVelocity, 0)) * Time.deltaTime); // * Time.deltaTime);

        if (characterController.isGrounded)
        {
            yVelocity = 0.0f;
        }
    }

    private void ManageHookshotStart()
    {

        if (Player.Instance.playerInput.actions["Fire"].WasPressedThisFrame())
        {
            RaycastHit hit;
            if (Physics.Raycast(sceneCamera.position, sceneCamera.forward, out hit, maxGrappleDistance, GrappleMask))
            {
                debugHitPointTransform.position = hit.point;
                grapplePosition = hit.point;
                state = State.Grappling;
            }
        }
    }

    private void ManageHookshotMovement()
    {
        Vector3 grappleDir = (grapplePosition - transform.position).normalized;
        float hookshotSpeed = 5f;

        characterController.Move(moveDirection * hookshotSpeed * Time.deltaTime);
    }

    //public void OnThrowSpawnGrenade()
    //{
    //    GameObject grenadeGo = Instantiate(grenadePrefab, attachedHeldGrenade.transform.position, attachedHeldGrenade.transform.rotation);

    //    attachedHeldGrenade.transform.parent = null;
    //    attachedHeldGrenade.SetActive(false);
    //    attachedHeldGrenade = null;

    //    if (grenadeGo != null)
    //    {
    //        grenadeGo.GetComponent<Rigidbody>().AddForce((transform.forward) * grenadeThrowForwardImpulse + new Vector3(0.0f, grenadeThrowUpwardImpulse, 0.0f), ForceMode.Impulse);

    //        grenade grenadeComponent = grenadeGo.GetComponent<grenade>();

    //        if (grenadeComponent != null)
    //        {
    //            grenadeComponent.activateGrenade();
    //        }
            
    //    }
    //}

    public void OnThrowComplete()
    {
        isThrowing = false;
        //AttachHeldGrenade();
    }

    public void OnTouchMine()
    {
        yVelocity = explosionForce;
        EnterRagdoll();
    }

    void EnterRagdoll()
    {
        isRagdolling = true;
        animator.enabled = false;
        capsule.enabled = false;
        characterController.enabled = false;
        ragdollStunTimer = ragdollStunDuration;
    }

    void ExitRagdoll()
    {
        isRagdolling = false;
        animator.enabled = true;
        capsule.enabled = true;
        characterController.enabled = true;
    }
}
