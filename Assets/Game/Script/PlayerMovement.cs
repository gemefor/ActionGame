using UnityEngine;


[RequireComponent(typeof(PlayerInput))]
[RequireComponent(typeof(Rigidbody))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float _moveSpeed = 5f;
    [SerializeField] private float _turnSpeed = 720f;
    
    [Header("Camera")]
    [SerializeField] private Transform _cameraTransform;

    private PlayerInput _input;
    private Rigidbody _rb;
    
    private void Awake()
    {
        _input = GetComponent<PlayerInput>();
        _rb = GetComponent<Rigidbody>();
        
        
        if (_cameraTransform == null)
        {
            _cameraTransform = Camera.main.transform;
        }
    }

    
    private void FixedUpdate()
    {
        Vector3 moveDirectrion = GetCameraRealativeDirection(_input.Direction);
        
        Move(moveDirectrion);
        Rotate(moveDirectrion);
    }

    private void Rotate(Vector3 direction)
    {
        if (direction == Vector3.zero) return;
        
        Quaternion targetRotation = Quaternion.LookRotation(direction);
        Quaternion newRotation = Quaternion.RotateTowards(_rb.rotation, targetRotation,
            Time.fixedDeltaTime * _turnSpeed);
        
        _rb.MoveRotation(newRotation);
    }

    private void Move(Vector3 directrion)
    {
        Vector3 newPosition = _rb.position + directrion * 
            _moveSpeed * Time.fixedDeltaTime;
        _rb.MovePosition(newPosition);
    }

    private Vector3 GetCameraRealativeDirection(Vector3 input)
    {
        Vector3 forward = _cameraTransform.forward;
        Vector3 right = _cameraTransform.right;
        
        forward.y = 0;
        right.y = 0;
        forward.Normalize();
        right.Normalize();
        
        return forward * input.z + right * input.x;
    }
}
