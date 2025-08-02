using System.Collections;
using System.Collections.Generic;
using com.AylanJ123.CodeDecay.Enemies;
using UnityEngine;

namespace com.AylanJ123.CodeDecay.Spawners
{
    /// <summary>
    /// Spawns enemies at random intervals and at the positions of this object's children.
    /// It operates continuously, spawning a single enemy at a time.
    /// </summary>
    public sealed class EnemySpawner : MonoBehaviour
    {
        [Tooltip("The enemy prefab to spawn")]
        [SerializeField]
        private GameObject enemyPrefab;

        [Header("Spawn Timing")]
        [Tooltip("The minimum time between enemy spawns")]
        [SerializeField]
        private float minSpawnInterval = 2.0f;
        [Tooltip("The maximum time between enemy spawns")]
        [SerializeField]
        private float maxSpawnInterval = 5.0f;

        private List<Transform> spawnPoints;

        private void Awake()
        {
            spawnPoints = new List<Transform>();
            foreach (Transform child in transform)
            {
                spawnPoints.Add(child);
            }
        }

        private void Start()
        {
            if (enemyPrefab == null)
            {
                Debug.LogError("Enemy prefab is not assigned.");
                return;
            }
            if (spawnPoints.Count == 0)
            {
                Debug.LogError("No spawn points found. Please add child transforms to the spawner.");
                return;
            }

            StartCoroutine(SpawnEnemyCoroutine());
        }

        /// <summary> Spawns enemies indefinitely at random intervals. </summary>
        private IEnumerator SpawnEnemyCoroutine()
        {
            while (true)
            {
                float randomInterval = Random.Range(minSpawnInterval, maxSpawnInterval);
                yield return new WaitForSeconds(randomInterval);

                int randomIndex = Random.Range(0, spawnPoints.Count);
                Transform spawnPoint = spawnPoints[randomIndex];
                Instantiate(enemyPrefab, spawnPoint.position, spawnPoint.rotation);
            }
        }
    }
}