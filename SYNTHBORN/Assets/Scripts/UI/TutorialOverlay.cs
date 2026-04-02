using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Synthborn.Core.Events;
using Synthborn.Core.Persistence;

namespace Synthborn.UI
{
    /// <summary>
    /// Tutorial hint overlay — displays text prompts with fade animation.
    /// Can be driven by TutorialManager (data-driven steps) or used standalone
    /// for the legacy timed sequence.
    /// </summary>
    public class TutorialOverlay : MonoBehaviour
    {
        [SerializeField] private TMP_Text _hintText;
        [SerializeField] private Button _skipButton;
        [SerializeField] private float _defaultDisplayDuration = 3.5f;
        [SerializeField] private float _fadeDuration = 0.5f;

        private CanvasGroup _canvasGroup;
        private TutorialManager _manager;

        private void Awake()
        {
            _canvasGroup = GetComponent<CanvasGroup>();
            if (_canvasGroup == null) _canvasGroup = gameObject.AddComponent<CanvasGroup>();
            _canvasGroup.alpha = 0f;

            _manager = GetComponentInParent<TutorialManager>();
            if (_manager == null) _manager = FindAnyObjectByType<TutorialManager>();
        }

        private void OnEnable()
        {
            if (_skipButton != null)
                _skipButton.onClick.AddListener(OnSkipClicked);
            GameEvents.OnPlayerDied += OnPlayerDied;
        }

        private void OnDisable()
        {
            if (_skipButton != null)
                _skipButton.onClick.RemoveListener(OnSkipClicked);
            GameEvents.OnPlayerDied -= OnPlayerDied;
        }

        private void Start()
        {
            // If no TutorialManager is driving us, fall back to legacy sequence
            if (_manager == null && !SaveManager.Data.tutorialCompleted)
                StartCoroutine(LegacyTutorialSequence());

            // Show skip button only during tutorial
            if (_skipButton != null)
                _skipButton.gameObject.SetActive(!SaveManager.Data.tutorialCompleted);
        }

        // ─── Public API (called by TutorialManager) ───

        /// <summary>Show a hint externally. Returns when the hint has fully displayed and faded.</summary>
        public IEnumerator ShowHintExternal(string text, float duration)
        {
            yield return ShowHint(text, duration);
        }

        /// <summary>Immediately hide the overlay.</summary>
        public void HideImmediate()
        {
            StopAllCoroutines();
            _canvasGroup.alpha = 0f;
        }

        // ─── Skip ───

        private void OnSkipClicked()
        {
            if (_manager != null)
                _manager.SkipTutorial();
            else
            {
                StopAllCoroutines();
                _canvasGroup.alpha = 0f;
                SaveManager.Data.tutorialCompleted = true;
                SaveManager.Save();
            }

            if (_skipButton != null) _skipButton.gameObject.SetActive(false);
        }

        private void OnPlayerDied()
        {
            StopAllCoroutines();
            _canvasGroup.alpha = 0f;
        }

        // ─── Legacy Sequence (fallback if no TutorialManager) ───

        private IEnumerator LegacyTutorialSequence()
        {
            bool gamepad = UnityEngine.InputSystem.Gamepad.current != null;

            yield return new WaitForSecondsRealtime(1f);
            yield return ShowHint(gamepad ? "Sol Stick ile hareket et" : "WASD ile hareket et");

            yield return new WaitForSecondsRealtime(1f);
            yield return ShowHint("Otomatik saldırıyorsun — düşmanlara yaklaş");

            yield return new WaitForSecondsRealtime(2f);
            yield return ShowHint(gamepad ? "Güney Tuşu ile atıl — saldırılardan kaç" : "SPACE ile atıl — saldırılardan kaç");

            yield return new WaitForSecondsRealtime(2f);
            yield return ShowHint("Kristalleri topla, seviye atla, mutasyon kazan");

            SaveManager.Data.tutorialCompleted = true;
            SaveManager.Save();
            if (_skipButton != null) _skipButton.gameObject.SetActive(false);
        }

        // ─── Display Logic ───

        private IEnumerator ShowHint(string text, float duration = 0f)
        {
            if (duration <= 0f) duration = _defaultDisplayDuration;
            if (_hintText != null) _hintText.text = text;

            // Fade in
            float t = 0f;
            while (t < _fadeDuration)
            {
                t += Time.unscaledDeltaTime;
                _canvasGroup.alpha = Mathf.Clamp01(t / _fadeDuration);
                yield return null;
            }
            _canvasGroup.alpha = 1f;

            yield return new WaitForSecondsRealtime(duration);

            // Fade out
            t = 0f;
            while (t < _fadeDuration)
            {
                t += Time.unscaledDeltaTime;
                _canvasGroup.alpha = 1f - Mathf.Clamp01(t / _fadeDuration);
                yield return null;
            }
            _canvasGroup.alpha = 0f;
        }
    }
}
