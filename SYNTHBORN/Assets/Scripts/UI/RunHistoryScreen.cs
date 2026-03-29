using UnityEngine;
using UnityEngine.UI;
using Synthborn.Core.Persistence;

namespace Synthborn.UI
{
    /// <summary>
    /// Shows last 10 runs in a scrollable list.
    /// </summary>
    public class RunHistoryScreen : MonoBehaviour
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
                CreateRow("No runs yet. Go play!");
                return;
            }

            // Show newest first
            for (int i = history.Count - 1; i >= 0; i--)
            {
                var r = history[i];
                int min = Mathf.FloorToInt(r.survivalTime / 60f);
                int sec = Mathf.FloorToInt(r.survivalTime % 60f);
                string result = r.victory ? "<color=green>WIN</color>" : "<color=red>DEAD</color>";
                CreateRow($"{r.date}  {result}  {min:00}:{sec:00}  Lv{r.finalLevel}  {r.enemiesKilled}kills  +{r.cellsEarned}cells");
            }
        }

        private void CreateRow(string text)
        {
            var go = new GameObject("Row", typeof(RectTransform), typeof(Text));
            go.transform.SetParent(_listContainer, false);
            var rt = go.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(700, 25);
            var t = go.GetComponent<Text>();
            t.text = text;
            t.fontSize = 14;
            t.color = Color.white;
            t.alignment = TextAnchor.MiddleLeft;
            t.font = _font != null ? _font : Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            t.supportRichText = true;
        }
    }
}
