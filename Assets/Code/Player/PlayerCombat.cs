using UnityEngine;
using MyBox;
using UnityEngine.Events;
using System;

namespace com.AylanJ123.CodeDecay.Player
{
    [Serializable]
    public class PlayerCombat
    {
        [Tooltip("The projectile prefab that will be instantiated")]
        [SerializeField, InitializationField, MustBeAssigned]
        private GameObject projectilePrefab;

        [Tooltip("The Transform from where the projectile will be fired")]
        [SerializeField, InitializationField, MustBeAssigned]
        private Transform firePoint;

        [Tooltip("The camera used for aiming raycasts")]
        [SerializeField, InitializationField, MustBeAssigned]
        private Camera mainCamera;

        [Tooltip("The Animator")]
        [SerializeField, InitializationField, MustBeAssigned]
        private Animator anim;

        [Tooltip("The maximum distance of the raycast")]
        [SerializeField, InitializationField, Min(1f)]
        private float projectileRange = 100f;

        [Tooltip("The base damage the player deals")]
        [SerializeField, InitializationField, Min(1f)]
        private float baseDamage = 10f;

        [Tooltip("The base cooldown between shots")]
        [SerializeField, InitializationField, Min(0.01f)]
        private float baseCooldown = 0.5f;

        [Tooltip("The current damage the player deals, including temporary modifiers")]
        [SerializeField, ReadOnly]
        public float currentDamage;

        [Tooltip("The current cooldown between shots, including temporary modifiers")]
        [SerializeField, ReadOnly]
        public float currentCooldown;

        public UnityEvent<float> OnDamageChanged;
        public UnityEvent<float> OnCooldownChanged;

        private float nextFireTime;

        public void Initialize()
        {
            currentDamage = baseDamage;
            currentCooldown = baseCooldown;
            OnDamageChanged?.Invoke(currentDamage);
            OnCooldownChanged?.Invoke(currentCooldown);
        }

        public void UpdateDamage(float modifier)
        {
            currentDamage = baseDamage + modifier;
            OnDamageChanged?.Invoke(currentDamage);
        }

        public void UpdateCooldown(float modifier)
        {
            currentCooldown = baseCooldown + modifier;
            if (currentCooldown < 0.01f) currentCooldown = 0.01f;
            OnCooldownChanged?.Invoke(currentCooldown);
        }

        public void Fire()
        {
            if (Time.time >= nextFireTime)
            {
                anim.ResetTrigger("Fire");
                anim.SetTrigger("Fire");
                nextFireTime = Time.time + Mathf.Max(currentCooldown, 0.015f);
                Vector3 targetPoint;

                if (Physics.Raycast(
                    mainCamera.transform.position,
                    mainCamera.transform.forward,
                    out RaycastHit hit,
                    projectileRange,
                    1 << 3 | 1 << 6)
                ) {
                    targetPoint = hit.point;
                    if (hit.collider.TryGetComponent(out IHealthStats healthStats))
                        healthStats.Damage(Mathf.Max(currentDamage, 0.1f));
                }
                else targetPoint = mainCamera.transform.position +
                        mainCamera.transform.forward * projectileRange;

                Vector3 directionToTarget = (targetPoint - firePoint.position).normalized;
                Quaternion rotation = Quaternion.LookRotation(directionToTarget);

                GameObject projectileInstance = GameObject.Instantiate(projectilePrefab, firePoint.position, rotation);
                GameObject.Destroy(projectileInstance, 6f);

                if (hit.collider != null && projectileInstance.transform.childCount > 0)
                {
                    Transform spark = projectileInstance.transform.GetChild(0);
                    spark.position = hit.point;
                    spark.LookAt(firePoint);
                }
            }
        }
    }
}