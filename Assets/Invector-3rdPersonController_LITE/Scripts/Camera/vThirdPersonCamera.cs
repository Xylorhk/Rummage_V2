using Invector;
using System.Collections;
using UnityEngine;

public class vThirdPersonCamera : MonoBehaviour
{
    #region inspector properties    

    public Transform target;
    [Tooltip("Lerp speed between Camera States")]
    public GameObject lerpTarget;
    public float lerpSpeed = 5f;
    public float smoothCameraRotation = 12f;
    [Tooltip("What layer will be culled")]
    public LayerMask cullingLayer = 1 << 0;
    public float playerAimCullingDistance = 1.5f;
    [Tooltip("Debug purposes, lock the camera behind the character for better align the states")]
    public bool lockCamera;

    public float rightOffset = 0f;
    public float defaultDistance = 2.5f;
    public float height = 1.4f;
    public float smoothFollow = 10f;
    public float xMouseSensitivity = 3f;
    public float yMouseSensitivity = 3f;
    public float yMinLimit = -40f;
    public float yMaxLimit = 80f;

    private AimTargetAssist aimAssist;

    private float lerpTimer = 0;
    private float maxLerpTimer = 0;
    private bool shouldMove = false;
    private bool playerRaycastBool = false;

    #endregion

    #region hide properties    

    [HideInInspector]
    public int indexList, indexLookPoint;
    [HideInInspector]
    public float offSetPlayerPivot;
    [HideInInspector]
    public string currentStateName;
    [HideInInspector]
    public Transform currentTarget;
    [HideInInspector]
    public Vector2 movementSpeed;

    private Transform targetLookAt;
    private Vector3 currentTargetPos;
    private Vector3 lookPoint;
    private Vector3 current_cPos;
    private Vector3 desired_cPos;
    private Camera _camera;
    private float distance = 5f;
    private float mouseY = 0f;
    private float mouseX = 0f;
    private float currentHeight;
    private float cullingDistance;
    private float checkHeightRadius = 0.4f;
    private float clipPlaneMargin = 0f;
    private float forward = -1f;
    private float xMinLimit = -360f;
    private float xMaxLimit = 360f;
    private float cullingHeight = 0.2f;
    private float cullingMinDist = 0.1f;

    #endregion

    

    void Start()
    {
        Init();
    }

    public void Init()
    {
        if (target == null)
            return;

        _camera = GetComponent<Camera>();
        currentTarget = target;
        currentTargetPos = new Vector3(currentTarget.position.x, currentTarget.position.y + offSetPlayerPivot, currentTarget.position.z);

        targetLookAt = new GameObject("targetLookAt").transform;
        targetLookAt.position = currentTarget.position;
        targetLookAt.hideFlags = HideFlags.HideInHierarchy;
        targetLookAt.rotation = currentTarget.rotation;

        mouseY = currentTarget.eulerAngles.x;
        mouseX = currentTarget.eulerAngles.y;

        distance = defaultDistance;
        currentHeight = height;

        aimAssist = GetComponent<AimTargetAssist>();

        lerpTimer = lerpSpeed * Time.fixedDeltaTime;
        maxLerpTimer = lerpTimer;
        lerpTimer = 0;
    }

    void Update()
    {
        if ((Player.Instance.playerInput.actions["Aim"].WasPressedThisFrame() || Player.Instance.playerInput.actions["Aim"].WasReleasedThisFrame()) && Player.Instance.anim.GetInteger("GripEnum") == 2)
        {
            shouldMove = !shouldMove;
            lerpTimer = maxLerpTimer;

        }

        if (((Player.Instance.playerInput.actions["Aim"].IsPressed() && !shouldMove) || (!Player.Instance.playerInput.actions["Aim"].IsPressed() && shouldMove)) && Player.Instance.anim.GetInteger("GripEnum") == 2)
        {
            shouldMove = !shouldMove;
        }

        if (shouldMove && (Player.Instance.anim.GetInteger("GripEnum") != 2 || !Player.Instance.vThirdPersonInput.CanMove()))
        {
            shouldMove = false;
        }

        if (shouldMove)
        {
            yMinLimit = -20;
            yMaxLimit = 50;
        }
        else
        {
            yMinLimit = -40;
            yMaxLimit = 80;
        }

        Vector3 playerRaycastOrigin = Player.Instance.itemHandler.itemDetection.pickupTransform.position;
        Vector3 playerRaycastDir = (aimAssist.aimTargetObj.transform.position - playerRaycastOrigin).normalized;

        RaycastHit hitInfo;
        if (!playerRaycastBool)
        {
            playerRaycastBool = Physics.Raycast(playerRaycastOrigin, playerRaycastDir, out hitInfo, playerAimCullingDistance, LayerMask.NameToLayer("Player"));
            playerRaycastBool = (playerRaycastBool && !(hitInfo.collider.gameObject.GetComponent<Grabbable>() != null || hitInfo.collider.gameObject.GetComponent<Item>() != null));
        }
        else
        {
            playerRaycastBool = Physics.Raycast(playerRaycastOrigin, playerRaycastDir, out hitInfo, playerAimCullingDistance + 0.5f, LayerMask.NameToLayer("Player"));
            playerRaycastBool = (playerRaycastBool && !(hitInfo.collider.gameObject.GetComponent<Grabbable>() != null || hitInfo.collider.gameObject.GetComponent<Item>() != null));
        }
    }

