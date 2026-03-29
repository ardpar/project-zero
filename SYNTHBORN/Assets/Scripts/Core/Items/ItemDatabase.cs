using UnityEngine;

namespace Synthborn.Core.Items
{
    /// <summary>
    /// Registry of all item definitions. Used for lookup by ID.
    /// </summary>
    [CreateAssetMenu(menuName = "Synthborn/Items/ItemDatabase")]
    public class ItemDatabase : ScriptableObject
    {
        public ItemData[] allItems;

        /// <summary>Find an item by ID.</summary>
        public ItemData GetById(string id)
        {
            if (allItems == null) return null;
            foreach (var item in allItems)
                if (item != null && item.Id == id) return item;
            return null;
        }
    }
}
