using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Animator))]
public abstract class Character : MonoBehaviour
{
    [Header("Character Settings")]
    public float moveSpeed = 5f;
    public int maxHealth = 3;

    [Header("Animation Settings")]
    public string moveXParam = "MoveX";  // Animator parameter names
    public string moveYParam = "MoveY";
    public string speedParam = "Speed";

    protected Rigidbody2D rb;
    protected Animator animator;
    protected int currentHealth;
    protected bool isDashing = false;

    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();

        rb.gravityScale = 0f;
        rb.freezeRotation = true;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        currentHealth = maxHealth;
    }

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

    /// <summary>
    /// Updates the animator blend values based on facing direction and movement vector.
    /// </summary>
    protected void UpdateAnimation(Vector2 moveInput, Vector2 facingDir)
    {
        if (animator == null) return;

        // Convert movement into local space relative to facing direction
        float moveRight = Vector2.Dot(moveInput.normalized, facingDir);                  // Forward/back
        float moveUp = Vector2.Dot(moveInput.normalized, new Vector2(-facingDir.y, facingDir.x)); // Sideways

        // Set animator parameters
        animator.SetFloat(moveXParam, moveRight);
        animator.SetFloat(moveYParam, moveUp);
        animator.SetFloat(speedParam, moveInput.sqrMagnitude);
    }
}
