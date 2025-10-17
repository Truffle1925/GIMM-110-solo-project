using UnityEngine;
using System.Collections;

/// <summary>
/// Base enemy AI behavior. All specific enemy types inherit from this.
/// Handles chasing, shooting, and dying with drops.
/// </summary>
public class Enemy : Character
{
    [Header("Target")]
    public Transform player;

    [Header("Attack")]
    public GameObject bulletPrefab;
    public Transform shootPoint;
    public float bulletSpeed = 12f;
    public float minShootCooldown = 0.5f;
    public float maxShootCooldown = 1.5f;

    [Header("Movement")]
    public float strafeSpeed = 3f;
    public float strafeWiggleAmount = 0.5f;
    public float minDistance = 2f;
    public float maxDistance = 6f;

    [Header("Evasion / Dash")]
    public float dashForce = 10f;
    public float dashDuration = 0.2f;
    public float dashCooldown = 3f;
    protected bool canDash = true;

    [Header("Drops")]
    public GameObject dropPrefabA;
    public GameObject dropPrefabB;
    public GameObject dropPrefabC;
    [Range(0f, 1f)] public float dropChance = 0.5f;
    [Range(0f, 1f)] public float dropAChance = 0.5f;
    [Range(0f, 1f)] public float dropBChance = 0.5f;
    [Range(0f, 1f)] public float dropCChance = 0.2f;

    [Header("Avoidance")]
    public LayerMask obstacleMask;
    public float avoidanceStrength = 5f;

    private float shootTimer;
    private float strafeTimer;
    private int strafeDir = 1;

    protected override void Awake()
    {
        base.Awake();
        shootTimer = Random.Range(minShootCooldown, maxShootCooldown);
    }

    protected virtual void Start()
    {
        if (player == null)
        {
            GameObject pgo = GameObject.FindWithTag("Player");
            if (pgo) player = pgo.transform;
        }
    }

    protected virtual void Update()
    {
        if (!player) return;
        if (!isDashing) MoveAggressively();
        HandleAttack();
        if (canDash && Random.value < 0.005f) StartCoroutine(Dash());
    }

    protected virtual void MoveAggressively()
    {
        Vector2 toPlayer = (player.position - transform.position);
        float dist = toPlayer.magnitude;
        Vector2 dirToPlayer = toPlayer.normalized;

        Vector2 radial = Vector2.zero;
        if (dist < minDistance) radial = -dirToPlayer * moveSpeed;
        else if (dist > maxDistance) radial = dirToPlayer * moveSpeed;

        strafeTimer -= Time.deltaTime;
        if (strafeTimer <= 0f)
        {
            strafeDir = Random.value > 0.5f ? 1 : -1;
            strafeTimer = Random.Range(0.5f, 1.5f);
        }

        Vector2 perpBase = new Vector2(-dirToPlayer.y, dirToPlayer.x);
        Vector2 perp = perpBase * strafeDir * strafeSpeed;
        perp += perpBase * Random.Range(-0.5f * strafeWiggleAmount, 0.5f * strafeWiggleAmount);

        Vector2 desiredVel = radial + perp;

        float lookDist = Mathf.Max(0.5f, Mathf.Min(1.5f, dist * 0.6f));
        Vector2 castDir = desiredVel.sqrMagnitude > 0.0001f ? desiredVel.normalized : dirToPlayer;
        RaycastHit2D hit = Physics2D.CircleCast(transform.position, 0.15f, castDir, lookDist, obstacleMask);
        if (hit.collider != null)
        {
            Vector2 away = ((Vector2)transform.position - hit.point).normalized;
            Vector2 normal = hit.normal;
            Vector2 tangent = new Vector2(-normal.y, normal.x);
            float sign = Vector2.Dot(tangent, perp) >= 0f ? 1f : -1f;
            Vector2 steer = (away + tangent * sign * 0.5f).normalized * Mathf.Max(avoidanceStrength, moveSpeed);
            desiredVel += steer;
        }

        desiredVel = Vector2.ClampMagnitude(desiredVel, moveSpeed);
        rb.linearVelocity = Vector2.Lerp(rb.linearVelocity, desiredVel, 0.2f);
        transform.up = dirToPlayer;
    }

    protected virtual void HandleAttack()
    {
        shootTimer -= Time.deltaTime;
        if (shootTimer <= 0f && CanSeePlayer())
        {
            ShootPredictive();
            shootTimer = Random.Range(minShootCooldown, maxShootCooldown);
        }
    }

    protected virtual void ShootPredictive()
    {
        if (!bulletPrefab || !shootPoint) return;

        Vector2 shootDir = (player.position - shootPoint.position).normalized;
        GameObject bullet = Instantiate(bulletPrefab, shootPoint.position, Quaternion.identity);
        bullet.transform.up = shootDir;
        Rigidbody2D bulletRb = bullet.GetComponent<Rigidbody2D>();
        if (bulletRb) bulletRb.linearVelocity = shootDir * bulletSpeed;
    }

    protected virtual IEnumerator Dash()
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

    protected virtual bool CanSeePlayer()
    {
        Vector2 dir = player.position - transform.position;
        RaycastHit2D hit = Physics2D.Raycast(transform.position, dir, dir.magnitude, obstacleMask);
        if (hit.collider == null) return true;
        return hit.collider.CompareTag("Player");
    }

    protected override void Die()
    {
        if (Random.value < dropChance)
        {
            // Handle A/B drop
            float roll = Random.value;
            GameObject toDrop = null;

            if (roll < dropAChance)
            {
                toDrop = dropPrefabA;
            }
            else if (roll < dropAChance + dropBChance)
            {
                toDrop = dropPrefabB;
            }

            if (toDrop != null)
                Instantiate(toDrop, transform.position, Quaternion.identity);
        }

        // Handle drop C separately (independent of the others)
        if (Random.value < dropCChance)
        {
            if (dropPrefabC != null)
                Instantiate(dropPrefabC, transform.position, Quaternion.identity);
        }


        Object.FindFirstObjectByType<StyleManager>()?.OnKill(100);
        base.Die();
    }

    protected virtual void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("PlayerBullet"))
        {
            TakeDamage(1);
            Destroy(other.gameObject);
        }
    }
}
