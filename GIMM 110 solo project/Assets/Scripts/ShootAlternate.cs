using UnityEngine;

/// <summary>
/// Secondary player gun — identical behaviour to Shoot but kept as a separate component
/// so you can assign different prefabs / settings and toggle between them.
/// Adds simple ammo fields and AddAmmo method for pickups.
/// </summary>
public class ShootAlternate : MonoBehaviour
{
    [Header("References")]
    public GameObject bulletPrefab;
    public Transform firingPoint;
    [Tooltip("Optional container for organization. DO NOT make this a child of the player if you want bullets to keep their world rotation.")]
    public Transform bulletContainer;

    [Header("Gun Settings")]
    [Tooltip("If <= 0, fires once per button press. If > 0, allows automatic fire with this cooldown between shots.")]
    public float fireCooldown = 0.1f; // default faster
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
        if (maxAmmo > 0 && currentAmmo <= 0)
            currentAmmo = maxAmmo;
    }

    void Update()
    {
        fireTimer -= Time.deltaTime;

        bool shouldFire = false;

        if (fireCooldown <= 0f)
        {
            shouldFire = Input.GetButtonDown("Fire1");
        }
        else
        {
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
                // empty
            }
        }
    }

    bool CanFire()
    {
        if (maxAmmo <= 0) return true;
        return currentAmmo > 0;
    }

    void Fire()
    {
        if (bulletPrefab == null || firingPoint == null) return;

        if (maxAmmo > 0)
            currentAmmo = Mathf.Max(0, currentAmmo - 1);

        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0f;
        Vector3 dir = (mousePos - firingPoint.position).normalized;

        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 90f;
        Quaternion rot = Quaternion.Euler(0f, 0f, angle);

        GameObject bullet = Instantiate(bulletPrefab, firingPoint.position, rot);

        if (bulletContainer != null)
            bullet.transform.SetParent(bulletContainer, true);

        var b = bullet.GetComponent<Bullet>();
        if (b != null)
        {
            if (bulletDamageOverride > 0) b.SetDamage(bulletDamageOverride);
            if (bulletSpeedOverride > 0f) b.SetSpeed(bulletSpeedOverride);
        }

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
