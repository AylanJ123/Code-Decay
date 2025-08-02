using UnityEngine;

namespace com.AylanJ123.CodeDecay
{
    [CreateAssetMenu(fileName = "New Speed Effect", menuName = "Code Decay/Effects/Speed")]
    public class SpeedEffect : ItemEffect
    {
        [Tooltip("The amount of speed this effect adds or removes")]
        public float value;

        public override void Apply(GameObject player)
        {
            if (player.TryGetComponent(out ISpeedStats speed))
                speed.ApplySpeedModification(value);
        }

        public override void Remove(GameObject player)
        {
            if (player.TryGetComponent(out ISpeedStats speed))
                speed.RemoveSpeedModification(value);
        }
    }
}