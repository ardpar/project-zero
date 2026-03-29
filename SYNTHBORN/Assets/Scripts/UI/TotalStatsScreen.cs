using UnityEngine;
using UnityEngine.UI;
using Synthborn.Core.Persistence;

namespace Synthborn.UI
{
    /// <summary>
    /// Shows lifetime player statistics: total kills, runs, playtime, bests.
    /// </summary>
    public class TotalStatsScreen : MonoBehaviour
    {
        [SerializeField] private GameObject _panel;
        [SerializeField] private Text _statsText;
        [SerializeField] private Font _font;

        public void Show()
        {
            if (_panel == null) return;
            _panel.SetActive(true);
            Refresh();
            PopupEscHandler.Register(_panel, Hide);
        }

        public void Hide()
        {
            if (_panel != null) _panel.SetActive(false);
            PopupEscHandler.Unregister();
        }

        private void Refresh()
        {
            if (_statsText == null) return;

            var data = SaveManager.Data;
            int bestMin = Mathf.FloorToInt(data.bestSurvivalTime / 60f);
            int bestSec = Mathf.FloorToInt(data.bestSurvivalTime % 60f);

            _statsText.text =
                $"Total Runs:  {data.totalRuns}\n" +
                $"Total Kills:  {data.totalKills}\n" +
                $"Best Time:  {bestMin:00}:{bestSec:00}\n" +
                $"Best Wave:  {data.bestWavesCleared}\n" +
                $"Cells Earned:  {data.totalCells}\n" +
                $"Mutations Unlocked:  {data.unlockedMutationIds.Count}\n" +
                $"Mutations Discovered:  {data.discoveredMutationIds.Count}\n" +
                $"Achievements:  {AchievementManager.UnlockedCount}/{AchievementManager.TotalCount}";
        }
    }
}
