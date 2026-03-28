using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Synthborn.Core.Events;

namespace Synthborn.UI
{
    /// <summary>
    /// Shows "WAVE X" and "WAVE CLEAR!" banners with fade animation.
    /// </summary>
    public class WaveBanner : MonoBehaviour
    {
        [SerializeField] private Text _bannerText;
        [SerializeField] private float _displayDuration = 1.5f;
        [SerializeField] private float _fadeDuration = 0.4f;

        private CanvasGroup _canvasGroup;

        private void Awake()
        {
            _canvasGroup = GetComponent<CanvasGroup>();
            if (_canvasGroup == null) _canvasGroup = gameObject.AddComponent<CanvasGroup>();
            _canvasGroup.alpha = 0f;
        }

        private void OnEnable()
        {
            GameEvents.OnWaveStarted += OnWaveStart;
            GameEvents.OnWaveCleared += OnWaveCleared;
        }

        private void OnDisable()
        {
            GameEvents.OnWaveStarted -= OnWaveStart;
            GameEvents.OnWaveCleared -= OnWaveCleared;
        }

        private void OnWaveStart(int wave) => StartCoroutine(ShowBanner($"WAVE {wave}"));
        private void OnWaveCleared() => StartCoroutine(ShowBanner("WAVE CLEAR!"));

        private IEnumerator ShowBanner(string text)
        {
            if (_bannerText != null) _bannerText.text = text;

            // Scale-in + fade-in
            float t = 0;
            transform.localScale = Vector3.one * 2f;
            while (t < _fadeDuration)
            {
                t += Time.unscaledDeltaTime;
                float p = t / _fadeDuration;
                _canvasGroup.alpha = p;
                transform.localScale = Vector3.Lerp(Vector3.one * 2f, Vector3.one, p);
                yield return null;
            }
            _canvasGroup.alpha = 1f;
            transform.localScale = Vector3.one;

            yield return new WaitForSecondsRealtime(_displayDuration);

            // Fade-out
            t = 0;
            while (t < _fadeDuration)
            {
                t += Time.unscaledDeltaTime;
                _canvasGroup.alpha = 1f - (t / _fadeDuration);
                yield return null;
            }
            _canvasGroup.alpha = 0f;
        }
    }
}
