using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;

public class HikerController : MonoBehaviour
{
    [Header("Movement settings")]
    public float baseWalkSpeed;
    public float walkAcceleration;
    public float decceleration;
    public float baseRunSpeed;
    public float runAcceleration;
    [Header("Ground snapping")]
    public float heightAboveGround;
    public float heightAdaptLerpRatio;
    public Vector3 feetBoxcastSize;
    public float boxcastStartOffset;
    public float gravityScale;
    public Transform groundBoxcastStart;
    public Transform groundBoxcastEnd;
    public Transform feetPos;
    public LayerMask groundMask;
    [Header("Rest settings")]
    public float sitTime;
    public float standUpTime;
    public float lieDownTime;
    public float getUpTime;
    public float inputMoveTimeToStandUp;
    public GameObject campfirePrefab;
    public float campfireDistance;
    public float campfireStartRaycastVertOffset;
    public float campfireSideRaycastOffset;
    public float campfireRaycastLength;
    public Vector3 campfireBoxcastSize;
    public float campfireMaxHeightDiffToPlace;
    public float maxGroundTiltToRest;
    public Vector2 minMaxGroundPitchToLieDown;
    public float maxGroundRollToLieDown;
    public float distFrontBackToTestGroundToLieDown;
    [Header("Pass Time settings")]
    public float maxTimeScale;
    public AnimationCurve timeScaleByPassTimeElapsed;
    public AnimationCurve timeScalePassRelease;

    [Header("Inputs")]
    public InputActionReference moveInput;
    public InputActionReference runInput;
    public InputActionReference respawnInput;
    public InputActionReference restInput;
    public InputActionReference waitInput;
    public PlayerInput playerInput;

    //------------------------------------------------------------<--....______....-->

    #region private properties

    private Rigidbody rb;

    private Vector2 moveInputValue;
    private Vector3 currentMoveDirection;
    private Vector3 currentInputDirection;
    private float currentSpeed;
    [HideInInspector]
    public bool isRunning;
    [HideInInspector]
    public bool isMoving;
    private bool runPressed;
    private float orientationDelta;
    private Vector3 currentRotationVelocity;

    private float currentGroundTargetHeight;
    private float currentVerticalVelocity;
    private bool isGrounded;
    private bool isFalling;
    [HideInInspector]
    public bool isResting;
    [HideInInspector]
    public bool isSittingDown;
    [HideInInspector]
    public bool isStandingUp;
    [HideInInspector]
    public bool isLyingDown;
    [HideInInspector]
    public bool isGettingUp;
    [HideInInspector]
    public bool isLyingOnTheGround;
    private bool canMove;
    private float moveInputHoldTime;

    private Campfire currentCampfire;

    public static HikerController I;
    private Vector3 spawnPos;
    private AudioSource playerSource;
    #endregion

    //------------------------------------------------------------<--....______....-->

    private void Awake()
    {
        I = this;
        GameData.hikerController = this;
        rb = GetComponent<Rigidbody>();
        isGameRunning = true;
        spawnPos = transform.position;
        respawnInput.action.performed += OnRespawnPress;
        restInput.action.performed += OnRestPress;
        playerSource = GetComponent<AudioSource>();
    }

    private void OnRespawnPress(InputAction.CallbackContext obj)
    {
        Teleport(spawnPos);
    }

    private void OnRestPress(InputAction.CallbackContext obj)
    {
        if(canMove && !isSittingDown && !isGettingUp && !isResting && isGrounded && currentSpeed < 0.1f)
        {
            TrySit();
        }
        else if (isResting && !isLyingOnTheGround && !isGettingUp && !isLyingDown)
        {
            TryLieDown();
        }
    }

    void OnEnable()
    {
        respawnInput.action.Enable();
    }

    private void Update()
    {
        UpdateMovement();
        UpdateBodyRotation();
        UpdatePassTime();
        UpdateRest();
    }

