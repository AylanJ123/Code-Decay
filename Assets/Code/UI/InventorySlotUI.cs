using com.AylanJ123.CodeDecay.Inventory;
using com.AylanJ123.CodeDecay.UI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using MyBox;

namespace com.AylanJ123.CodeDecay.Player
{
    /// <summary>
    /// Handles the visual representation and user interaction for a single inventory slot.
    /// It manages UI updates, tooltips, and drag-and-drop functionality.
    /// </summary>
    public class InventorySlotUI : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler, IPointerEnterHandler, IPointerExitHandler
    {
        [Tooltip("The image component for the main item icon")]
        [SerializeField, InitializationField, MustBeAssigned]
        private Image iconImage;
        [Tooltip("The image component for the item detail icon")]
        [SerializeField, InitializationField, MustBeAssigned]
        private Image detailImage;
        [Tooltip("The panel that highlights the slot when it's hovered over")]
        [SerializeField, InitializationField, MustBeAssigned]
        private Image highlightPanel;

        [Tooltip("The prefab for the item being dragged")]
        [SerializeField, InitializationField, MustBeAssigned]
        private GameObject dragItemUIPrefab;

        private PlayerInventoryUI parentUI;
        private ItemData itemData;
        private Vector2Int slotIndex;
        private SlotType slotType;

        private DragItemUI currentDragUI;
        private RectTransform rectTransform;
        private bool isDragging;

        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
        }

        /// <summary> Initializes the slot with a reference to its parent UI, its index and type </summary>
        /// <param name="parentUI"> The parent inventory UI manager </param>
        /// <param name="index"> The coordinates of this slot in the inventory matrix </param>
        /// <param name="type"> The type of this slot </param>
        public void Initialize(PlayerInventoryUI parentUI, Vector2Int index, Inventory.SlotType type)
        {
            this.parentUI = parentUI;
            slotIndex = index;
            slotType = type;
            UpdateSlotUI(null);
        }

        /// <summary> Updates the UI to show the provided item data </summary>
        /// <param name="data"> The ItemData to display </param>
        public void UpdateSlotUI(ItemData data)
        {
            itemData = data;

            if (itemData != null)
            {
                iconImage.sprite = itemData.icon;
                iconImage.enabled = true;

                if (detailImage != null)
                {
                    detailImage.sprite = itemData.iconDetail;
                    detailImage.color = itemData.highlightColor;
                    detailImage.enabled = true;
                }
            }
            else
            {
                iconImage.enabled = false;
                if (detailImage != null)
                {
                    detailImage.enabled = false;
                }
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (itemData == null) return;

            parentUI.ShowTooltip(itemData);
            highlightPanel.color = new Color32(255, 255, 255, 100);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (itemData == null) return;

            parentUI.HideTooltip();
            highlightPanel.color = new Color32(255, 255, 255, 255);
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (itemData == null) return;

            isDragging = true;

            // Create a temporary UI element for dragging
            GameObject newObj = Instantiate(dragItemUIPrefab, parentUI.transform.parent);
            currentDragUI = newObj.GetComponent<DragItemUI>();
            currentDragUI.Initialize(itemData);

            // Disable raycast target so drop events can pass through
            currentDragUI.GetComponent<CanvasGroup>().blocksRaycasts = false;

            // Hide the original slot's images while dragging
            iconImage.enabled = false;
            if (detailImage != null) detailImage.enabled = false;
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (!isDragging || currentDragUI == null) return;

            currentDragUI.transform.position = eventData.position;
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (!isDragging) return;

            // Check if an item was dropped on a valid slot
            if (eventData.pointerEnter == null || eventData.pointerEnter.GetComponentInParent<InventorySlotUI>() == null)
            {
                // If not, eject the item to the world
                parentUI.EjectItem(slotIndex, slotType);
            }

            // Destroy the temporary drag item UI
            if (currentDragUI != null)
            {
                Destroy(currentDragUI.gameObject);
                currentDragUI = null;
            }

            isDragging = false;
        }

        public void OnDrop(PointerEventData eventData)
        {
            InventorySlotUI droppedSlot = eventData.pointerDrag.GetComponentInParent<InventorySlotUI>();
            if (droppedSlot == null || droppedSlot == this) return;

            if (slotType == SlotType.Deletion)
            {
                parentUI.DeleteItem(droppedSlot.slotIndex, droppedSlot.slotType);
                return;
            }

            parentUI.SwapItems(droppedSlot.slotIndex, droppedSlot.slotType, slotIndex, slotType);
        }
    }
}
