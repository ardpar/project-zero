// PROTOTYPE - NOT FOR PRODUCTION
// Question: Is the core loop fun?
// Date: 2026-03-27

using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    private int _maxHP;
    private int _currentHP;
    private int _xpValue;

    public void Init(int maxHP, int xpValue)
    {
        _maxHP = maxHP;
        _currentHP = maxHP;
        _xpValue = xpValue;
    }

    public void TakeDamage(int damage)
    {
        if (_currentHP <= 0) return;

        _currentHP -= Mathf.Max(damage, 1);
        if (_currentHP <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        GameEvents.EnemyDied(transform.position, _xpValue);
        ObjectPool.Instance.Return(gameObject);
    }
}