    #region Base Movement
    private void UpdateMovement()
    {
        moveInputValue = moveInput.action.ReadValue<Vector2>();
        moveInputValue = Vector2.ClampMagnitude(moveInputValue, 1f);

        runPressed = runInput.action.IsPressed();
        isMoving = false;
        isRunning = false;
        canMove = !isStandingUp && !isSittingDown && !isResting;

        if(moveInputValue.magnitude > 0)
        {
            moveInputHoldTime += Time.unscaledDeltaTime;
        }
        else
        {
            moveInputHoldTime = 0;
        }

        if (isGrounded)
        {
            if(canMove)
            {
                if (moveInputValue.magnitude > 0)
                {
                    isMoving = true;
                    isRunning = runPressed;

                    currentInputDirection.x = Mathf.Cos(Mathf.Deg2Rad * -GameData.cameraManager.CamCurrentOrientation) * moveInputValue.x - Mathf.Sin(Mathf.Deg2Rad * -GameData.cameraManager.CamCurrentOrientation) * moveInputValue.y;
                    currentInputDirection.z = Mathf.Sin(Mathf.Deg2Rad * -GameData.cameraManager.CamCurrentOrientation) * moveInputValue.x + Mathf.Cos(Mathf.Deg2Rad * -GameData.cameraManager.CamCurrentOrientation) * moveInputValue.y;

                    currentMoveDirection = currentInputDirection.normalized;

                    if (currentSpeed < (runPressed ? baseRunSpeed : baseWalkSpeed) * moveInputValue.magnitude)
                    {
                        currentSpeed += (runPressed ? runAcceleration : walkAcceleration) * moveInputValue.magnitude * Time.deltaTime;
                    }
                    else
                    {
                        currentSpeed = (runPressed ? baseRunSpeed : baseWalkSpeed) * moveInputValue.magnitude;
                    }
                }
                else
                {
                    if (currentSpeed > 0)
                    {
                        currentSpeed -= decceleration * Time.deltaTime;
                    }
                    else
                    {
                        currentSpeed = 0;
                    }
                }
            }

            TestGround();

            SetVerticalVelocityToSnapToSpecifiedHeight(currentGroundTargetHeight);
        }
        else
        {
            RaycastHit landFeetRay;
            Physics.BoxCast(groundBoxcastStart.position, feetBoxcastSize ,Vector3.down, out landFeetRay, Quaternion.identity, groundBoxcastStart.position.y - feetPos.position.y, groundMask);

            if(landFeetRay.collider != null)
            {
                isGrounded = true;
                isFalling = false;
                OnLanding();
            }
            else
            {
                SetVertVelForFalling();
            }
        }

        DebugManager.AddFrameLine("Vertical velocity : " + currentVerticalVelocity);
        DebugManager.AddFrameLine("Angular vel y : " + currentRotationVelocity.y);
        DebugManager.AddFrameLine("Current speed : " + currentSpeed);
        DebugManager.AddFrameLine("Sitting : " + isResting);
        DebugManager.AddFrameLine(isGrounded ? "<color=#ffdba6>Grounded</color>" : (isFalling ? "<color=#86fce3>Falling</color>" : "<color=#d1d1d1>Floating?</color>"));
    }

    private void UpdateBodyRotation()
    {
        if(!isResting)
        {

            orientationDelta = Mathf.DeltaAngle(transform.rotation.eulerAngles.y, GameData.cameraManager.CamCurrentOrientation);
            currentRotationVelocity.y = orientationDelta / Time.deltaTime;
            currentRotationVelocity.x = 0;
            currentRotationVelocity.z = 0;

            if (Mathf.Abs(orientationDelta) < 4f)
            {
                currentRotationVelocity.y = 0;
            }

            currentRotationVelocity *= Mathf.Deg2Rad;
        }
        else
        {
            currentRotationVelocity.y = 0;
        }
    }

    private void FixedUpdate()
    {
        ApplyMovementToRigibody();
    }

