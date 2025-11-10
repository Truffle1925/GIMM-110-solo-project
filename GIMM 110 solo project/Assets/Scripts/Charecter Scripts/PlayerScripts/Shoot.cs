using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class Shoot : MonoBehaviour
{
    [Header("References")]
    public GameObject bulletPrefab;
    public Transform firingPoint;
    public Transform bulletContainer;

    [Header("Audio Settings")]
    public AudioClip gunshotClip;
    public AudioSource audioSource;

    [Header("Gun Settings")]
    public float fireCooldown = 0f;
    public float bulletSpeedOverride = 0f;
    public int bulletDamageOverride = 0;

    [Header("Ammo (<=0 = infinite)")]
    public int maxAmmo = 0;
    public int currentAmmo = 0;
    public bool isActiveWeapon = false;

    [Header("UI")]
    public List<Image> MainBullet = new List<Image>();

    float fireTimer = 0f;
    bool initialized = false; // NEW: Prevents double-refreshing ammo when switching

    void Start()
    {
        if (maxAmmo > 0 && currentAmmo <= 0)
            currentAmmo = maxAmmo;

        if (audioSource == null && gunshotClip != null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }

        UpdateAmmoUI();
        initialized = true;
    }

    void OnEnable()
    {
        UpdateAmmoUI();
        isActiveWeapon = true;


    }

    void OnDisable()
    {
        // Hide icons when inactive but don’t modify ammo values
        foreach (var image in MainBullet)
        {
            if (image != null)
                image.gameObject.SetActive(false);
        }
        isActiveWeapon = false;
    }

    void SetAmmoImagesActive(bool isActive)
    {
        if (MainBullet == null) return;

        if (isActiveWeapon == true)
            foreach (var image in MainBullet)
            {
                if (image != null)
                    image.gameObject.SetActive(isActive);
            }
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
        if (isActiveWeapon)
            UpdateAmmoUI();
        return currentAmmo - before;
    }

    public void UpdateAmmoUI()
    {
        if (MainBullet == null || MainBullet.Count == 0)
            return;

        if (maxAmmo > 0)
        {
            // Hide all first
            foreach (Image Bullet in MainBullet)
                Bullet.gameObject.SetActive(false);

            // Show bullets that match current ammo
            for (int i = 0; i < Mathf.Min(currentAmmo, MainBullet.Count); i++)
                MainBullet[i].gameObject.SetActive(true);
        }
        else
        {
            foreach (Image Bullet in MainBullet)
                Bullet.gameObject.SetActive(false);
        }
    }
}
