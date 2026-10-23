using UnityEngine;

namespace PinballSmasher
{
    [RequireComponent(typeof(Rigidbody))]
    public class Robot : MonoBehaviour
    {
        [Header("Movement")]
        [SerializeField] private float _moveForce = 8f;
        [SerializeField] private float _maxSpeed = 3f;

        [Header("Impact")]
        [SerializeField] private float _hitSpeedThreshold = 6f; // с какой скорости игрок считается "тараном"
        [SerializeField] private float _impactMultiplier = 1.5f;
        [SerializeField] private float _stunDuration = 1f;
        [SerializeField] private float _pushPlayerForce = 3f; // толчок, если игрок врезался медленно

        private Rigidbody _rb;
        private Rigidbody _playerBody;
        private Vector3 _playerVelocityBeforeHit;
        private float _stunnedUntil;

        public bool IsStunned => Time.time < _stunnedUntil;
        public bool LastHitByPlayer { get; private set; }
        public float LastPlayerHitTime { get; private set; }
        public bool IsDead { get; private set; }

        private void Awake()
        {
            _rb = GetComponent<Rigidbody>();
        }

        private void Start()
        {
            GameObject player = GameObject.FindWithTag("Player");
            if (player != null)
            {
                _playerBody = player.GetComponent<Rigidbody>();
            }
        }

        private void FixedUpdate()
        {
            if (!HasPlayer()) return;

            // OnCollisionEnter приходит уже после расчёта столкновения, когда игрок успел затормозить.
            // Поэтому скорость игрока запоминаем здесь — до шага физики.
            _playerVelocityBeforeHit = _playerBody.linearVelocity;

            if (IsStunned) return;

            Vector3 direction = _playerBody.position - _rb.position;
            direction.y = 0f;
            if (direction.sqrMagnitude < 0.0001f) return;
            direction.Normalize();

            Vector3 force = direction * _moveForce;
            Vector3 horizontalVelocity = GetHorizontalVelocity(_rb.linearVelocity);

            // Как у игрока: сам робот не разгоняется выше _maxSpeed, но отлетать от удара может быстрее.
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

        private void OnCollisionEnter(Collision collision)
        {
            if (IsDead || !HasPlayer() || collision.rigidbody != _playerBody) return;

            Vector3 awayFromPlayer = _rb.position - _playerBody.position;
            awayFromPlayer.y = 0f;
            if (awayFromPlayer.sqrMagnitude < 0.0001f) return;
            awayFromPlayer.Normalize();

            float playerSpeed = GetHorizontalVelocity(_playerVelocityBeforeHit).magnitude;

            if (playerSpeed >= _hitSpeedThreshold)
            {
                // Сила удара — скорость сближения вдоль линии игрок → робот.
                float impactSpeed = Mathf.Abs(Vector3.Dot(collision.relativeVelocity, awayFromPlayer));
                impactSpeed = Mathf.Max(impactSpeed, playerSpeed);

                _rb.AddForce(awayFromPlayer * impactSpeed * _impactMultiplier, ForceMode.Impulse);
                _stunnedUntil = Time.time + _stunDuration;

                LastHitByPlayer = true;
                LastPlayerHitTime = Time.time;
            }
            else
            {
                _playerBody.AddForce(-awayFromPlayer * _pushPlayerForce, ForceMode.Impulse);
            }
        }

        // Вызывает KillZone; возвращает false, если робот уже засчитан.
        public bool MarkDead()
        {
            if (IsDead) return false;
            IsDead = true;
            return true;
        }

        private bool HasPlayer()
        {
            return _playerBody != null && _playerBody.gameObject.activeInHierarchy;
        }

        private static Vector3 GetHorizontalVelocity(Vector3 velocity)
        {
            velocity.y = 0f;
            return velocity;
        }
    }
}
