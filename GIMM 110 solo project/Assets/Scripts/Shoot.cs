using UnityEngine;

/// <summary>
/// Primary player gun. Adjustable variables exposed in the Inspector.
/// Bullets are spawned unparented so they don't inherit player rotation after firing.
/// Supports single-shot (fireCooldown <= 0) or automatic fire (fireCooldown > 0).
/// Adds a simple ammo system: set maxAmmo <= 0 for infinite ammo.
/// </summary>
public class Shoot : MonoBehaviour
{
    [Header("References")]
    public GameObject bulletPrefab;
    public Transform firingPoint;
    [Tooltip("Optional container for organization. DO NOT make this a child of the player if you want bullets to keep their world rotation.")]
    public Transform bulletContainer;

    [Header("Gun Settings")]
    [Tooltip("If <= 0, fires once per button press. If > 0, allows automatic fire with this cooldown between shots.")]
    public float fireCooldown = 0f;
    public float bulletSpeedOverride = 0f; // 0 = use prefab's speed
    public int bulletDamageOverride = 0;   // 0 = use prefab's damage

    [Header("Ammo (<=0 = infinite)")]
    [Tooltip("Maximum ammo in this weapon. Set to 0 or a negative value for infinite ammo.")]
    public int maxAmmo = 0;
    [Tooltip("Current ammo count. If maxAmmo <= 0 this value is ignored.")]
    public int currentAmmo = 0;

    float fireTimer = 0f;

    void Start()
    {
        // initialize ammo to max if configured and current not set
        if (maxAmmo > 0 && currentAmmo <= 0)
            currentAmmo = maxAmmo;
    }

    void Update()
    {
        fireTimer -= Time.deltaTime;

        bool shouldFire = false;

        if (fireCooldown <= 0f)
        {
            // single shot mode
            shouldFire = Input.GetButtonDown("Fire1");
        }
        else
        {
            // automatic mode while holding Fire1
            shouldFire = Input.GetButton("Fire1") && fireTimer <= 0f;
        }

        if (shouldFire)
        {
            if (CanFire())
            {
                Fire();
                if (fireCooldown > 0f) fireTimer = fireCooldown;
            }
            else
            {
                // no ammo - could play empty sound here
            }
        }
    }

    bool CanFire()
    {
        // infinite ammo if maxAmmo <= 0
        if (maxAmmo <= 0) return true;
        return currentAmmo > 0;
    }

    void Fire()
    {
        if (bulletPrefab == null || firingPoint == null) return;

        // consume ammo if applicable
        if (maxAmmo > 0)
            currentAmmo = Mathf.Max(0, currentAmmo - 1);

        // Get mouse position and compute direction
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0f;
        Vector3 dir = (mousePos - firingPoint.position).normalized;

        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 90f;
        Quaternion rot = Quaternion.Euler(0f, 0f, angle);

        // Instantiate WITHOUT parent so bullet rotation/velocity never change if player rotates.
        GameObject bullet = Instantiate(bulletPrefab, firingPoint.position, rot);

        // Optionally move under a container for project view (but container should NOT be child of player)
        if (bulletContainer != null)
            bullet.transform.SetParent(bulletContainer, true);

        // Try to set bullet speed/damage if the component exists
        var b = bullet.GetComponent<Bullet>();
        if (b != null)
        {
            if (bulletDamageOverride > 0) b.SetDamage(bulletDamageOverride);
            if (bulletSpeedOverride > 0f) b.SetSpeed(bulletSpeedOverride);
        }

        // If the prefab uses EnemyBullet by mistake, try that as well (no harm)
        var eb = bullet.GetComponent<EnemyBullet>();
        if (eb != null)
        {
            if (bulletDamageOverride > 0) eb.SetDamage(bulletDamageOverride);
            if (bulletSpeedOverride > 0f) eb.SetSpeed(bulletSpeedOverride);
        }
    }

    /// <summary>
    /// Adds ammo to this weapon. Returns amount actually added.
    /// If maxAmmo <= 0 this weapon has infinite ammo and nothing is added.
    /// </summary>
    public int AddAmmo(int amount)
    {
        if (maxAmmo <= 0 || amount <= 0) return 0;
        int before = currentAmmo;
        currentAmmo = Mathf.Clamp(currentAmmo + amount, 0, maxAmmo);
        return currentAmmo - before;
    }
}
