using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class ShootAlternate : MonoBehaviour
{
    [Header("References")]
    public GameObject bulletPrefab;
    public Transform firingPoint;
    [Tooltip("Optional container for organization. DO NOT make this a child of the player if you want bullets to keep their world rotation.")]
    public Transform bulletContainer;

    [Header("Audio Settings")]
    [Tooltip("Sound to play when the gun fires.")]
    public AudioClip gunshotClip;
    [Tooltip("AudioSource used to play gun sounds. If not assigned, one will be created at runtime.")]
    public AudioSource audioSource;

    [Header("Gun Settings")]
    [Tooltip("If <= 0, fires once per button press. If > 0, allows automatic fire with this cooldown between shots.")]
    public float fireCooldown = 0.1f; // default faster
    public float bulletSpeedOverride = 0f; // 0 = use prefab's speed
    public int bulletDamageOverride = 0;   // 0 = use prefab's damage

    [Header("Shotgun Settings")]
    [Tooltip("Number of pellets per shot (3 for a simple spread).")]
    public int pelletCount = 3;
    [Tooltip("Total spread angle in degrees between the outer pellets.")]
    public float spreadAngle = 10f;

    [Header("Ammo (<=0 = infinite)")]
    public int maxAmmo = 0;
    public int currentAmmo = 0;

    [Header("UI")]
    [Tooltip("Slider showing remaining ammo. Assign in Inspector.")]
    public Slider ammoBar;

    float fireTimer = 0f;

    void Start()
    {
        if (maxAmmo > 0 && currentAmmo <= 0)
            currentAmmo = maxAmmo;

        UpdateAmmoUI();
    }

    void OnEnable()
    {
        UpdateAmmoUI();
        if (ammoBar != null)
            ammoBar.gameObject.SetActive(true);
    }

    void OnDisable()
    {
        if (ammoBar != null)
            ammoBar.gameObject.SetActive(false);
    }

    void Update()
    {
        fireTimer -= Time.deltaTime;

        bool shouldFire = false;

        if (fireCooldown <= 0f)
            shouldFire = Input.GetButtonDown("Fire1");
        else
            shouldFire = Input.GetButton("Fire1") && fireTimer <= 0f;

        if (shouldFire)
        {
            if (CanFire())
            {
                Fire();
                if (fireCooldown > 0f) fireTimer = fireCooldown;
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
        {
            currentAmmo = Mathf.Max(0, currentAmmo - 1);
            UpdateAmmoUI();
        }

        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0f;
        Vector3 dir = (mousePos - firingPoint.position).normalized;
        float baseAngle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 90f;

        // Fire multiple pellets
        for (int i = 0; i < pelletCount; i++)
        {
            float angleOffset = 0f;
            if (pelletCount > 1)
                angleOffset = Mathf.Lerp(-spreadAngle / 2f, spreadAngle / 2f, i / (float)(pelletCount - 1));

            Quaternion rot = Quaternion.Euler(0f, 0f, baseAngle + angleOffset);
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
        // Play gunshot sound
        if (gunshotClip != null)
        {
            if (audioSource != null)
                audioSource.PlayOneShot(gunshotClip);
            else
                AudioSource.PlayClipAtPoint(gunshotClip, firingPoint.position);
        }
    }

    public int AddAmmo(int amount)
    {
        if (maxAmmo <= 0 || amount <= 0) return 0;
        int before = currentAmmo;
        currentAmmo = Mathf.Clamp(currentAmmo + amount, 0, maxAmmo);
        UpdateAmmoUI();
        return currentAmmo - before;
    }

    void UpdateAmmoUI()
    {
        if (ammoBar == null) return;

        if (maxAmmo > 0)
        {
            ammoBar.maxValue = maxAmmo;
            ammoBar.value = currentAmmo;
        }
        else
        {
            ammoBar.gameObject.SetActive(false);
        }
    }
}

