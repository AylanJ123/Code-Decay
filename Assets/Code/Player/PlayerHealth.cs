using UnityEngine;
using MyBox;
using System;
using UnityEngine.Events;

namespace com.AylanJ123.CodeDecay.Player
{
    [Serializable]
    public class PlayerHealth
    {
        [Tooltip("The player's maximum health")]
        [SerializeField, InitializationField, Min(1)]
        private float maxHealth = 100f;

        [Tooltip("The player's current health")]
        [SerializeField, ReadOnly]
        private float currentHealth;

        public UnityEvent<float, float> OnHealthChanged;
        public UnityEvent OnDeath;

        public void Initialize()
        {
            currentHealth = maxHealth;
        }

        public void Heal(float amount)
        {
            currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
            OnHealthChanged?.Invoke(currentHealth, maxHealth);
        }

        public void Damage(float amount)
        {
            currentHealth = Mathf.Max(currentHealth - amount, 0);
            OnHealthChanged?.Invoke(currentHealth, maxHealth);
            if (currentHealth <= 0) Die();
        }

        public void Die()
        {
            OnDeath?.Invoke();
            Debug.Log("Player has died. Restarting.");
        }

    }
}
