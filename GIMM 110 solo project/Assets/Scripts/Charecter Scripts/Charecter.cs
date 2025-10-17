using UnityEngine;
using System.Collections;

/// <summary>
/// Base class for all characters (player and enemies).
/// Handles shared elements such as health, movement references, and dashing structure.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public abstract class Character : MonoBehaviour
{
    [Header("Character Settings")]
    public float moveSpeed = 5f;
    public int maxHealth = 3;

    protected Rigidbody2D rb;
    protected int currentHealth;
    protected bool isDashing = false;

    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb == null) rb = gameObject.AddComponent<Rigidbody2D>();

        rb.gravityScale = 0f;
        rb.freezeRotation = true;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        currentHealth = maxHealth;
    }

    // Common health system
    public virtual void TakeDamage(int dmg)
    {
        currentHealth -= dmg;
        if (currentHealth <= 0)
            Die();
    }

    protected virtual void Die()
    {
        Destroy(gameObject);
    }
}

