using UnityEngine;
using Synthborn.Core.Events;

namespace Synthborn.Core.VFX
{
    /// <summary>
    /// Spawns a burst of small sprites when an enemy dies.
    /// Simple sprite-based particles (no ParticleSystem dependency).
    /// </summary>
    public class DeathParticleSpawner : MonoBehaviour
    {
        [SerializeField] private int _particlesPerDeath = 6;
        [SerializeField] private float _particleSpeed = 3f;
        [SerializeField] private float _particleLifetime = 0.4f;

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
            // Get enemy color for particles
            var sr = enemy != null ? enemy.GetComponent<SpriteRenderer>() : null;
            Color color = sr != null ? sr.color : Color.red;

            for (int i = 0; i < _particlesPerDeath; i++)
            {
                var go = new GameObject("DeathParticle");
                go.transform.position = (Vector3)position;
                go.transform.localScale = Vector3.one * Random.Range(0.05f, 0.12f);

                var psr = go.AddComponent<SpriteRenderer>();
                psr.sprite = sr != null ? sr.sprite : null;
                psr.color = color;
                psr.sortingOrder = 5;

                var dp = go.AddComponent<DeathParticle>();
                float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
                Vector2 dir = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
                dp.Init(dir * _particleSpeed * Random.Range(0.5f, 1f), _particleLifetime, color);
            }
        }
    }

    public class DeathParticle : MonoBehaviour
    {
        private Vector2 _velocity;
        private float _lifetime;
        private float _timer;
        private SpriteRenderer _sr;
        private Color _baseColor;

        public void Init(Vector2 velocity, float lifetime, Color color)
        {
            _velocity = velocity;
            _lifetime = lifetime;
            _baseColor = color;
            _sr = GetComponent<SpriteRenderer>();
        }

        private void Update()
        {
            _timer += Time.deltaTime;
            if (_timer >= _lifetime)
            {
                Destroy(gameObject);
                return;
            }

            transform.Translate(_velocity * Time.deltaTime);
            _velocity *= 0.92f; // decelerate

            if (_sr != null)
            {
                float alpha = 1f - (_timer / _lifetime);
                _sr.color = new Color(_baseColor.r, _baseColor.g, _baseColor.b, alpha);
            }
        }
    }
}
