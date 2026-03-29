using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Synthborn.UI
{
    /// <summary>
    /// Scales a button up on hover/select for visual feedback.
    /// Works with both mouse and gamepad selection.
    /// </summary>
    public class ButtonHoverEffect : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, ISelectHandler, IDeselectHandler
    {
        [SerializeField] private float _hoverScale = 1.08f;
        [SerializeField] private float _animDuration = 0.12f;

        private Vector3 _originalScale;
        private Coroutine _scaleCoroutine;

        private void Awake()
        {
            _originalScale = transform.localScale;
        }

        public void OnPointerEnter(PointerEventData eventData) => AnimateTo(_hoverScale);
        public void OnPointerExit(PointerEventData eventData) => AnimateTo(1f);
        public void OnSelect(BaseEventData eventData) => AnimateTo(_hoverScale);
        public void OnDeselect(BaseEventData eventData) => AnimateTo(1f);

        private void OnDisable()
        {
            if (_scaleCoroutine != null) StopCoroutine(_scaleCoroutine);
            transform.localScale = _originalScale;
        }

        private void AnimateTo(float targetMultiplier)
        {
            if (_scaleCoroutine != null) StopCoroutine(_scaleCoroutine);
            _scaleCoroutine = StartCoroutine(ScaleTo(_originalScale * targetMultiplier));
        }

        private IEnumerator ScaleTo(Vector3 target)
        {
            Vector3 start = transform.localScale;
            float elapsed = 0f;
            while (elapsed < _animDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.SmoothStep(0f, 1f, elapsed / _animDuration);
                transform.localScale = Vector3.Lerp(start, target, t);
                yield return null;
            }
            transform.localScale = target;
            _scaleCoroutine = null;
        }
    }
}
