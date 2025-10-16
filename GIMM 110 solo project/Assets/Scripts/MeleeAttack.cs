using UnityEngine;

public class MeleeAttack : MonoBehaviour
{
    [Header("Melee Settings")]
    public float attackRange = 1.5f;
    public float attackDamage = 25f;
    public float attackCooldown = 0.5f;
    public string enemyTag = "Enemy"; // Tag to detect enemies by

    [Header("References")]
    public Transform attackPoint;   // Empty GameObject in front of player
    public AudioSource audioSource; // Optional
    public AudioClip swingSound;
    public AudioClip hitSound;

    private float nextAttackTime = 0f;

    void Update()
    {
        // Attack when Fire1 is pressed (usually left click)
        if (Time.time >= nextAttackTime && Input.GetButtonDown("Fire1"))
        {
            Attack();
            nextAttackTime = Time.time + attackCooldown;
        }
    }

    void Attack()
    {
        // Play swing sound
        if (audioSource && swingSound)
            audioSource.PlayOneShot(swingSound);

        // Detect all colliders within range
        Collider2D[] hitObjects = Physics2D.OverlapCircleAll(attackPoint.position, attackRange);

        foreach (Collider2D obj in hitObjects)
        {
            // Only affect objects with the specified tag
            if (obj.CompareTag(enemyTag))
            {
                // Send damage message (if enemy has TakeDamage)
                obj.SendMessage("TakeDamage", attackDamage, SendMessageOptions.DontRequireReceiver);

                if (audioSource && hitSound)
                    audioSource.PlayOneShot(hitSound);
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        if (attackPoint == null) return;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(attackPoint.position, attackRange);
    }
}

