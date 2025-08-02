using MyBox;
using System.Collections.Generic;
using UnityEngine;

namespace com.AylanJ123.CodeDecay
{
    /// <summary>
    /// Base class for all inventory items using Scriptable Objects.
    /// This allows for easy creation and management of item data.
    /// </summary>
    public abstract class ItemData : ScriptableObject
    {
        [Tooltip("The unique ID of the item. This can be used for saving and loading")]
        public int itemId;
        [Tooltip("The display name of the item")]
        public string itemName;
        [TextArea(4, 4), Tooltip("The description of the item")]
        public string description;
        [Tooltip("The icon to display in the inventory UI")]
        public Sprite icon;
        [Tooltip("The highlight color of the item")]
        public Color32 highlightColor;

        [DisplayInspector, Tooltip("The list of effects this upgrade or potion has")]
        public List<ItemEffect> effects;

        /// <summary> Applies all effects of the item to the player </summary>
        /// <param name="player"> The player root GameObject </param>
        public virtual void Use(GameObject player)
        {
            foreach (ItemEffect effect in effects) effect.Apply(player);
        }

        /// <summary> Removes all effects of the item from the player </summary>
        /// <param name="player"> The player root GameObject </param>
        public virtual void Remove(GameObject player)
        {
            foreach (ItemEffect effect in effects) effect.Remove(player);
        }

    }
}