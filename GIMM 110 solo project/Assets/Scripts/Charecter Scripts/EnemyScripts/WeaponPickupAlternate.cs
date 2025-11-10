using UnityEngine;

public class AltWeaponPickup : MonoBehaviour
{
    [Header("Pickup Settings")]
    public int ammoAmount = 5;
    public AudioClip pickupSound;

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Look for the alternate weapon shoot script
        ShootAlternate shootAlt = other.GetComponentInChildren<ShootAlternate>();
        if (shootAlt == null)
            return;

        // Skip if infinite ammo weapon
        if (shootAlt.maxAmmo <= 0)
            return;

        // Try adding ammo
        int added = shootAlt.AddAmmo(ammoAmount);

        Destroy(gameObject);
    }
}
