using UnityEngine;
using UnityEngine.UI;

namespace Synthborn.UI
{
    /// <summary>
    /// Animated pulsing glow effect for high-rarity items.
    /// Attach to the border Image of Anomalous or Architect-Grade items.
    /// </summary>
    public class RarityGlow : MonoBehaviour
    {
        private Image _image;
        private Color _baseColor;
        private float _phase;

        [SerializeField] private float _pulseSpeed = 2f;
        [SerializeField] private float _minAlpha = 0.15f;
        [SerializeField] private float _maxAlpha = 0.5f;

        private void Awake()
        {
            _image = GetComponent<Image>();
            if (_image != null)
                _baseColor = _image.color;
            _phase = Random.Range(0f, Mathf.PI * 2f); // Stagger phase
        }

        private void Update()
        {
            if (_image == null) return;
            _phase += Time.unscaledDeltaTime * _pulseSpeed;
            float alpha = Mathf.Lerp(_minAlpha, _maxAlpha, (Mathf.Sin(_phase) + 1f) * 0.5f);
            _image.color = new Color(_baseColor.r, _baseColor.g, _baseColor.b, alpha);
        }

        /// <summary>Initialize with a rarity color.</summary>
        public void Init(Color rarityColor, float speed = 2f)
        {
            _baseColor = rarityColor;
            _pulseSpeed = speed;
            if (_image == null) _image = GetComponent<Image>();
            if (_image != null) _image.color = rarityColor;
        }
    }
}
