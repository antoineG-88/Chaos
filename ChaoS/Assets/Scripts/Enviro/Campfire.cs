using UnityEngine;
using System.Collections.Generic;

public class Campfire : MonoBehaviour
{
    public float maxLitTime;
    [Space]
    public Light pointLight;
    public List<MeshRenderer> wood;
    public Material woodLitMat;
    public Material woodExtinguishedMat;

    [HideInInspector]
    public bool isLit;
    private float currentLitTimeElapsed;

    private void Start()
    {
        isLit = true;
    }

    private void Update()
    {
        currentLitTimeElapsed += Time.deltaTime;
        if(currentLitTimeElapsed > maxLitTime)
        {
            Extinguish();
        }
    }

    public void Extinguish()
    {
        if(isLit)
        {
            foreach (MeshRenderer renderer in wood)
            {
                renderer.material = woodExtinguishedMat;
            }
            isLit = false;
            pointLight.enabled = false;
        }
    }

    public void Lit()
    {
        if (!isLit)
        {
            foreach (MeshRenderer renderer in wood)
            {
                renderer.material = woodLitMat;
            }
            isLit = true;
            pointLight.enabled = true;
        }
    }
}
