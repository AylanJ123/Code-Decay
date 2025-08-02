using UnityEngine;

namespace com.AylanJ123.CodeDecay
{
    [CreateAssetMenu(fileName = "New Health Effect", menuName = "Code Decay/Effects/Health")]
    public class HealthEffect : ItemEffect
    {
        [Tooltip("The amount of health this effect restores or removes")]
        public float value;

        public override void Apply(GameObject player)
        {
            if (player.TryGetComponent(out IHealthStats health))
            {
                if (value > 0) health.Heal(value);
                else health.Damage(value);
            }
        }

        public override void Remove(GameObject player)
        {
            //One time application, uneeded.
        }
    }
}