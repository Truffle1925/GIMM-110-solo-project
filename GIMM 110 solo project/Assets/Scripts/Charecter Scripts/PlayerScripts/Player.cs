using UnityEngine;

/// <summary>
/// Handles player movement, dashing, and weapon switching.
/// Inherits shared health and Rigidbody2D setup from Character.
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

    private StyleManager styleManager;

    protected override void Awake()
    {
        base.Awake();
        styleManager = Object.FindFirstObjectByType<StyleManager>();
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
    }

    private void FixedUpdate()
    {
        if (isDashing)
            rb.linearVelocity = dashDirection * dashSpeed;
        else
            rb.linearVelocity = movement * moveSpeed;
    }

    private void HandleInput()
    {
        float moveX = Input.GetAxis("Horizontal");
        float moveY = Input.GetAxis("Vertical");
        movement = new Vector2(moveX, moveY);

        if (Input.GetKeyDown(KeyCode.Space))
            TryStartDash();
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
}
