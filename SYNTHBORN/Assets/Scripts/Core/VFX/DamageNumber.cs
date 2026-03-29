using UnityEngine;
using Synthborn.Core.Pool;

namespace Synthborn.Core.VFX
{
    /// <summary>
    /// Floating damage number that drifts up and fades out.
    /// Pooled by DamageNumberSpawner — returns itself to pool when lifetime expires.
    /// </summary>
    public class DamageNumber : MonoBehaviour, IPoolable
    {
        private TextMesh _text;
        private float _lifetime;
        private float _timer;
        private Color _baseColor;
        private Vector3 _velocity;
        private ObjectPool<DamageNumber> _pool;

        private void Awake()
        {
            _text = GetComponent<TextMesh>();
            if (_text == null)
            {
                _text = gameObject.AddComponent<TextMesh>();
                _text.alignment = TextAlignment.Center;
                _text.anchor = TextAnchor.MiddleCenter;
                _text.characterSize = 0.15f;
                _text.fontSize = 48;
            }
        }

        /// <summary>Set pool reference for self-return.</summary>
        public void SetPool(ObjectPool<DamageNumber> pool) => _pool = pool;

        public void Init(int damage, bool isCrit, Vector2 position)
        {
            transform.position = (Vector3)position + Vector3.back;

            _text.text = damage.ToString();
            _text.fontSize = isCrit ? 64 : 48;
            _text.characterSize = isCrit ? 0.2f : 0.15f;
            _baseColor = isCrit ? new Color(1f, 0.9f, 0.2f) : Color.white;
            _text.color = _baseColor;

            _lifetime = 0.8f;
            _timer = 0f;
            _velocity = new Vector3(Random.Range(-0.5f, 0.5f), 1.5f, 0f);
        }

        private void Update()
        {
            _timer += Time.deltaTime;
            if (_timer >= _lifetime)
            {
                ReturnToPool();
                return;
            }

            transform.position += _velocity * Time.deltaTime;
            _velocity.y *= 0.95f;

            float alpha = 1f - (_timer / _lifetime);
            _text.color = new Color(_baseColor.r, _baseColor.g, _baseColor.b, alpha);
        }

        private void ReturnToPool()
        {
            if (_pool != null)
                _pool.Return(this);
            else
                gameObject.SetActive(false);
        }

        public void OnPoolGet()
        {
            _timer = 0f;
            gameObject.SetActive(true);
        }

        public void OnPoolReturn()
        {
            gameObject.SetActive(false);
        }
    }
}
