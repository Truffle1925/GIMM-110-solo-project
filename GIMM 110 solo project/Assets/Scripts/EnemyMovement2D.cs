using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    [Header("Movement Settings")]
    public Transform player;
    public float moveSpeed = 3f;
    public float minDistance = 3f;
    public float maxDistance = 6f;
    public float strafeSpeed = 3f;
    public float strafeChangeInterval = 1.5f;
    public float strafeWiggleAmount = 0.5f;

    [Header("Shooting Settings")]
    public GameObject bulletPrefab;
    public Transform shootPoint;
    public float shootCooldown = 2f;
    public float bulletSpeed = 10f;
    [SerializeField] private int bulletDamage = 1; // Damage per bullet

    [Header("Health Settings")]
    [SerializeField] private int maxHealth = 3;
    private int currentHealth;

    private float shootTimer;
    private float strafeTimer;
    private int strafeDirection = 1;

    void Start()
    {
        currentHealth = maxHealth;
    }

    void Update()
    {
        if (player != null)
        {
            MoveEnemy();
            HandleShooting();
        }
    }

    void MoveEnemy()
    {
        Vector3 direction = player.position - transform.position;
        float distance = direction.magnitude;
        Vector3 move = Vector3.zero;

        if (distance > maxDistance)
            move += direction.normalized * moveSpeed * Time.deltaTime;
        else if (distance < minDistance)
            move -= direction.normalized * moveSpeed * Time.deltaTime;

        strafeTimer -= Time.deltaTime;
        if (strafeTimer <= 0f)
        {
            strafeDirection = Random.value > 0.5f ? 1 : -1;
            strafeTimer = strafeChangeInterval;
        }

        Vector3 strafeDir = Vector3.Cross(direction.normalized, Vector3.forward) * strafeDirection;
        strafeDir += Vector3.Cross(direction.normalized, Vector3.forward) * Random.Range(-strafeWiggleAmount, strafeWiggleAmount);
        move += strafeDir * strafeSpeed * Time.deltaTime;

        transform.position += move;
        transform.up = direction.normalized;
    }

    void HandleShooting()
    {
        shootTimer -= Time.deltaTime;

        if (shootTimer <= 0f)
        {
            ShootPredicted();
            shootTimer = shootCooldown;
        }
    }

    void ShootPredicted()
    {
        if (bulletPrefab != null && shootPoint != null)
        {
            Rigidbody2D playerRb = player.GetComponent<Rigidbody2D>();
            Vector3 targetPosition = player.position;

            if (playerRb != null)
            {
                Vector3 playerVelocity = playerRb.linearVelocity;
                float distance = Vector3.Distance(transform.position, player.position);
                float timeToReach = distance / bulletSpeed;
                targetPosition += playerVelocity * timeToReach;
            }

            Vector3 shootDirection = (targetPosition - shootPoint.position).normalized;
            GameObject bullet = Instantiate(bulletPrefab, shootPoint.position, Quaternion.identity);
            bullet.transform.up = shootDirection;

            Rigidbody2D bulletRb = bullet.GetComponent<Rigidbody2D>();
            if (bulletRb != null)
            {
                bulletRb.linearVelocity = shootDirection * bulletSpeed;
            }

            // Set the bullet's damage if it has a Bullet script
            Bullet bulletScript = bullet.GetComponent<Bullet>();
            if (bulletScript != null)
            {
                bulletScript.SetDamage(bulletDamage);
            }
        }
    }

    public void TakeDamage(int amount)
    {
        currentHealth -= amount;
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("PlayerBullet"))
        {
            TakeDamage(1);
            Destroy(collision.gameObject);
            Debug.Log(currentHealth);
        }
    }
}


