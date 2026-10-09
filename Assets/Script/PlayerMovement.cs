using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float _speed = 5f;
    
    private PlayerInput _input;
    
    void Awake()
    {
        _input = GetComponent<PlayerInput>();
    }

    
    void Update()
    {
        Vector3 direction = _input.Direction;
        transform.position += direction * _speed * Time.deltaTime;
    }
}
