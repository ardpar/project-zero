// Implements: ADR-002 — AttackSlotData ScriptableObject (Tier 1)
// Design doc: auto-attack-system.md — Core Rules 17-20
//
// One asset per attack slot "template". The default slot references the basic
// projectile. Mutations add new slots by registering new AttackSlotData assets.

using UnityEngine;
namespace Synthborn.Combat
{
    /// <summary>
    /// Defines the static parameters for one attack slot.
    /// Runtime cooldown state lives in <see cref="AttackSlotState"/>.
    /// Create via Assets > Create > Synthborn/Combat > Attack Slot Data.
    /// </summary>
    [CreateAssetMenu(menuName = "Synthborn/Combat/Attack Slot Data", fileName = "AttackSlotData")]
    public class AttackSlotData : ScriptableObject
    {
        [Tooltip("Human-readable identifier for debugging / UI display.")]
        public string slotId = "DefaultProjectile";
        [Tooltip("Base time between firings in seconds. GDD default: 1.0. Safe range: 0.5-2.0.")]
        [Range(0.1f, 5f)]
        public float baseInterval = 1.0f;
        [Tooltip("Projectile data asset fired by this slot.")]
        public ProjectileData projectileData;
    }
}
