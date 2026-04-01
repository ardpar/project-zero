using UnityEngine;
using Synthborn.Core.Events;
using Synthborn.Core.Pool;

namespace Synthborn.Player
{
    /// <summary>
    /// Spawns fading afterimage sprites during dash.
    /// Uses ObjectPool to avoid per-dash allocations.
    /// </summary>
    public class DashTrail : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer _sourceRenderer;
        [SerializeField] private float _spawnInterval = 0.03f;
        [SerializeField] private float _fadeDuration = 0.2f;
        [SerializeField] private Color _trailColor = new(0.5f, 0.8f, 1f, 0.5f);
        [SerializeField] private int _poolSize = 10;

        private bool _isDashing;
        private float _spawnTimer;
        private ObjectPool<DashGhost> _pool;

        private void Awake()
        {
            _pool = new ObjectPool<DashGhost>(() =>
            {
                var go = new GameObject("DashGhost");
                go.transform.SetParent(transform.parent);
                var sr = go.AddComponent<SpriteRenderer>();
                sr.sortingOrder = -1;
                var ghost = go.AddComponent<DashGhost>();
                ghost.SetPool(_pool);
                return ghost;
            }, _poolSize);
        }

        private void OnEnable()
        {
            GameEvents.OnPlayerDashStarted += OnDashStart;
            GameEvents.OnPlayerDashEnded += OnDashEnd;
        }

        private void OnDisable()
        {
            GameEvents.OnPlayerDashStarted -= OnDashStart;
            GameEvents.OnPlayerDashEnded -= OnDashEnd;
        }

        private void OnDashStart(Vector2 dir)
        {
            _isDashing = true;
            _spawnTimer = 0f;
        }

        private void OnDashEnd()
        {
            _isDashing = false;
        }

        private void Update()
        {
            if (!_isDashing || _sourceRenderer == null) return;

            _spawnTimer -= Time.deltaTime;
            if (_spawnTimer <= 0f)
            {
                _spawnTimer = _spawnInterval;
                SpawnGhost();
            }
        }

        private void SpawnGhost()
        {
            var ghost = _pool.Get();
            ghost.Init(
                transform.position,
                transform.localScale,
                _sourceRenderer.sprite,
                _trailColor,
                _sourceRenderer.sortingOrder - 1,
                _fadeDuration);
        }
    }

    /// <summary>
    /// Pooled dash ghost — fades SpriteRenderer alpha to 0 then returns to pool.
    /// </summary>
    public class DashGhost : MonoBehaviour, IPoolable
    {
        private SpriteRenderer _sr;
        private ObjectPool<DashGhost> _pool;
        private float _duration;
        private float _timer;
        private Color _startColor;

        private void Awake() => _sr = GetComponent<SpriteRenderer>();

        /// <summary>Set pool reference for self-return.</summary>
        public void SetPool(ObjectPool<DashGhost> pool) => _pool = pool;

        public void Init(Vector3 position, Vector3 scale, Sprite sprite, Color color, int sortOrder, float duration)
        {
            transform.position = position;
            transform.localScale = scale;
            _sr.sprite = sprite;
            _sr.color = color;
            _sr.sortingOrder = sortOrder;
            _startColor = color;
            _duration = duration;
            _timer = 0f;
        }

        private void Update()
        {
            _timer += Time.deltaTime;
            if (_sr != null)
            {
                float alpha = Mathf.Lerp(_startColor.a, 0f, _timer / _duration);
                _sr.color = new Color(_startColor.r, _startColor.g, _startColor.b, alpha);
            }
            if (_timer >= _duration)
                _pool?.Return(this);
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
