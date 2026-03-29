using System.Collections;
using UnityEngine;
using Synthborn.Core.Events;
using Synthborn.Core.Persistence;

namespace Synthborn.Core.Audio
{
    /// <summary>
    /// Adaptive music system that crossfades between intensity stems
    /// based on wave progression, boss encounters, and game state.
    /// Uses two AudioSources for seamless crossfading.
    /// </summary>
    public class AdaptiveMusicManager : MonoBehaviour
    {
        [SerializeField] private AdaptiveMusicConfig _config;

        private AudioSource _sourceA;
        private AudioSource _sourceB;
        private bool _aIsActive = true;
        private MusicIntensity _currentIntensity = MusicIntensity.Calm;
        private Coroutine _crossfadeCoroutine;

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
        }

        private void OnDisable()
        {
            GameEvents.OnWaveStarted -= OnWaveStarted;
            GameEvents.OnWaveCleared -= OnWaveCleared;
            GameEvents.OnBossSpawned -= OnBossSpawned;
            GameEvents.OnBossDefeated -= OnBossDefeated;
            GameEvents.OnPlayerDied -= OnPlayerDied;
        }

        private void Start()
        {
            // Begin with calm music
            TransitionTo(MusicIntensity.Calm);
        }

        // ─────────────────────────────────────────────
        // Event Handlers
        // ─────────────────────────────────────────────

        private void OnWaveStarted(int waveNumber)
        {
            if (_config == null) return;
            var target = _config.GetIntensityForWave(waveNumber);
            TransitionTo(target);
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
            // Fade out all music on death
            if (_crossfadeCoroutine != null)
                StopCoroutine(_crossfadeCoroutine);
            _crossfadeCoroutine = StartCoroutine(FadeOut());
        }

        // ─────────────────────────────────────────────
        // Core Logic
        // ─────────────────────────────────────────────

        private void TransitionTo(MusicIntensity target)
        {
            if (_config == null) return;
            if (target == _currentIntensity && ActiveSource.isPlaying) return;

            var clip = _config.GetStem(target);
            if (clip == null)
            {
                // No clip for this intensity — just stop
                if (_crossfadeCoroutine != null) StopCoroutine(_crossfadeCoroutine);
                ActiveSource.Stop();
                _currentIntensity = target;
                return;
            }

            _currentIntensity = target;

            if (!ActiveSource.isPlaying)
            {
                // Nothing playing — start directly
                ActiveSource.clip = clip;
                ActiveSource.volume = _config.masterVolume * SaveManager.Data.musicVolume;
                ActiveSource.Play();
                return;
            }

            // Crossfade from active to inactive
            if (_crossfadeCoroutine != null)
                StopCoroutine(_crossfadeCoroutine);
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
            float startVolumeA = _sourceA.volume;
            float startVolumeB = _sourceB.volume;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / duration);

                _sourceA.volume = Mathf.Lerp(startVolumeA, 0f, t);
                _sourceB.volume = Mathf.Lerp(startVolumeB, 0f, t);

                yield return null;
            }

            _sourceA.Stop();
            _sourceB.Stop();
            _sourceA.clip = null;
            _sourceB.clip = null;

            _crossfadeCoroutine = null;
        }

        // ─────────────────────────────────────────────
        // Helpers
        // ─────────────────────────────────────────────

        private static void ConfigureSource(AudioSource source)
        {
            source.playOnAwake = false;
            source.loop = true;
            source.spatialBlend = 0f; // 2D sound
            source.priority = 0;     // Highest priority for music
            source.volume = 0f;
        }
    }
}
