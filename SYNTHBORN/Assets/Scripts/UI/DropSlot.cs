using UnityEngine;
using UnityEngine.EventSystems;
using Synthborn.Core.Items;

namespace Synthborn.UI
{
    /// <summary>
    /// Drop target for equipment slots and inventory grid cells.
    /// Accepts dragged items and triggers equip/unequip/swap logic.
    /// </summary>
    public class DropSlot : MonoBehaviour, IDropHandler
    {
        public enum SlotMode { Equipment, Inventory }

        public SlotMode Mode { get; set; } = SlotMode.Inventory;
        public ItemSlotType EquipSlotType { get; set; }
        public int GridIndex { get; set; }

        /// <summary>Callback when an item is dropped here.</summary>
        public System.Action<DragDropItem, DropSlot> OnItemDropped { get; set; }

        public void OnDrop(PointerEventData eventData)
        {
            var dragItem = eventData.pointerDrag?.GetComponent<DragDropItem>();
            if (dragItem == null) return;

            OnItemDropped?.Invoke(dragItem, this);
        }
    }
}
