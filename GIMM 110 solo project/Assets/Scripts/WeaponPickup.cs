using UnityEngine;

/// <summary>
/// Pickup object that refills ammo for player's primary or secondary weapon,
/// or grants a new weapon prefab to the secondary slot if a bullet prefab is assigned.
/// Expect the pickup prefab to have a Collider2D with IsTrigger = true.
/// </summary>
public class WeaponPickup : MonoBehaviour
{
    public enum TargetSlot { Primary, Secondary }

    [Header("Pickup Settings")]
    [Tooltip("Which weapon slot to target")]
    public TargetSlot target = TargetSlot.Secondary;

    [Tooltip("Amount of ammo to add when picked up (used when bulletPrefab is not set)")]
    public int ammoAmount = 10;

    [Tooltip("If set, this bullet prefab will be assigned to the player's secondary weapon (granting a new weapon).")]
    public GameObject bulletPrefab;

    [Tooltip("Optional lifetime for the pickup (seconds). 0 means infinite.")]
    public float lifetime = 30f;

    void Start()
    {
        if (lifetime > 0f)
            Destroy(gameObject, lifetime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        Movement2D mover = other.GetComponent<Movement2D>();
        if (mover == null)
        {
            Debug.LogWarning("WeaponPickup: Player has no Movement2D component to receive ammo/weapon.");
            Destroy(gameObject);
            return;
        }

        // If this pickup grants a new weapon prefab, assign it (secondary slot)
        if (bulletPrefab != null)
        {
            // Prefer existing secondary ShootAlternate
            if (mover.secondaryShoot != null)
            {
                mover.secondaryShoot.bulletPrefab = bulletPrefab;

                // Optionally restore ammo on the new weapon if it supports ammo
                if (mover.secondaryShoot.maxAmmo > 0)
                    mover.secondaryShoot.currentAmmo = mover.secondaryShoot.maxAmmo;

                Debug.Log($"WeaponPickup: assigned new weapon prefab to secondary slot: {bulletPrefab.name}");
            }
            else
            {
                // Add ShootAlternate dynamically and configure it
                var added = other.gameObject.AddComponent<ShootAlternate>();
                added.bulletPrefab = bulletPrefab;

                // try to set firingPoint to primary's firingPoint if available
                if (mover.primaryShoot != null)
                    added.firingPoint = mover.primaryShoot.firingPoint;

                // initialize ammo if desired (optional: give full magazine if weapon has maxAmmo)
                if (added.maxAmmo > 0)
                    added.currentAmmo = added.maxAmmo;

                mover.secondaryShoot = added;
                Debug.Log($"WeaponPickup: added new ShootAlternate with prefab {bulletPrefab.name} to player.");
            }

            // Optionally switch to secondary immediately
            mover.AssignPickedWeapon(bulletPrefab);
        }
        else
        {
            // No weapon prefab — treat as ammo refill for target slot
            int added = 0;

            if (target == TargetSlot.Primary)
            {
                if (mover.primaryShoot != null)
                    added = mover.primaryShoot.AddAmmo(ammoAmount);
                else
                    Debug.LogWarning("WeaponPickup: Player has no primary Shoot component to refill.");
            }
            else // Secondary
            {
                if (mover.secondaryShoot != null)
                    added = mover.secondaryShoot.AddAmmo(ammoAmount);
                else
                    Debug.LogWarning("WeaponPickup: Player has no secondary ShootAlternate component to refill.");
            }

            if (added > 0)
                Debug.Log($"WeaponPickup: added {added} ammo to {target} slot.");
            else
                Debug.Log("WeaponPickup: no ammo added (weapon missing, infinite, or already full).");
        }

        Destroy(gameObject);
    }
}
