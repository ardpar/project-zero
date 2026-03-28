using UnityEngine;
using Synthborn.Core.Events;

namespace Synthborn.Core.Audio
{
    /// <summary>
    /// Plays procedural placeholder SFX using generated AudioClips.
    /// Replace with real clips later.
    /// </summary>
    public class SFXManager : MonoBehaviour
    {
        private AudioSource _source;

        // Generated placeholder clips
        private AudioClip _shootClip;
        private AudioClip _hitClip;
        private AudioClip _killClip;
        private AudioClip _xpPickupClip;
        private AudioClip _levelUpClip;
        private AudioClip _dashClip;
        private AudioClip _mutationClip;
        private AudioClip _synergyClip;

        private void Awake()
        {
            _source = gameObject.AddComponent<AudioSource>();
            _source.playOnAwake = false;
            _source.volume = 0.3f;

            // Generate simple placeholder tones
            _shootClip = GenerateTone(800f, 0.05f, "Shoot");
            _hitClip = GenerateTone(400f, 0.08f, "Hit");
            _killClip = GenerateTone(600f, 0.15f, "Kill");
            _xpPickupClip = GenerateTone(1200f, 0.06f, "XPPickup");
            _levelUpClip = GenerateChirp(400f, 1200f, 0.3f, "LevelUp");
            _dashClip = GenerateTone(200f, 0.1f, "Dash");
            _mutationClip = GenerateChirp(600f, 1000f, 0.2f, "Mutation");
            _synergyClip = GenerateChirp(800f, 1600f, 0.4f, "Synergy");
        }

        private void OnEnable()
        {
            GameEvents.OnDamageDealt += OnDamage;
            GameEvents.OnEnemyDied += OnKill;
            GameEvents.OnXPGemCollected += OnXPPickup;
            GameEvents.OnLevelUp += OnLevelUp;
            GameEvents.OnPlayerDashStarted += OnDash;
            GameEvents.OnMutationApplied += OnMutation;
            GameEvents.OnSynergyActivated += OnSynergy;
        }

        private void OnDisable()
        {
            GameEvents.OnDamageDealt -= OnDamage;
            GameEvents.OnEnemyDied -= OnKill;
            GameEvents.OnXPGemCollected -= OnXPPickup;
            GameEvents.OnLevelUp -= OnLevelUp;
            GameEvents.OnPlayerDashStarted -= OnDash;
            GameEvents.OnMutationApplied -= OnMutation;
            GameEvents.OnSynergyActivated -= OnSynergy;
        }

        private void OnDamage(Vector2 p, int d, bool crit) => Play(_hitClip);
        private void OnKill(Vector2 p, GameObject e, int x) => Play(_killClip);
        private void OnXPPickup(int v) => Play(_xpPickupClip, 0.2f);
        private void OnLevelUp(int l) => Play(_levelUpClip, 0.5f);
        private void OnDash(Vector2 d) => Play(_dashClip);
        private void OnMutation(string id, bool s) => Play(_mutationClip, 0.4f);
        private void OnSynergy(string id, string n) => Play(_synergyClip, 0.5f);

        private void Play(AudioClip clip, float volume = 0.3f)
        {
            if (clip != null && _source != null)
                _source.PlayOneShot(clip, volume);
        }

        private static AudioClip GenerateTone(float frequency, float duration, string name)
        {
            int sampleRate = 44100;
            int samples = (int)(sampleRate * duration);
            var clip = AudioClip.Create(name, samples, 1, sampleRate, false);
            float[] data = new float[samples];
            for (int i = 0; i < samples; i++)
            {
                float t = (float)i / sampleRate;
                float envelope = 1f - (t / duration); // linear fade out
                data[i] = Mathf.Sin(2f * Mathf.PI * frequency * t) * envelope * 0.5f;
            }
            clip.SetData(data, 0);
            return clip;
        }

        private static AudioClip GenerateChirp(float freqStart, float freqEnd, float duration, string name)
        {
            int sampleRate = 44100;
            int samples = (int)(sampleRate * duration);
            var clip = AudioClip.Create(name, samples, 1, sampleRate, false);
            float[] data = new float[samples];
            for (int i = 0; i < samples; i++)
            {
                float t = (float)i / sampleRate;
                float freq = Mathf.Lerp(freqStart, freqEnd, t / duration);
                float envelope = 1f - (t / duration);
                data[i] = Mathf.Sin(2f * Mathf.PI * freq * t) * envelope * 0.5f;
            }
            clip.SetData(data, 0);
            return clip;
        }
    }
}
