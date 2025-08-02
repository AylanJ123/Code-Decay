using System.Collections.Generic;
using UnityEngine;
using com.AylanJ123.CodeDecay.Inventory;

namespace com.AylanJ123.CodeDecay.Managers
{
    /// <summary>
    /// A simple serializable class to hold the inventory data.
    /// The matrix is flattened into a single list for serialization and restored on deserialization.
    /// </summary>
    [System.Serializable]
    public class InventoryData
    {
        public List<int> itemIds;
        public List<int> hotbarItemIds;
    }

    /// <summary>
    /// Handles saving and loading the player's inventory using PlayerPrefs and JSON.
    /// This is a static helper class, designed to be called by other components.
    /// </summary>
    public static class InventoryPersistenceManager
    {
        private const string InventoryKey = "PlayerInventoryData";

        /// <summary>
        /// Saves the current state of the inventory to PlayerPrefs.
        /// </summary>
        /// <param name="mainInventory">The 2D array of the main inventory matrix.</param>
        /// <param name="hotbar">The array of hotbar slots.</param>
        public static void SaveInventory(InventorySlot[][] mainInventory, InventorySlot[] hotbar)
        {
            InventoryData inventoryData = new InventoryData
            {
                itemIds = new List<int>(),
                hotbarItemIds = new List<int>()
            };

            foreach (var row in mainInventory)
            {
                foreach (var slot in row)
                {
                    inventoryData.itemIds.Add(slot.itemData != null ? slot.itemData.itemId : -1);
                }
            }

            foreach (var slot in hotbar)
            {
                inventoryData.hotbarItemIds.Add(slot.itemData != null ? slot.itemData.itemId : -1);
            }

            string json = JsonUtility.ToJson(inventoryData);
            PlayerPrefs.SetString(InventoryKey, json);
            PlayerPrefs.Save();
            Debug.Log("Inventory saved to PlayerPrefs.");
        }

        /// <summary>
        /// Loads the inventory data from PlayerPrefs.
        /// </summary>
        /// <returns> An InventoryData object containing the saved IDs or null if no data exists. </returns>
        public static InventoryData LoadInventory()
        {
            if (!PlayerPrefs.HasKey(InventoryKey))
            {
                Debug.Log("No inventory data found. Starting with an empty inventory.");
                return null;
            }

            string json = PlayerPrefs.GetString(InventoryKey);
            InventoryData inventoryData = JsonUtility.FromJson<InventoryData>(json);

            Debug.Log("Inventory loaded from PlayerPrefs.");
            return inventoryData;
        }
    }
}