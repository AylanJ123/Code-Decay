using com.AylanJ123.CodeDecay.Player;
using UnityEngine;
using UnityEngine.Events;

namespace com.AylanJ123.CodeDecay.Inventory
{
    /// <summary>
    /// The player's specific inventory implementation.
    /// It inherits from AbstractInventory and contains special slots for
    /// deletion and potions, separate from the main matrix. This class
    /// is a Singleton to allow easy global access.
    /// </summary>
    public sealed class PlayerInventory : AbstractInventory
    {
        public static PlayerInventory Instance { get; private set; }

        [Header("Special Slots")]
        [SerializeField]
        [Tooltip("The exclusive slot for destroying items")]
        private InventorySlot deleteSlot;

        [SerializeField]
        [Tooltip("Slots for active potions (useable via hotkeys)")]
        private InventorySlot[] activePotionSlots = new InventorySlot[2];

        private PlayerEffectsManager playerEffectsManager;

        protected override void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this);
                return;
            }

            Instance = this;

            // Initialize the base matrix first
            base.Awake();

            // Find the PlayerEffectsManager component
            playerEffectsManager = GetComponent<PlayerEffectsManager>();

            // Configure the slot types for the special slots
            if (deleteSlot != null)
            {
                deleteSlot.slotType = SlotType.Deletion;
            }

            foreach (InventorySlot slot in activePotionSlots)
            {
                if (slot != null)
                {
                    slot.slotType = SlotType.Potion;
                }
            }
        }

        /// <summary>
        /// Adds an item to the inventory and applies its effects if it's an upgrade.
        /// </summary>
        /// <param name="itemData">The ItemData to add.</param>
        /// <returns>True if the item was added, false otherwise.</returns>
        public override bool AddItem(ItemData itemData)
        {
            if (base.AddItem(itemData))
            {
                if (itemData is UpgradeData)
                {
                    itemData.Use(gameObject);
                }
                OnInventoryUpdated?.Invoke();
                return true;
            }
            return false;
        }

        /// <summary>
        /// Removes an item from a specific slot.
        /// </summary>
        /// <param name="indexData">The matrix coordinates of the slot to remove the item from.</param>
        public override void RemoveItem(Vector2Int indexData)
        {
            if (indexData.x >= 0 && indexData.x < size.x && indexData.y >= 0 && indexData.y < size.y && !matrix[indexData.x][indexData.y].IsEmpty)
            {
                ItemData itemToRemove = matrix[indexData.x][indexData.y].itemData;

                if (itemToRemove is UpgradeData)
                {
                    itemToRemove.Remove(gameObject);
                }

                matrix[indexData.x][indexData.y].ClearSlot();
                OnInventoryUpdated?.Invoke();
            }
            else
            {
                Debug.LogError($"Invalid slot coordinates: row={indexData.x}, column={indexData.y}.");
            }
        }

        /// <summary>
        /// Removes a potion from an active slot and applies its effect.
        /// </summary>
        /// <param name="slotIndex">The index of the active potion slot (0 or 1).</param>
        public void UseActivePotion(int slotIndex)
        {
            if (slotIndex >= 0 && slotIndex < activePotionSlots.Length)
            {
                InventorySlot slot = activePotionSlots[slotIndex];

                if (!slot.IsEmpty && slot.itemData is PotionData potion)
                {
                    playerEffectsManager.StartTimedEffect(potion);
                    slot.ClearSlot();
                    OnInventoryUpdated?.Invoke();
                }
            }
        }
    }
}
