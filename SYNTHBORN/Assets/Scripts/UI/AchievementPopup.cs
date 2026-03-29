using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Synthborn.Core.Persistence;

namespace Synthborn.UI
{
    /// <summary>
    /// Shows a brief achievement unlock notification during gameplay.
    /// Subscribes to AchievementManager.OnAchievementUnlocked.
    /// </summary>
    public class AchievementPopup : MonoBehaviour
    {
        [SerializeField] private Text _achievementText;
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private float _displayDuration = 2.5f;
        [SerializeField] private float _fadeDuration = 0.4f;

        private void Awake()
        {
            if (_canvasGroup == null) _canvasGroup = GetComponent<CanvasGroup>();
            if (_canvasGroup == null) _canvasGroup = gameObject.AddComponent<CanvasGroup>();
            _canvasGroup.alpha = 0f;
            _canvasGroup.blocksRaycasts = false;
        }

        private void OnEnable()
        {
            AchievementManager.OnAchievementUnlocked += ShowPopup;
        }

        private void OnDisable()
        {
            AchievementManager.OnAchievementUnlocked -= ShowPopup;
        }

        private void ShowPopup(AchievementDef achievement)
        {
            if (_achievementText != null)
                _achievementText.text = $"Achievement Unlocked!\n{achievement.Name}";
            StartCoroutine(PopupSequence());
        }

        private IEnumerator PopupSequence()
        {
            float t = 0f;
            while (t < _fadeDuration)
            {
                t += Time.unscaledDeltaTime;
                _canvasGroup.alpha = Mathf.Clamp01(t / _fadeDuration);
                yield return null;
            }
            _canvasGroup.alpha = 1f;

            yield return new WaitForSecondsRealtime(_displayDuration);

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
