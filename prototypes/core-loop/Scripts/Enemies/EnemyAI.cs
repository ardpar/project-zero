// PROTOTYPE - NOT FOR PRODUCTION
// Question: Is the core loop fun?
// Date: 2026-03-27

using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    [SerializeField] private float _baseMoveSpeed = 2.5f;
    [SerializeField] private int _contactDamage = 10;
    [SerializeField] private float _contactInterval = 0.5f;

    private Transform _player;
    private float _contactTimer;
    private float _speedMultiplier = 1f;

    public void Init(float speedMultiplier)
    {
        _speedMultiplier = speedMultiplier;
        _contactTimer = 0f;
    }

    private void OnEnable()
    {
        var player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
            _player = player.transform;
    }

    private void Update()
    {
        if (_player == null || Time.timeScale == 0f) return;

        // Simple chase — move toward player
        Vector2 direction = ((Vector2)_player.position - (Vector2)transform.position).normalized;
        transform.Translate(direction * _baseMoveSpeed * _speedMultiplier * Time.deltaTime);

        _contactTimer -= Time.deltaTime;
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (_contactTimer > 0f) return;

        if (other.CompareTag("Player"))
        {
            var health = other.GetComponent<PlayerHealth>();
            if (health != null)
            {
                health.TakeDamage(_contactDamage);
                _contactTimer = _contactInterval;
            }
        }
    }
}
