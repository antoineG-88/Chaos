using UnityEngine;
using UnityEngine.InputSystem;

public class CameraManager : MonoBehaviour
{
    [Header("Global settings")]
    public float camBaseSensitivity;
    public float camMouseSensitivity;
    public float camGamepadSensitivity;
    public float camMovementLerpRatio;

    [Header("Pivot settings")]
    public float neckCamHorizontalMovementRange;
    public Vector2 neckCamPitchMinMax;
    [NaughtyAttributes.CurveRange(0f, 0f, 1f, 1f, NaughtyAttributes.EColor.Yellow)]
    public AnimationCurve camSensitivityRatioByNeckRangeRatio;

    [Header("Bobbing settings")]
    public Animator bobbingAnimator;
    [NaughtyAttributes.AnimatorParam("bobbingAnimator")]
    public string movingParam;
    [NaughtyAttributes.AnimatorParam("bobbingAnimator")]
    public string runningParam;
    [NaughtyAttributes.AnimatorParam("bobbingAnimator")]
    public string sittingParam;
    [NaughtyAttributes.AnimatorParam("bobbingAnimator")]
    public string lyingParam;

    [Header("Ref")]
    public Transform camBaseTargetPos;
    public Transform camBase;
    //public Transform camNeck;
    public Camera cam;

    [Header("Inputs")]
    public InputActionReference aimCamInput;
    public InputActionReference escapeInput;
    public PlayerInput playerInput;

    private float currentOrientationAngle;
    private Vector2 aimInputValue;
    private Vector2 currentAimMovement;
    private Vector3 targetAimRotation;
    //private float currentRestOrientation;
    private bool isInRest;
    private bool isLying;
    private bool isRotatingNeck;
    private Vector3 targetNeckRotation;
    private bool isGameFocused;

    void Awake()
    {
        GameData.cameraManager = this;
        isRotatingNeck = true;
        isGameFocused = true;
    }

    public float CamCurrentOrientation { get { return currentOrientationAngle; } }

    void Update()
    {
        UpdateCamPos();
        UpdateCameraAim();
        UpdateBobbingOffset();

        if(escapeInput.action.IsPressed())
        {
            isGameFocused = false;
        }
    }

    public void SetAim(Quaternion aimRotation)
    {
        targetAimRotation = aimRotation.eulerAngles;
        camBase.rotation = Quaternion.Euler(0f, aimRotation.eulerAngles.y, 0f);
        transform.rotation = aimRotation;
    }

    private void UpdateCamPos()
    {
        camBase.position = camBaseTargetPos.position;
    }

