using UnityEngine;

public class WeaponPickup : MonoBehaviour
{
    [Header("Pickup Settings")]
    public int ammoAmount = 10;
    public AudioClip pickupSound;

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Look for the main weapon shoot script
        Shoot shoot = other.GetComponentInChildren<Shoot>();
        if (shoot == null)
            return;

        // Skip if infinite ammo weapon
        if (shoot.maxAmmo <= 0)
            return;

        // Try adding ammo
        int added = shoot.AddAmmo(ammoAmount);

        Destroy(gameObject);
    }
}
