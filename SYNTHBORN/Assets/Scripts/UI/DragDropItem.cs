using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace Synthborn.UI
{
    /// <summary>
    /// Makes a UI element draggable. Snaps back if not dropped on a valid target.
    /// Carries item data for the inventory system.
    /// </summary>
    public class DragDropItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        public string ItemId { get; set; }
        public bool IsEquipped { get; set; }
        public int SourceSlotIndex { get; set; } = -1;

        private RectTransform _rectTransform;
        private CanvasGroup _canvasGroup;
        private Canvas _canvas;
        private Vector2 _startPosition;
        private Transform _startParent;

        private void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
            _canvasGroup = GetComponent<CanvasGroup>();
            if (_canvasGroup == null) _canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            _startPosition = _rectTransform.anchoredPosition;
            _startParent = transform.parent;

            // Find root canvas for proper dragging
            _canvas = GetComponentInParent<Canvas>();

            _canvasGroup.alpha = 0.7f;
            _canvasGroup.blocksRaycasts = false;

            // Reparent to canvas root so it renders on top
            transform.SetParent(_canvas.transform, true);
        }

        public void OnDrag(PointerEventData eventData)
        {
            _rectTransform.anchoredPosition += eventData.delta / _canvas.scaleFactor;
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            _canvasGroup.alpha = 1f;
            _canvasGroup.blocksRaycasts = true;

            // If not dropped on valid target, snap back
            transform.SetParent(_startParent, true);
            _rectTransform.anchoredPosition = _startPosition;
        }

        /// <summary>Reset to start position (called by drop target on success).</summary>
        public void SnapBack()
        {
            transform.SetParent(_startParent, true);
            _rectTransform.anchoredPosition = _startPosition;
        }
    }
}
