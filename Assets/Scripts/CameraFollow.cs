using UnityEngine;

namespace PinballSmasher
{
    public class CameraFollow : MonoBehaviour
    {
        [SerializeField] private Transform _target;
        [SerializeField] private Vector3 _offset = new Vector3(0f, 10f, -8f);
        [SerializeField] private float _followSpeed = 5f; // чем больше, тем плотнее камера держится за целью

        private void LateUpdate()
        {
            if (_target == null) return;

            Vector3 desiredPosition = _target.position + _offset;
            transform.position = Vector3.Lerp(transform.position, desiredPosition,
                _followSpeed * Time.deltaTime);

            transform.LookAt(_target);
        }
    }
}
