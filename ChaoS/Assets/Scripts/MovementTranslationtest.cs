using UnityEngine;

public class MovementTranslationtest : MonoBehaviour
{
    public float speed;
    public Rigidbody rb;
    public bool useTranslate;
    public float height;
    public float lerpRatio;


    private float targetHeight;
    private float newHeight;
    private Vector3 startPos;

    private void Start()
    {
        startPos = transform.position;
    }

    private float currentSpeed;

    private void Update()
    {
        targetHeight = startPos.y + height;
        newHeight = Mathf.Lerp(transform.position.y, targetHeight, lerpRatio * Time.deltaTime);
        currentSpeed = (newHeight - transform.position.y) / Time.deltaTime;

        if (useTranslate)
        {
            transform.position = new Vector3(transform.position.x, newHeight, transform.position.z);
        }
        else
        {
            rb.linearVelocity = Vector3.up * currentSpeed;
        }


        if(Input.GetKeyDown(KeyCode.R))
        {
            Restart();
        }
    }


    private void Restart()
    {
        transform.position = startPos;
    }
}
