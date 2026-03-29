using System;
using UnityEngine;

namespace Synthborn.Core.Skills
{
    /// <summary>Kalibrasyon Ağacı branch types (Arena terminology).</summary>
    public enum SkillBranch
    {
        SubstrateDensity = 0,       // was Might — Damage + Crit
        StructuralIntegrity = 1,    // was Vitality — HP + Armor
        SignalConductivity = 2,     // was Agility — Speed + Dash
        DataYield = 3               // was Fortune — XP + Loot
    }

    /// <summary>
    /// Single node in the passive skill tree.
    /// </summary>
    [Serializable]
    public class SkillNodeData
    {
        public string id;
        public string displayName;
        [TextArea] public string description;
        public SkillBranch branch;
        public int tier; // 0-9, determines position in branch

        [Header("Cost")]
        public int skillPointCost = 1;
        public string prerequisiteNodeId; // empty = no prerequisite

        [Header("Stat Bonuses")]
        public float hpModifier;
        public float damageModifier;
        public float speedModifier;
        public float critChance;
        public float critDamageBonus;
        public float attackSpeedModifier;
        public int armorFlat;
        public float dashCooldownModifier;
        public float xpGainBonus;
        public float fragmentGainBonus;
        public float dropRateBonus;
    }
}
