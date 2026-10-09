using UnityEngine;

public class PlayerInput : MonoBehaviour
{
    public Vector3 Direction { get; private set; }

    private 
    void Awake()
    {
        transform.position = new Vector3(0f, 0.5f, 0f);
    }


    void FixedUpdate()
    {
        float x = Input.GetAxisRaw("Horizontal");
        float z = Input.GetAxisRaw("Vertical");
        Direction = new Vector3(x, 0f, z).normalized;
    }
}
