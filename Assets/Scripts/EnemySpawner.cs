using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PinballSmasher
{
    public class EnemySpawner : MonoBehaviour
    {
        [SerializeField] private Robot _robotPrefab;

        [Header("Placement")]
        [SerializeField] private float _spawnRadius = 8f; // центр — позиция спавнера
        [SerializeField] private float _spawnHeight = 1f;

        [Header("Waves")]
        [SerializeField] private int _firstWaveSize = 2;
        [SerializeField] private int _waveSizeIncrease = 2;
        [SerializeField] private float _delayBetweenWaves = 2f;

        private readonly List<Robot> _aliveRobots = new List<Robot>();
        private bool _waitingForNextWave;

        private void Start()
        {
            StartCoroutine(StartNextWave(0f));
        }

        private void Update()
        {
            if (_waitingForNextWave) return;
            if (GameManager.Instance != null && GameManager.Instance.IsGameOver) return;

            // Уничтоженные объекты в Unity сравниваются с null как true.
            _aliveRobots.RemoveAll(robot => robot == null);

            if (_aliveRobots.Count == 0)
            {
                StartCoroutine(StartNextWave(_delayBetweenWaves));
            }
        }

        private IEnumerator StartNextWave(float delay)
        {
            _waitingForNextWave = true;

            if (delay > 0f)
            {
                yield return new WaitForSeconds(delay);
            }

            if (GameManager.Instance != null && GameManager.Instance.IsGameOver) yield break;

            int wave = GameManager.Instance != null ? GameManager.Instance.NextWave() : 1;
            int count = _firstWaveSize + (wave - 1) * _waveSizeIncrease;

            for (int i = 0; i < count; i++)
            {
                SpawnRobot();
            }

            _waitingForNextWave = false;
        }

        private void SpawnRobot()
        {
            if (_robotPrefab == null)
            {
                Debug.LogWarning("EnemySpawner: не назначен префаб робота.", this);
                return;
            }

            float angle = Random.Range(0f, Mathf.PI * 2f);
            Vector3 offset = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * _spawnRadius;
            Vector3 position = transform.position + offset + Vector3.up * _spawnHeight;
            Quaternion rotation = Quaternion.LookRotation(-offset);

            _aliveRobots.Add(Instantiate(_robotPrefab, position, rotation));
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            const int segments = 48;
            Vector3 previous = transform.position + Vector3.right * _spawnRadius;
            for (int i = 1; i <= segments; i++)
            {
                float angle = i * Mathf.PI * 2f / segments;
                Vector3 next = transform.position + new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * _spawnRadius;
                Gizmos.DrawLine(previous, next);
                previous = next;
            }
        }
    }
}
