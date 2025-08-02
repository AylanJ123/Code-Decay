using com.AylanJ123.CodeDecay.Inventory;
using com.AylanJ123.CodeDecay.Player;
using MyBox;
using UnityEngine;
using UnityEngine.InputSystem;

namespace com.AylanJ123.CodeDecay.Input
{
    public sealed class PlayerController : MonoBehaviour, IHealthStats, ICombatStats, ISpeedStats, ICooldownStats, IStats
    {
        [SerializeField, InitializationField, Min(0.1f)]
        [Tooltip("The speed at which the player moves per second")]
        private float moveSpeed = 5f;

        [SerializeField, InitializationField, Min(0.1f)]
        [Tooltip("The speed at which the player looks around")]
        private float lookSpeed = 1f;

        [SerializeField, InitializationField, MustBeAssigned]
        [Tooltip("The camera's inmediate parent")]
        private Transform cameraParent;

        // Decoupled components
        [field: SerializeField] public PlayerCombat PlayerCombat { get; private set; }
        [field: SerializeField] public PlayerHealth PlayerHealth { get; private set; }
        [field: SerializeField] public PlayerUI PlayerUI { get; private set; }

        // Character controller component
        private CharacterController characterController;

        // Game Actions Input System
        private GameActions gameActions;
        private Vector2 moveInput;
        private Vector2 lookInput;
        private float verticalLookRotation;
        private float yVelocity;
        private bool isFiring;
        private bool isHandlingWithMouse;
        private bool isDead;

        // Temporal status
        [SerializeField, ReadOnly, Foldout("Modifiers", true)]
        private float tempDamageModifier;
        [SerializeField, ReadOnly] private float tempSpeedModifier;
        [SerializeField, ReadOnly] private float tempCooldownModifier;

        [Header("Death and Respawn")]
        [Tooltip("The prefab for the item dropped in the world")]
        [SerializeField, MustBeAssigned]
        private GameObject worldItemPrefab;
        [Tooltip("The forward force applied to ejected items")]
        [SerializeField]
        private float ejectionForwardForce = 5f;
        [Tooltip("The upward force applied to ejected items")]
        [SerializeField]
        private float ejectionUpwardForce = 5f;
        [Tooltip("The vertical position for player respawn")]
        [SerializeField]
        private float respawnHeight = 2f;

        private void Awake()
        {
            PlayerHealth.Initialize();
            PlayerCombat.Initialize();
            characterController = GetComponent<CharacterController>();
            gameActions = new GameActions();

            // Subscribe to the player's death event
            PlayerHealth.OnDeath.AddListener(OnPlayerDeath);

            // Movement actions
            gameActions.Gameplay.Move.performed += ctx => moveInput = ctx.ReadValue<Vector2>();
            gameActions.Gameplay.Move.canceled += ctx => moveInput = Vector2.zero;

            gameActions.Gameplay.Look.performed += ctx => lookInput = ctx.ReadValue<Vector2>();
            gameActions.Gameplay.Look.canceled += ctx => lookInput = Vector2.zero;

            // Firing action
            gameActions.Gameplay.Fire.performed += _ => isFiring = true;
            gameActions.Gameplay.Fire.canceled += _ => isFiring = false;

            // Potion actions
            gameActions.Gameplay.DrinkSlot1.performed += _ => PlayerInventory.Instance.UseActivePotion(0);
            gameActions.Gameplay.DrinkSlot2.performed += _ => PlayerInventory.Instance.UseActivePotion(1);

            // Inventory UI action
            gameActions.Gameplay.OpenInventory.performed += _ =>
            {
                if (isDead) return;
                isHandlingWithMouse = !isHandlingWithMouse;
                if (isHandlingWithMouse) isFiring = false;
                PlayerUI.ToggleInventory();
            };
        }

        private void OnEnable()
        {
            gameActions.Gameplay.Enable();
            Cursor.lockState = CursorLockMode.Locked;
        }

        private void OnDisable()
        {
            gameActions.Gameplay.Disable();
            Cursor.lockState = CursorLockMode.None;
        }

        private void Update()
        {
            if (isHandlingWithMouse || isDead) return;
            HandleMovement();
            HandleLook();
            if (isFiring) PlayerCombat.Fire();
        }

        /// <summary>
        /// Handles the player's movement based on input.
        /// </summary>
        private void HandleMovement()
        {
            Vector3 movement = new(moveInput.x, 0, moveInput.y);

            // Apply gravity
            if (characterController.isGrounded) yVelocity = Physics.gravity.y * Time.deltaTime;
            else yVelocity += Physics.gravity.y * Time.deltaTime;

            float modSpeed = Mathf.Max(moveSpeed + tempSpeedModifier, .1f);
            characterController.Move(movement.z * modSpeed * Time.deltaTime * transform.forward);
            characterController.Move(movement.x * modSpeed * Time.deltaTime * transform.right);

            characterController.Move(yVelocity * Time.deltaTime * Vector3.up);
        }

        /// <summary>
        /// Handles the camera rotation based on mouse input.
        /// </summary>
        private void HandleLook()
        {
            transform.Rotate(Vector3.up, lookInput.x * lookSpeed);
            verticalLookRotation -= lookInput.y * lookSpeed;
            verticalLookRotation = Mathf.Clamp(verticalLookRotation, -90f, 90f);
            cameraParent.localRotation = Quaternion.Euler(verticalLookRotation, 0, 0);
        }