    void LateUpdate()
    {
        if (target == null || targetLookAt == null) return;

        CameraMovement();
    }

    /// <summary>
    /// Set the target for the camera
    /// </summary>
    /// <param name="New cursorObject"></param>
    public void SetTarget(Transform newTarget)
    {
        currentTarget = newTarget ? newTarget : target;
    }

    public void SetMainTarget(Transform newTarget)
    {
        target = newTarget;
        currentTarget = newTarget;
        mouseY = currentTarget.rotation.eulerAngles.x;
        mouseX = currentTarget.rotation.eulerAngles.y;
        Init();
    }

    /// <summary>    
    /// Convert a point in the screen in a Ray for the world
    /// </summary>
    /// <param name="Point"></param>
    /// <returns></returns>
    public Ray ScreenPointToRay(Vector3 Point)
    {
        return this.GetComponent<Camera>().ScreenPointToRay(Point);
    }

    /// <summary>
    /// Camera Rotation behaviour
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    public void RotateCamera(float x, float y)
    {
        // free rotation 
        mouseX += x * xMouseSensitivity;
        mouseY -= y * yMouseSensitivity;

        movementSpeed.x = x;
        movementSpeed.y = -y;
        if (!lockCamera)
        {
            mouseY = vExtensions.ClampAngle(mouseY, yMinLimit, yMaxLimit);
            mouseX = vExtensions.ClampAngle(mouseX, xMinLimit, xMaxLimit);
        }
        else
        {
            mouseY = currentTarget.root.localEulerAngles.x;
            mouseX = currentTarget.root.localEulerAngles.y;
        }
    }

    private void OnDrawGizmos()
    {
        if (Player.Instance == null)
        {
            return;
        }

        Gizmos.color = Color.magenta;

        Vector3 playerRaycastOrigin = Player.Instance.itemHandler.itemDetection.pickupTransform.position;
        Vector3 playerRaycastDir = (aimAssist.aimTargetObj.transform.position - playerRaycastOrigin).normalized;
        Gizmos.DrawLine(playerRaycastOrigin, playerRaycastOrigin + playerRaycastDir * playerAimCullingDistance);


        Gizmos.color = Color.blue;
        Gizmos.DrawCube(current_cPos, new Vector3(0.2f, 0.2f, 0.2f));

        Gizmos.color = Color.magenta;
        Gizmos.DrawSphere(desired_cPos, 0.2f);
    }

    void LerpCamera()
    {
        if (Player.Instance.anim.GetInteger("GripEnum") == 2)
        {
            if (shouldMove)
            {
                current_cPos = Vector3.Lerp(current_cPos, lerpTarget.transform.position, lerpSpeed * Time.fixedDeltaTime);

                lerpTimer -= Time.fixedDeltaTime;
                if (lerpTimer <= 0 || Vector3.Distance(current_cPos, lerpTarget.transform.position) < 0.05f)
                {
                    current_cPos = lerpTarget.transform.position;
                }
            }
            else
            {
                current_cPos = Vector3.Lerp(current_cPos, currentTargetPos + new Vector3(0, currentHeight, 0), lerpSpeed * Time.fixedDeltaTime);

                lerpTimer -= Time.fixedDeltaTime;
                if (lerpTimer <= 0 || Vector3.Distance(current_cPos, currentTargetPos + new Vector3(0, currentHeight, 0)) < 0.05f)
                {
                    current_cPos = currentTargetPos + new Vector3(0, currentHeight, 0);
                }
            }
        }
        else
        {
            current_cPos = currentTargetPos + new Vector3(0, currentHeight, 0);
        }
    }

