using UnityEngine;

namespace Synthborn.Core.Audio
{
    /// <summary>
    /// Intensity tiers for adaptive music. Maps to wave progression.
    /// </summary>
    public enum MusicIntensity { Calm, Low, Medium, High, Boss }

    /// <summary>
    /// Configuration asset for the adaptive music system.
    /// Each intensity tier has a looping AudioClip stem.
    /// </summary>
    [CreateAssetMenu(menuName = "Synthborn/Audio/AdaptiveMusicConfig")]
    public class AdaptiveMusicConfig : ScriptableObject
    {
        [Header("Menu")]
        [Tooltip("Plays in the main menu (separate from gameplay stems).")]
        public AudioClip menuStem;

        [Header("Stems (looping clips per intensity tier)")]
        [Tooltip("Plays during wave breaks and menus.")]
        public AudioClip calmStem;

        [Tooltip("Plays during early waves (1-3).")]
        public AudioClip lowStem;

        [Tooltip("Plays during mid waves (4-6).")]
        public AudioClip mediumStem;

        [Tooltip("Plays during late waves (7+) and elite encounters.")]
        public AudioClip highStem;

        [Tooltip("Plays during boss fight. Falls back to highStem if null.")]
        public AudioClip bossStem;

        [Header("Volume")]
        [Range(0f, 1f)] public float masterVolume = 0.4f;

        [Header("Crossfade")]
        [Tooltip("Duration of crossfade between intensity tiers (seconds, unscaled).")]
        [Range(0.5f, 5f)] public float crossfadeDuration = 1.5f;

        [Header("Wave Thresholds")]
        [Tooltip("Waves 1 to this value = Low intensity.")]
        public int lowToMediumWave = 4;
        [Tooltip("Waves from lowToMedium to this value = Medium. Above = High.")]
        public int mediumToHighWave = 7;

        /// <summary>Get the AudioClip for a given intensity tier.</summary>
        public AudioClip GetStem(MusicIntensity intensity)
        {
            return intensity switch
            {
                MusicIntensity.Calm   => calmStem,
                MusicIntensity.Low    => lowStem,
                MusicIntensity.Medium => mediumStem,
                MusicIntensity.High   => highStem,
                MusicIntensity.Boss   => bossStem != null ? bossStem : highStem,
                _ => calmStem
            };
        }

        /// <summary>Map a wave number (1-based) to an intensity tier.</summary>
        public MusicIntensity GetIntensityForWave(int waveNumber)
        {
            if (waveNumber < lowToMediumWave) return MusicIntensity.Low;
            if (waveNumber < mediumToHighWave) return MusicIntensity.Medium;
            return MusicIntensity.High;
        }
    }
}
