// PROTOTYPE - NOT FOR PRODUCTION
// Question: Is the core loop fun?
// Date: 2026-03-27

using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [SerializeField] private int _baseMaxHP = 100;
    [SerializeField] private float _invulnerabilityWindow = 0.5f;

    private int _currentHP;
    private int _maxHP;
    private float _invulTimer;
    private float _hpModifier;

    public int CurrentHP => _currentHP;
    public int MaxHP => _maxHP;
    public float HPRatio => (float)_currentHP / _maxHP;
    public bool IsInvulnerable => _invulTimer > 0f;

    private void Start()
    {
        _maxHP = _baseMaxHP;
        _currentHP = _maxHP;
    }

    private void Update()
    {
        if (_invulTimer > 0f)
            _invulTimer -= Time.deltaTime;
    }

    private void OnEnable()
    {
        GameEvents.OnMutationSelected += HandleMutation;
    }

    private void OnDisable()
    {
        GameEvents.OnMutationSelected -= HandleMutation;
    }

    public void TakeDamage(int rawDamage)
    {
        if (_invulTimer > 0f || _currentHP <= 0) return;

        int damage = Mathf.Max(rawDamage, 1);
        _currentHP = Mathf.Max(_currentHP - damage, 0);
        _invulTimer = _invulnerabilityWindow;

        GameEvents.PlayerDamaged(damage);

        if (_currentHP <= 0)
        {
            GameEvents.PlayerDied();
            gameObject.SetActive(false);
        }
    }

    public void Heal(int amount)
    {
        _currentHP = Mathf.Min(_currentHP + amount, _maxHP);
    }

    private void HandleMutation(MutationData data)
    {
        _hpModifier += data.HPModifier;
        int newMax = Mathf.RoundToInt(_baseMaxHP * (1f + Mathf.Clamp(_hpModifier, -0.3f, 2f)));
        _maxHP = Mathf.Max(newMax, 35);
        _currentHP = Mathf.Min(_currentHP, _maxHP);
    }
}
