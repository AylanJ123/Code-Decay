using UnityEngine;
using com.AylanJ123.CodeDecay.Inventory;
using MyBox;
using DG.Tweening;

namespace com.AylanJ123.CodeDecay.Player
{
    /// <summary>
    /// Handles the player's interaction with pickable items.
    /// This component is placed on the player and detects WorldItems in its trigger.
    /// </summary>
    [RequireComponent(typeof(BoxCollider))]
    public class PlayerItemPickup : MonoBehaviour
    {
        [Tooltip("The duration of the pickup animation")]
        [SerializeField, InitializationField, Min(0.1f)]
        private float pickupAnimationDuration = 0.2f;

        private void OnTriggerEnter(Collider other)
        {
            if (other.TryGetComponent(out WorldItem item) && !item.IsBeingPickedUp)
            {
                if (Time.time < item.PickUpAfter) return;
                bool inventoryFree = PlayerInventory.Instance.AddItem(item.ItemData);
                if (!inventoryFree) return;
                item.StartPickup();
                item.transform.DOJump(transform.position, .75f, 1, pickupAnimationDuration).SetEase(Ease.OutQuad).OnComplete(() =>
                {
                    Destroy(item.gameObject);
                }).Play();
            }
        }
    }
}