        [ButtonMethod]
        private void Die()
        {
            PlayerHealth.Die();
        }

        /// <summary>
        /// Responds to the player's death event.
        /// </summary>
        private void OnPlayerDeath()
        {
            isDead = true;
            Debug.Log("Player has died. Respawning...");

            // Teleport the player back to the origin
            characterController.enabled = false;
            transform.position = new Vector3(0, respawnHeight, 0);
            characterController.enabled = true;

            // Reset health and stats
            PlayerHealth.Initialize();
            ResetStats();

            // Handle inventory items
            HandleInventoryOnDeath();

            isDead = false;
        }

        /// <summary>
        /// Resets all temporary stat modifiers to zero.
        /// </summary>
        private void ResetStats()
        {
            tempDamageModifier = 0;
            tempSpeedModifier = 0;
            tempCooldownModifier = 0;
            PlayerCombat.UpdateDamage(tempDamageModifier);
            PlayerCombat.UpdateCooldown(tempCooldownModifier);
        }

        /// <summary>
        /// Iterates through the inventory and handles item deletion or ejection.
        /// </summary>
        private void HandleInventoryOnDeath()
        {
            // Handle main inventory
            Vector2Int size = PlayerInventory.Instance.GetSize();
            for (int x = 0; x < size.x; x++)
            {
                for (int y = 0; y < size.y; y++)
                {
                    Vector2Int index = new(x, y);
                    if (PlayerInventory.Instance.GetItem(index) != null)
                    {
                        if (Random.value <= 0.15f)
                        {
                            // 15% chance to be deleted
                            PlayerInventory.Instance.RemoveItem(index, Inventory.SlotType.Any);
                        }
                        else
                        {
                            // Eject the item
                            ItemData ejectedItem = PlayerInventory.Instance.RemoveItem(index, Inventory.SlotType.Any);
                            if (ejectedItem != null)
                            {
                                EjectItemToWorld(ejectedItem);
                            }
                        }
                    }
                }
            }

            // Handle hotbar slots
            for (int i = 0; i < 2; i++)
            {
                if (PlayerInventory.Instance.GetHotbarItem(i) != null)
                {
                    if (Random.value <= 0.15f)
                    {
                        // 15% chance to be deleted
                        PlayerInventory.Instance.RemoveItem(new Vector2Int(i, -1), Inventory.SlotType.Potion);
                    }
                    else
                    {
                        // Eject the item
                        ItemData ejectedItem = PlayerInventory.Instance.RemoveItem(new Vector2Int(i, -1), Inventory.SlotType.Potion);
                        if (ejectedItem != null)
                        {
                            EjectItemToWorld(ejectedItem);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Spawns a WorldItem prefab with an applied force.
        /// </summary>
        /// <param name="itemData"> The data of the item to eject. </param>
        private void EjectItemToWorld(ItemData itemData)
        {
            GameObject newObj = Instantiate(worldItemPrefab, transform.position, Quaternion.identity);
            WorldItem newWorldItem = newObj.GetComponent<WorldItem>();
            newWorldItem.transform.position = newWorldItem.transform.position + new Vector3(Random.Range(-3, 3), newWorldItem.transform.position.y + Random.Range(0, 3), Random.Range(-3, 3));
            newWorldItem.Initialize(itemData, true);

            Rigidbody rb = newObj.GetComponent<Rigidbody>();
            if (rb != null)
            {
                // Yeet the item forward and upward
                Vector3 yeetDirection = transform.forward * ejectionForwardForce + Vector3.up * ejectionUpwardForce;
                rb.AddForce(yeetDirection, ForceMode.Impulse);
            }
            else
            {
                Debug.LogWarning("WorldItem prefab is missing a Rigidbody component!");
            }
        }

        public void Heal(float amount)
        {
            PlayerHealth.Heal(amount);
        }

        public void Damage(float amount)
        {
            PlayerHealth.Damage(amount);
        }

        public void ApplyDamageModification(float amount)
        {
            tempDamageModifier += amount;
            PlayerCombat.UpdateDamage(tempDamageModifier);
        }

        public void RemoveDamageModification(float amount)
        {
            tempDamageModifier -= amount;
            PlayerCombat.UpdateDamage(tempDamageModifier);
        }

        public void ApplySpeedModification(float amount)
        {
            tempSpeedModifier += amount;
        }

        public void RemoveSpeedModification(float amount)
        {
            tempSpeedModifier -= amount;
        }

        public void ApplyCooldownModification(float amount)
        {
            tempCooldownModifier += amount;
            PlayerCombat.UpdateCooldown(tempCooldownModifier);
        }

        public void RemoveCooldownModification(float amount)
        {
            tempCooldownModifier -= amount;
            PlayerCombat.UpdateCooldown(tempCooldownModifier);
        }

        public void Cleanse()
        {
            tempCooldownModifier = tempDamageModifier = tempSpeedModifier = 0;
            PlayerCombat.UpdateCooldown(tempCooldownModifier);
            PlayerCombat.UpdateDamage(tempDamageModifier);
        }
    }
}
