using com.AylanJ123.CodeDecay.Managers;
using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;

namespace com.AylanJ123.CodeDecay.Inventory
{
    /// <summary>
    /// The player's specific inventory implementation.
    /// It inherits from AbstractInventory and contains special slots for
    /// deletion and potions, separate from the main matrix.
    /// </summary>
    public sealed class PlayerInventory : AbstractInventory
    {
        public static PlayerInventory Instance { get; private set; }

        [Header("Special Slots")]
        [SerializeField]
        [Tooltip("The exclusive slot for destroying items.")]
        private InventorySlot deleteSlot;

        [SerializeField]
        [Tooltip("Slots for active potions (useable via hotkeys).")]
        private InventorySlot[] activePotionSlots = new InventorySlot[2];

        protected override void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
                return;
            }

            base.Awake();
            deleteSlot.slotType = SlotType.Deletion;
            foreach (InventorySlot slot in activePotionSlots)
                slot.slotType = SlotType.Potion;
        }

        private void Start()
        {
            LoadInventory();
            OnInventoryUpdated?.Invoke();
        }

        private void OnApplicationQuit()
        {
            SaveInventory();
            OnInventoryUpdated?.Invoke();
        }

        private void SaveInventory()
        {
            InventoryPersistenceManager.SaveInventory(matrix, activePotionSlots);
        }

        private void LoadInventory()
        {
            InventoryData loadedData = InventoryPersistenceManager.LoadInventory();
            if (loadedData == null) return;

            // Clear current inventory to load the new one
            for (int x = 0; x < size.x; x++)
            {
                for (int y = 0; y < size.y; y++)
                {
                    matrix[x][y].ClearSlot();
                }
            }
            foreach (var slot in activePotionSlots)
            {
                slot.ClearSlot();
            }

            // Load the main inventory
            int index = 0;
            for (int x = 0; x < size.x; x++)
            {
                for (int y = 0; y < size.y; y++)
                {
                    int itemId = loadedData.itemIds[index];
                    if (itemId != -1)
                    {
                        ItemData itemToLoad = ItemDatabase.Instance.GetItemById(itemId);
                        if (itemToLoad != null)
                        {
                            matrix[x][y].itemData = itemToLoad;
                        }
                    }
                    index++;
                }
            }

            // Load the hotbar
            for (int i = 0; i < activePotionSlots.Length; i++)
            {
                int itemId = loadedData.hotbarItemIds[i];
                if (itemId != -1)
                {
                    ItemData itemToLoad = ItemDatabase.Instance.GetItemById(itemId);
                    if (itemToLoad != null)
                    {
                        activePotionSlots[i].itemData = itemToLoad;
                    }
                }
            }

            OnInventoryUpdated?.Invoke();
        }

        /// <summary>
        /// Adds an item to the inventory.
        /// </summary>
        /// <returns> True if the item was added, false otherwise. </returns>
        public override bool AddItem(ItemData itemData)
        {
            bool added = base.AddItem(itemData);
            if (added)
            {
                OnInventoryUpdated?.Invoke();
            }
            return added;
        }

        /// <summary>
        /// Retrieves all items from the main inventory and active potion slots.
        /// </summary>
        /// <returns> A list of all ItemData objects in the inventory. </returns>
        public List<ItemData> GetAllItems()
        {
            List<ItemData> allItems = new List<ItemData>();
            for (int x = 0; x < size.x; x++)
            {
                for (int y = 0; y < size.y; y++)
                {
                    if (!matrix[x][y].IsEmpty)
                    {
                        allItems.Add(matrix[x][y].itemData);
                    }
                }
            }
            foreach (InventorySlot slot in activePotionSlots)
            {
                if (!slot.IsEmpty)
                {
                    allItems.Add(slot.itemData);
                }
            }
            return allItems;
        }

        /// <summary>
        /// Removes an item from a specific slot.
        /// </summary>
        /// <returns> The removed or consumed element. </returns>
        public ItemData RemoveItem(Vector2Int index, SlotType type)
        {
            InventorySlot slot = GetSlot(index, type);
            if (slot == null)
            {
                Debug.LogError($"Invalid slot coordinates: row={index.x}, column={index.y}, type={type}");
                return null;
            }

            if (slot.IsEmpty) return null;

            ItemData itemToRemove = slot.itemData;
            slot.ClearSlot();
            OnInventoryUpdated?.Invoke();
            return itemToRemove;
        }

        /// <summary>
        /// Removes a potion from an active slot and applies its effect.
        /// </summary>
        /// <param name="slotIndex"> The index of the active potion slot (0 or 1). </param>
        public void UseActivePotion(int slotIndex)
        {
            if (slotIndex >= 0 && slotIndex < activePotionSlots.Length)
            {
                InventorySlot slot = activePotionSlots[slotIndex];
                if (!slot.IsEmpty && slot.itemData is PotionData potion)
                {
                    // Call the effect manager to handle the timed effect
                    // playerEffectsManager.StartTimedEffect(potion);
                    slot.ClearSlot();
                    OnInventoryUpdated?.Invoke();
                }
            }
        }

        /// <summary> Swaps items between any two slots. </summary>
        public void SwapItems(Vector2Int fromIndex, SlotType fromType, Vector2Int toIndex, SlotType toType)
        {
            InventorySlot fromSlot = GetSlot(fromIndex, fromType);
            InventorySlot toSlot = GetSlot(toIndex, toType);

            if (fromSlot == null || toSlot == null)
            {
                Debug.LogError("One of the slots is invalid.");
                return;
            }

            // Check if items can be swapped
            if (!toSlot.CanStoreItem(fromSlot.itemData) || !fromSlot.CanStoreItem(toSlot.itemData))
            {
                Debug.LogError("Cannot swap items between these slot types.");
                return;
            }

            (toSlot.itemData, fromSlot.itemData) = (fromSlot.itemData, toSlot.itemData);
            OnInventoryUpdated?.Invoke();
        }

        public Vector2Int GetSize() => size;

        public ItemData GetItem(Vector2Int indexData)
        {
            return IsValidSlot(indexData) ? matrix[indexData.x][indexData.y].itemData : null;
        }

        public ItemData GetHotbarItem(int index)
        {
            return index >= 0 && index < activePotionSlots.Length ? activePotionSlots[index].itemData : null;
        }

        public ItemData GetTrashItem()
        {
            return deleteSlot.itemData;
        }

        private InventorySlot GetSlot(Vector2Int index, SlotType type)
        {
            return type switch
            {
                SlotType.Any when IsValidSlot(index) => matrix[index.x][index.y],
                SlotType.Potion when index.x >= 0 && index.x < activePotionSlots.Length => activePotionSlots[index.x],
                SlotType.Deletion => deleteSlot,
                _ => null
            };
        }

        private bool IsValidSlot(Vector2Int index)
        {
            return index.x >= 0 && index.x < size.x && index.y >= 0 && index.y < size.y;
        }
    }
}