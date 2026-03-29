using UnityEngine;
using Synthborn.Core.Persistence;

namespace Synthborn.Core.Audio
{
    /// <summary>
    /// Plays the menu music stem on loop in the MainMenu scene.
    /// Reads the clip from AdaptiveMusicConfig.menuStem.
    /// </summary>
    public class MainMenuMusic : MonoBehaviour
    {
        [SerializeField] private AdaptiveMusicConfig _config;

        private AudioSource _source;

        private void Awake()
        {
            _source = gameObject.AddComponent<AudioSource>();
            _source.playOnAwake = false;
            _source.loop = true;
            _source.spatialBlend = 0f;
            _source.priority = 0;
        }

        private void Start()
        {
            if (_config == null || _config.menuStem == null) return;

            _source.clip = _config.menuStem;
            _source.volume = _config.masterVolume * SaveManager.Data.musicVolume;
            _source.Play();
        }
    }
}
