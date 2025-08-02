using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace com.AylanJ123.CodeDecay.Player
{
    /// <summary> Manages active, timed effects on the player </summary>
    public sealed class PlayerEffectsManager : MonoBehaviour
    {
        private readonly Dictionary<ItemEffect, Coroutine> activeEffects = new();

        /// <summary> Starts a new timed effect for a potion </summary>
        /// <param name="potionData"> Potion data with duration and effects </param>
        public void StartTimedEffect(PotionData potionData)
        {
            foreach (ItemEffect effect in potionData.effects)
            {
                // Only apply an effect once
                if (activeEffects.ContainsKey(effect)) continue;

                effect.Apply(gameObject);
                Coroutine durationCoroutine = StartCoroutine(RemoveEffectAfterTime(effect, potionData.duration));
                activeEffects.Add(effect, durationCoroutine);
            }
        }

        private IEnumerator RemoveEffectAfterTime(ItemEffect effect, float duration)
        {
            yield return new WaitForSeconds(duration);
            effect.Remove(gameObject);
            activeEffects.Remove(effect);
        }
    }
}