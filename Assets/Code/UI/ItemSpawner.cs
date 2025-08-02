using com.AylanJ123.CodeDecay.Inventory;
using System.Collections.Generic;
using UnityEngine;

namespace com.AylanJ123.CodeDecay
{
    /// <summary>
    /// Spawns a random selection of WorldItem prefabs for testing purposes
    /// </summary>
    public sealed class ItemSpawner : MonoBehaviour
    {
        [Header("Item Data")]
        [Tooltip("List of all possible potion ItemData assets to spawn")]
        [SerializeField]
        private List<ItemData> potionItems;
        [Tooltip("List of all possible upgrade ItemData assets to spawn")]
        [SerializeField]
        private List<ItemData> upgradeItems;

        [Header("World Item References")]
        [Tooltip("The WorldItem prefab that will be spawned")]
        [SerializeField]
        private GameObject worldItemPrefab;
        [Tooltip("The transform to spawn items around, typically the player")]
        [SerializeField]
        private Transform spawnAroundTransform;

        [Header("Spawn Settings")]
        [Tooltip("The number of items to spawn")]
        [SerializeField]
        private int numberOfItemsToSpawn = 5;
        [Tooltip("The radius around the spawnAroundTransform to spawn items")]
        [SerializeField]
        private float spawnRadius = 15f;

        private void Start()
        {
            SpawnRandomItems();
        }

        /// <summary>
        /// Spawns a random selection of items from the provided lists
        /// in a random location within the spawnRadius.
        /// </summary>
        private void SpawnRandomItems()
        {
            for (int i = 0; i < numberOfItemsToSpawn; i++)
            {
                // Randomly choose between a potion or an upgrade
                ItemData itemToSpawn;
                if (Random.value > 0.5f)
                {
                    if (potionItems.Count == 0) continue;
                    itemToSpawn = potionItems[Random.Range(0, potionItems.Count)];
                }
                else
                {
                    if (upgradeItems.Count == 0) continue;
                    itemToSpawn = upgradeItems[Random.Range(0, upgradeItems.Count)];
                }

                // Get a random position within the spawn radius
                Vector3 randomPosition = Random.insideUnitSphere * spawnRadius;
                randomPosition += spawnAroundTransform.position;
                randomPosition.y = spawnAroundTransform.position.y; // Keep it on the same plane as the player

                // Instantiate the prefab and initialize it with the ItemData
                WorldItem spawnedItem = Instantiate(worldItemPrefab, randomPosition, Quaternion.identity).GetComponent<WorldItem>();
                spawnedItem.Initialize(itemToSpawn);
            }
        }
    }
}

