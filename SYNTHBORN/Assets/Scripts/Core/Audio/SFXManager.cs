using UnityEngine;
using Synthborn.Core.Events;
using Synthborn.Core.Persistence;

namespace Synthborn.Core.Audio
{
    /// <summary>
    /// Plays SFX in response to game events.
    /// Assign real AudioClips in Inspector; falls back to procedural tones if null.
    /// </summary>
    public class SFXManager : MonoBehaviour
    {
        [Header("SFX Clips (assign real clips, or leave null for procedural)")]
        [SerializeField] private AudioClip _shootClip;
        [SerializeField] private AudioClip _hitClip;
        [SerializeField] private AudioClip _killClip;
        [SerializeField] private AudioClip _xpPickupClip;
        [SerializeField] private AudioClip _levelUpClip;
        [SerializeField] private AudioClip _dashClip;
        [SerializeField] private AudioClip _mutationClip;
        [SerializeField] private AudioClip _synergyClip;
        [SerializeField] private AudioClip _playerHitClip;
        [SerializeField] private AudioClip _playerDeathClip;
        [SerializeField] private AudioClip _bossSpawnClip;
        [SerializeField] private AudioClip _hpPickupClip;

        private AudioSource _source;

        private void Awake()
        {
            _source = GetComponent<AudioSource>();
            if (_source == null)
            {
                _source = gameObject.AddComponent<AudioSource>();
                _source.playOnAwake = false;
            }
            _source.volume = 0.3f;

            // Fallback: generate procedural tones for any unassigned clip
            if (_shootClip == null) _shootClip = GenerateTone(800f, 0.05f);
            if (_hitClip == null) _hitClip = GenerateTone(400f, 0.08f);
            if (_killClip == null) _killClip = GenerateTone(600f, 0.15f);
            if (_xpPickupClip == null) _xpPickupClip = GenerateTone(1200f, 0.06f);
            if (_levelUpClip == null) _levelUpClip = GenerateChirp(400f, 1200f, 0.3f);
            if (_dashClip == null) _dashClip = GenerateTone(200f, 0.1f);
            if (_mutationClip == null) _mutationClip = GenerateChirp(600f, 1000f, 0.2f);
            if (_synergyClip == null) _synergyClip = GenerateChirp(800f, 1600f, 0.4f);
            if (_playerHitClip == null) _playerHitClip = GenerateTone(250f, 0.12f);
            if (_playerDeathClip == null) _playerDeathClip = GenerateChirp(600f, 100f, 0.6f);
            if (_bossSpawnClip == null) _bossSpawnClip = GenerateChirp(100f, 400f, 0.5f);
            if (_hpPickupClip == null) _hpPickupClip = GenerateChirp(600f, 900f, 0.15f);
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
            GameEvents.OnPlayerHPChanged += OnPlayerHPChanged;
            GameEvents.OnPlayerDied += OnPlayerDied;
            GameEvents.OnBossSpawned += OnBossSpawned;
            GameEvents.OnPlayerHealRequested += OnHPPickup;
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
            GameEvents.OnPlayerHPChanged -= OnPlayerHPChanged;
            GameEvents.OnPlayerDied -= OnPlayerDied;
            GameEvents.OnBossSpawned -= OnBossSpawned;
            GameEvents.OnPlayerHealRequested -= OnHPPickup;
        }

        private void OnDamage(Vector2 p, int d, bool crit) => Play(_hitClip);
        private void OnKill(Vector2 p, GameObject e, int x) => Play(_killClip);
        private void OnXPPickup(int v) => Play(_xpPickupClip, 0.2f);
        private void OnLevelUp(int l) => Play(_levelUpClip, 0.5f);
        private void OnDash(Vector2 d) => Play(_dashClip);
        private void OnMutation(string id, bool s) => Play(_mutationClip, 0.4f);
        private void OnSynergy(string id, string n) => Play(_synergyClip, 0.5f);

        private int _lastHP = int.MaxValue;
        private void OnPlayerHPChanged(int current, int max)
        {
            if (current < _lastHP) Play(_playerHitClip, 0.4f);
            _lastHP = current;
        }
        private void OnPlayerDied() => Play(_playerDeathClip, 0.6f);
        private void OnBossSpawned() => Play(_bossSpawnClip, 0.6f);
        private void OnHPPickup(float _) => Play(_hpPickupClip, 0.3f);

        private void Play(AudioClip clip, float volume = 0.3f)
        {
            if (clip != null && _source != null)
                _source.PlayOneShot(clip, volume * SaveManager.Data.sfxVolume);
        }

        private static AudioClip GenerateTone(float freq, float dur)
        {
            int sr = 44100; int s = (int)(sr * dur);
            var c = AudioClip.Create("tone", s, 1, sr, false);
            float[] d = new float[s];
            for (int i = 0; i < s; i++) { float t = (float)i/sr; d[i] = Mathf.Sin(2f*Mathf.PI*freq*t)*(1f-t/dur)*0.5f; }
            c.SetData(d, 0); return c;
        }

        private static AudioClip GenerateChirp(float f1, float f2, float dur)
        {
            int sr = 44100; int s = (int)(sr * dur);
            var c = AudioClip.Create("chirp", s, 1, sr, false);
            float[] d = new float[s];
            for (int i = 0; i < s; i++) { float t = (float)i/sr; float f = Mathf.Lerp(f1,f2,t/dur); d[i] = Mathf.Sin(2f*Mathf.PI*f*t)*(1f-t/dur)*0.5f; }
            c.SetData(d, 0); return c;
        }
    }
}
