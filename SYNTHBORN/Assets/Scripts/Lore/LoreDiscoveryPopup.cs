using System.Collections;
using UnityEngine;
using TMPro;
using Synthborn.Core.Events;

namespace Synthborn.Lore
{
    /// <summary>
    /// Shows a brief popup when a lore fragment is discovered.
    /// "SIGNAL INTERCEPTED: [title]" with fade animation.
    /// </summary>
    public class LoreDiscoveryPopup : MonoBehaviour
    {
        [SerializeField] private TMP_Text _popupText;
        [SerializeField] private float _displayDuration = 2f;
        [SerializeField] private float _fadeDuration = 0.5f;

        private CanvasGroup _canvasGroup;

        private void Awake()
        {
            _canvasGroup = GetComponent<CanvasGroup>();
            if (_canvasGroup == null) _canvasGroup = gameObject.AddComponent<CanvasGroup>();
            _canvasGroup.alpha = 0f;
        }

        private void OnEnable() => GameEvents.OnLoreFragmentDiscovered += OnDiscovered;
        private void OnDisable() => GameEvents.OnLoreFragmentDiscovered -= OnDiscovered;

        private void OnDiscovered(string fragmentId, string title)
        {
            StartCoroutine(ShowPopup(title));
        }

        private IEnumerator ShowPopup(string title)
        {
            if (_popupText != null)
                _popupText.text = $"SİNYAL TESPİT EDİLDİ: {title}";

            // Fade in
            float t = 0;
            while (t < _fadeDuration)
            {
                t += Time.unscaledDeltaTime;
                _canvasGroup.alpha = t / _fadeDuration;
                yield return null;
            }
            _canvasGroup.alpha = 1f;

            yield return new WaitForSecondsRealtime(_displayDuration);

            // Fade out
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
