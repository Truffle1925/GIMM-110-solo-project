using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Enemy AI with FSM for waypoint pathfinding and player chasing.
/// Waypoint state handles navigation between rooms.
/// Chasing state handles movement, dodging, and attacks.
/// Adds leg animation handling similar to Player.
/// </summary>
[RequireComponent(typeof(Rigidbody2D), typeof(Animator))]
public class Enemy : Character
{
    public enum EnemyState { WaypointPathfinding, PlayerChasing }

    [Header("FSM")]
    public EnemyState currentState = EnemyState.WaypointPathfinding;

    [Header("Target")]
    public Transform player;

    [Header("Attack")]
    public GameObject bulletPrefab;
    public Transform shootPoint;
    public float bulletSpeed = 12f;
    public float minShootCooldown = 0.5f;
    public float maxShootCooldown = 1.5f;

    [Header("Sound")]
    public AudioClip shootSound;
    public AudioSource audioSource;

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
    [Range(0f, 1f)] public float dropCChance = 0.5f;

    [Header("Avoidance")]
    public LayerMask obstacleMask;
    public float avoidanceStrength = 5f;

    [Header("Waypoint Pathfinding")]
    public float roomChaseRange = 6f;
    public GameObject currentRoom;
    private WaypointManager waypointManager;
    private List<GameObject> currentPath = new List<GameObject>();
    private int pathIndex = 0;
    private Room currentPlayerRoom = null;
    public float baseMoveSpeedMultiplier = 1.3f;
    public float chaseSpeedMultiplier = 2.2f;
    private float pathRefreshTimer = 0f;
    public float pathRefreshInterval = 2f;
    private Room lastKnownPlayerRoom = null;

    public float baseScoreValue = 100f;
    private StyleManager styleManager;

    private float shootTimer;
    private float strafeTimer;
    private int strafeDir = 1;

    public WaveManagerTMP waveManager;

    [Header("Leg Animation")]
    public Transform legs;
    public Animator legAnimator;
    private Vector2 lastMoveDir;

    protected override void Awake()
    {
        base.Awake();
        shootTimer = Random.Range(minShootCooldown, maxShootCooldown);
        waypointManager = FindObjectOfType<WaypointManager>();
        styleManager = FindObjectOfType<StyleManager>();

        if (!legAnimator && legs != null)
            legAnimator = legs.GetComponent<Animator>();

        if (!audioSource)
            audioSource = GetComponent<AudioSource>();
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

        UpdatePlayerRoomStatus();
        UpdateState();

        switch (currentState)
        {
            case EnemyState.WaypointPathfinding:
                WaypointPathfindingUpdate();
                break;
            case EnemyState.PlayerChasing:
                PlayerChasingUpdate();
                break;
        }

        // Update top-body animator if any (from Character)
        UpdateAnimationFromVelocity();
    }

    private void FixedUpdate()
    {
        HandleLegsAnimation();
    }

    private void HandleLegsAnimation()
    {
        if (!legs || !legAnimator) return;

        float speed = rb.linearVelocity.magnitude;
        bool legsMoving = speed > 0.05f;

        legAnimator.SetBool("isMoving", legsMoving);
        legAnimator.SetFloat("Speed", speed);

        if (legsMoving)
        {
            Vector2 moveDir = rb.linearVelocity.normalized;
            lastMoveDir = moveDir;
            float angle = Mathf.Atan2(moveDir.y, moveDir.x) * Mathf.Rad2Deg - 90f;
            legs.rotation = Quaternion.Euler(0, 0, angle);
        }

        Debug.Log($"[Enemy Legs] Speed: {speed:F2}, Moving: {legsMoving}, LastDir: {lastMoveDir}, Angle: {legs.rotation.eulerAngles.z:F1}");
    }

    void UpdateState()
    {
        bool inSameRoom = PlayerInSameRoom();

        if (inSameRoom && currentState != EnemyState.PlayerChasing)
        {
            SwitchState(EnemyState.PlayerChasing);
        }
        else if (!inSameRoom && currentState != EnemyState.WaypointPathfinding)
        {
            SwitchState(EnemyState.WaypointPathfinding);
        }
    }

    void SwitchState(EnemyState newState)
    {
        if (currentState == newState) return;
        currentState = newState;
        Debug.Log($"{name} switched to state: {currentState}");
    }

    void WaypointPathfindingUpdate()
    {
        pathRefreshTimer -= Time.deltaTime;
        if (pathRefreshTimer <= 0f)
        {
            BuildPathToPlayerRoom();
            pathRefreshTimer = pathRefreshInterval;
        }

        FollowPath();
    }

