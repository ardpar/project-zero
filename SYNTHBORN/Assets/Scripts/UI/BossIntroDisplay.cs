using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Synthborn.Core.Events;

namespace Synthborn.UI
{
    /// <summary>
    /// Shows a dramatic boss name banner when the boss spawns.
    /// Fades in, holds, fades out over ~2 seconds.
    /// </summary>
    public class BossIntroDisplay : MonoBehaviour
    {
        [SerializeField] private Text _bossNameText;
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private string _bossName = "CAVERN GUARDIAN";
        [SerializeField] private float _fadeInDuration = 0.3f;
        [SerializeField] private float _holdDuration = 1.5f;
        [SerializeField] private float _fadeOutDuration = 0.5f;

        private void Awake()
        {
            if (_canvasGroup == null) _canvasGroup = GetComponent<CanvasGroup>();
            if (_canvasGroup == null) _canvasGroup = gameObject.AddComponent<CanvasGroup>();
            _canvasGroup.alpha = 0f;
        }

        private void OnEnable()
        {
            GameEvents.OnBossSpawned += ShowBossIntro;
        }

        private void OnDisable()
        {
            GameEvents.OnBossSpawned -= ShowBossIntro;
        }

        private void ShowBossIntro()
        {
            if (_bossNameText != null) _bossNameText.text = _bossName;
            StartCoroutine(IntroSequence());
        }

        private IEnumerator IntroSequence()
        {
            // Fade in
            float t = 0f;
            while (t < _fadeInDuration)
            {
                t += Time.unscaledDeltaTime;
                _canvasGroup.alpha = Mathf.Clamp01(t / _fadeInDuration);
                yield return null;
            }
            _canvasGroup.alpha = 1f;

            // Hold
            yield return new WaitForSecondsRealtime(_holdDuration);

            // Fade out
            t = 0f;
            while (t < _fadeOutDuration)
            {
                t += Time.unscaledDeltaTime;
                _canvasGroup.alpha = 1f - Mathf.Clamp01(t / _fadeOutDuration);
                yield return null;
            }
            _canvasGroup.alpha = 0f;
        }
    }
}
