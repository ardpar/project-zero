// PROTOTYPE - NOT FOR PRODUCTION
// Question: Is the core loop fun?
// Date: 2026-03-27

using UnityEngine;

public class XPGem : MonoBehaviour
{
    private int _xpValue;
    private float _pickupRadius;
    private float _magnetSpeed;
    private float _lifetime;
    private Transform _player;
    private bool _magnetized;
    private const float MAX_LIFETIME = 30f;

    public void Init(int xpValue, float pickupRadius, float magnetSpeed)
    {
        _xpValue = xpValue;
        _pickupRadius = pickupRadius;
        _magnetSpeed = magnetSpeed;
        _lifetime = MAX_LIFETIME;
        _magnetized = false;

        var player = GameObject.FindGameObjectWithTag("Player");
        if (player != null) _player = player.transform;
    }

    private void Update()
    {
        if (_player == null || Time.timeScale == 0f) return;

        _lifetime -= Time.deltaTime;
        if (_lifetime <= 0f)
        {
            ObjectPool.Instance.Return(gameObject);
            return;
        }

        float dist = Vector2.Distance(transform.position, _player.position);

        if (dist < _pickupRadius || _magnetized)
        {
            _magnetized = true;
            // Move toward player
            Vector2 dir = ((Vector2)_player.position - (Vector2)transform.position).normalized;
            transform.Translate(dir * _magnetSpeed * Time.deltaTime);

            // Collect
            if (dist < 0.3f)
            {
                XPSystem.Instance.AddXP(_xpValue);
                ObjectPool.Instance.Return(gameObject);
            }
        }
    }
}
