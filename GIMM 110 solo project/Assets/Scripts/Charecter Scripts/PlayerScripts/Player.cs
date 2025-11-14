using UnityEngine;

/// <summary>
/// Handles player movement, dashing, leg rotation, and weapon switching.
/// Inherits shared physics and animation setup from Character.
/// </summary>
public class Player : Character
{
    [Header("Movement Settings")]
    private Vector2 movement;

    [Header("Dash Settings")]
    [SerializeField] float dashSpeed = 25f;
    [SerializeField] float dashDuration = 0.15f;
    [SerializeField] float dashCooldown = 1.0f;
    private float dashTimer = 0f;
    private float dashCooldownTimer = 0f;
    private Vector2 dashDirection = Vector2.zero;

    [Header("Weapon Switching")]
    public Shoot primaryShoot;
    public ShootAlternate secondaryShoot;
    private int selectedWeapon = 0; // 0 = primary, 1 = secondary

    [Header("Leg Animation")]
    public Transform legs;
    public Animator legAnimator;
    private Vector2 lastMoveDir;
    private Vector2 lastPosition;
    private StyleManager styleManager;

    protected override void Awake()
    {
        base.Awake();
        styleManager = Object.FindFirstObjectByType<StyleManager>();

        if (!legAnimator && legs != null)
            legAnimator = legs.GetComponent<Animator>();

        lastPosition = transform.position; // Initialize fallback velocity tracking
    }

    private void Start()
    {
        UpdateWeaponState();
    }

    private void Update()
    {
        HandleInput();
        RotateTowardsMouse();
        HandleWeaponSwitch();

        if (dashTimer > 0f)
        {
            dashTimer -= Time.deltaTime;
            if (dashTimer <= 0f) EndDash();
        }

        if (dashCooldownTimer > 0f)
            dashCooldownTimer -= Time.deltaTime;

        // ✅ Update animation parameters based on velocity
        UpdateAnimationFromVelocity();
    }

    private void FixedUpdate()
    {
        // Maintain your movement logic
        if (isDashing)
            rb.linearVelocity = dashDirection * dashSpeed;
        else
            rb.linearVelocity = movement * moveSpeed;

        HandleLegsAnimation();
    }

    private void HandleLegsAnimation()
    {
        if (!legs || !legAnimator) return;

        // Compute speed using physics or fallback
        Vector2 currentVelocity = rb.linearVelocity;

        // Fallback if linearVelocity is zero (e.g., new movement system)
        if (currentVelocity.sqrMagnitude < 0.0001f)
        {
            currentVelocity = ((Vector2)transform.position - lastPosition) / Time.fixedDeltaTime;
        }
        lastPosition = transform.position;

        float speed = currentVelocity.magnitude;
        bool legsMoving = speed > 0.05f;

        legAnimator.SetBool("isMoving", legsMoving);
        legAnimator.SetFloat("Speed", speed);

        if (legsMoving)
        {
            Vector2 moveDir = currentVelocity.normalized;
            lastMoveDir = moveDir;

            float angle = Mathf.Atan2(moveDir.y, moveDir.x) * Mathf.Rad2Deg - 90f;
            legs.rotation = Quaternion.Euler(0, 0, angle);
        }

        // ✅ Debug output
        Debug.Log($"[Player Legs] Speed: {speed:F2}, Moving: {legsMoving}, LastDir: {lastMoveDir}, Angle: {legs.rotation.eulerAngles.z:F1}");
    }

    private void HandleInput()
    {
        float moveX = Input.GetAxisRaw("Horizontal");
        float moveY = Input.GetAxisRaw("Vertical");
        movement = new Vector2(moveX, moveY).normalized;

        if (movement.sqrMagnitude > 0.01f)
            lastMoveDir = movement;

        if (Input.GetKeyDown(KeyCode.Space))
            TryStartDash();

        // ✅ Debug input
        Debug.Log($"[Player Input] Move: {movement}, Dashing: {isDashing}");
    }

    private void RotateTowardsMouse()
    {
        if (Camera.main == null) return;

        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 direction = (mousePos - transform.position).normalized;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        transform.rotation = Quaternion.Euler(0, 0, angle - 90);
    }

    private void HandleWeaponSwitch()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll > 0f)
            selectedWeapon = (selectedWeapon + 1) % 2;
        else if (scroll < 0f)
            selectedWeapon = (selectedWeapon - 1 + 2) % 2;

        if (Input.GetKeyDown(KeyCode.Alpha1)) selectedWeapon = 0;
        if (Input.GetKeyDown(KeyCode.Alpha2)) selectedWeapon = 1;

        UpdateWeaponState();
    }

    private void UpdateWeaponState()
    {
        if (primaryShoot) primaryShoot.enabled = (selectedWeapon == 0);
        if (secondaryShoot) secondaryShoot.enabled = (selectedWeapon == 1);
        styleManager?.OnWeaponSwitch();
    }

    public bool TryStartDash()
    {
        if (isDashing || dashCooldownTimer > 0f) return false;

        Vector2 input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        dashDirection = input.sqrMagnitude > 0.001f ? input.normalized : (Vector2)transform.up;

        StartDash();
        return true;
    }

    private void StartDash()
    {
        isDashing = true;
        dashTimer = dashDuration;
        dashCooldownTimer = dashCooldown;
        styleManager?.OnDodge();
    }

    private void EndDash()
    {
        isDashing = false;
    }

    public void AssignPickedWeapon(GameObject bulletPrefab)
    {
        if (!bulletPrefab) return;

        if (secondaryShoot != null)
        {
            secondaryShoot.bulletPrefab = bulletPrefab;
        }
        else
        {
            var added = gameObject.AddComponent<ShootAlternate>();
            added.bulletPrefab = bulletPrefab;
            added.firingPoint = primaryShoot ? primaryShoot.firingPoint : null;
            secondaryShoot = added;
        }

        selectedWeapon = 1;
        UpdateWeaponState();
    }

    // ✅ Debug-friendly animation update using linearVelocity or fallback
    private void UpdateAnimationFromVelocity()
    {
        if (!animator) return;

        Vector2 vel = rb.linearVelocity;
        if (vel.sqrMagnitude < 0.0001f)
            vel = ((Vector2)transform.position - lastPosition) / Time.deltaTime;

        float speed = vel.magnitude;
        bool isMoving = speed > 0.05f;

        animator.SetBool("IsMoving", isMoving);
        animator.SetFloat("Speed", speed);
        if (isMoving)
        {
            animator.SetFloat("MoveX", vel.x);
            animator.SetFloat("MoveY", vel.y);
        }

        Debug.Log($"[Character] Speed: {speed:F2} | isMoving: {isMoving} | Velocity: {vel}");
    }
}
