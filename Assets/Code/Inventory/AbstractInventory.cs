using UnityEngine;
using UnityEngine.Events;
using MyBox;

namespace com.AylanJ123.CodeDecay.Inventory
{
    /// <summary> Abstract base class for all inventory types, manages raw slots </summary>
    public abstract class AbstractInventory : MonoBehaviour
    {
        [SerializeField, ReadOnly, Header("Inventory Data")]
        protected InventorySlot[][] matrix;

        [SerializeField, InitializationField]
        protected Vector2Int size;

        public UnityEvent OnInventoryUpdated;

        protected virtual void Awake()
        {
            matrix = new InventorySlot[size.x][];
            for (int i = 0; i < size.x; i++)
            {
                matrix[i] = new InventorySlot[size.y];
                for (int j = 0; j < size.y; j++)
                {
                    matrix[i][j] = new InventorySlot();
                }
            }
        }

        /// <summary> Adds an item to the inventory </summary>
        /// <param name="itemData"> The ItemData to add </param>
        /// <returns> True if the item was added, false otherwise </returns>
        public virtual bool AddItem(ItemData itemData)
        {
            foreach (InventorySlot[] row in matrix)
            {
                foreach (InventorySlot slot in row)
                {
                    if (slot.IsEmpty && slot.CanStoreItem(itemData))
                    {
                        slot.itemData = itemData;
                        OnInventoryUpdated.Invoke();
                        return true;
                    }
                }
            }
            Debug.Log("Inventory is full.");
            return false;
        }

        /// <summary> Removes an item from a specific slot </summary>
        /// <param name="indexData"> The index of the slot to remove the item from </param>
        public virtual void RemoveItem(Vector2Int indexData)
        {
            matrix[indexData.x][indexData.y].ClearSlot();
            OnInventoryUpdated.Invoke();
        }
    }
}