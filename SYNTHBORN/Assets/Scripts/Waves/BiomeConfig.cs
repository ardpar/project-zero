using UnityEngine;

namespace Synthborn.Waves
{
    /// <summary>
    /// Visual configuration for a biome layer. Used by Arena Map for color-coding
    /// and by the gameplay scene for background tint.
    /// </summary>
    [CreateAssetMenu(menuName = "Synthborn/Waves/BiomeConfig")]
    public class BiomeConfig : ScriptableObject
    {
        [Tooltip("Which biome layer this config applies to.")]
        public BiomeLayer biomeLayer;

        [Tooltip("Display name for UI.")]
        public string displayName = "The Atrium";

        [Tooltip("Short lore description for map tooltip.")]
        [TextArea(2, 4)]
        public string loreDescription = "Era 1 - Active Operation";

        [Tooltip("Background tint for gameplay camera.")]
        public Color backgroundTint = new Color(0.08f, 0.07f, 0.12f);

        [Tooltip("UI accent color for Arena Map room buttons.")]
        public Color mapAccentColor = new Color(0.3f, 0.25f, 0.4f);

        [Tooltip("Completed room color on Arena Map.")]
        public Color mapCompletedColor = new Color(0.15f, 0.35f, 0.15f);

        [Tooltip("Locked room color on Arena Map.")]
        public Color mapLockedColor = new Color(0.1f, 0.1f, 0.1f);

        [Header("Audio")]
        [Tooltip("Calm/ambient music stem for this biome. Plays during wave breaks and calibration intervals.")]
        public AudioClip biomeCalmStem;

        [Tooltip("Combat music stem for this biome. Plays during waves.")]
        public AudioClip biomeCombatStem;

        [Tooltip("Boss music stem for this biome. Falls back to biomeCombatStem if null.")]
        public AudioClip biomeBossStem;
    }
}