    private RaycastHit[] groundCastHits = new RaycastHit[4];
    private void TestGround()
    {
        bool groundFound = false;
        int boxcastHit = 0;
        float minCastHeight = 0;

        for (int i = 0; i < 4; i++)
        {
            Physics.BoxCast(groundBoxcastStart.position + new Vector3(i % 2 == 0 ? boxcastStartOffset : -boxcastStartOffset, 0f, i > 1 ? boxcastStartOffset : -boxcastStartOffset), feetBoxcastSize, Vector3.down, out groundCastHits[i], Quaternion.identity, groundBoxcastStart.position.y - groundBoxcastEnd.position.y, groundMask);
            if (groundCastHits[i].collider != null)
            {
                boxcastHit++;
                if (minCastHeight > groundCastHits[i].point.y || !groundFound)
                {
                    minCastHeight = groundCastHits[i].point.y;
                    groundFound = true;
                }
            }
        }

        if (groundFound && boxcastHit > 2)
        {
            currentGroundTargetHeight = minCastHeight;
        }
        else
        {
            if(!isFalling)
            {
                isGrounded = false;
                isFalling = true;
            }
        }
    }
    float newHeight;
    private void SetVerticalVelocityToSnapToSpecifiedHeight(float worldheight)
    {
        newHeight = Mathf.Lerp(transform.position.y, worldheight + heightAboveGround, heightAdaptLerpRatio * Time.deltaTime);
        currentVerticalVelocity = (newHeight - transform.position.y) / Time.deltaTime;
        if(Mathf.Abs(currentVerticalVelocity) < 0.01f)
        {
            currentVerticalVelocity = 0;
        }
    }

    private void SetVertVelForFalling()
    {
        currentVerticalVelocity -= gravityScale * Time.deltaTime;
    }

    private void OnLanding()
    {
        DebugManager.AddPersistentLineForSpecifiedTime("--Landed--", 2f);
    }

    public void Teleport(Vector3 destPosition, Quaternion destRotation)
    {
        isFalling = true;
        isGrounded = false;
        currentSpeed = 0;
        rb.linearVelocity = Vector3.zero;
        rb.position = destPosition;
        GameData.cameraManager.SetAim(destRotation);
    }

    public void Teleport(Vector3 destPosition)
    {
        isFalling = true;
        isGrounded = false;
        currentSpeed = 0;
        rb.linearVelocity = Vector3.zero;
        rb.position = destPosition;
    }

    private void ApplyMovementToRigibody()
    {
        Vector3 newVelocity = Vector3.zero;
        newVelocity.x = currentMoveDirection.x * currentSpeed;
        newVelocity.z = currentMoveDirection.z * currentSpeed;

        newVelocity.y = currentVerticalVelocity;

        rb.linearVelocity = newVelocity;

        rb.angularVelocity = currentRotationVelocity;
    }
    #endregion
    
    //------------------------------------------------------------<--....______....-->

    #region Rest

    private void UpdateRest()
    {
        if (isResting && !isLyingOnTheGround && !isLyingDown && !isGettingUp && !isStandingUp && moveInputHoldTime > inputMoveTimeToStandUp)
        {
            StartCoroutine(StandUp());
        }
        else if (isLyingOnTheGround && !isGettingUp && moveInputHoldTime > inputMoveTimeToStandUp)
        {
            StartCoroutine(GetUp());
        }
    }

    private void TrySit()
    {
        RaycastHit sitRay;
        Physics.BoxCast(groundBoxcastStart.position, feetBoxcastSize, Vector3.down, out sitRay, Quaternion.identity, 2f, groundMask);

        if(sitRay.collider != null)
        {
            DebugManager.AddPersistentLineForSpecifiedTime("Try to sit, angle is " + Vector3.Angle(Vector3.up, sitRay.normal), sitTime);
            if (Vector3.Angle(Vector3.up,sitRay.normal) < maxGroundTiltToRest)
            {
                StartCoroutine(Sit());
            }
        }
        else
        {
            DebugManager.AddPersistentLineForSpecifiedTime("Try to sit, but no ground", sitTime);
        }
    }

