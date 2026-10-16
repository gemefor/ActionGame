using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    
    [SerializeField] private Transform _target; // позиция игрока
    [SerializeField] private Vector3 _offset = new Vector3(0f, 8f, -6f);
    [SerializeField] private float _smoothTime = 0.15f;
    [SerializeField] private bool _lookAtTarget = true;
    
    private Vector3 _velocity;

    private void LateUpdate()
    {
        if (_target == null) return; // Проверка поставлен ли таргет, в нашем случае персонаж.
        
        Vector3 desiredPosition = _target.position + _offset; // Вычисляем позицию камеры относительно игрока. 
        transform.position = Vector3.SmoothDamp(transform.position,
            desiredPosition, ref _velocity, _smoothTime); // 

        if (_lookAtTarget)
        {
            transform.LookAt(_target);
        }
        
    }
}    
