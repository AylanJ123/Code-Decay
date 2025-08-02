using UnityEngine;

namespace com.AylanJ123.CodeDecay
{
    [CreateAssetMenu(fileName = "New Damage Effect", menuName = "Code Decay/Effects/Damage")]
    public class DamageEffect : ItemEffect
    {
        [Tooltip("The amount of damage this effect adds or removes")]
        public float value;

        public override void Apply(GameObject player)
        {
            if (player.TryGetComponent(out ICombatStats combat))
                combat.ApplyDamageModification(value);
        }

        public override void Remove(GameObject player)
        {
            if (player.TryGetComponent(out ICombatStats combat))
                combat.RemoveDamageModification(value);
        }
    }
}