// PROTOTYPE - NOT FOR PRODUCTION
// Question: Is the core loop fun?
// Date: 2026-03-27

using UnityEngine;

public class Projectile : MonoBehaviour
{
    private Vector2 _direction;
    private float _speed;
    private int _damage;
    private float _lifetime;
    private const float MAX_LIFETIME = 3f;

    public void Init(Vector2 direction, float speed, int damage)
    {
        _direction = direction;
        _speed = speed;
        _damage = damage;
        _lifetime = MAX_LIFETIME;
    }

    private void Update()
    {
        transform.Translate(_direction * _speed * Time.deltaTime);
        _lifetime -= Time.deltaTime;
        if (_lifetime <= 0f)
        {
            ObjectPool.Instance.Return(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Enemy"))
        {
            var health = other.GetComponent<EnemyHealth>();
            if (health != null)
                health.TakeDamage(_damage);
            ObjectPool.Instance.Return(gameObject);
        }
    }
}
