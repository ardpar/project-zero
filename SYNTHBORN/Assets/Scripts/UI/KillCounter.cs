using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Synthborn.Core.Events;

namespace Synthborn.UI
{
    /// <summary>
    /// Shows kill milestone popups (10, 25, 50, 100, 200, 500).
    /// </summary>
    public class KillCounter : MonoBehaviour
    {
        [SerializeField] private Text _popupText;

        private int _killCount;
        private CanvasGroup _canvasGroup;
        private static readonly int[] Milestones = { 10, 25, 50, 100, 200, 500 };

        private void Awake()
        {
            _canvasGroup = GetComponent<CanvasGroup>();
            if (_canvasGroup == null) _canvasGroup = gameObject.AddComponent<CanvasGroup>();
            _canvasGroup.alpha = 0f;
        }

        private void OnEnable() => GameEvents.OnEnemyDied += OnKill;
        private void OnDisable() => GameEvents.OnEnemyDied -= OnKill;

        private void OnKill(Vector2 p, GameObject e, int xp)
        {
            _killCount++;
            foreach (int m in Milestones)
            {
                if (_killCount == m)
                {
                    StartCoroutine(ShowPopup($"{m} KILLS!"));
                    break;
                }
            }
        }

        private IEnumerator ShowPopup(string text)
        {
            if (_popupText != null) _popupText.text = text;
            _canvasGroup.alpha = 1f;
            transform.localScale = Vector3.one * 1.5f;

            float t = 0f;
            while (t < 0.2f)
            {
                t += Time.unscaledDeltaTime;
                transform.localScale = Vector3.Lerp(Vector3.one * 1.5f, Vector3.one, t / 0.2f);
                yield return null;
            }

            yield return new WaitForSecondsRealtime(1.5f);

            t = 0f;
            while (t < 0.3f)
            {
                t += Time.unscaledDeltaTime;
                _canvasGroup.alpha = 1f - (t / 0.3f);
                yield return null;
            }
            _canvasGroup.alpha = 0f;
        }
    }
}
