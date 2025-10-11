using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D))]
public class FastEnemyAI : MonoBehaviour
{
    [Header("References")]
    public Transform player;
    public GameObject bulletPrefab;
    public Transform shootPoint;

    [Header("Movement")]
    public float moveSpeed = 5f;
    public float strafeSpeed = 3f;
    public float strafeWiggleAmount = 0.5f;
    public float minDistance = 2f;  
    public float maxDistance = 6f;

    [Header("Attack")]
    public float bulletSpeed = 12f;
    public float minShootCooldown = 0.5f;
    public float maxShootCooldown = 1.5f;

    [Header("Dash/Evasion")]
    public float dashForce = 10f;
    public float dashDuration = 0.2f;
    public float dashCooldown = 3f;

    [Header("Health")]
    public int maxHealth = 3;

    [Header("Avoidance")]
    public LayerMask obstacleMask;
    [Tooltip("Strength of the avoidance steering")]
    public float avoidanceStrength = 5f;

    private Rigidbody2D rb;
    private float shootTimer;
    private float strafeTimer;
    private int strafeDir = 1;
    private bool isDashing = false;
    private bool canDash = true;
    private int currentHealth;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb == null) rb = gameObject.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.freezeRotation = true;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        currentHealth = maxHealth;
        shootTimer = Random.Range(minShootCooldown, maxShootCooldown);

        // ensure player assigned
        if (player == null)
        {
            var pgo = GameObject.FindWithTag("Player");
            if (pgo) player = pgo.transform;
        }
    }

    void Update()
    {
        if (!player) return;
        if (!isDashing)
            MoveAggressively();
        HandleAttack();
        if (canDash && Random.value < 0.005f)
            StartCoroutine(Dash());
    }

    void MoveAggressively()
    {
        Vector2 toPlayer = (player.position - transform.position);
        float dist = toPlayer.magnitude;
        Vector2 dirToPlayer = toPlayer.normalized;

        // desired radial movement: approach or retreat if outside/inside ranges
        Vector2 radial = Vector2.zero;
        if (dist < minDistance) radial = -dirToPlayer * moveSpeed;
        else if (dist > maxDistance) radial = dirToPlayer * moveSpeed;

        // strafing perpendicular component (explicit perpendicular)
        strafeTimer -= Time.deltaTime;
        if (strafeTimer <= 0f)
        {
            strafeDir = Random.value > 0.5f ? 1 : -1;
            strafeTimer = Random.Range(0.5f, 1.5f);
        }
        Vector2 perpBase = new Vector2(-dirToPlayer.y, dirToPlayer.x);
        Vector2 perp = perpBase * strafeDir * strafeSpeed;
        perp += perpBase * Random.Range(-0.5f * strafeWiggleAmount, 0.5f * strafeWiggleAmount);

        // combine desired velocity
        Vector2 desiredVel = radial + perp;
        if (desiredVel.sqrMagnitude < 0.01f)
            desiredVel = dirToPlayer * (moveSpeed * 0.5f); // keep some movement if idle

        // obstacle avoidance: circlecast ahead; steer away if wall detected
        float lookDist = Mathf.Max(0.5f, Mathf.Min(1.5f, dist * 0.6f)); // scale look distance with range
        Vector2 castDir = desiredVel.sqrMagnitude > 0.0001f ? desiredVel.normalized : dirToPlayer;
        RaycastHit2D hit = Physics2D.CircleCast((Vector2)transform.position, 0.15f, castDir, lookDist, obstacleMask);
        if (hit.collider != null)
        {
            // compute steer away vector using hit normal and a tangent
            Vector2 away = ((Vector2)transform.position - hit.point).normalized;
            Vector2 normal = hit.normal;
            Vector2 tangent = new Vector2(-normal.y, normal.x);
            float sign = Vector2.Dot(tangent, perp) >= 0f ? 1f : -1f;
            Vector2 steer = (away + tangent * sign * 0.5f).normalized * Mathf.Max(avoidanceStrength, moveSpeed);
            desiredVel += steer;
        }

        // clamp and smooth velocity so collisions are handled by physics
        desiredVel = Vector2.ClampMagnitude(desiredVel, moveSpeed);
        rb.linearVelocity = Vector2.Lerp(rb.linearVelocity, desiredVel, 0.2f); // smoothing factor, tweak as needed

        // face player
        if (dirToPlayer.sqrMagnitude > 0.0001f)
            transform.up = dirToPlayer;
    }

    void HandleAttack()
    {
        shootTimer -= Time.deltaTime;

        if (shootTimer <= 0f && CanSeePlayer())
        {
            ShootPredictive();
            shootTimer = Random.Range(minShootCooldown, maxShootCooldown);
        }
    }

    void ShootPredictive()
    {
        if (!bulletPrefab || !shootPoint) return;

        Vector2 shootDir = (player.position - shootPoint.position).normalized;

        GameObject bullet = Instantiate(bulletPrefab, shootPoint.position, Quaternion.identity);
        bullet.transform.up = shootDir;

        Rigidbody2D bulletRb = bullet.GetComponent<Rigidbody2D>();
        if (bulletRb)
            bulletRb.linearVelocity = shootDir * bulletSpeed;
    }

    IEnumerator Dash()
    {
        canDash = false;
        isDashing = true;
        Vector2 dashDir = (Random.insideUnitCircle + (Vector2)(player.position - transform.position)).normalized;
        rb.AddForce(dashDir * dashForce, ForceMode2D.Impulse);
        yield return new WaitForSeconds(dashDuration);
        isDashing = false;
        yield return new WaitForSeconds(dashCooldown);
        canDash = true;
    }

    bool CanSeePlayer()
    {
        Vector2 dir = player.position - transform.position;
        RaycastHit2D hit = Physics2D.Raycast(transform.position, dir, dir.magnitude, obstacleMask);
        if (hit.collider == null) return true;
        return hit.collider.CompareTag("Player");
    }

    public void TakeDamage(int dmg)
    {
        currentHealth -= dmg;
        if (currentHealth <= 0) Die();
    }

    void Die()
    {
        // Add effects here (explosion, sound, etc.)
        Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("PlayerBullet"))
        {
            TakeDamage(1);
            Destroy(other.gameObject);
        }
    }
}



