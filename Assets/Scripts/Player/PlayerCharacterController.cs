using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//Sourced from: https://www.youtube.com/watch?v=twMkGTqyZvI&t=850s
public class PlayerCharacterController : MonoBehaviour
{
    [SerializeField] private Camera playerCam;
    [SerializeField] private Transform playerTransform;

    private CharacterController characterController;
    public LineRenderer lr;
    private Vector3 characterVelocityMomentum;
    private Vector3 currentGrapplePosition;
    private Vector3 hookshotPosition;
    private float cameraVerticalAngle;
    private float characterVelocityY;

    private State state;    

    [Header("Grapple Data")]
    public LayerMask grappleLayer;
    public Transform grappleTip;
    public float maxGrappleDistance = 200f;
    public float hookshotThrowSpeed = 500f;
    public float hookshotSpeedMin = 10f;
    public float hookshotSpeedMax = 40f;
    public float hookshotSpeedMultiplier = 5f;
    public float reachedHookshotPositionDistance = 2f;
    public float mouseSensitivity = 1f;
    public bool isGrappling = false;

    [Header("Player Data")]
    public float moveSpeed = 20f;
    public float jumpSpeed = 30f;
    public float gravityDownForce = -60f;

    private enum State
    {
        Normal,
        HookshotThrown,
        HookshotFlyingPlayer,
    }

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
        //lr = GetComponent<LineRenderer>();
        Cursor.lockState = CursorLockMode.Locked;
        state = State.Normal;
    }

    private void Update()
    {
        switch (state)
        {
            default:
            case State.Normal:
                HandleCharacterLook();
                HandleCharacterMovement();
                HandleHookshotStart();
                break;
            case State.HookshotThrown:
                HandleHookshotThrow();
                HandleCharacterLook();
                HandleCharacterMovement();
                break;
            case State.HookshotFlyingPlayer:
                HandleCharacterLook();
                HandleHookshotMovement();
                break;
        }
    }

    void LateUpdate()
    {
        DrawRope();
    }

    private void HandleCharacterLook()
    {
        float lookX = Input.GetAxisRaw("Mouse X");
        float lookY = Input.GetAxisRaw("Mouse Y");

        // Rotate the transform with the input speed around its local Y axis
        transform.Rotate(new Vector3(0f, lookX * mouseSensitivity, 0f), Space.Self);

        // Add vertical inputs to the camera's vertical angle
        cameraVerticalAngle -= lookY * mouseSensitivity;

        // Limit the camera's vertical angle to min/max
        cameraVerticalAngle = Mathf.Clamp(cameraVerticalAngle, -89f, 89f);

        // Apply the vertical angle as a local rotation to the camera transform along its right axis (makes it pivot up and down)
        playerCam.transform.localEulerAngles = new Vector3(cameraVerticalAngle, 0, 0);
    }

    private void HandleCharacterMovement()
    {
        float moveX = Input.GetAxisRaw("Horizontal");
        float moveZ = Input.GetAxisRaw("Vertical");        

        Vector3 characterVelocity = transform.right * moveX * moveSpeed + transform.forward * moveZ * moveSpeed;

        if (characterController.isGrounded)
        {
            characterVelocityY = 0f;
            // Jump
            if (TestInputJump())
            {
                characterVelocityY = jumpSpeed;
            }
        }

        // Apply gravity to the velocity
        characterVelocityY += gravityDownForce * Time.deltaTime;

        // Apply Y velocity to move vector
        characterVelocity.y = characterVelocityY;

        // Apply momentum
        characterVelocity += characterVelocityMomentum;

        // Move Character Controller
        characterController.Move(characterVelocity * Time.deltaTime);

        // Dampen momentum
        if (characterVelocityMomentum.magnitude > 0f)
        {
            float momentumDrag = 3f;
            characterVelocityMomentum -= characterVelocityMomentum * momentumDrag * Time.deltaTime;
            if (characterVelocityMomentum.magnitude < .0f)
            {
                characterVelocityMomentum = Vector3.zero;
            }
        }
    }

    private void ResetGravityEffect()
    {
        characterVelocityY = 0f;
    }

    private void HandleHookshotStart()
    {
        if (TestInputDownHookshot())
        {
            if (Physics.Raycast(playerCam.transform.position, playerCam.transform.forward, out RaycastHit raycastHit, maxGrappleDistance, grappleLayer))
            {
                // Hit something
                hookshotPosition = raycastHit.point;
                state = State.HookshotThrown;

            }
        }
    }

    private void HandleHookshotThrow()
    {
        state = State.HookshotFlyingPlayer;
        lr.positionCount = 2;
        isGrappling = true;
    }

    private void HandleHookshotMovement()
    {
        Vector3 hookshotDir = (hookshotPosition - transform.position).normalized;

        float hookshotSpeed = Mathf.Clamp(Vector3.Distance(transform.position, hookshotPosition), hookshotSpeedMin, hookshotSpeedMax);

        // Move Character Controller
        characterController.Move(hookshotDir * hookshotSpeed * hookshotSpeedMultiplier * Time.deltaTime);

        if (Vector3.Distance(transform.position, hookshotPosition) < reachedHookshotPositionDistance)
        {
            // Reached Hookshot Position
            StopHookshot();
        }

        if (TestInputDownHookshot())
        {
            // Cancel Hookshot
            StopHookshot();
        }

        if (TestInputJump())
        {
            // Cancelled with Jump
            float momentumExtraSpeed = 7f;
            characterVelocityMomentum = hookshotDir * hookshotSpeed * momentumExtraSpeed;
            float jumpSpeed = 40f;
            characterVelocityMomentum += Vector3.up * jumpSpeed;
            StopHookshot();
        }
    }

    private void StopHookshot()
    {
        state = State.Normal;
        isGrappling = false;
        lr.positionCount = 0;
        ResetGravityEffect();
    }

    void DrawRope()
    {
        //If not grappling, don't draw rope
        if (!isGrappling) return;
        
        //swing
        //currentGrapplePosition = Vector3.Lerp(currentGrapplePosition, hookshotPosition, Time.deltaTime * 8f);
        lr.SetPosition(0, grappleTip.position);
        lr.SetPosition(1, hookshotPosition);
    }

    private bool TestInputDownHookshot()
    {
        return Input.GetMouseButtonDown(0);
    }

    private bool TestInputJump()
    {
        return Input.GetKeyDown(KeyCode.Space);
    }

    public bool IsGrappling()
    {
        return isGrappling;
    }

    public Vector3 GetGrapplePoint()
    {
        return hookshotPosition;
    }
}