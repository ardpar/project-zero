using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Synthborn.Core.Events;
using Synthborn.Core.Items;

namespace Synthborn.UI
{
    /// <summary>
    /// Shows a brief loot notification when an item drops during gameplay.
    /// </summary>
    public class LootPopup : MonoBehaviour
    {
        [SerializeField] private Text _lootText;
        [SerializeField] private CanvasGroup _canvasGroup;

        private readonly System.Collections.Generic.Queue<(string name, int rarity)> _queue = new();
        private bool _showing;

        private void Awake()
        {
            if (_canvasGroup == null) _canvasGroup = GetComponent<CanvasGroup>();
            if (_canvasGroup == null) _canvasGroup = gameObject.AddComponent<CanvasGroup>();
            _canvasGroup.alpha = 0f;
            _canvasGroup.blocksRaycasts = false;
        }

        private void OnEnable()
        {
            GameEvents.OnLootDropped += OnLoot;
        }

        private void OnDisable()
        {
            GameEvents.OnLootDropped -= OnLoot;
        }

        private void OnLoot(string id, string name, int rarity)
        {
            _queue.Enqueue((name, rarity));
            if (!_showing) StartCoroutine(ShowQueue());
        }

        private IEnumerator ShowQueue()
        {
            _showing = true;
            while (_queue.Count > 0)
            {
                var (name, rarity) = _queue.Dequeue();
                string rarityName = ((ItemRarity)rarity).ToString();
                Color col = ((ItemRarity)rarity) switch
                {
                    ItemRarity.Baseline => Color.white,
                    ItemRarity.Calibrated => new Color(0.2f, 0.8f, 0.2f),
                    ItemRarity.Reinforced => new Color(0.3f, 0.5f, 1f),
                    ItemRarity.Anomalous => new Color(0.7f, 0.3f, 0.9f),
                    ItemRarity.ArchitectGrade => new Color(1f, 0.85f, 0.2f),
                    _ => Color.white
                };

                if (_lootText != null)
                {
                    _lootText.text = $"<color=#{ColorUtility.ToHtmlStringRGB(col)}>[{rarityName}] {name}</color>";
                }

                // Fade in
                float t = 0f;
                while (t < 0.3f) { t += Time.unscaledDeltaTime; _canvasGroup.alpha = t / 0.3f; yield return null; }
                _canvasGroup.alpha = 1f;

                yield return new WaitForSecondsRealtime(2f);

                // Fade out
                t = 0f;
                while (t < 0.3f) { t += Time.unscaledDeltaTime; _canvasGroup.alpha = 1f - t / 0.3f; yield return null; }
                _canvasGroup.alpha = 0f;
            }
            _showing = false;
        }
    }
}