    /// <summary>
    /// Camera behaviour
    /// </summary>    
    void CameraMovement()
    {
        if (currentTarget == null)
            return;

        distance = Mathf.Lerp(distance, defaultDistance, smoothFollow * Time.deltaTime);
        cullingDistance = Mathf.Lerp(cullingDistance, distance, Time.deltaTime);
        var camDir = (forward * targetLookAt.forward) + (rightOffset * targetLookAt.right);

        camDir = camDir.normalized;

        Vector3 targetPos;
        if (!shouldMove || playerRaycastBool)
        {
            targetPos = new Vector3(currentTarget.position.x, currentTarget.position.y + offSetPlayerPivot, currentTarget.position.z);
        }
        else
        {
            Vector3 tempTargetPos =  lerpTarget.transform.position;
            targetPos = new Vector3(tempTargetPos.x, tempTargetPos.y + offSetPlayerPivot, tempTargetPos.z);
        }
        
        currentTargetPos = targetPos;
        desired_cPos = targetPos + new Vector3(0, height, 0);

        
        if (playerRaycastBool)
        {
            current_cPos = currentTargetPos + new Vector3(0, currentHeight, 0);
        }
        else
        {
            LerpCamera();
        }
        
        
        RaycastHit hitInfo;

        ClipPlanePoints planePoints = _camera.NearClipPlanePoints(current_cPos + (camDir * (distance)), clipPlaneMargin);
        ClipPlanePoints oldPoints = _camera.NearClipPlanePoints(desired_cPos + (camDir * distance), clipPlaneMargin);

        //Check if Height is not blocked 
        if (Physics.SphereCast(targetPos, checkHeightRadius, Vector3.up, out hitInfo, cullingHeight + 0.2f, cullingLayer))
        {
            var t = hitInfo.distance - 0.2f;
            t -= height;
            t /= (cullingHeight - height);
            cullingHeight = Mathf.Lerp(height, cullingHeight, Mathf.Clamp(t, 0.0f, 1.0f));
        }

        //Check if desired target position is not blocked       
        if (CullingRayCast(desired_cPos, oldPoints, out hitInfo, distance + 0.2f, cullingLayer, Color.blue))
        {
            distance = hitInfo.distance - 0.2f;
            if (distance < defaultDistance)
            {
                var t = hitInfo.distance;
                t -= cullingMinDist;
                t /= cullingMinDist;
                currentHeight = Mathf.Lerp(cullingHeight, height, Mathf.Clamp(t, 0.0f, 1.0f));
                current_cPos = currentTargetPos + new Vector3(0, currentHeight, 0);
            }
        }
        else
        {
            currentHeight = height;
        }
        //Check if target position with culling height applied is not blocked
        if (CullingRayCast(current_cPos, planePoints, out hitInfo, distance, cullingLayer, Color.cyan)) distance = Mathf.Clamp(cullingDistance, 0.0f, defaultDistance);
        var lookPoint = current_cPos + targetLookAt.forward * 2f;
        lookPoint += (targetLookAt.right * Vector3.Dot(camDir * (distance), targetLookAt.right));
        targetLookAt.position = current_cPos;

        Quaternion newRot = Quaternion.Euler(mouseY, mouseX, 0);
        targetLookAt.rotation = Quaternion.Slerp(targetLookAt.rotation, newRot, smoothCameraRotation * Time.deltaTime);
        transform.position = current_cPos + (camDir * (distance));
        var rotation = Quaternion.LookRotation((lookPoint) - transform.position);

        transform.rotation = rotation;
        movementSpeed = Vector2.zero;
    }

    /// <summary>
    /// Custom Raycast using NearClipPlanesPoints
    /// </summary>
    /// <param name="_to"></param>
    /// <param name="from"></param>
    /// <param name="hitInfo"></param>
    /// <param name="distance"></param>
    /// <param name="cullingLayer"></param>
    /// <returns></returns>
    bool CullingRayCast(Vector3 from, ClipPlanePoints _to, out RaycastHit hitInfo, float distance, LayerMask cullingLayer, Color color)
    {
        bool value = false;

        if (Physics.Raycast(from, _to.LowerLeft - from, out hitInfo, distance, cullingLayer))
        {
            value = true;
            cullingDistance = hitInfo.distance;
        }

        if (Physics.Raycast(from, _to.LowerRight - from, out hitInfo, distance, cullingLayer))
        {
            value = true;
            if (cullingDistance > hitInfo.distance) cullingDistance = hitInfo.distance;
        }

        if (Physics.Raycast(from, _to.UpperLeft - from, out hitInfo, distance, cullingLayer))
        {
            value = true;
            if (cullingDistance > hitInfo.distance) cullingDistance = hitInfo.distance;
        }

        if (Physics.Raycast(from, _to.UpperRight - from, out hitInfo, distance, cullingLayer))
        {
            value = true;
            if (cullingDistance > hitInfo.distance) cullingDistance = hitInfo.distance;
        }

        return hitInfo.collider && value;
    }
}
