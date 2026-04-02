using System.Collections;
using UnityEngine;

namespace Synthborn.UI
{
    /// <summary>
    /// Reusable popup animation: scale + alpha fade-in when enabled.
    /// Attach to any popup panel. Works with CanvasGroup for alpha.
    /// </summary>
    [RequireComponent(typeof(CanvasGroup))]
    public class PopupAnimator : MonoBehaviour
    {
        [SerializeField] private float _animDuration = 0.2f;
        [SerializeField] private float _startScale = 0.8f;
        [SerializeField] private AnimationCurve _curve = AnimationCurve.EaseInOut(0, 0, 1, 1);

        private CanvasGroup _canvasGroup;
        private Vector3 _targetScale;

        private void Awake()
        {
            _canvasGroup = GetComponent<CanvasGroup>();
            _targetScale = transform.localScale;
        }

        private void OnEnable()
        {
            StartCoroutine(AnimateIn());
        }

        private IEnumerator AnimateIn()
        {
            transform.localScale = _targetScale * _startScale;
            _canvasGroup.alpha = 0f;

            float elapsed = 0f;
            while (elapsed < _animDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = _curve.Evaluate(Mathf.Clamp01(elapsed / _animDuration));

                transform.localScale = Vector3.Lerp(_targetScale * _startScale, _targetScale, t);
                _canvasGroup.alpha = t;

                yield return null;
            }

            transform.localScale = _targetScale;
            _canvasGroup.alpha = 1f;
        }

        /// <summary>Animate out and then deactivate.</summary>
        public void AnimateOutAndHide()
        {
            StartCoroutine(AnimateOut());
        }

        private IEnumerator AnimateOut()
        {
            float elapsed = 0f;
            Vector3 startScale = transform.localScale;

            while (elapsed < _animDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / _animDuration);

                transform.localScale = Vector3.Lerp(startScale, _targetScale * _startScale, t);
                _canvasGroup.alpha = 1f - t;

                yield return null;
            }

            gameObject.SetActive(false);
        }
    }
}