    private IEnumerator Sit()
    {
        DebugManager.AddPersistentLineForSpecifiedTime("Sitting...", sitTime);
        isSittingDown = true;
        yield return new WaitForSeconds(sitTime);
        GameData.cameraManager.EnableRestCam();
        PlaceCampfire(transform.position + new Vector3(GameData.cameraManager.transform.forward.x, 0f, GameData.cameraManager.transform.forward.z).normalized * campfireDistance);
        isSittingDown = false;
        isResting = true;
        moveInputHoldTime = 0;
    }

    private IEnumerator StandUp()
    {
        DebugManager.AddPersistentLineForSpecifiedTime("Standing up...", standUpTime);
        isResting = false;
        isStandingUp = true;
        GameData.cameraManager.DisableRestCam();
        yield return new WaitForSeconds(standUpTime);
        LeaveCampfire();
        isStandingUp = false;
        moveInputHoldTime = 0;
    }
    
    private void TryLieDown()
    {
        //DebugManager.AddPersistentLineForSpecifiedTime("Try to lie down", 2f);
        RaycastHit lieRay = TestRay(groundBoxcastStart.position, Vector3.down, 2f, groundMask, true);

        float groundTiltAngle = Vector3.Angle(Vector3.up, lieRay.normal);
        Vector3 groundSlopeDirection = Vector3.ProjectOnPlane(lieRay.normal, Vector3.up);
        Vector3 playerDirectionAngle = Vector3.ProjectOnPlane(GameData.cameraManager.cam.transform.forward, Vector3.up).normalized;
        if (groundSlopeDirection.magnitude < 0.001f)
        {
            groundSlopeDirection = playerDirectionAngle;
        }
        groundSlopeDirection.Normalize();

        if (lieRay.collider != null)
        {
            float camDirAngle = Vector3.Angle(groundSlopeDirection, playerDirectionAngle);
            float lieDownRoll = Mathf.Sin(Mathf.Deg2Rad * camDirAngle) * groundTiltAngle;
            float lieDownPitch = Mathf.Cos(Mathf.Deg2Rad * camDirAngle) * groundTiltAngle;
            DebugManager.AddPersistentLineForSpecifiedTime("Lie down, slope/cam angle : " + camDirAngle, 2f);
            DebugManager.AddPersistentLineForSpecifiedTime("Lie down roll : " + lieDownRoll, 2f);
            DebugManager.AddPersistentLineForSpecifiedTime("Lie down ground pitch : " + lieDownPitch, 2f);

            if (Mathf.Abs(lieDownRoll) < maxGroundRollToLieDown)
            {
                if(minMaxGroundPitchToLieDown.x < lieDownPitch && lieDownPitch < minMaxGroundPitchToLieDown.y)
                {
                    StartCoroutine(LieDown());
                }
                else
                {
                    DebugManager.AddPersistentLineForSpecifiedTime("Can't lie down because ground is to steep : " + lieDownPitch, 2f);
                }
            }
            else
            {
                DebugManager.AddPersistentLineForSpecifiedTime("Can't lie down because you'll roll away while trying to rest ... : " + Mathf.Abs(lieDownRoll), 2f);
            }
        }
    }

    private IEnumerator LieDown()
    {
        DebugManager.AddPersistentLineForSpecifiedTime("Lying down...", lieDownTime);
        isLyingDown = true;
        yield return new WaitForSeconds(lieDownTime);
        GameData.cameraManager.EnableLyingCam();
        isLyingDown = false;
        isLyingOnTheGround = true;
        moveInputHoldTime = 0;
    }

    private IEnumerator GetUp()
    {
        DebugManager.AddPersistentLineForSpecifiedTime("Getting up...", getUpTime);
        isLyingOnTheGround = false;
        isGettingUp = true;
        GameData.cameraManager.DisableLyingCam();
        yield return new WaitForSeconds(getUpTime);
        isGettingUp = false;
        moveInputHoldTime = 0;
    }

