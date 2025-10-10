    using UnityEngine;

/// <summary>
/// Copy of Bullet specifically for enemy projectiles.
/// Damages the player on hit and otherwise behaves identically to Bullet.
/// </summary>
public class EnemyBullet : MonoBehaviour
{
    [SerializeField] private int damage = 1;
    [SerializeField] private float speed = 100f;
    Rigidbody2D rb;

    private void Awake()
    {
        Destroy(gameObject, 2f);
    }

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        // Force bullet above floor
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            sr.sortingOrder = 10;
        }

        if (rb != null)
            rb.linearVelocity = transform.up * speed;
    }

    private void FixedUpdate()
    {
        // Keep physics-driven movement only.
    }

    public void SetDamage(int newDamage)
    {
        damage = newDamage;
    }

    public void SetSpeed(float newSpeed)
    {
        speed = newSpeed;
        if (rb == null) rb = GetComponent<Rigidbody2D>();
        if (rb != null)
            rb.linearVelocity = transform.up * speed;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Hurt the player and destroy the bullet
        if (collision.CompareTag("Player"))
        {
            // If your player health script uses a different API, change this accordingly.
            var ph = collision.GetComponent<PlayerHealth>();
            if (ph != null)
            {
                ph.TakeDamage(damage);
            }

            Destroy(gameObject);
        }
        else if (collision.CompareTag("Wall") || collision.CompareTag("Enemy"))
        {
            Destroy(gameObject);
        }
    }
}
