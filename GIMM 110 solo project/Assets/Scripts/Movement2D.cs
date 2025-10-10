using UnityEngine;

public class Movement2D : MonoBehaviour
{
    #region Variables
    [Header("Movement Settings")]
    Rigidbody2D rb;
    Vector2 movement;

    // SerializeField allows you to see private variables in the inspector while keeping them private
    [SerializeField] float moveSpeed = 10f; // f is used to specify that the number is a float

    [Header("Dash Settings")]
    [Tooltip("Dash speed in units/sec")]
    [SerializeField] float dashSpeed = 25f;
    [Tooltip("How long the dash lasts (seconds)")]
    [SerializeField] float dashDuration = 0.15f;
    [Tooltip("Cooldown after a dash (seconds)")]
    [SerializeField] float dashCooldown = 1.0f;

    private bool isDashing = false;
    private float dashTimer = 0f;
    private float dashCooldownTimer = 0f;
    private Vector2 dashDirection = Vector2.zero;

    [Header("Weapon Switching")]
    [Tooltip("Assign your primary Shoot component (player GameObject)")]
    public Shoot primaryShoot;
    [Tooltip("Assign your secondary Shoot component (player GameObject)")]
    public ShootAlternate secondaryShoot;

    private int selectedWeapon = 0; // 0 = primary, 1 = secondary
    #endregion // Marks the end of the region

    #region Unity Methods
    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        UpdateWeaponState();
    }

    private void Update()
    {
        // Read input each frame (used for normal movement)
        PlayerInput();
        RotateTowardsMouse();
        HandleWeaponSwitchInput();

        // Dash timers
        if (dashTimer > 0f)
        {
            dashTimer -= Time.deltaTime;
            if (dashTimer <= 0f)
                EndDash();
        }

        if (dashCooldownTimer > 0f)
            dashCooldownTimer -= Time.deltaTime;
    }

    private void FixedUpdate()
    {
        if (isDashing)
        {
            rb.linearVelocity = dashDirection * dashSpeed;        
        }
        else
        {
            rb.linearVelocity = movement * moveSpeed;
        }
    }
    #endregion

    #region Custom Methods
    private void PlayerInput()
    {
        float moveX = Input.GetAxis("Horizontal"); // A/D or Left/Right arrow
        float moveY = Input.GetAxis("Vertical");   // W/S or Up/Down arrow
        movement = new Vector2(moveX, moveY);
    }

    private void RotateTowardsMouse()
    {
        if (Camera.main == null) return;
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 direction = (mousePos - transform.position).normalized;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle - 90));
    }

    private void HandleWeaponSwitchInput()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll > 0f)
        {
            selectedWeapon = (selectedWeapon + 1) % 2;
            UpdateWeaponState();
        }
        else if (scroll < 0f)
        {
            selectedWeapon = (selectedWeapon - 1 + 2) % 2;
            UpdateWeaponState();
        }

        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            selectedWeapon = 0;
            UpdateWeaponState();
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            selectedWeapon = 1;
            UpdateWeaponState();
            Debug.Log("2 key detected");
        }
    }

    private void UpdateWeaponState()
    {
        if (primaryShoot != null) primaryShoot.enabled = (selectedWeapon == 0);
        if (secondaryShoot != null) secondaryShoot.enabled = (selectedWeapon == 1);
    }

    /// <summary>
    /// Start a dash; dash direction is taken from current input axes (allows dashing in any direction).
    /// If there is no input, dash will use the player's facing direction (transform.up).
    /// Returns true if dash started, false if dash was on cooldown or already active.
    /// </summary>
    public bool TryStartDash()
    {
        if (isDashing) return false;
        if (dashCooldownTimer > 0f) return false;

        // Prefer raw input so dash direction isn't dependent on update ordering
        float inputX = Input.GetAxisRaw("Horizontal");
        float inputY = Input.GetAxisRaw("Vertical");
        Vector2 input = new Vector2(inputX, inputY);

        if (input.sqrMagnitude > 0.0001f)
            dashDirection = input.normalized;
        else
            dashDirection = (Vector2)transform.up;

        StartDash();
        return true;
    }

    private void StartDash()
    {
        isDashing = true;
        dashTimer = dashDuration;
        dashCooldownTimer = dashCooldown;
    }

    private void EndDash()
    {
        isDashing = false;
        dashTimer = 0f;
        // normal movement will resume automatically in FixedUpdate
    }

    /// <summary>
    /// Returns true when dash is available to start (not currently dashing and not on cooldown).
    /// </summary>
    public bool CanDash()
    {
        return !isDashing && dashCooldownTimer <= 0f;
    }
    #endregion
}

