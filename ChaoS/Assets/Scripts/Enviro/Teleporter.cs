using UnityEngine;

public class Teleporter : MonoBehaviour
{
    public Transform destination;

    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject == HikerController.I.gameObject)
        {
            HikerController.I.Teleport(destination.position, destination.rotation);
        }
    }
}
