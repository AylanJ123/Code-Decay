using UnityEngine;
using MyBox;
using System;
using UnityEngine.Events;

namespace com.AylanJ123.CodeDecay.Player
{
    [Serializable]
    public class PlayerUI
    {
        [Tooltip("The Canvas Group of the inventory UI")]
        [SerializeField, InitializationField, MustBeAssigned]
        private CanvasGroup inventoryCanvas;

        public UnityEvent OnInventoryOpened;
        public UnityEvent OnInventoryClosed;

        public void ToggleInventory()
        {
            bool isVisible = inventoryCanvas.alpha > 0;

            if (isVisible)
            {
                inventoryCanvas.alpha = 0;
                inventoryCanvas.interactable = false;
                inventoryCanvas.blocksRaycasts = false;
                SetCursorState(true);
                OnInventoryClosed?.Invoke();
            }
            else
            {
                inventoryCanvas.alpha = 1;
                inventoryCanvas.interactable = true;
                inventoryCanvas.blocksRaycasts = true;
                SetCursorState(false);
                OnInventoryOpened?.Invoke();
            }
        }

        public void SetCursorState(bool locked)
        {
            if (locked)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
            else
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
        }
    }
}
