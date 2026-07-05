using UnityEngine;

public class DimensionDoor : MonoBehaviour
{
    public Transform PlayerCam;
    public Camera DoorRenderCam;
    public DimensionDoor linkedDoor;

    private Vector3 playerCamPosDiff;
    private Vector3 playerCamRotDiff;

    void LateUpdate()
    {
        playerCamPosDiff = linkedDoor.transform.InverseTransformPoint(PlayerCam.position);
        playerCamRotDiff = linkedDoor.transform.InverseTransformDirection(PlayerCam.rotation.eulerAngles);

        DoorRenderCam.transform.localPosition = playerCamPosDiff;
        DoorRenderCam.transform.localRotation = Quaternion.Euler(playerCamRotDiff);

        DoorRenderCam.nearClipPlane = playerCamPosDiff.magnitude - 1f;
    }
}