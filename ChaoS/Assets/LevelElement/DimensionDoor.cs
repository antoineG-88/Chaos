using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;

public class DimensionDoor : MonoBehaviour
{
    public Camera PlayerCam;
    public Camera DoorRenderCam;
    public DimensionDoor linkedDoor;
    public MeshRenderer Illusion;
    public AnimationCurve IllusionOpeningAnim;
    public float IllusionOpeningAnimTime;
    public float IllusionNearClipPlaneMargin = 1f;

    public UnityEvent OnIllusionOpened;
    public UnityEvent OnIllusionClosed;

    private Vector3 playerCamPosDiff;
    private Vector3 playerCamRotDiff;
    private Vector3 illusionBaseScale;
    private Coroutine currentAnim;
    private HikerController nearbyPlayer;
    public bool CanTeleport;

    private void Start()
    {
        illusionBaseScale = Illusion.transform.localScale;
        SetEnableIllusionCam(false);
        CanTeleport = true;
    }

    public void Teleport()
    {
        if(CanTeleport)
        {
            //nearbyPlayer.Teleport(linkedDoor.DoorRenderCam.transform.position, linkedDoor.DoorRenderCam.transform.rotation);
            //linkedDoor.CanTeleport = false;
            //DebugManager.AddPersistentLineForSpecifiedTime("Teleported", 2f);
        }
    }

    void LateUpdate()
    {
        playerCamPosDiff = linkedDoor.transform.InverseTransformPoint(PlayerCam.transform.position);
        playerCamRotDiff = linkedDoor.transform.InverseTransformDirection(PlayerCam.transform.rotation.eulerAngles);

        DoorRenderCam.transform.localPosition = playerCamPosDiff;
        DoorRenderCam.transform.localRotation = Quaternion.Euler(playerCamRotDiff);

        DoorRenderCam.nearClipPlane = Mathf.Max(Vector3.Distance(DoorRenderCam.transform.position, Illusion.transform.position) - IllusionNearClipPlaneMargin, 0.03f);
        DoorRenderCam.fieldOfView = PlayerCam.fieldOfView;
    }

    private void SetEnableIllusionCam(bool enable)
    {
        linkedDoor.DoorRenderCam.enabled = enable;
        linkedDoor.DoorRenderCam.gameObject.SetActive(enable);
        Illusion.gameObject.SetActive(enable);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (currentAnim != null)
            {
                StopCoroutine(currentAnim);
            }
            currentAnim = StartCoroutine(PlayOpenAnim(true));
            nearbyPlayer = other.GetComponent<HikerController>();
        }
    }


    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (currentAnim != null)
            {
                StopCoroutine(currentAnim);
            }
            currentAnim = StartCoroutine(PlayOpenAnim(false));
            nearbyPlayer = null;
        }
    }

    private IEnumerator PlayOpenAnim(bool open)
    {
        if (open)
        {
            SetEnableIllusionCam(true);
            OnIllusionOpened.Invoke();
        }

        float elapsedTime = 0;
        while (elapsedTime < IllusionOpeningAnimTime)
        {
            elapsedTime += Time.deltaTime;
            Illusion.transform.localScale = illusionBaseScale * IllusionOpeningAnim.Evaluate((open ? 0f : 1f) + (open ? 1f : -1f) * elapsedTime / IllusionOpeningAnimTime);
            yield return new WaitForEndOfFrame();
        }

        if (open)
        {
            Illusion.transform.localScale = illusionBaseScale;
        }
        else
        {
            Illusion.transform.localScale = Vector3.zero;
            SetEnableIllusionCam(false);
            OnIllusionClosed.Invoke();
        }
        currentAnim = null;
    }

    public void SetCanTeleport(bool value)
    {
        CanTeleport = value;
    }
}