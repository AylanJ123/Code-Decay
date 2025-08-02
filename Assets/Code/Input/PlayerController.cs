using com.AylanJ123.CodeDecay.Inventory;
using com.AylanJ123.CodeDecay.Player;
using MyBox;
using UnityEngine;

namespace com.AylanJ123.CodeDecay.Input
{
    /// <summary>
    /// Handles player movement and camera look, and processes input actions.
    /// This script is the central hub for player controls, but it delegates
    /// functionality to other components for easier coding.
    /// </summary>
    public sealed class PlayerController : MonoBehaviour
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
        private PlayerCombat playerCombat;
        private PlayerHealth playerHealth;

        // Character controller component
        private CharacterController characterController;

        private GameActions gameActions;
        private Vector2 moveInput;
        private Vector2 lookInput;
        private float verticalLookRotation;

        private void Awake()
        {
            playerCombat = GetComponent<PlayerCombat>();
            playerHealth = GetComponent<PlayerHealth>();
            characterController = GetComponent<CharacterController>();

            gameActions = new GameActions();

            // Movement actions
            gameActions.Gameplay.Move.performed += ctx => moveInput = ctx.ReadValue<Vector2>();
            gameActions.Gameplay.Move.canceled += ctx => moveInput = Vector2.zero;

            gameActions.Gameplay.Look.performed += ctx => lookInput = ctx.ReadValue<Vector2>();
            gameActions.Gameplay.Look.canceled += ctx => lookInput = Vector2.zero;

            // Firing action
            //gameActions.Gameplay.Fire.performed += _ => playerCombat.Fire();

            // Potion actions
            gameActions.Gameplay.DrinkSlot1.performed += _ => PlayerInventory.Instance.UseActivePotion(0);
            gameActions.Gameplay.DrinkSlot2.performed += _ => PlayerInventory.Instance.UseActivePotion(1);

            // Inventory UI action
            gameActions.Gameplay.OpenInventory.performed += _ =>
            {
                //PlayerUI.ToggleInventory();
                Debug.Log("Toggle Inventory UI");
            };
        }

        private void OnEnable() => gameActions.Gameplay.Enable();

        private void OnDisable() => gameActions.Gameplay.Disable();

        private void Update()
        {
            HandleMovement();
            HandleLook();
        }

        /// <summary> Handles the player's movement based on input </summary>
        private void HandleMovement()
        {
            Vector3 movement = new(moveInput.x, 0, moveInput.y);
            characterController.Move(movement.z * moveSpeed * Time.deltaTime * transform.forward);
            characterController.Move(movement.x * moveSpeed * Time.deltaTime * transform.right);
        }

        /// <summary> Handles the camera rotation based on mouse input </summary>
        private void HandleLook()
        {
            transform.Rotate(Vector3.up, lookInput.x * lookSpeed);
            verticalLookRotation -= lookInput.y * lookSpeed;
            verticalLookRotation = Mathf.Clamp(verticalLookRotation, -90f, 90f);
            cameraParent.localRotation = Quaternion.Euler(verticalLookRotation, 0, 0);
        }
    }
}