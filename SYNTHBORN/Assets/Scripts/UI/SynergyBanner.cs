using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Synthborn.Core.Events;

namespace Synthborn.UI
{
    /// <summary>
    /// Shows a "SYNERGY!" banner when a synergy activates.
    /// Fades in, holds, fades out.
    /// </summary>
    public class SynergyBanner : MonoBehaviour
    {
        [SerializeField] private GameObject _bannerPanel;
        [SerializeField] private Text _bannerText;
        [SerializeField] private float _displayDuration = 2f;
        [SerializeField] private float _fadeDuration = 0.3f;

        private CanvasGroup _canvasGroup;
        private Coroutine _activeRoutine;

        private void Awake()
        {
            if (_bannerPanel != null)
            {
                _canvasGroup = _bannerPanel.GetComponent<CanvasGroup>();
                if (_canvasGroup == null)
                    _canvasGroup = _bannerPanel.AddComponent<CanvasGroup>();
                _bannerPanel.SetActive(false);
            }
        }

        private void OnEnable()
        {
            GameEvents.OnSynergyActivated += ShowBanner;
        }

        private void OnDisable()
        {
            GameEvents.OnSynergyActivated -= ShowBanner;
        }

        private void ShowBanner(string synergyId, string displayName)
        {
            if (_bannerPanel == null || _bannerText == null) return;

            if (_activeRoutine != null)
                StopCoroutine(_activeRoutine);

            _activeRoutine = StartCoroutine(BannerRoutine(displayName));
        }

        private IEnumerator BannerRoutine(string displayName)
        {
            _bannerText.text = $"SYNERGY!\n{displayName}";
            _bannerPanel.SetActive(true);

            // Fade in (uses unscaled time since game might be paused)
            float t = 0f;
            while (t < _fadeDuration)
            {
                t += Time.unscaledDeltaTime;
                _canvasGroup.alpha = t / _fadeDuration;
                yield return null;
            }
            _canvasGroup.alpha = 1f;

            // Hold
            yield return new WaitForSecondsRealtime(_displayDuration);

            // Fade out
            t = 0f;
            while (t < _fadeDuration)
            {
                t += Time.unscaledDeltaTime;
                _canvasGroup.alpha = 1f - (t / _fadeDuration);
                yield return null;
            }

            _bannerPanel.SetActive(false);
            _activeRoutine = null;
        }
    }
}