    private void PlaceCampfire(Vector3 wantedPosition)
    {
        bool canBePlaced = true;
        Vector3 adjustedPos = Vector3.zero;

        RaycastHit campRay;
        Physics.Raycast(wantedPosition + Vector3.up * campfireStartRaycastVertOffset, Vector3.down, out campRay, campfireRaycastLength, groundMask);
        if(campRay.collider != null)
        {
            adjustedPos = campRay.point;
            for (int i = 0; i < 4; i++)
            {
                Debug.DrawRay(wantedPosition + new Vector3(i % 2 == 0 ? campfireSideRaycastOffset : -campfireSideRaycastOffset, 0f, i > 1 ? campfireSideRaycastOffset : -campfireSideRaycastOffset) + Vector3.up * campfireStartRaycastVertOffset, Vector3.down * campfireRaycastLength, Color.green, 5f);
                Physics.Raycast(wantedPosition + new Vector3(i % 2 == 0 ? campfireSideRaycastOffset : -campfireSideRaycastOffset, 0f, i > 1 ? campfireSideRaycastOffset : -campfireSideRaycastOffset) + Vector3.up * campfireStartRaycastVertOffset, Vector3.down, out campRay, campfireRaycastLength, groundMask);
                if(campRay.collider == null)
                {
                    canBePlaced = false;
                    canBePlaced = false;
                    DebugManager.AddPersistentLineForSpecifiedTime("Can't place campfire because not enough space in front", 5f);
                    break;
                }
                else
                {
                    Debug.DrawRay(campRay.point, Vector3.one, Color.red, 5f);
                }
            }

            Debug.DrawRay(wantedPosition + Vector3.up * campfireStartRaycastVertOffset, Vector3.down * campfireRaycastLength, Color.blue, 5f);

            Physics.BoxCast(wantedPosition + Vector3.up * campfireStartRaycastVertOffset, campfireBoxcastSize, Vector3.down, out campRay, Quaternion.identity, campfireRaycastLength, groundMask);
            if (campRay.collider != null)
            {
                Debug.DrawRay(campRay.point, Vector3.one, Color.magenta, 5f);
                DebugManager.AddPersistentLineForSpecifiedTime("Height diff campfire placement : " + Mathf.Abs(adjustedPos.y - campRay.point.y), 5f);
                if (Mathf.Abs(adjustedPos.y - campRay.point.y) > campfireMaxHeightDiffToPlace)
                {
                    canBePlaced = false;
                    DebugManager.AddPersistentLineForSpecifiedTime("Can't place campfire because ground too steep", 5f);
                }
            }
            else
            {
                canBePlaced = false;
            }
        }
        else
        {
            canBePlaced = false;
            DebugManager.AddPersistentLineForSpecifiedTime("Can't place campfire because no ground in front", 5f);
        }

        if(canBePlaced)
        {
            DebugManager.AddPersistentLineForSpecifiedTime("Campfire succesfully placed", 5f);
            currentCampfire = Instantiate(campfirePrefab, adjustedPos, Quaternion.identity).GetComponent<Campfire>();
        }
    }

    private void LeaveCampfire()
    {
        if(currentCampfire != null)
        {
            //currentCampfire.Extinguish();
            currentCampfire = null;
        }
    }

    private bool isPressingWaitInput;
    private bool isPassingTime;
    private float passRealTimeElapsed;
    private float currentPassTimeScale;
    private float endPassRealTime;

