using System.Collections.Generic;
using UnityEngine;
using Synthborn.Core.Events;
using Synthborn.Mutations;

namespace Synthborn.Player
{
    /// <summary>
    /// Manages layered sprite rendering for the player.
    /// Listens for mutation events and swaps slot sprites accordingly.
    /// Sorting: Back(-1) < Base(0) < Legs(1) < Arms(2) < Head(3).
    /// </summary>
    public class SpriteCompositor : MonoBehaviour
    {
        [Header("Slot Renderers (assign child SpriteRenderers)")]
        [SerializeField] private SpriteRenderer _baseRenderer;
        [SerializeField] private SpriteRenderer _backRenderer;
        [SerializeField] private SpriteRenderer _legsRenderer;
        [SerializeField] private SpriteRenderer _armsRenderer;
        [SerializeField] private SpriteRenderer _headRenderer;

        private MutationManager _mutationManager;
        private MutationDatabase _database;

        public void Initialize(MutationManager manager, MutationDatabase database)
        {
            _mutationManager = manager;
            _database = database;
        }

        private void OnEnable()
        {
            GameEvents.OnMutationApplied += OnMutationApplied;
        }

        private void OnDisable()
        {
            GameEvents.OnMutationApplied -= OnMutationApplied;
        }

        private void OnMutationApplied(string mutationId, bool isSlot)
        {
            if (!isSlot || _mutationManager == null || _database == null) return;

            var mutation = _database.GetById(mutationId);
            if (mutation == null || mutation.slotSprite == null) return;

            var renderer = GetRendererForSlot(mutation.slot);
            if (renderer == null) return;

            renderer.sprite = mutation.slotSprite;
            renderer.enabled = true;
            renderer.color = GetSlotColor(mutation.slot);
        }

        /// <summary>Reset all slot sprites for new run.</summary>
        public void ResetSprites()
        {
            if (_backRenderer != null) { _backRenderer.sprite = null; _backRenderer.enabled = false; }
            if (_legsRenderer != null) { _legsRenderer.sprite = null; _legsRenderer.enabled = false; }
            if (_armsRenderer != null) { _armsRenderer.sprite = null; _armsRenderer.enabled = false; }
            if (_headRenderer != null) { _headRenderer.sprite = null; _headRenderer.enabled = false; }
        }

        private SpriteRenderer GetRendererForSlot(MutationSlot slot)
        {
            return slot switch
            {
                MutationSlot.Back => _backRenderer,
                MutationSlot.Legs => _legsRenderer,
                MutationSlot.Arms => _armsRenderer,
                MutationSlot.Head => _headRenderer,
                _ => null
            };
        }

        /// <summary>Placeholder colors until real sprites exist.</summary>
        private static Color GetSlotColor(MutationSlot slot)
        {
            return slot switch
            {
                MutationSlot.Arms => new Color(1f, 0.4f, 0.4f),   // Red
                MutationSlot.Legs => new Color(0.4f, 1f, 0.4f),   // Green
                MutationSlot.Back => new Color(0.4f, 0.6f, 1f),   // Blue
                MutationSlot.Head => new Color(1f, 0.9f, 0.3f),   // Yellow
                _ => Color.white
            };
        }
    }
}