    void BuildPathToPlayerRoom()
    {
        if (waypointManager == null || player == null) return;

        PlayerRoomTracker tracker = player.GetComponent<PlayerRoomTracker>();
        Room playerRoom = tracker != null && tracker.currentRoom != null ? tracker.currentRoom.GetComponent<Room>() : null;

        if (playerRoom == null)
        {
            Debug.LogWarning("Player room not found; using closest waypoint to player position.");
        }

        if (playerRoom != lastKnownPlayerRoom)
        {
            GameObject startWP = waypointManager.GetClosestWaypoint(transform.position);
            GameObject goalWP = playerRoom != null
                ? waypointManager.GetRoomRepresentativeWaypoint(playerRoom)
                : waypointManager.GetClosestWaypoint(player.position);

            if (startWP == null || goalWP == null)
            {
                Debug.LogWarning("Cannot build path: missing start or goal waypoint");
                return;
            }

            currentPath = FindPathBFS(startWP, goalWP);
            pathIndex = 0;
            lastKnownPlayerRoom = playerRoom;
        }
    }

    void FollowPath()
    {
        if (currentPath == null || currentPath.Count == 0) return;
        if (pathIndex >= currentPath.Count) return;

        GameObject targetWaypoint = currentPath[pathIndex];
        Vector2 dir = (targetWaypoint.transform.position - transform.position).normalized;
        rb.MovePosition(rb.position + dir * moveSpeed * baseMoveSpeedMultiplier * Time.deltaTime);

        if (dir.sqrMagnitude > 0.0001f)
            transform.up = dir;

        if (legAnimator != null)
        {
            legAnimator.SetBool("isMoving", true);
            legAnimator.SetFloat("Speed", moveSpeed * baseMoveSpeedMultiplier);
            lastMoveDir = dir;

            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 90f;
            if (legs != null)
                legs.rotation = Quaternion.Euler(0, 0, angle);
        }

        float dist = Vector2.Distance(transform.position, targetWaypoint.transform.position);
        if (dist < 0.2f)
        {
            pathIndex++;
            if (pathIndex >= currentPath.Count)
                BuildPathToPlayerRoom();
        }
    }

    List<GameObject> FindPathBFS(GameObject start, GameObject goal)
    {
        List<GameObject> openList = new List<GameObject>();
        Dictionary<GameObject, GameObject> cameFrom = new Dictionary<GameObject, GameObject>();

        openList.Add(start);
        cameFrom[start] = null;

        while (openList.Count > 0)
        {
            GameObject current = openList[0];
            openList.RemoveAt(0);

            if (current == goal)
                return ReconstructPath(cameFrom, start, goal);

            Waypoint wp = current.GetComponent<Waypoint>();
            if (wp == null) continue;

            foreach (var conn in wp.connections)
            {
                GameObject neighbor = conn.target;
                if (neighbor != null && !cameFrom.ContainsKey(neighbor))
                {
                    openList.Add(neighbor);
                    cameFrom[neighbor] = current;
                }
            }
        }

        return new List<GameObject>();
    }

    List<GameObject> ReconstructPath(Dictionary<GameObject, GameObject> cameFrom, GameObject start, GameObject goal)
    {
        List<GameObject> path = new List<GameObject>();
        GameObject current = goal;

        while (current != null)
        {
            path.Add(current);
            current = cameFrom.ContainsKey(current) ? cameFrom[current] : null;
        }

        path.Reverse();
        return path;
    }

    void PlayerChasingUpdate()
    {
        MoveAggressively();
        HandleAttack();

        if (canDash && Random.value < 0.005f)
            StartCoroutine(Dash());
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

        // 🔊 Call the new sound method when firing
        PlayShootSound();
    }

    // --- NEW METHOD ---
    protected virtual void PlayShootSound()
    {
        if (audioSource && shootSound)
            audioSource.PlayOneShot(shootSound);
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
            GameObject toDrop = (Random.value < dropAChance) ? dropPrefabA : dropPrefabB;
            if (toDrop != null) Instantiate(toDrop, transform.position, Quaternion.identity);
        }

        if (Random.value < dropCChance)
        {
            if (dropPrefabC != null) Instantiate(dropPrefabC, transform.position, Quaternion.identity);
        }

        waveManager?.RemoveEnemy(gameObject);
        styleManager?.OnKill(baseScoreValue);
        base.Die();
    }

    protected virtual void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("PlayerBullet"))
        {
            TakeDamage(1);
            Destroy(other.gameObject);
        }

        if (other.CompareTag("RoomBounds"))
        {
            Room room = other.GetComponentInParent<Room>();
            if (room != null) currentRoom = room.gameObject;
        }
    }

    protected virtual void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("RoomBounds"))
        {
            Room room = other.GetComponentInParent<Room>();
            if (room != null && currentRoom == room.gameObject) currentRoom = null;
        }
    }

    void UpdatePlayerRoomStatus()
    {
        if (player == null) return;

        PlayerRoomTracker tracker = player.GetComponent<PlayerRoomTracker>();
        currentPlayerRoom = tracker != null && tracker.currentRoom != null ? tracker.currentRoom.GetComponent<Room>() : null;
    }

    bool PlayerInSameRoom()
    {
        if (currentRoom == null || currentPlayerRoom == null)
            return false;

        return currentRoom == currentPlayerRoom.gameObject;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = currentState == EnemyState.WaypointPathfinding ? Color.yellow : Color.red;
        Gizmos.DrawSphere(transform.position, 0.3f);
    }
}
