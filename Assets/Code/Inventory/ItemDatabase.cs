using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace com.AylanJ123.CodeDecay.Managers
{
    /// <summary>
    /// A ScriptableObject that acts as a central database for all ItemData objects.
    /// This allows for easy lookup of items by their unique ID.
    /// </summary>
    [CreateAssetMenu(fileName = "ItemDatabase", menuName = "Code Decay/Item Database")]
    public sealed class ItemDatabase : ScriptableObject
    {
        private static ItemDatabase instance;
        public static ItemDatabase Instance
        {
            get
            {
                if (instance == null) instance = Resources.Load<ItemDatabase>("ItemDatabase");
                return instance;
            }
        }

        [SerializeField]
        private List<ItemData> allItems;

        private Dictionary<int, ItemData> itemDictionary;

        public ItemData GetItemById(int id)
        {
            itemDictionary ??= allItems.ToDictionary(item => item.itemId);
            return itemDictionary.ContainsKey(id) ? itemDictionary[id] : null;
        }
    }
}
