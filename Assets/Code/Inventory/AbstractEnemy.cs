using System.Collections;
using com.AylanJ123.CodeDecay.Inventory;
using com.AylanJ123.CodeDecay.Player;
using UnityEngine;

namespace com.AylanJ123.CodeDecay.Enemies
{
    /// <summary>
    /// A simplified enemy that moves, attacks the player, and drops items on death.
    /// This enemy uses a CharacterController for movement and a simple
    /// DotProduct and Raycast check for attacking.
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public sealed class AbstractEnemy : MonoBehaviour, IHealthStats
    {
        [Header("Movement")]
        [Tooltip("The CharacterController component for movement")]
        [SerializeField]
        private CharacterController controller;
        [Tooltip("The speed at which the enemy moves")]
        [SerializeField]
        private float moveSpeed = 3.0f;
        [Tooltip("The maximum distance from the player to start moving")]
        [SerializeField]
        private float followDistance = 10.0f;
        [Tooltip("The force of gravity on the enemy")]
        [SerializeField]
        private float gravity = -9.81f;

        [Header("Combat")]
        [Tooltip("The Transform of the player's object to target.")]
        [SerializeField]
        private Transform playerTarget;
        [Tooltip("The minimum distance to maintain from the player")]
        [SerializeField]
        private float minAttackDistance = 5.0f;
        [Tooltip("The angle in which the enemy can see the player")]
        [SerializeField]
        private float viewAngle = 60.0f;
        [Tooltip("The cooldown between shots")]
        [SerializeField]
        private float fireRate = 1.0f;
        [Tooltip("The damage the enemy deals")]
        [SerializeField]
        private float damage = 10.0f;
        [Tooltip("The radius around the player to cast a raycast for randomness")]
        [SerializeField]
        private float shotRandomnessRadius = 2.0f;
        [Tooltip("The enemy's health")]
        [SerializeField]
        private float health = 100f;

        [Header("Animations")]
        [Tooltip("The Animator component")]
        [SerializeField]
        private Animator enemyAnimator;

        [Header("Loot")]
        [Tooltip("The prefab for a WorldItem")]
        [SerializeField]
        private GameObject worldItemPrefab;
        [Tooltip("The items the enemy can drop")]
        [SerializeField]
        private LootTableItem[] lootTableItems;

        private Vector3 horizontalMoveDirection;
        private float verticalVelocity;
        private bool canShoot = true;

        private static readonly int Walking = Animator.StringToHash("Walking");
        private static readonly int Aggro = Animator.StringToHash("Aggro");
        private static readonly int Speed = Animator.StringToHash("Speed");
        private static readonly int Fire = Animator.StringToHash("Fire");

        private IHealthStats playerHealth;

        private void Start()
        {
            if (playerTarget == null)
            {
                playerTarget = PlayerInventory.Instance.transform;
                playerHealth = playerTarget.GetComponent<IHealthStats>();
            }
            controller = GetComponent<CharacterController>();
        }

        private void Update()
        {
            if (playerTarget == null) return;

            float distanceToPlayer = Vector3.Distance(transform.position, playerTarget.position);

            // Aggro logic
            if (distanceToPlayer < followDistance)
            {
                enemyAnimator.SetBool(Aggro, true);
            }
            else
            {
                enemyAnimator.SetBool(Aggro, false);
            }

            // Apply gravity
            if (controller.isGrounded && verticalVelocity < 0)
            {
                verticalVelocity = -2f; // Keep the character grounded
            }
            verticalVelocity += gravity * Time.deltaTime;

            // Movement logic
            if (distanceToPlayer > minAttackDistance && distanceToPlayer < followDistance)
            {
                // Move only on the XZ plane, ignoring the Y axis of the player
                horizontalMoveDirection = (playerTarget.position - transform.position);
                horizontalMoveDirection.y = 0;
                horizontalMoveDirection.Normalize();

                // Animation parameters
                enemyAnimator.SetBool(Walking, true);
                enemyAnimator.SetFloat(Speed, moveSpeed);
            }
            else
            {
                horizontalMoveDirection = Vector3.zero;
                // Animation parameters
                enemyAnimator.SetBool(Walking, false);
                enemyAnimator.SetFloat(Speed, 0);
            }

            // Combine horizontal movement and gravity
            Vector3 finalMoveVector = horizontalMoveDirection * moveSpeed;
            finalMoveVector.y = verticalVelocity;
            controller.Move(finalMoveVector * Time.deltaTime);

            // Rotation logic
            if (distanceToPlayer <= followDistance)
            {
                // Create a look direction that only considers the XZ plane
                Vector3 lookDirection = playerTarget.position - transform.position;
                lookDirection.y = 0;

                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(lookDirection), Time.deltaTime * 5f);
            }

            // Attack logic
            Vector3 directionToPlayer = (playerTarget.position - transform.position).normalized;
            float dotProduct = Vector3.Dot(transform.forward, directionToPlayer);

            if (dotProduct > Mathf.Cos(viewAngle * 0.5f * Mathf.Deg2Rad) && distanceToPlayer < followDistance)
            {
                if (canShoot)
                {
                    TryShoot();
                }
            }
        }

        /// <summary> Shoots a raycast at a random point near the player </summary>
        private void TryShoot()
        {
            canShoot = false;
            enemyAnimator.SetTrigger(Fire);
            StartCoroutine(ShootRandomPointCoroutine());
        }

        private IEnumerator ShootRandomPointCoroutine()
        {
            yield return new WaitForSeconds(0.5f); // Wait for animation to start

            Vector3 randomOffset = Random.insideUnitSphere * shotRandomnessRadius;
            Vector3 targetPosition = playerTarget.position + randomOffset;
            Vector3 direction = (targetPosition - transform.position).normalized;

            RaycastHit hit;
            // Layer 7 is the player layer
            if (Physics.Raycast(transform.position, direction, out hit, followDistance, 1 << 7))
            {
                if (playerHealth != null)
                {
                    playerHealth.Damage(damage);
                    Debug.Log("Player hit!");
                }
            }

            yield return new WaitForSeconds(fireRate - 0.5f); // Wait for the rest of the cooldown
            canShoot = true;
        }

        /// <summary> Decreases health and handles death </summary>
        /// <param name="amount"> The amount of damage taken </param>
        public void Damage(float amount)
        {
            health -= amount;
            if (health <= 0)
            {
                Die();
            }
        }

        /// <summary> Drops items and destroys the enemy </summary>
        private void Die()
        {
            foreach (LootTableItem lootItem in lootTableItems)
            {
                if (Random.Range(0, 100) < lootItem.dropChance)
                {
                    GameObject obj = Instantiate(worldItemPrefab, transform.position + new Vector3(Random.Range(-1f, 1f), 0.5f, Random.Range(-1f, 1f)), Quaternion.identity);
                    WorldItem droppedItem = obj.GetComponent<WorldItem>();
                    if (droppedItem != null)
                    {
                        droppedItem.Initialize(lootItem.itemData, false);
                    }
                }
            }

            Destroy(gameObject);
        }

        public void Heal(float amount)
        {
        }
    }

    [System.Serializable]
    public class LootTableItem
    {
        public ItemData itemData;
        public int dropChance; // 0-100
    }
}