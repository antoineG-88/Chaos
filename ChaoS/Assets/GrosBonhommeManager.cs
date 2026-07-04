using UnityEngine;
using UnityEngine.InputSystem;

public class GrosBonhommeManager : MonoBehaviour
{
    public Transform destination;
    public float speed;
    public Transform player;

    private void Update()
    {
        Vector3 direction = Vector3.zero;
        direction = destination.position - transform.position;
        if (direction.magnitude > speed * Time.deltaTime)
        {
            direction.Normalize();
            transform.position += direction * speed * Time.deltaTime;
        }
        else
        {
            transform.position = destination.position;
        }

        transform.forward = direction;

        if (Input.GetKeyDown(KeyCode.U))
        {
            destination.position = player.position;
        }
    }
}
