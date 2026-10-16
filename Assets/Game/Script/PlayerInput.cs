using UnityEngine;

public class PlayerInput : MonoBehaviour
{
    public Vector3 Direction { get; private set; }
    
    private void Update()
    {
        float x = Input.GetAxisRaw("Horizontal");
        float z = Input.GetAxisRaw("Vertical");
        Direction = new Vector3(x, 0f, z).normalized;
    }
}
