using UnityEngine;
using Synthborn.Core.Events;
using Synthborn.Core.Pool;

namespace Synthborn.Core.VFX
{
    /// <summary>
    /// Spawns a burst of small sprites when an enemy dies.
    /// Pooled to avoid GC pressure during mass kills.
    /// </summary>
    public class DeathParticleSpawner : MonoBehaviour
    {
        [SerializeField] private int _particlesPerDeath = 6;
        [SerializeField] private float _particleSpeed = 3f;
        [SerializeField] private float _particleLifetime = 0.4f;
        [SerializeField] private int _poolPrewarm = 64;

        private ObjectPool<DeathParticle> _pool;

        private void Awake()
        {
            _pool = new ObjectPool<DeathParticle>(() =>
            {
                var go = new GameObject("DeathParticle");
                go.transform.SetParent(transform);
                var sr = go.AddComponent<SpriteRenderer>();
                sr.sortingOrder = 5;
                var dp = go.AddComponent<DeathParticle>();
                dp.SetPool(_pool);
                return dp;
            }, _poolPrewarm);
        }

        private void OnEnable()
        {
            GameEvents.OnEnemyDied += OnEnemyDied;
        }

        private void OnDisable()
        {
            GameEvents.OnEnemyDied -= OnEnemyDied;
        }

        private void OnEnemyDied(Vector2 position, GameObject enemy, int xp)
        {
            var sr = enemy != null ? enemy.GetComponent<SpriteRenderer>() : null;
            Color color = sr != null ? sr.color : Color.red;
            Sprite sprite = sr != null ? sr.sprite : null;

            for (int i = 0; i < _particlesPerDeath; i++)
            {
                var dp = _pool.Get();
                dp.transform.position = (Vector3)position;
                dp.transform.localScale = Vector3.one * Random.Range(0.05f, 0.12f);

                float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
                Vector2 dir = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
                dp.Init(dir * _particleSpeed * Random.Range(0.5f, 1f), _particleLifetime, color, sprite);
            }
        }
    }

    /// <summary>
    /// Single death particle — pooled, fades and returns itself.
    /// </summary>
    public class DeathParticle : MonoBehaviour, IPoolable
    {
        private Vector2 _velocity;
        private float _lifetime;
        private float _timer;
        private SpriteRenderer _sr;
        private Color _baseColor;
        private ObjectPool<DeathParticle> _pool;

        /// <summary>Set pool reference for self-return.</summary>
        public void SetPool(ObjectPool<DeathParticle> pool) => _pool = pool;

        private void Awake()
        {
            _sr = GetComponent<SpriteRenderer>();
        }

        public void Init(Vector2 velocity, float lifetime, Color color, Sprite sprite)
        {
            _velocity = velocity;
            _lifetime = lifetime;
            _baseColor = color;
            _timer = 0f;
            if (_sr != null)
            {
                _sr.sprite = sprite;
                _sr.color = color;
            }
        }

        private void Update()
        {
            _timer += Time.deltaTime;
            if (_timer >= _lifetime)
            {
                _pool?.Return(this);
                return;
            }

            transform.Translate(_velocity * Time.deltaTime);
            _velocity *= 0.92f;

            if (_sr != null)
            {
                float alpha = 1f - (_timer / _lifetime);
                _sr.color = new Color(_baseColor.r, _baseColor.g, _baseColor.b, alpha);
            }
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
