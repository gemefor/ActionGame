using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace PinballSmasher
{
    [RequireComponent(typeof(Collider))]
    public class Bumper : MonoBehaviour
    {
        private static readonly int EmissionColorId = Shader.PropertyToID("_EmissionColor");

        [Header("Bounce")]
        [SerializeField] private float _bounceForce = 15f;

        [Header("Pulse")]
        [SerializeField] private float _pulseScale = 1.2f;
        [SerializeField] private float _pulseDuration = 0.15f;
        [SerializeField, ColorUsage(false, true)] private Color _flashEmission = new Color(4f, 2f, 0.5f);
        [SerializeField] private Renderer _renderer; // если пусто — берётся с этого объекта

        [Header("Events")]
        public UnityEvent OnBumped;

        private Material _material;
        private Color _baseEmission;
        private Vector3 _baseScale;
        private Coroutine _pulseRoutine;

        private void Awake()
        {
            _baseScale = transform.localScale;

            if (_renderer == null)
            {
                _renderer = GetComponent<Renderer>();
            }

            if (_renderer != null)
            {
                // .material создаёт копию, чтобы вспышка не красила все бамперы с этим материалом.
                _material = _renderer.material;
                _material.EnableKeyword("_EMISSION");
                _baseEmission = _material.GetColor(EmissionColorId);
            }
        }

        private void OnCollisionEnter(Collision collision)
        {
            Rigidbody body = collision.rigidbody;
            if (body == null) return;

            Vector3 direction = body.position - transform.position;
            direction.y = 0f;

            // Если тело точно над центром, толкаем по нормали контакта.
            if (direction.sqrMagnitude < 0.0001f)
            {
                direction = -collision.GetContact(0).normal;
                direction.y = 0f;
            }
            if (direction.sqrMagnitude < 0.0001f) return;

            body.AddForce(direction.normalized * _bounceForce, ForceMode.Impulse);

            if (_pulseRoutine != null)
            {
                StopCoroutine(_pulseRoutine);
            }
            _pulseRoutine = StartCoroutine(Pulse());

            OnBumped?.Invoke();
        }

        // Мгновенно раздуваемся и вспыхиваем, затем за _pulseDuration плавно возвращаемся.
        private IEnumerator Pulse()
        {
            float elapsed = 0f;
            while (elapsed < _pulseDuration)
            {
                float k = 1f - elapsed / _pulseDuration;
                ApplyPulse(k);
                elapsed += Time.deltaTime;
                yield return null;
            }

            ApplyPulse(0f);
            _pulseRoutine = null;
        }

        private void ApplyPulse(float k)
        {
            transform.localScale = _baseScale * Mathf.Lerp(1f, _pulseScale, k);

            if (_material != null)
            {
                _material.SetColor(EmissionColorId, Color.Lerp(_baseEmission, _flashEmission, k));
            }
        }

        private void OnDisable()
        {
            if (_pulseRoutine != null)
            {
                StopCoroutine(_pulseRoutine);
                _pulseRoutine = null;
            }
            ApplyPulse(0f);
        }

        private void OnDestroy()
        {
            if (_material != null)
            {
                Destroy(_material);
            }
        }
    }
}
