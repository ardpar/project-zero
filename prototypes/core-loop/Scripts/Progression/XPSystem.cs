// PROTOTYPE - NOT FOR PRODUCTION
// Question: Is the core loop fun?
// Date: 2026-03-27

using UnityEngine;

public class XPSystem : MonoBehaviour
{
    public static XPSystem Instance { get; private set; }

    [SerializeField] private float _pickupRadius = 1.0f;
    [SerializeField] private float _magnetSpeed = 15f;

    // Stepped XP curve (hardcoded for prototype)
    private readonly int[] _xpTable = { 20, 25, 30, 35, 40, 50, 60, 70, 80, 100, 120, 150, 180, 220, 270, 330, 400, 500, 600, 750 };

    private int _currentXP;
    private int _level = 1;
    private float _xpModifier;

    public int CurrentXP => _currentXP;
    public int XPToNext => GetXPForLevel(_level);
    public int Level => _level;
    public float XPRatio => (float)_currentXP / XPToNext;

    private void Awake()
    {
        Instance = this;
    }

    private void OnEnable()
    {
        GameEvents.OnEnemyDied += HandleEnemyDied;
        GameEvents.OnMutationSelected += HandleMutation;
    }

    private void OnDisable()
    {
        GameEvents.OnEnemyDied -= HandleEnemyDied;
        GameEvents.OnMutationSelected -= HandleMutation;
    }

    private void HandleEnemyDied(Vector2 pos, int xpValue)
    {
        // Spawn XP gem at enemy death position
        var go = ObjectPool.Instance.GetXPGem(pos);
        var gem = go.GetComponent<XPGem>();
        int effectiveXP = Mathf.RoundToInt(xpValue * (1f + _xpModifier));
        gem.Init(effectiveXP, _pickupRadius, _magnetSpeed);
    }

    public void AddXP(int amount)
    {
        _currentXP += amount;
        while (_currentXP >= XPToNext)
        {
            _currentXP -= XPToNext;
            _level++;
            Debug.Log($"LEVEL UP! Now level {_level}");
            GameEvents.LevelUp();
        }
    }

    private int GetXPForLevel(int level)
    {
        if (level - 1 < _xpTable.Length)
            return _xpTable[level - 1];
        // Fallback: growth rate 1.25
        return Mathf.RoundToInt(_xpTable[^1] * Mathf.Pow(1.25f, level - _xpTable.Length));
    }

    private void HandleMutation(MutationData data)
    {
        _xpModifier += data.XPModifier;
    }
}
