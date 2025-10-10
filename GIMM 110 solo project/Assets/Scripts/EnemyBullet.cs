using UnityEngine;

/// <summary>
/// Enemy projectile: constant velocity set once on spawn, deals damage to player on hit,
/// and is destroyed when hitting walls or other obstacles.
/// </summary>
public class EnemyBullet : MonoBehaviour
{
    [SerializeField] private int damage = 1;
    [SerializeField] private float speed = 20f;
    private Rigidbody2D rb;

    private void Awake()
    {
        // Lifetime safeguard
        Destroy(gameObject, 5f);
    }

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        // Ensure bullet sprite renders above floor if present
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null)
            sr.sortingOrder = 10;

        // Set initial velocity once and never change it afterwards
        if (rb != null)
            rb.linearVelocity = transform.up * speed;
    }

    // Intentionally empty so the bullet keeps the initial velocity set in Start()
    private void FixedUpdate() { }

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
        // Damage player and destroy bullet
        if (collision.CompareTag("Player"))
        {
            var ph = collision.GetComponent<PlayerHealth>();
            if (ph != null)
            {
                ph.TakeDamage(damage);
            }

            Destroy(gameObject);
            return;
        }

        // Destroy on hitting walls or other obstacles
        if (collision.CompareTag("Wall") || collision.CompareTag("Obstacle") || collision.CompareTag("Environment"))
        {
            Destroy(gameObject);
            return;
        }

        // Destroy on hitting other entities as needed
        if (collision.CompareTag("Bullet") || collision.CompareTag("Enemy"))
        {
            Destroy(gameObject);
            return;
        }
    }
}
