using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace PinballSmasher
{
    public class UIManager : MonoBehaviour
    {
        [Header("HUD")]
        [SerializeField] private TMP_Text _scoreText;
        [SerializeField] private TMP_Text _waveText;
        [SerializeField] private TMP_Text _comboText;
        [SerializeField] private Image _comboBar; // Image Type = Filled
        [SerializeField] private Image _dashBar;  // Image Type = Filled

        [Header("Combo Punch")]
        [SerializeField] private float _punchScale = 1.5f;
        [SerializeField] private float _punchDuration = 0.2f;

        [Header("Game Over")]
        [SerializeField] private GameObject _gameOverPanel;
        [SerializeField] private TMP_Text _finalScoreText;

        [Header("Player")]
        [SerializeField] private PlayerBall _player; // если пусто — ищется по тегу Player

        private GameManager _gameManager;
        private int _lastMultiplier = 1;
        private Coroutine _punchRoutine;

        // Подписываемся в Start, а не в OnEnable: к этому моменту GameManager.Instance уже задан в Awake.
        private void Start()
        {
            if (_player == null)
            {
                GameObject playerObject = GameObject.FindWithTag("Player");
                if (playerObject != null)
                {
                    _player = playerObject.GetComponent<PlayerBall>();
                }
            }

            if (_gameOverPanel != null)
            {
                _gameOverPanel.SetActive(false);
            }

            _gameManager = GameManager.Instance;
            if (_gameManager == null)
            {
                Debug.LogWarning("UIManager: в сцене нет GameManager.", this);
                return;
            }

            _gameManager.OnScoreChanged.AddListener(HandleScoreChanged);
            _gameManager.OnComboChanged.AddListener(HandleComboChanged);
            _gameManager.OnComboTimerChanged.AddListener(HandleComboTimerChanged);
            _gameManager.OnWaveChanged.AddListener(HandleWaveChanged);
            _gameManager.OnGameOver.AddListener(HandleGameOver);

            // События, вызванные до нашего Start (например, первая волна), уже прошли — берём текущее состояние.
            HandleScoreChanged(_gameManager.Score);
            HandleWaveChanged(_gameManager.Wave);
            HandleComboChanged(_gameManager.ComboCount, _gameManager.Multiplier);
            HandleComboTimerChanged(0f);
        }

        private void OnDestroy()
        {
            if (_gameManager == null) return;

            _gameManager.OnScoreChanged.RemoveListener(HandleScoreChanged);
            _gameManager.OnComboChanged.RemoveListener(HandleComboChanged);
            _gameManager.OnComboTimerChanged.RemoveListener(HandleComboTimerChanged);
            _gameManager.OnWaveChanged.RemoveListener(HandleWaveChanged);
            _gameManager.OnGameOver.RemoveListener(HandleGameOver);
        }

        private void Update()
        {
            if (_dashBar != null && _player != null)
            {
                _dashBar.fillAmount = 1f - _player.DashCooldown01;
            }
        }

        private void HandleScoreChanged(int score)
        {
            if (_scoreText != null)
            {
                _scoreText.text = $"SCORE: {score}";
            }
        }

        private void HandleWaveChanged(int wave)
        {
            if (_waveText != null)
            {
                _waveText.text = $"WAVE {wave}";
            }
        }

        private void HandleComboChanged(int comboCount, int multiplier)
        {
            if (_comboText == null) return;

            bool visible = multiplier > 1;
            _comboText.gameObject.SetActive(visible);

            if (visible)
            {
                _comboText.text = $"x{multiplier} COMBO!";

                if (multiplier > _lastMultiplier)
                {
                    PlayPunch();
                }
            }
            else
            {
                StopPunch();
            }

            _lastMultiplier = multiplier;
        }

        private void HandleComboTimerChanged(float value)
        {
            if (_comboBar != null)
            {
                _comboBar.fillAmount = value;
            }
        }

        private void HandleGameOver()
        {
            if (_gameOverPanel != null)
            {
                _gameOverPanel.SetActive(true);
            }

            if (_finalScoreText != null)
            {
                _finalScoreText.text = $"SCORE: {_gameManager.Score}\nWAVE {_gameManager.Wave}\n\nPress R to restart";
            }
        }

        private void PlayPunch()
        {
            StopPunch();
            _punchRoutine = StartCoroutine(Punch(_comboText.transform));
        }

        private void StopPunch()
        {
            if (_punchRoutine != null)
            {
                StopCoroutine(_punchRoutine);
                _punchRoutine = null;
            }

            if (_comboText != null)
            {
                _comboText.transform.localScale = Vector3.one;
            }
        }

        // Резко увеличиваем и плавно возвращаем к обычному размеру.
        private IEnumerator Punch(Transform target)
        {
            float elapsed = 0f;
            while (elapsed < _punchDuration)
            {
                float t = elapsed / _punchDuration;
                float scale = Mathf.Lerp(_punchScale, 1f, 1f - (1f - t) * (1f - t));
                target.localScale = Vector3.one * scale;
                elapsed += Time.deltaTime;
                yield return null;
            }

            target.localScale = Vector3.one;
            _punchRoutine = null;
        }
    }
}
