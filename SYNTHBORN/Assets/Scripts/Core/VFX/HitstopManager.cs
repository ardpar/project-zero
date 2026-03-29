using System.Collections;
using UnityEngine;
using Synthborn.Core.Events;

namespace Synthborn.Core.VFX
{
    /// <summary>
    /// Freezes game time briefly on impactful hits for "juice" feel.
    /// Listens to damage events; triggers hitstop on crits and kills.
    /// </summary>
    public class HitstopManager : MonoBehaviour
    {
        [SerializeField] private float _critHitstopDuration = 0.04f;
        [SerializeField] private float _killHitstopDuration = 0.06f;
        [SerializeField] private float _bossHitstopDuration = 0.12f;

        private Coroutine _hitstopRoutine;

        private void OnEnable()
        {
            GameEvents.OnDamageDealt += OnDamage;
            GameEvents.OnEnemyDied += OnKill;
            GameEvents.OnBossDefeated += OnBossDefeated;
        }

        private void OnDisable()
        {
            GameEvents.OnDamageDealt -= OnDamage;
            GameEvents.OnEnemyDied -= OnKill;
            GameEvents.OnBossDefeated -= OnBossDefeated;
        }

        private void OnDamage(Vector2 pos, int damage, bool isCrit)
        {
            if (isCrit)
                TriggerHitstop(_critHitstopDuration);
        }

        private void OnKill(Vector2 pos, GameObject enemy, int xp)
        {
            TriggerHitstop(_killHitstopDuration);
        }

        private void OnBossDefeated()
        {
            TriggerHitstop(_bossHitstopDuration);
        }

        private void TriggerHitstop(float duration)
        {
            if (_hitstopRoutine != null)
                StopCoroutine(_hitstopRoutine);
            _hitstopRoutine = StartCoroutine(HitstopRoutine(duration));
        }

        private IEnumerator HitstopRoutine(float duration)
        {
            Time.timeScale = 0f;
            yield return new WaitForSecondsRealtime(duration);
            Time.timeScale = 1f;
            _hitstopRoutine = null;
        }
    }
}
