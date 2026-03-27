// PROTOTYPE - NOT FOR PRODUCTION
// Question: Is the core loop fun?
// Date: 2026-03-27

using UnityEngine;

public class AutoAttack : MonoBehaviour
{
    [SerializeField] private float _baseInterval = 1.0f;
    [SerializeField] private float _attackRange = 8f;
    [SerializeField] private float _coneAngle = 60f;
    [SerializeField] private int _baseDamage = 10;
    [SerializeField] private float _projectileSpeed = 10f;

    private float _cooldown;
    private float _attackSpeedModifier;
    private float _damageModifier;
    private PlayerController _player;

    private float EffectiveInterval => Mathf.Max(_baseInterval * (1f - _attackSpeedModifier), 0.1f);
    public int EffectiveDamage => Mathf.Max(Mathf.RoundToInt(_baseDamage * (1f + _damageModifier)), 1);

    private void Awake()
    {
        _player = GetComponent<PlayerController>();
    }

    private void OnEnable()
    {
        GameEvents.OnMutationSelected += HandleMutation;
    }

    private void OnDisable()
    {
        GameEvents.OnMutationSelected -= HandleMutation;
    }

    private void Update()
    {
        if (Time.timeScale == 0f) return;

        _cooldown -= Time.deltaTime;
        if (_cooldown <= 0f)
        {
            TryFire();
        }
    }

    private void TryFire()
    {
        var target = FindTarget();
        if (target == null) return;

        Vector2 direction = ((Vector2)target.position - _player.Position).normalized;
        FireProjectile(direction);
        _cooldown = EffectiveInterval;
    }

    private Transform FindTarget()
    {
        Collider2D[] enemies = Physics2D.OverlapCircleAll(_player.Position, _attackRange, LayerMask.GetMask("Enemy"));
        if (enemies.Length == 0) return null;

        Transform best = null;
        float bestScore = float.MaxValue;
        Vector2 moveDir = _player.MoveDirection;

        foreach (var enemy in enemies)
        {
            Vector2 toEnemy = ((Vector2)enemy.transform.position - _player.Position);
            float dist = toEnemy.magnitude;
            float dot = Vector2.Dot(moveDir, toEnemy.normalized);
            bool inCone = dot > Mathf.Cos(_coneAngle * Mathf.Deg2Rad);

            float score = dist / (inCone ? 2f : 1f); // Cone bonus: halves effective distance
            if (score < bestScore)
            {
                bestScore = score;
                best = enemy.transform;
            }
        }

        return best;
    }

    private void FireProjectile(Vector2 direction)
    {
        var go = ObjectPool.Instance.GetProjectile(_player.Position);
        var proj = go.GetComponent<Projectile>();
        proj.Init(direction, _projectileSpeed, EffectiveDamage);
    }

    private void HandleMutation(MutationData data)
    {
        _attackSpeedModifier += data.AttackSpeedModifier;
        _damageModifier += data.DamageModifier;
    }
}
