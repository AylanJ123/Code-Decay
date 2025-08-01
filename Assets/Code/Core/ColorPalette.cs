using UnityEngine;

namespace com.AylanJ123.CodeDecay
{
    /// <summary> A ScriptableObject to store the game's main color palette </summary>
    [CreateAssetMenu(fileName = "MainColorPalette", menuName = "Code Decay/Main Color Palette")]
    public class MainColorPalette : ScriptableObject
    {
        // Primary Colors
        public Color deepCyberspaceBlue = new Color32(10, 25, 49, 255);
        public Color vibrantCyan = new Color32(0, 228, 255, 255);
        public Color cyberMagenta = new Color32(255, 0, 127, 255);

        // Corruption Colors
        public Color glitchGreen = new Color32(170, 255, 0, 255);
        public Color corruptedRed = new Color32(255, 77, 77, 255);
    }

}
