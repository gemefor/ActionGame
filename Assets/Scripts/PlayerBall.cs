using UnityEngine;

namespace PinballSmasher
{
    [RequireComponent(typeof(Rigidbody))]
    public class PlayerBall : MonoBehaviour
    {
        [Header("Movement")]
        [SerializeField] private float _moveForce = 20f;
        [SerializeField] private float _maxSpeed = 10f; // предел скорости от разгона клавишами

        [Header("Rush")]
        [SerializeField] private float _rushForce = 15f;
        [SerializeField] private float _rushCooldown = 1.5f;

        [Header("Camera")]
        [SerializeField] private Transform _cameraTransform;

        private Rigidbody _rb;
        private Vector3 _inputDirection;
        private bool _rushRequested;
        private float _nextRushTime;

        public float CurrentSpeed => _rb.linearVelocity.magnitude;

        // Оставшаяся доля перезарядки рывка: 1 — только что использован, 0 — готов.
        public float DashCooldown01 => _rushCooldown > 0f
            ? Mathf.Clamp01((_nextRushTime - Time.time) / _rushCooldown)
            : 0f;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody>();

            if (_cameraTransform == null && Camera.main != null)
            {
                _cameraTransform = Camera.main.transform;
            }
        }

        // Ввод читаем в Update, чтобы не пропустить нажатие пробела между шагами физики.
        private void Update()
        {
            float x = Input.GetAxisRaw("Horizontal");
            float z = Input.GetAxisRaw("Vertical");
            _inputDirection = GetCameraRelativeDirection(new Vector3(x, 0f, z));

            if (Input.GetKeyDown(KeyCode.Space))
            {
                _rushRequested = true;
            }
        }

        private void FixedUpdate()
        {
            Move();
            TryRush();
        }

        private void Move()
        {
            if (_inputDirection == Vector3.zero) return;

            Vector3 force = _inputDirection * _moveForce;
            Vector3 horizontalVelocity = GetHorizontalVelocity();

            // На пределе скорости убираем составляющую силы вдоль движения:
            // сам шар быстрее не разгонится, но может поворачивать и тормозить.
            // Скорость от рывка и отскоков от бамперов при этом не срезается.
            if (horizontalVelocity.magnitude >= _maxSpeed)
            {
                Vector3 velocityDirection = horizontalVelocity.normalized;
                float alongVelocity = Vector3.Dot(force, velocityDirection);
                if (alongVelocity > 0f)
                {
                    force -= velocityDirection * alongVelocity;
                }
            }

            _rb.AddForce(force, ForceMode.Force);
        }

        private void TryRush()
        {
            if (!_rushRequested) return;
            _rushRequested = false;

            if (Time.time < _nextRushTime) return;

            // Рывок по направлению ввода, а без ввода — по текущему движению.
            Vector3 direction = _inputDirection;
            if (direction == Vector3.zero)
            {
                direction = GetHorizontalVelocity().normalized;
            }
            if (direction == Vector3.zero) return;

            _rb.AddForce(direction * _rushForce, ForceMode.Impulse);
            _nextRushTime = Time.time + _rushCooldown;
        }

        private Vector3 GetHorizontalVelocity()
        {
            Vector3 velocity = _rb.linearVelocity;
            velocity.y = 0f;
            return velocity;
        }

        private Vector3 GetCameraRelativeDirection(Vector3 input)
        {
            if (_cameraTransform == null) return input.normalized;

            Vector3 forward = _cameraTransform.forward;
            Vector3 right = _cameraTransform.right;

            forward.y = 0f;
            right.y = 0f;
            forward.Normalize();
            right.Normalize();

            return (forward * input.z + right * input.x).normalized;
        }
    }
}
