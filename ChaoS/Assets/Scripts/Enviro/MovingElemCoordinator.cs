using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class MovingElemCoordinator : MonoBehaviour
{
    public List<SimpleMovinglvlElem> elements;
    public bool loop;
    public string shortcutStart;
    public string shortcutReset;

    public float timeBetweenMovingElement;

    private void Update()
    {
        if(Input.GetKeyDown(shortcutStart))
        {
            StartCoroutine(ChainEvents());
        }

        if (Input.GetKeyDown(shortcutReset))
        {
            ResetAll();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.tag == "Player")
        {
            StartCoroutine(ChainEvents());
        }
    }


    private IEnumerator ChainEvents()
    {
        for (int i = 0; i < elements.Count; i++)
        {
            elements[i].StartMovement();
            yield return new WaitForSeconds(timeBetweenMovingElement);
        }

        if (loop)
        {
            StartCoroutine(ChainBackwardsEvents());
        }
    }

    private IEnumerator ChainBackwardsEvents()
    {
        for (int i = 0; i < elements.Count; i++)
        {
            elements[i].StartBackwardMovement();
            yield return new WaitForSeconds(timeBetweenMovingElement);
        }
        if (loop)
        {
            StartCoroutine(ChainEvents());
        }
    }

    private void ResetAll()
    {
        for (int i = 0; i < elements.Count; i++)
        {
            elements[i].ResetToOriginal();
        }
    }
}
