using UnityEngine;
using Synthborn.Core.Events;

namespace Synthborn.Core
{
    /// <summary>
    /// Tracks run statistics: time, kills, XP, level, mutations.
    /// Listens to game events and accumulates counters.
    /// </summary>
    public class RunStats : MonoBehaviour
    {
        public float SurvivalTime { get; private set; }
        public int EnemiesKilled { get; private set; }
        public int TotalXPCollected { get; private set; }
        public int FinalLevel { get; private set; } = 1;
        public int MutationsAcquired { get; private set; }
        public int SynergiesTriggered { get; private set; }
        public int WavesCleared { get; private set; }

        private bool _running = true;

        private void OnEnable()
        {
            GameEvents.OnEnemyDied += OnKill;
            GameEvents.OnXPGemCollected += OnXP;
            GameEvents.OnLevelUp += OnLevel;
            GameEvents.OnMutationApplied += OnMutation;
            GameEvents.OnSynergyActivated += OnSynergy;
            GameEvents.OnWaveCleared += OnWaveCleared;
            GameEvents.OnPlayerDied += OnRunEnd;
            GameEvents.OnBossDefeated += OnRunEnd;
        }

        private void OnDisable()
        {
            GameEvents.OnEnemyDied -= OnKill;
            GameEvents.OnXPGemCollected -= OnXP;
            GameEvents.OnLevelUp -= OnLevel;
            GameEvents.OnMutationApplied -= OnMutation;
            GameEvents.OnSynergyActivated -= OnSynergy;
            GameEvents.OnWaveCleared -= OnWaveCleared;
            GameEvents.OnPlayerDied -= OnRunEnd;
            GameEvents.OnBossDefeated -= OnRunEnd;
        }

        private void Update()
        {
            if (_running && Time.timeScale > 0f)
                SurvivalTime += Time.deltaTime;
        }

        private void OnKill(Vector2 p, GameObject e, int xp) => EnemiesKilled++;
        private void OnXP(int v) => TotalXPCollected += v;
        private void OnLevel(int l) => FinalLevel = l;
        private void OnMutation(string id, bool s) => MutationsAcquired++;
        private void OnSynergy(string id, string n) => SynergiesTriggered++;
        private void OnWaveCleared() => WavesCleared++;
        private void OnRunEnd() => _running = false;
    }
}
