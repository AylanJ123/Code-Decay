using UnityEngine;

namespace com.AylanJ123.CodeDecay
{
    [CreateAssetMenu(fileName = "New Cooldown Effect", menuName = "Code Decay/Effects/Cooldown")]
    public class CooldownEffect : ItemEffect
    {
        [Tooltip("The amount of shooting cooldown this effect adds or removes")]
        public float value;

        public override void Apply(GameObject player)
        {
            if (player.TryGetComponent(out ICooldownStats cooldown))
                cooldown.ApplyCooldownModification(value);
        }

        public override void Remove(GameObject player)
        {
            if (player.TryGetComponent(out ICooldownStats cooldown))
                cooldown.RemoveCooldownModification(value);
        }
    }
}