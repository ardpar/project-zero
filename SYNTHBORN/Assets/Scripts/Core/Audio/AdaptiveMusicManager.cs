using System.Collections;
using UnityEngine;
using Synthborn.Core.Events;
using Synthborn.Core.Persistence;

namespace Synthborn.Core.Audio
{
    /// <summary>
    /// Adaptive music system that crossfades between intensity stems
    /// based on wave progression, boss encounters, biome, and game state.
    /// Uses two AudioSources for seamless crossfading.
    ///
    /// Biome integration: call SetBiomeStems() when entering a new biome.
    /// Biome stems override config defaults when present.
    /// </summary>
    public class AdaptiveMusicManager : MonoBehaviour
    {
        [SerializeField] private AdaptiveMusicConfig _config;

        private AudioSource _sourceA;
        private AudioSource _sourceB;
        private bool _aIsActive = true;
        private MusicIntensity _currentIntensity = MusicIntensity.Calm;
        private Coroutine _crossfadeCoroutine;

        // Biome-specific stem overrides (set externally via SetBiomeStems)
        private AudioClip _biomeCalmOverride;
        private AudioClip _biomeCombatOverride;
        private AudioClip _biomeBossOverride;

        private AudioSource ActiveSource => _aIsActive ? _sourceA : _sourceB;
        private AudioSource InactiveSource => _aIsActive ? _sourceB : _sourceA;

        private void Awake()
        {
            _sourceA = gameObject.AddComponent<AudioSource>();
            _sourceB = gameObject.AddComponent<AudioSource>();
            ConfigureSource(_sourceA);
            ConfigureSource(_sourceB);
        }

        private void OnEnable()
        {
            GameEvents.OnWaveStarted += OnWaveStarted;
            GameEvents.OnWaveCleared += OnWaveCleared;
            GameEvents.OnBossSpawned += OnBossSpawned;
            GameEvents.OnBossDefeated += OnBossDefeated;
            GameEvents.OnPlayerDied += OnPlayerDied;
            GameEvents.OnCalibrationIntervalStarted += OnCalibrationInterval;
        }

        private void OnDisable()
        {
            GameEvents.OnWaveStarted -= OnWaveStarted;
            GameEvents.OnWaveCleared -= OnWaveCleared;
            GameEvents.OnBossSpawned -= OnBossSpawned;
            GameEvents.OnBossDefeated -= OnBossDefeated;
            GameEvents.OnPlayerDied -= OnPlayerDied;
            GameEvents.OnCalibrationIntervalStarted -= OnCalibrationInterval;
        }

        private void Start()
        {
            TransitionTo(MusicIntensity.Calm);
        }

        // ─── Public API ───

        /// <summary>
        /// Set biome-specific music stems. Called by TrialManager or GameBootstrap
        /// when entering a new biome. Pass null for any stem to use config defaults.
        /// </summary>
        public void SetBiomeStems(AudioClip calmStem, AudioClip combatStem, AudioClip bossStem)
        {
            _biomeCalmOverride = calmStem;
            _biomeCombatOverride = combatStem;
            _biomeBossOverride = bossStem;

            // Immediately transition to biome's calm if we're currently calm
            if (_currentIntensity == MusicIntensity.Calm)
                TransitionTo(MusicIntensity.Calm);
        }

        /// <summary>Clear biome overrides, reverting to config defaults.</summary>
        public void ClearBiomeStems()
        {
            _biomeCalmOverride = null;
            _biomeCombatOverride = null;
            _biomeBossOverride = null;
        }

        // ─── Event Handlers ───

        private void OnCalibrationInterval()
        {
            TransitionTo(MusicIntensity.Calm);
        }

        private void OnWaveStarted(int waveNumber)
        {
            if (_config == null) return;
            TransitionTo(_config.GetIntensityForWave(waveNumber));
        }

        private void OnWaveCleared()
        {
            TransitionTo(MusicIntensity.Calm);
        }

        private void OnBossSpawned()
        {
            TransitionTo(MusicIntensity.Boss);
        }

        private void OnBossDefeated()
        {
            TransitionTo(MusicIntensity.Calm);
        }

        private void OnPlayerDied()
        {
            if (_crossfadeCoroutine != null) StopCoroutine(_crossfadeCoroutine);
            _crossfadeCoroutine = StartCoroutine(FadeOut());
            ClearBiomeStems();
        }

        // ─── Core Logic ───

        private AudioClip ResolveClip(MusicIntensity intensity)
        {
            switch (intensity)
            {
                case MusicIntensity.Calm:
                    return _biomeCalmOverride ?? (_config != null ? _config.GetStem(MusicIntensity.Calm) : null);

                case MusicIntensity.Boss:
                    return _biomeBossOverride ?? _biomeCombatOverride
                        ?? (_config != null ? _config.GetStem(MusicIntensity.Boss) : null);

                default: // Low, Medium, High
                    return _biomeCombatOverride ?? (_config != null ? _config.GetStem(intensity) : null);
            }
        }

        private void TransitionTo(MusicIntensity target)
        {
            if (_config == null) return;

            var clip = ResolveClip(target);

            // Skip if same clip already playing
            if (target == _currentIntensity && ActiveSource.isPlaying && ActiveSource.clip == clip) return;

            _currentIntensity = target;

            if (clip == null)
            {
                if (_crossfadeCoroutine != null) StopCoroutine(_crossfadeCoroutine);
                ActiveSource.Stop();
                return;
            }

            if (!ActiveSource.isPlaying)
            {
                ActiveSource.clip = clip;
                ActiveSource.volume = _config.masterVolume * SaveManager.Data.musicVolume;
                ActiveSource.Play();
                return;
            }

            if (_crossfadeCoroutine != null) StopCoroutine(_crossfadeCoroutine);
            _crossfadeCoroutine = StartCoroutine(Crossfade(clip));
        }

        private IEnumerator Crossfade(AudioClip newClip)
        {
            var fadingOut = ActiveSource;
            var fadingIn = InactiveSource;
            _aIsActive = !_aIsActive;

            fadingIn.clip = newClip;
            fadingIn.volume = 0f;
            fadingIn.Play();

            float duration = _config.crossfadeDuration;
            float targetVolume = _config.masterVolume * SaveManager.Data.musicVolume;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                fadingIn.volume = Mathf.Lerp(0f, targetVolume, t);
                fadingOut.volume = Mathf.Lerp(targetVolume, 0f, t);
                yield return null;
            }

            fadingIn.volume = targetVolume;
            fadingOut.volume = 0f;
            fadingOut.Stop();
            fadingOut.clip = null;
            _crossfadeCoroutine = null;
        }

        private IEnumerator FadeOut()
        {
            float duration = _config != null ? _config.crossfadeDuration : 1.5f;
            float startA = _sourceA.volume;
            float startB = _sourceB.volume;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                _sourceA.volume = Mathf.Lerp(startA, 0f, t);
                _sourceB.volume = Mathf.Lerp(startB, 0f, t);
                yield return null;
            }

            _sourceA.Stop(); _sourceB.Stop();
            _sourceA.clip = null; _sourceB.clip = null;
            _crossfadeCoroutine = null;
        }

        private static void ConfigureSource(AudioSource source)
        {
            source.playOnAwake = false;
            source.loop = true;
            source.spatialBlend = 0f;
            source.priority = 0;
            source.volume = 0f;
        }
    }
}