    private void UpdateCameraAim()
    {
        aimInputValue = aimCamInput.action.ReadValue<Vector2>();

        if(Application.isFocused)
        {
            if(isGameFocused)
            {
                currentAimMovement = aimInputValue * camBaseSensitivity * 0.1f;
                currentAimMovement *= playerInput.currentControlScheme == "Manette" ? camGamepadSensitivity : camMouseSensitivity;
                Cursor.visible = false;
                Cursor.lockState = CursorLockMode.Locked;
            }
            else
            {
                if(Input.GetMouseButtonDown(0))
                {
                    isGameFocused = true;
                }
                currentAimMovement = Vector2.zero;
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None;
            }
        }

        //currentAimMovement.x *= Mathf.Sign(Mathf.DeltaAngle(targetAimRotation.y, currentRestOrientation)) != Mathf.Sign(currentAimMovement.x) ? restCamSensitivityRatioByOrientation.Evaluate(Mathf.Abs(Mathf.DeltaAngle(targetAimRotation.y, currentRestOrientation)) / (neckCamHorizontalMovementRange / 2f)) : 1f;
        //targetAimRotation.y = Mathf.Clamp(targetAimRotation.y + currentAimMovement.x, currentRestOrientation - neckCamHorizontalMovementRange / 2f, currentRestOrientation + neckCamHorizontalMovementRange / 2f);
        
        if (!isRotatingNeck)
        {
            currentAimMovement.x *= Mathf.Sign(targetAimRotation.y) == Mathf.Sign(currentAimMovement.x) ? camSensitivityRatioByNeckRangeRatio.Evaluate(Mathf.Abs(targetAimRotation.y) / neckCamHorizontalMovementRange) : 1f;
            currentAimMovement.y *= Mathf.Sign(targetAimRotation.x) == Mathf.Sign(-currentAimMovement.y) ? camSensitivityRatioByNeckRangeRatio.Evaluate(Mathf.Abs(targetAimRotation.x) / neckCamPitchMinMax.y) : 1f;
        }

        if (Input.GetKeyDown(KeyCode.K))
        {
            if(isRotatingNeck)
            {
                DisableNeckControl();
            }
            else
            {
                EnableNeckControl();
            }
        }

        if (isRotatingNeck)
        {
            targetNeckRotation.y = targetNeckRotation.y + currentAimMovement.x;
        }
        else
        {
            targetAimRotation.y = Mathf.Clamp(targetAimRotation.y + currentAimMovement.x, -neckCamHorizontalMovementRange, neckCamHorizontalMovementRange);
        }

        targetAimRotation.x = Mathf.Clamp(targetAimRotation.x - currentAimMovement.y, neckCamPitchMinMax.x, neckCamPitchMinMax.y);

        if (Quaternion.Angle(transform.localRotation, Quaternion.Euler(targetAimRotation)) > 0.01f)
        {
            transform.localRotation = Quaternion.Lerp(transform.localRotation, Quaternion.Euler(targetAimRotation), camMovementLerpRatio * Time.deltaTime);
        }
        else
        {
            transform.localRotation = Quaternion.Euler(targetAimRotation);
            /*if (Mathf.Abs(targetAimRotation.y) > 720f)
            {
                targetAimRotation.y = transform.localRotation.eulerAngles.y;
            }*/
        }

        if (Quaternion.Angle(transform.localRotation, Quaternion.Euler(targetNeckRotation)) > 0.01f)
        {
            camBase.rotation = Quaternion.Lerp(camBase.rotation, Quaternion.Euler(targetNeckRotation), camMovementLerpRatio * Time.deltaTime);
        }
        else
        {
            camBase.rotation = Quaternion.Euler(targetNeckRotation);
            if (Mathf.Abs(targetNeckRotation.y) > 720f)
            {
                targetNeckRotation.y = camBase.rotation.eulerAngles.y;
            }
        }

        currentOrientationAngle = transform.rotation.eulerAngles.y;

        DebugManager.AddFrameLine("Orientation Angle : " + currentOrientationAngle);
        DebugManager.AddFrameLine("Local cam rotation : X = " + transform.localRotation.eulerAngles.y.ToString("0.0") + " - Y = " + transform.localRotation.eulerAngles.x.ToString("0.0"));
        DebugManager.AddFrameLine(isRotatingNeck ? "<Color=#a09ae3>Camera is free</Color>" : "<Color=#d9bfdb>Camera is restrained</Color>");
    }

    private void EnableNeckControl()
    {
        isRotatingNeck = true;
        float synchRotation = transform.rotation.eulerAngles.y;
        camBase.rotation = Quaternion.Euler(0f, synchRotation, 0f);
        targetNeckRotation.y = synchRotation;
        transform.localRotation = Quaternion.Euler(transform.localRotation.eulerAngles.x, 0f, transform.localRotation.eulerAngles.z);
        targetAimRotation.y = 0;
        DebugManager.AddPersistentLineForSpecifiedTime("Enabled neck rotation", 2f);
    }

    private void DisableNeckControl()
    {
        isRotatingNeck = false;
        DebugManager.AddPersistentLineForSpecifiedTime("Disabled neck rotation", 2f);
    }

    private void UpdateBobbingOffset()
    {
        bobbingAnimator.SetBool(movingParam, GameData.hikerController.isMoving);
        bobbingAnimator.SetBool(runningParam, GameData.hikerController.isRunning);
        bobbingAnimator.SetBool(sittingParam, GameData.hikerController.isResting || GameData.hikerController.isSittingDown);
        bobbingAnimator.SetBool(lyingParam, GameData.hikerController.isLyingDown || GameData.hikerController.isLyingOnTheGround);
    }

    public void EnableRestCam()
    {
        //targetAimRotation.y = transform.rotation.eulerAngles.y;
        //currentRestOrientation = targetAimRotation.y;
        DisableNeckControl();
        isInRest = true;
    }

    public void EnableLyingCam()
    {
        //targetAimRotation.y = transform.rotation.eulerAngles.y;
        //currentRestOrientation = targetAimRotation.y;
        isLying = true;
    }

    public void DisableRestCam()
    {
        EnableNeckControl();
        isInRest = false;
    }

    public void DisableLyingCam()
    {
        isLying = false;
    }
}
