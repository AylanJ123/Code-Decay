using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace com.AylanJ123.CodeDecay.Inventory
{
    /// <summary>
    /// Manages the application and removal of all item effects on the player.
    /// This script listens for inventory changes to correctly manage effects.
    /// </summary>
    public sealed class PlayerEffectsManager : MonoBehaviour
    {
        /// <summary>
        /// A dictionary to hold the active coroutines for timed potion effects.
        /// </summary>
        private readonly Dictionary<ItemEffect, Coroutine> activePotionCoroutines = new();

        /// <summary> The player's inventory, which this manager listens to. </summary>
        private PlayerInventory playerInventory;

        private void Start()
        {
            playerInventory = PlayerInventory.Instance;

            if (playerInventory != null)
            {
                playerInventory.OnInventoryUpdated.AddListener(OnInventoryUpdated);
            }
        }

        private void OnDestroy()
        {
            if (playerInventory != null)
            {
                playerInventory.OnInventoryUpdated.RemoveListener(OnInventoryUpdated);
            }

            foreach (Coroutine coroutine in activePotionCoroutines.Values)
            {
                StopCoroutine(coroutine);
            }
        }

        /// <summary> Re-evaluates and applies all item effects in the inventory. </summary>
        private void OnInventoryUpdated()
        {
            GetComponent<IStats>().Cleanse();
            // Re-apply effects from all items currently in the inventory
            foreach (ItemData item in playerInventory.GetAllItems())
            {
                if (item is UpgradeData upgrade)
                {
                    upgrade.Use(gameObject);
                }
            }
        }

        /// <summary> Starts a new timed effect for a potion </summary>
        /// <param name="potionData"> Potion data with duration and effects </param>
        public void StartTimedEffect(PotionData potionData)
        {
            foreach (ItemEffect effect in potionData.effects)
            {
                if (activePotionCoroutines.ContainsKey(effect))
                {
                    StopCoroutine(activePotionCoroutines[effect]);
                    activePotionCoroutines.Remove(effect);
                }

                effect.Apply(gameObject);
                Coroutine durationCoroutine = StartCoroutine(RemoveEffectAfterTime(effect, potionData));
                activePotionCoroutines.Add(effect, durationCoroutine);
            }
        }

        private IEnumerator RemoveEffectAfterTime(ItemEffect effect, PotionData potionData)
        {
            yield return new WaitForSeconds(potionData.duration);
            effect.Remove(gameObject);
            activePotionCoroutines.Remove(effect);
        }
    }
}