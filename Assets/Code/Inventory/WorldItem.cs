using UnityEngine;
using MyBox;

namespace com.AylanJ123.CodeDecay.Inventory
{
    /// <summary>
    /// Represents an item dropped in the game world, with a defined life cycle.
    /// </summary>
    public sealed class WorldItem : MonoBehaviour
    {
        [Tooltip("The data of the item this object represents.")]
        [SerializeField, ReadOnly]
        private ItemData itemData;

        [Tooltip("The transform of the image that will be colored.")]
        [SerializeField, InitializationField, MustBeAssigned]
        private Transform colorImageTransform;

        [Tooltip("The transform of the image that will not be colored.")]
        [SerializeField, InitializationField, MustBeAssigned]
        private Transform mainImageTransform;

        [Tooltip("The time in seconds before the item can be picked up.")]
        [SerializeField, InitializationField, Min(0.1f)]
        private float pickupDelay = 0.5f;

        [Tooltip("The life span in seconds for dropped items.")]
        [SerializeField, InitializationField, Min(1f)]
        private float droppedLifeSpan = 30f;

        [Tooltip("The life span in seconds for items naturally spawned in the world.")]
        [SerializeField, InitializationField, Min(1f)]
        private float worldLifeSpan = 60f;

        /// <summary> Is the item currently being picked up? </summary>
        public bool IsBeingPickedUp { get; private set; }
        private Rigidbody rb;

        public ItemData ItemData => itemData;

        public float PickUpAfter { get; private set; }
        private float despawnTime;

        private void Awake()
        {
            rb = GetComponent<Rigidbody>();
        }

        private void Start()
        {
            PickUpAfter = Time.time + pickupDelay;
        }

        private void Update()
        {
            if (IsBeingPickedUp) return;
            if (Time.time > despawnTime)
            {
                IsBeingPickedUp = true;
                Destroy(gameObject);
            }
        }

        /// <summary> Initializes the item with specific ItemData and context </summary>
        /// <param name="data"> The item data </param>
        /// <param name="isDroppedByPlayer"> Was dropped by the player? </param>
        public void Initialize(ItemData data, bool isDroppedByPlayer = false)
        {
            itemData = data;
            despawnTime = Time.time + (isDroppedByPlayer ? droppedLifeSpan : worldLifeSpan);
            SpriteRenderer srColor = colorImageTransform
                .GetComponentInChildren<SpriteRenderer>();
            SpriteRenderer sr = mainImageTransform
                .GetComponentInChildren<SpriteRenderer>();
            srColor.color = itemData.highlightColor;
            srColor.sprite = itemData.iconDetail;
            sr.sprite = itemData.icon;
        }

        /// <summary> Starts the pickup process, preventing further interaction </summary>
        public void StartPickup()
        {
            IsBeingPickedUp = true;
        }

        /// <summary>
        /// Applies a force to the rigidbody
        /// </summary>
        /// <param name="force"> The force to be applied </param>
        public void ApplyForce(Vector3 force)
        {
            rb.AddForce(force, ForceMode.Impulse);
        }
    }
}
