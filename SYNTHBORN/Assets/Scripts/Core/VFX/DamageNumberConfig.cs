using UnityEngine;

namespace Synthborn.Core.VFX
{
    /// <summary>
    /// ScriptableObject for DamageNumber tuning parameters.
    /// Centralizes visual/physics values for floating damage text.
    /// </summary>
    [CreateAssetMenu(menuName = "Synthborn/VFX/Damage Number Config", fileName = "DamageNumberConfig")]
    public class DamageNumberConfig : ScriptableObject
    {
        [Header("Lifetime")]
        [Tooltip("How long the number floats before returning to pool. Default 0.8s.")]
        [Range(0.3f, 2f)]
        public float lifetime = 0.8f;

        [Header("Movement")]
        [Tooltip("Horizontal drift range. Number spawns with random X velocity in [-drift, drift].")]
        [Range(0.1f, 2f)]
        public float horizontalDrift = 0.5f;

        [Tooltip("Initial upward speed. Default 1.5.")]
        [Range(0.5f, 4f)]
        public float upwardSpeed = 1.5f;

        [Tooltip("Y velocity damping per frame. Default 0.95.")]
        [Range(0.8f, 0.99f)]
        public float yDamping = 0.95f;

        [Header("Text Size")]
        [Tooltip("Normal hit font size. Default 48.")]
        public int normalFontSize = 48;

        [Tooltip("Critical hit font size. Default 64.")]
        public int critFontSize = 64;

        [Tooltip("Normal hit character size. Default 0.15.")]
        public float normalCharSize = 0.15f;

        [Tooltip("Critical hit character size. Default 0.2.")]
        public float critCharSize = 0.2f;

        [Header("Colors")]
        public Color normalColor = Color.white;
        public Color critColor = new(1f, 0.9f, 0.2f);
    }
}
