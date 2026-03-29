using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Synthborn.Core.Persistence;

namespace Synthborn.UI
{
    /// <summary>
    /// Shows top 10 runs ranked by survival time in a scrollable list.
    /// Accessed from the Main Menu.
    /// </summary>
    public class LeaderboardScreen : MonoBehaviour
    {
        [SerializeField] private GameObject _panel;
        [SerializeField] private Transform _listContainer;
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
            if (_listContainer != null)
                foreach (Transform child in _listContainer) Destroy(child.gameObject);

            var history = SaveManager.Data.runHistory;
            if (history == null || history.Count == 0)
            {
                CreateRow("#  ---  No runs yet. Go play!", Color.white);
                return;
            }

            // Sort by survival time descending, take top 10
            var top10 = history
                .OrderByDescending(r => r.survivalTime)
                .Take(10)
                .ToList();

            CreateRow("#   TIME     KILLS   LEVEL   WAVES   CELLS", new Color(1f, 0.85f, 0.3f));

            for (int i = 0; i < top10.Count; i++)
            {
                var r = top10[i];
                int min = Mathf.FloorToInt(r.survivalTime / 60f);
                int sec = Mathf.FloorToInt(r.survivalTime % 60f);
                string rank = (i + 1).ToString().PadLeft(2);
                string line = $"{rank}  {min:00}:{sec:00}    {r.enemiesKilled,5}   Lv{r.finalLevel,3}   W{r.wavesCleared,3}   +{r.cellsEarned}";

                Color rowColor = i switch
                {
                    0 => new Color(1f, 0.85f, 0.3f),  // Gold
                    1 => new Color(0.8f, 0.8f, 0.85f), // Silver
                    2 => new Color(0.8f, 0.5f, 0.2f),  // Bronze
                    _ => Color.white
                };
                CreateRow(line, rowColor);
            }
        }

        private void CreateRow(string text, Color color)
        {
            var go = new GameObject("Row", typeof(RectTransform), typeof(Text));
            go.transform.SetParent(_listContainer, false);
            var rt = go.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(700, 25);
            var t = go.GetComponent<Text>();
            t.text = text;
            t.fontSize = 14;
            t.color = color;
            t.alignment = TextAnchor.MiddleLeft;
            t.font = _font != null ? _font : Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        }
    }
}
