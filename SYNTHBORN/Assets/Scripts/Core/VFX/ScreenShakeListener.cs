using UnityEngine;
using Synthborn.Core.Events;

namespace Synthborn.Core.VFX
{
    /// <summary>
    /// Listens to game events and triggers screen shake on CameraController.
    /// </summary>
    public class ScreenShakeListener : MonoBehaviour
    {
        [SerializeField] private CameraController _camera;
        [SerializeField] private float _damageShakeDuration = 0.1f;
        [SerializeField] private float _damageShakeIntensity = 0.08f;
        [SerializeField] private float _killShakeDuration = 0.12f;
        [SerializeField] private float _killShakeIntensity = 0.12f;
        [SerializeField] private float _bossShakeDuration = 0.4f;
        [SerializeField] private float _bossShakeIntensity = 0.3f;

        private void OnEnable()
        {
            GameEvents.OnDamageDealt += OnDamage;
            GameEvents.OnEnemyDied += OnEnemyDied;
            GameEvents.OnBossSpawned += OnBossSpawn;
        }

        private void OnDisable()
        {
            GameEvents.OnDamageDealt -= OnDamage;
            GameEvents.OnEnemyDied -= OnEnemyDied;
            GameEvents.OnBossSpawned -= OnBossSpawn;
        }

        private void OnDamage(Vector2 pos, int dmg, bool crit)
        {
            if (_camera == null) return;
            float mult = crit ? 1.5f : 1f;
            _camera.TriggerShake(_damageShakeDuration, _damageShakeIntensity * mult);
        }

        private void OnEnemyDied(Vector2 pos, GameObject enemy, int xp)
        {
            if (_camera != null)
                _camera.TriggerShake(_killShakeDuration, _killShakeIntensity);
        }

        private void OnBossSpawn()
        {
            if (_camera != null)
                _camera.TriggerShake(_bossShakeDuration, _bossShakeIntensity);
        }
    }
}
