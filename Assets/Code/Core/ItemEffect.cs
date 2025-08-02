using UnityEngine;

namespace com.AylanJ123.CodeDecay
{
    /// <summary>
    /// Abstract base class for all item effects.
    /// This allows for different types of effects to be created as Scriptable Objects.
    /// </summary>
    public abstract class ItemEffect : ScriptableObject
    {
        /// <summary> Applies the effect to the player </summary>
        /// <param name="player"> The player root GameObject </param>
        public abstract void Apply(GameObject player);

        /// <summary> Removes or reverts effects from the player </summary>
        /// <param name="player"> The player root GameObject </param>
        public abstract void Remove(GameObject player);
    }
}
