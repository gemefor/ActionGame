using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace PinballSmasher
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [SerializeField] private int _pointsPerRobot = 100;

        [Header("Combo")]
        [SerializeField] private float _comboTime = 3f;
        [SerializeField] private int _maxMultiplier = 5;

        [Header("Events")]
        public UnityEvent<int> OnScoreChanged;
        public UnityEvent<int, int> OnComboChanged; // (comboCount, multiplier)
        public UnityEvent<float> OnComboTimerChanged; // 1 — комбо только что продлено, 0 — сброшено
        public UnityEvent<int> OnWaveChanged;
        public UnityEvent OnGameOver;

        public int Score { get; private set; }
        public int ComboCount { get; private set; }
        public int Multiplier => Mathf.Min(1 + ComboCount / 2, _maxMultiplier);
        public int Wave { get; private set; }
        public bool IsGameOver { get; private set; }

        private float _comboTimeLeft;

        private void Awake()
        {
            // Синглтон в пределах сцены: при перезапуске сцены создаётся заново.
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.R))
            {
                RestartScene();
            }

            UpdateComboTimer();
        }

        public void RegisterRobotKill(Robot robot)
        {
            if (IsGameOver) return;

            ComboCount++;
            _comboTimeLeft = _comboTime;

            Score += _pointsPerRobot * Multiplier;

            OnScoreChanged?.Invoke(Score);
            OnComboChanged?.Invoke(ComboCount, Multiplier);
            OnComboTimerChanged?.Invoke(1f);
        }

        private void UpdateComboTimer()
        {
            if (ComboCount == 0) return;

            _comboTimeLeft -= Time.deltaTime;
            if (_comboTimeLeft <= 0f)
            {
                ResetCombo();
                return;
            }

            OnComboTimerChanged?.Invoke(_comboTimeLeft / _comboTime);
        }

        private void ResetCombo()
        {
            ComboCount = 0;
            _comboTimeLeft = 0f;

            OnComboChanged?.Invoke(ComboCount, Multiplier);
            OnComboTimerChanged?.Invoke(0f);
        }

        public int NextWave()
        {
            Wave++;
            OnWaveChanged?.Invoke(Wave);
            return Wave;
        }

        public void TriggerGameOver()
        {
            if (IsGameOver) return;

            IsGameOver = true;
            if (ComboCount > 0)
            {
                ResetCombo();
            }
            Debug.Log($"Game Over. Счёт: {Score}, волна: {Wave}. Нажмите R для перезапуска.");
            OnGameOver?.Invoke();
        }

        public void RestartScene()
        {
            Scene scene = SceneManager.GetActiveScene();
            SceneManager.LoadScene(scene.buildIndex);
        }
    }
}
