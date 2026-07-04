using UnityEngine;

public class EventManager : MonoBehaviour
{
    [HideInInspector]
    public float currentTimeScale;

    private void Awake()
    {
        GameData.eventManager = this;
    }

    private void Update()
    {
        UpdateTimeScale();
    }

    private void UpdateTimeScale()
    {

    }
}
