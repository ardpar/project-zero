// PROTOTYPE - NOT FOR PRODUCTION
// Question: Is the core loop fun?
// Date: 2026-03-27

using UnityEngine;

[System.Serializable]
public class MutationData
{
    public string Name;
    public string Description;
    public MutationRarity Rarity;

    // Stat modifiers (additive)
    public float DamageModifier;
    public float AttackSpeedModifier;
    public float SpeedModifier;
    public float DashCdModifier;
    public float HPModifier;
    public float XPModifier;

    public enum MutationRarity { Common, Uncommon, Rare, Legendary }
}
