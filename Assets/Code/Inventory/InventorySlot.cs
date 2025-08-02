using UnityEngine;

namespace com.AylanJ123.CodeDecay.Inventory
{
    /// <summary> The different types of slots an inventory can have </summary>
    public enum SlotType
    {
        Any,
        Potion,
        Deletion
    }

    /// <summary> Represents a single slot in the inventory </summary>
    [System.Serializable]
    public class InventorySlot
    {
        [Tooltip("The item data currently held by this slot")]
        public ItemData itemData;

        [Tooltip("The type of this slot, which determines what it can hold")]
        public SlotType slotType;

        public bool IsEmpty => itemData == null;

        /// <summary>
        /// Clears the slot by setting the item data to null.
        /// </summary>
        public void ClearSlot()
        {
            itemData = null;
        }

        /// <summary> Checks if a given item is valid for this slot </summary>
        /// <param name="itemToStore"> The item to check </param>
        /// <returns> True if the item can be stored, false if it has to be deleted </returns>
        public bool CanStoreItem(ItemData itemToStore)
        {
            return slotType switch
            {
                SlotType.Any => true,
                SlotType.Potion => itemToStore is PotionData,
                SlotType.Deletion => false,
                _ => false,
            };
        }
    }
}