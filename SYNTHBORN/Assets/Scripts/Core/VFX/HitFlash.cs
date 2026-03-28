using System.Collections;
using UnityEngine;

namespace Synthborn.Core.VFX
{
    /// <summary>
    /// Flashes a SpriteRenderer white on damage. Attach to any entity with a sprite.
    /// Call Flash() from EntityHealth or via event.
    /// </summary>
    public class HitFlash : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer _renderer;
        [SerializeField] private float _flashDuration = 0.05f;

        private Color _originalColor;
        private Coroutine _flashRoutine;
        private static readonly Color FlashColor = Color.white;

        private void Awake()
        {
            if (_renderer == null)
                _renderer = GetComponent<SpriteRenderer>();
            if (_renderer != null)
                _originalColor = _renderer.color;
        }

        public void Flash()
        {
            if (_renderer == null) return;
            if (_flashRoutine != null)
                StopCoroutine(_flashRoutine);
            _flashRoutine = StartCoroutine(FlashRoutine());
        }

        private IEnumerator FlashRoutine()
        {
            _renderer.color = FlashColor;
            yield return new WaitForSeconds(_flashDuration);
            _renderer.color = _originalColor;
            _flashRoutine = null;
        }
    }
}
