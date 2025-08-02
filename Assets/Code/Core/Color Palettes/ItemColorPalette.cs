using UnityEngine;

namespace com.AylanJ123.CodeDecay
{
    /// <summary>
    /// A ScriptableObject to hold the color palette for different items
    /// </summary>
    [CreateAssetMenu(fileName = "ItemColorPalette", menuName = "Code Decay/Item Color Palette")]
    public class ItemColorPalette : ScriptableObject
    {
        // Potion Colors
        public Color overcharged = new Color32(255, 140, 0, 255);
        public Color combatProtocol = new Color32(106, 13, 173, 255);
        public Color nanobot = new Color32(192, 192, 192, 255);

        // Upgrade Colors
        public Color heatSink = new Color32(205, 127, 50, 255);
        public Color powerCore = new Color32(255, 105, 180, 255);
        public Color fragmentedCPU = new Color32(255, 215, 0, 255);
    }
}