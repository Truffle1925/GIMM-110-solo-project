using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Animator))]
public abstract class Character : MonoBehaviour
{
    [Header("Character Settings")]
    public float moveSpeed = 5f;
    public int maxHealth = 3;

    [Header("Animation Settings")]
    public string moveXParam = "MoveX";
    public string moveYParam = "MoveY";
    public string speedParam = "Speed";
    public float moveThreshold = 0.05f;

    protected Rigidbody2D rb;
    protected Animator animator;
    protected int currentHealth;
    protected bool isDashing = false;
    protected bool isMoving = false;

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
    /// Updates animation parameters based on Rigidbody2D velocity.
    /// </summary>
    protected void UpdateAnimationFromVelocity()
    {
        if (!animator || !rb) return;

        Vector2 vel = rb.linearVelocity;
        float speed = vel.magnitude;
        isMoving = speed > moveThreshold;

        animator.SetBool("isMoving", isMoving); // ✅ Used for Move/Idle transitions
        animator.SetFloat(speedParam, speed);

        if (isMoving)
        {
            Vector2 dir = vel.normalized;
            animator.SetFloat(moveXParam, dir.x);
            animator.SetFloat(moveYParam, dir.y);
        }

        // ✅ Debug movement info
        Debug.Log($"[Character] Speed: {speed:F2} | isMoving: {isMoving} | Velocity: {vel}");
    }
}
