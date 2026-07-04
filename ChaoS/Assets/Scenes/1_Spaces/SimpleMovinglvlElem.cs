using UnityEngine;
using System.Collections;
using NaughtyAttributes;

public class SimpleMovinglvlElem : MonoBehaviour
{
    public ElemKey startKey;
    public ElemKey endKey;

    public bool animPos;
    public bool animRot;
    public bool animScale;

    [CurveRange(0f, 0f, 1f, 1f)]
    public AnimationCurve movementCurve;
    public float movementTime;
    public AudioClip movementSound;

    [Range(0f, 1f)]
    public float startOffset;

    private Vector3 originalPos;
    private Quaternion originalRot;
    private Vector3 originalScale;
    private AudioSource source;

    private void Start()
    {
        source = GetComponent<AudioSource>();
        originalPos = transform.position;
        originalRot = transform.rotation;
        originalScale = transform.localScale;
    }

    public void StartMovement()
    {
        StartCoroutine(Move());
        source.PlayOneShot(movementSound);
    }
    public void StartBackwardMovement()
    {
        StartCoroutine(MoveBackwards());
    }

    public void ResetToOriginal()
    {
        transform.position = originalPos;
        transform.rotation = originalRot;
        transform.localScale = originalScale;
    }

    private IEnumerator Move()
    {
        float timeElapsed = 0;
        while (timeElapsed < movementTime)
        {
            if(animPos)
                transform.position = Vector3.Lerp(startKey.position, endKey.position, movementCurve.Evaluate(timeElapsed / movementTime));
            if (animRot)
                transform.rotation = Quaternion.Lerp(Quaternion.Euler(startKey.rotation), Quaternion.Euler(endKey.rotation), movementCurve.Evaluate(timeElapsed / movementTime));
            if (animScale)
                transform.localScale = Vector3.Lerp(startKey.scale, endKey.scale, movementCurve.Evaluate(timeElapsed / movementTime));

            timeElapsed += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }

        if (animPos)
            transform.position = endKey.position;
        if (animRot)
            transform.rotation = Quaternion.Euler(endKey.rotation);
        if (animScale)
            transform.localScale = endKey.scale;
    }

    private IEnumerator MoveBackwards()
    {
        float timeElapsed = 0;
        while (timeElapsed < movementTime)
        {
            timeElapsed += Time.deltaTime;

            if (animPos)
                transform.position = Vector3.Lerp(endKey.position, startKey.position, movementCurve.Evaluate(timeElapsed / movementTime));
            if (animRot)
                transform.rotation = Quaternion.Lerp(Quaternion.Euler(endKey.rotation), Quaternion.Euler(startKey.rotation), movementCurve.Evaluate(timeElapsed / movementTime));
            if (animScale)
                transform.localScale = Vector3.Lerp(endKey.scale, startKey.scale, movementCurve.Evaluate(timeElapsed / movementTime));

            yield return new WaitForEndOfFrame();
        }

        if (animPos)
            transform.position = startKey.position;
        if (animRot)
            transform.rotation = Quaternion.Euler(startKey.rotation);
        if (animScale)
            transform.localScale = startKey.scale;
    }

    [Button]
    public void SaveAsStartKey()
    {
        startKey.position = transform.position;
        startKey.rotation = transform.rotation.eulerAngles;
        startKey.scale = transform.localScale;
    }

    [Button]
    public void SaveAsEndKey()
    {
        endKey.position = transform.position;
        endKey.rotation = transform.rotation.eulerAngles;
        endKey.scale = transform.localScale;
    }
}

[System.Serializable]
public struct ElemKey
{
    [SerializeField, ShowIf("animPos")]
    public Vector3 position;

    [SerializeField, ShowIf("animPos")]
    public Vector3 rotation;

    [SerializeField, ShowIf("animScale")]
    public Vector3 scale;
}
