using UnityEngine;

/// <summary>
/// Secondary player gun — identical behaviour to Shoot but kept as a separate component
/// so you can assign different prefabs / settings and toggle between them.
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

    float fireTimer = 0f;

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
            Fire();
            fireTimer = fireCooldown;
        }
    }

    void Fire()
    {
        if (bulletPrefab == null || firingPoint == null) return;

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
}
