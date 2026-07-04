using UnityEngine;

public class DayNightCycleManager : MonoBehaviour
{
    [Header("Cycle settings")]
    public float baseCycleTime;
    public float baseSunYaw;
    [NaughtyAttributes.CurveRange(0f,1f,-180f,180f, NaughtyAttributes.EColor.Yellow)]
    public AnimationCurve sunYawByCycleTime;
    [NaughtyAttributes.CurveRange(0f, 1f, -180f, 180f, NaughtyAttributes.EColor.Yellow)]
    public AnimationCurve sunPitchByCycleTime;
    public bool usePreviewAsStartCycleRatio;
    [NaughtyAttributes.HideIf("usePreviewAsStartCycleRatio")]
    public float startCycleRatio;

    [Space]
    [Range(0f, 1f)]
    public float cycleRatioPreview;

    [Space]
    [Header("References")]
    public Light sun;

    //------------------------------------------------------------<--....______....-->

    private float currentCycleTime;
    private float currentCycle;
    private float currentCycleMovementRatio;
    private float currentCycleTimeScale;

    private void Awake()
    {
        GameData.dayNightCycleManager = this;
    }

    private void Start()
    {
        currentCycleTime = (usePreviewAsStartCycleRatio ? cycleRatioPreview : startCycleRatio) * baseCycleTime;
        currentCycle = 0;
        currentCycleMovementRatio = 1;
    }

    private void Update()
    {
        UpdateCycle();
    }

    public void SetCycleTimeScale(float timeScale)
    {
        currentCycleTimeScale = timeScale;
    }

    private void UpdateCycle()
    {
        currentCycleTime += Time.unscaledDeltaTime * currentCycleMovementRatio * currentCycleTimeScale;
        if(currentCycleTime > baseCycleTime)
        {
            currentCycle++;
            DebugManager.AddPersistentLineForSpecifiedTime("Cycle " + currentCycle + " begins", 2f);
            currentCycleTime -= baseCycleTime;
        }

        SetSunRotation(sunYawByCycleTime.Evaluate(currentCycleTime / baseCycleTime), sunPitchByCycleTime.Evaluate(currentCycleTime / baseCycleTime));
    }

    private void SetSunRotation(float yaw, float pitch)
    {
        sun.transform.rotation = Quaternion.Euler(pitch, baseSunYaw + yaw, 0f);
    }

    private void OnDrawGizmosSelected()
    {
        if(Application.isEditor)
        {
            if(sun != null)
            SetSunRotation(sunYawByCycleTime.Evaluate(cycleRatioPreview), sunPitchByCycleTime.Evaluate(cycleRatioPreview));
        }
    }
}
