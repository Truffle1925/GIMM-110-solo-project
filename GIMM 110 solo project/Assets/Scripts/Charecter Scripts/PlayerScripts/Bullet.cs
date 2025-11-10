using UnityEngine;

public class Bullet : MonoBehaviour
{
    [SerializeField] private int damage = 1;
    [SerializeField] private float speed = 20f;
    Rigidbody2D rb;

    private void Awake()
    {
        // self-destruct after a short lifetime
        Destroy(gameObject, 5f);
    }

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            Debug.LogWarning($"Bullet '{name}' has no Rigidbody2D — add one and set gravityScale=0.", this);
            return;
        }

        // ensure sprite is rendered above floor if present
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null)
            sr.sortingOrder = 10;

        // set initial velocity based on current rotation (transform.up)
        rb.linearVelocity = transform.up * speed;
    }

    // Do not move the bullet in FixedUpdate manually - rely on Rigidbody2D velocity set once
    private void FixedUpdate()
    {
        // Intentionally empty to keep bullet velocity constant after spawn.
    }

    public void SetDamage(int newDamage)
    {
        damage = newDamage;
    }

    // Allows shooters to override speed immediately after Instantiate.
    public void SetSpeed(float newSpeed)
    {
        speed = newSpeed;
        if (rb == null) rb = GetComponent<Rigidbody2D>();
        if (rb != null)
            rb.linearVelocity = transform.up * speed;
        else
            Debug.LogWarning($"Bullet '{name}': SetSpeed called but no Rigidbody2D found.", this);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Destroy on hitting walls or other obstacles; do NOT damage the player from player bullets.
        if (collision.CompareTag("Wall") || collision.CompareTag("Enemy"))
        {
            Destroy(gameObject);
        }
    }
}

