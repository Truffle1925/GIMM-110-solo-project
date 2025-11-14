using UnityEngine;
using UnityEngine.UI;

public class MeleeAttack : MonoBehaviour
{
    [Header("Melee Settings")]
    public float attackRange = 1.5f;
    public float attackDamage = 25f;
    public float attackCooldown = 0.5f;
    public string enemyTag = "Enemy";

    [Header("References")]
    public Transform attackPoint;
    public AudioSource audioSource;
    public AudioClip swingSound;
    public AudioClip hitSound;
    public Image Melee;

    private float nextAttackTime = 0f;
    private WeaponManager weaponManager;  // cached reference

    void Start()
    {
        // Cache reference to WeaponManager once
        weaponManager = FindObjectOfType<WeaponManager>();
    }

    void Update()
    {
        if (Time.time >= nextAttackTime && Input.GetButtonDown("Fire1"))
        {
            Attack();
            nextAttackTime = Time.time + attackCooldown;
        }
    }

    void Attack()
    {
        // Trigger melee animation
        if (weaponManager != null)
            weaponManager.PlayMeleeAttackAnimation();

        // Play swing sound
        if (audioSource && swingSound)
            audioSource.PlayOneShot(swingSound);

        // Detect enemies in range
        Collider2D[] hitObjects = Physics2D.OverlapCircleAll(attackPoint.position, attackRange);
        foreach (Collider2D obj in hitObjects)
        {
            if (obj.CompareTag(enemyTag))
            {
                obj.SendMessage("TakeDamage", attackDamage, SendMessageOptions.DontRequireReceiver);
                if (audioSource && hitSound)
                    audioSource.PlayOneShot(hitSound);
            }
        }
    }

    void OnEnable()
    {
        if (Melee != null)
            Melee.gameObject.SetActive(true);
    }

    void OnDisable()
    {
        if (Melee != null)
            Melee.gameObject.SetActive(false);
    }

    void OnDrawGizmosSelected()
    {
        if (attackPoint == null) return;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(attackPoint.position, attackRange);
    }
}
