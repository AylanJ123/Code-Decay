using UnityEngine;

namespace com.AylanJ123.CodeDecay
{
    [CreateAssetMenu(fileName = "New Potion", menuName = "Code Decay/Items/Potion")]
    public class PotionData : ItemData
    {
        [Tooltip("The duration of the effect in seconds")]
        public float duration;
    }
}
