using UnityEngine;
using Synthborn.Core.Events;

namespace Synthborn.Player
{
    /// <summary>
    /// Spawns fading afterimage sprites during dash.
    /// Listens to dash events and creates ghost sprites at intervals.
    /// </summary>
    public class DashTrail : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer _sourceRenderer;
        [SerializeField] private float _spawnInterval = 0.03f;
        [SerializeField] private float _fadeDuration = 0.2f;
        [SerializeField] private Color _trailColor = new(0.5f, 0.8f, 1f, 0.5f);

        private bool _isDashing;
        private float _spawnTimer;

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
            var ghost = new GameObject("DashGhost");
            ghost.transform.position = transform.position;
            ghost.transform.localScale = transform.localScale;

            var sr = ghost.AddComponent<SpriteRenderer>();
            sr.sprite = _sourceRenderer.sprite;
            sr.color = _trailColor;
            sr.sortingOrder = _sourceRenderer.sortingOrder - 1;

            ghost.AddComponent<FadeAndDestroy>().duration = _fadeDuration;
        }
    }

    /// <summary>Fades a SpriteRenderer alpha to 0 then destroys the GameObject.</summary>
    public class FadeAndDestroy : MonoBehaviour
    {
        public float duration = 0.2f;
        private SpriteRenderer _sr;
        private float _timer;

        private void Awake() => _sr = GetComponent<SpriteRenderer>();

        private void Update()
        {
            _timer += Time.deltaTime;
            if (_sr != null)
            {
                var c = _sr.color;
                c.a = Mathf.Lerp(c.a, 0f, _timer / duration);
                _sr.color = c;
            }
            if (_timer >= duration)
                Destroy(gameObject);
        }
    }
}
