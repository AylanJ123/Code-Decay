using com.AylanJ123.CodeDecay.Inventory;
using com.AylanJ123.CodeDecay.Player;
using MyBox;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

namespace com.AylanJ123.CodeDecay.UI
{
    /// <summary>
    /// Manages the player's inventory UI, creating and updating the slots.
    /// It acts as the View and Controller for the inventory system.
    /// </summary>
    public sealed class PlayerInventoryUI : MonoBehaviour
    {
        [Header("UI References")]
        [Tooltip("The parent transform for all inventory slot UIs")]
        [SerializeField, InitializationField, MustBeAssigned]
        private Transform inventorySlotsParent;

        [Tooltip("The parent transform for the hotbar slots")]
        [SerializeField, InitializationField, MustBeAssigned]
        private Transform hotbarSlotsParent;

        [Tooltip("The single slot UI for deleting items")]
        [SerializeField, InitializationField, MustBeAssigned]
        private GameObject trashSlotUI;

        [Tooltip("The prefab for a single inventory slot UI element")]
        [SerializeField, InitializationField, MustBeAssigned]
        private GameObject slotUIPrefab;

        [Header("Tooltip Settings")]
        [Tooltip("The parent transform for the tooltip UI")]
        [SerializeField, InitializationField, MustBeAssigned]
        private RectTransform tooltipPanel;
        [Tooltip("The Canvas Group component of the tooltip")]
        [SerializeField, InitializationField, MustBeAssigned]
        private CanvasGroup tooltipCanvasGroup;
        [Tooltip("The TextMeshProUGUI component of the tooltip")]
        [SerializeField, InitializationField, MustBeAssigned]
        private TextMeshProUGUI tooltipText;
        [Tooltip("The horizontal and vertical offset for the tooltip position")]
        [SerializeField]
        private Vector2 tooltipOffset = new(10f, -10f);

        private InventorySlotUI[][] slotUIMatrix;
        private InventorySlotUI[] hotbarSlotUIs;
        private InventorySlotUI trashSlotUIComponent;

        [Header("World References")]
        [Tooltip("The WorldItem prefab that is instantiated when an item is ejected")]
        [SerializeField, InitializationField, MustBeAssigned]
        private GameObject worldItemPrefab;
        [Tooltip("The transform to spawn ejected items from")]
        [SerializeField, InitializationField, MustBeAssigned]
        private Transform playerTransform;

        private void Start()
        {
            PlayerInventory.Instance.OnInventoryUpdated.AddListener(UpdateUI);
            CreateAllSlots();
            tooltipCanvasGroup.alpha = 0; // Ocultar el tooltip al inicio usando el CanvasGroup
            tooltipCanvasGroup.blocksRaycasts = false;
        }

        private void OnDestroy()
        {
            if (PlayerInventory.Instance != null)
            {
                PlayerInventory.Instance.OnInventoryUpdated.RemoveListener(UpdateUI);
            }
        }

        private void Update()
        {
            // The tooltip's position is updated only if it's visible.
            if (tooltipCanvasGroup.alpha > 0)
            {
                UpdateTooltipPosition();
            }
        }

        /// <summary> Creates all UI slots: the main inventory, the hotbar, and the trash slot </summary>
        private void CreateAllSlots()
        {
            CreateInventorySlots();
            FindAndInitializeHotbarSlots();
            FindAndInitializeTrashSlot();
        }

        /// <summary> Creates the UI slots to match the inventory's size </summary>
        private void CreateInventorySlots()
        {
            Vector2Int size = PlayerInventory.Instance.GetSize();
            slotUIMatrix = new InventorySlotUI[size.x][];

            for (int i = 0; i < size.x; i++)
            {
                slotUIMatrix[i] = new InventorySlotUI[size.y];
                for (int j = 0; j < size.y; j++)
                {
                    GameObject newObj = Instantiate(slotUIPrefab, inventorySlotsParent);
                    InventorySlotUI newSlotUI = newObj.GetComponent<InventorySlotUI>();
                    newSlotUI.Initialize(this, new Vector2Int(i, j), Inventory.SlotType.Any);
                    slotUIMatrix[i][j] = newSlotUI;
                }
            }
        }

        /// <summary> Finds and initializes the existing hotbar slots </summary>
        private void FindAndInitializeHotbarSlots()
        {
            hotbarSlotUIs = hotbarSlotsParent.GetComponentsInChildren<InventorySlotUI>(true);
            for (int i = 0; i < hotbarSlotUIs.Length; i++)
            {
                hotbarSlotUIs[i].Initialize(this, new Vector2Int(i, -1), Inventory.SlotType.Potion);
            }
        }