    private void UpdatePassTime()
    {
        if(isResting)
        {
            isPressingWaitInput = waitInput.action.ReadValue<float>() > 0;
            if (isPressingWaitInput)
            {
                if(!isPassingTime)
                {
                    isPassingTime = true;
                    passRealTimeElapsed = 0;
                    currentPassTimeScale = 1;
                    DebugManager.AddPersistentLineForSpecifiedTime("Started passing time", 2f);
                }
                passRealTimeElapsed += Time.unscaledDeltaTime;
                currentPassTimeScale = timeScaleByPassTimeElapsed.Evaluate(passRealTimeElapsed);
                DebugManager.AddFrameLine("Wait time scale : " + currentPassTimeScale);
            }
            else
            {
                if(isPassingTime)
                {
                    isPassingTime = false;
                    endPassRealTime = Time.unscaledTime;
                    DebugManager.AddPersistentLineForSpecifiedTime("Stopped passing time", 2f);
                }
            }

            Time.timeScale = Mathf.Min(currentPassTimeScale, maxTimeScale);

            GameData.dayNightCycleManager.SetCycleTimeScale(currentPassTimeScale);
        }
        
        if(!isPassingTime && Time.timeScale != 1)
        {
            Time.timeScale = Mathf.Lerp(Mathf.Min(currentPassTimeScale, maxTimeScale), 1f, timeScalePassRelease.Evaluate(Time.unscaledTime - endPassRealTime));
            GameData.dayNightCycleManager.SetCycleTimeScale(Mathf.Lerp(currentPassTimeScale, 1f, timeScalePassRelease.Evaluate(Time.unscaledTime - endPassRealTime)));
            DebugManager.AddFrameLine("End Wait time scale : " + Time.timeScale);
        }
    }

    #endregion

    static public RaycastHit TestRay(Vector3 origin, Vector3 direction, float distance, int layerMask, bool debug)
    {
        RaycastHit hit;
        Physics.Raycast(origin, direction, out hit, distance, layerMask);
        DebugManager.DebugRaycast(origin, direction, hit, distance);
        return hit;
    }

    static public RaycastHit TestBoxcast(Vector3 origin, Vector3 halfExtent, Vector3 direction, Quaternion boxRotation, float distance, LayerMask mask)
    {
        RaycastHit hit;
        Physics.BoxCast(origin, halfExtent, direction, out hit, boxRotation, distance, mask);
        DebugManager.DebugRaycast(origin, direction, hit, distance);
        return hit;
    }

    //------------------------------------------------------------<--....______....-->

    private bool isGameRunning = false; 
    private void OnDrawGizmos()
    {
        for (int i = 0; i < 4; i++)
        {
            Gizmos.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, Vector3.one);
            Gizmos.color = Color.green;
            Gizmos.DrawLine(groundBoxcastStart.position + new Vector3(i % 2 == 0 ? boxcastStartOffset : -boxcastStartOffset, 0f, i > 1 ? boxcastStartOffset : -boxcastStartOffset), groundBoxcastEnd.position + new Vector3(i % 2 == 0 ? boxcastStartOffset : -boxcastStartOffset, 0f, i > 1 ? boxcastStartOffset : -boxcastStartOffset));

            Gizmos.matrix = Matrix4x4.TRS(groundBoxcastStart.position + new Vector3(i % 2 == 0 ? boxcastStartOffset : -boxcastStartOffset, 0f, i > 1 ? boxcastStartOffset : -boxcastStartOffset), Quaternion.identity, Vector3.one);
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(Vector3.zero, feetBoxcastSize * 2);

            Gizmos.matrix = Matrix4x4.TRS(groundBoxcastEnd.position + new Vector3(i % 2 == 0 ? boxcastStartOffset : -boxcastStartOffset, 0f, i > 1 ? boxcastStartOffset : -boxcastStartOffset), Quaternion.identity, Vector3.one);
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(Vector3.zero, feetBoxcastSize * 2);
        }

        if (isGameRunning)
        {
            if(isGrounded)
            {
                for (int i = 0; i < 4; i++)
                {
                    Gizmos.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, Vector3.one);
                    Gizmos.color = Color.magenta;
                    Gizmos.DrawSphere(groundCastHits[i].point, 0.1f); 
                }
            }  
        }
    }
}
