using UnityEngine;
using UnityEngine.Events;

public class TriggerZoneEventCaller : MonoBehaviour
{
    public UnityEvent OnZoneEnter;
    public UnityEvent OnZoneExit;
    public UnityEvent OnZoneStay;
    public string TagFilter;

    private void OnTriggerEnter(Collider other)
    {
        if(TagFilter == "" || other.CompareTag(TagFilter))
        {
            OnZoneEnter.Invoke();
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (TagFilter == "" || other.CompareTag(TagFilter))
        {
            OnZoneExit.Invoke();
        }
    }
    private void OnTriggerStay(Collider other)
    {
        if (TagFilter == "" || other.CompareTag(TagFilter))
        {
            OnZoneStay.Invoke();
        }
    }
}