        /// <summary> Finds and initializes the existing trash slot </summary>
        private void FindAndInitializeTrashSlot()
        {
            trashSlotUIComponent = trashSlotUI.GetComponent<InventorySlotUI>();
            trashSlotUIComponent.Initialize(this, Vector2Int.zero, SlotType.Deletion);
        }

        /// <summary> Updates the entire inventory UI to reflect the current state of the player's inventory </summary>
        private void UpdateUI()
        {
            Vector2Int size = PlayerInventory.Instance.GetSize();
            for (int i = 0; i < size.x; i++)
            {
                for (int j = 0; j < size.y; j++)
                {
                    ItemData itemData = PlayerInventory.Instance.GetItem(new Vector2Int(i, j));
                    slotUIMatrix[i][j].UpdateSlotUI(itemData);
                }
            }

            for (int i = 0; i < hotbarSlotUIs.Length; i++)
            {
                ItemData itemData = PlayerInventory.Instance.GetHotbarItem(i);
                hotbarSlotUIs[i].UpdateSlotUI(itemData);
            }

            trashSlotUIComponent.UpdateSlotUI(PlayerInventory.Instance.GetTrashItem());
        }

        /// <summary> Displays the tooltip for an item </summary>
        /// <param name="data"> The ItemData to display </param>
        public void ShowTooltip(ItemData data)
        {
            tooltipText.text = $"<b>{data.itemName}</b>\n{data.description}";
            tooltipCanvasGroup.alpha = 1;
        }

        /// <summary> Hides the tooltip panel </summary>
        public void HideTooltip()
        {
            tooltipCanvasGroup.alpha = 0;
        }

        /// <summary> Updates the tooltip position based on the cursor location </summary>
        private void UpdateTooltipPosition()
        {
            if (Mouse.current == null) return;
            Vector2 mousePosition = Mouse.current.position.ReadValue();

            // Get the RectTransform of the parent Canvas to convert mouse position to local point.
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(tooltipPanel.parent as RectTransform, mousePosition, null, out Vector2 localPoint))
            {
                return;
            }

            // Adjust the position based on the cursor and the size of the tooltip panel
            // to prevent it from going off-screen.
            float pivotX = (mousePosition.x > Screen.width / 2) ? 1 : 0;
            float pivotY = (mousePosition.y > Screen.height / 2) ? 0 : 1;
            tooltipPanel.pivot = new Vector2(pivotX, pivotY);

            tooltipPanel.anchoredPosition = localPoint + tooltipOffset * new Vector2(pivotX == 1 ? -1 : 1, pivotY == 1 ? -1 : 1);
        }

        /// <summary> Swaps the items between two slots based on UI interaction </summary>
        /// <param name="fromIndex"> The coordinates of the source slot </param>
        /// <param name="fromType"> The type of the source slot </param>
        /// <param name="toIndex"> The coordinates of the destination slot </param>
        /// <param name="toType"> The type of the destination slot </param>
        public void SwapItems(Vector2Int fromIndex, SlotType fromType, Vector2Int toIndex, SlotType toType)
        {
            PlayerInventory.Instance.SwapItems(fromIndex, fromType, toIndex, toType);
        }

        /// <summary> Deletes the item from the specified slot </summary>
        /// <param name="index"> The coordinates of the slot </param>
        /// <param name="type"> The type of the slot </param>
        public void DeleteItem(Vector2Int index, SlotType type)
        {
            PlayerInventory.Instance.RemoveItem(index, type);
        }

        /// <summary> Ejects an item from the inventory and spawns it in the world </summary>
        /// <param name="index"> The coordinates of the slot </param>
        /// <param name="type"> The type of the slot </param>
        public void EjectItem(Vector2Int index, Inventory.SlotType type)
        {
            ItemData ejectedItemData = PlayerInventory.Instance.RemoveItem(index, type);
            if (ejectedItemData != null)
            {
                GameObject newObj = Instantiate(worldItemPrefab, playerTransform.position, Quaternion.identity);
                WorldItem newWorldItem = newObj.GetComponent<WorldItem>();
                newWorldItem.Initialize(ejectedItemData);
                newWorldItem.transform.position = newWorldItem.transform.position + new Vector3(Random.Range(-1, 1), newWorldItem.transform.position.y, Random.Range(-1, 1));
            }
        }
    }
}