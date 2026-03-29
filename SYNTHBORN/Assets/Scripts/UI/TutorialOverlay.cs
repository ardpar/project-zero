using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Synthborn.Core.Events;
using Synthborn.Core.Persistence;

namespace Synthborn.UI
{
    /// <summary>
    /// Shows tutorial hints on the first run. Non-blocking overlay that
    /// displays control prompts sequentially and fades them out.
    /// Persists completion via SaveManager.
    /// </summary>
    public class TutorialOverlay : MonoBehaviour
    {
        [SerializeField] private Text _hintText;
        [SerializeField] private float _displayDuration = 3.5f;
        [SerializeField] private float _fadeDuration = 0.5f;

        private CanvasGroup _canvasGroup;
        private bool _tutorialActive;

        private void Awake()
        {
            _canvasGroup = GetComponent<CanvasGroup>();
            if (_canvasGroup == null) _canvasGroup = gameObject.AddComponent<CanvasGroup>();
            _canvasGroup.alpha = 0f;
        }

        private void Start()
        {
            if (SaveManager.Data.tutorialCompleted) return;

            _tutorialActive = true;
            StartCoroutine(TutorialSequence());
        }

        private void OnEnable()
        {
            GameEvents.OnPlayerDied += OnPlayerDied;
        }

        private void OnDisable()
        {
            GameEvents.OnPlayerDied -= OnPlayerDied;
        }

        private void OnPlayerDied()
        {
            if (!_tutorialActive) return;
            StopAllCoroutines();
            _canvasGroup.alpha = 0f;
            CompleteTutorial();
        }

        private IEnumerator TutorialSequence()
        {
            bool gamepad = UnityEngine.InputSystem.Gamepad.current != null;

            yield return new WaitForSecondsRealtime(1f);
            yield return ShowHint(gamepad ? "Left Stick to move" : "WASD to move");

            yield return new WaitForSecondsRealtime(1f);
            yield return ShowHint("You attack automatically — get close to enemies");

            yield return new WaitForSecondsRealtime(2f);
            yield return ShowHint(gamepad ? "South Button to dash — dodge attacks" : "SPACE to dash — dodge attacks");

            yield return new WaitForSecondsRealtime(2f);
            yield return ShowHint("Collect gems to level up and gain mutations");

            CompleteTutorial();
        }

        private void CompleteTutorial()
        {
            _tutorialActive = false;
            SaveManager.Data.tutorialCompleted = true;
            SaveManager.Save();
        }

        private IEnumerator ShowHint(string text)
        {
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

            yield return new WaitForSecondsRealtime(_displayDuration);

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
