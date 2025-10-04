using UnityEngine;

public class Movement2D : MonoBehaviour
{
    // Regions help to visually organize your code into sections.
    #region Variables
    // Headers are like titles for the Unity Inspector.
    [Header("Movement Settings")]

    /* In C# if you do not specify a variable modifier (i.e. public, private, protected), it defaults to private
    The private variable modifier stops other scripts from accessing those variables */
    Rigidbody2D rb;
    Vector2 movement;

    // SerializeField allows you to see private variables in the inspector while keeping them private
    [SerializeField] float moveSpeed = 10f; // f is used to specify that the number is a float
    #endregion // Marks the end of the region

    #region Unity Methods
    // Start is called before the first frame update
    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    private void Update()
    {
        // Calls the PlayerInput method on every frame
        PlayerInput();
        //new method to make the player face the mouse
        RotateTowardsMouse();
    }

    // FixedUpdate is used for physics calculations and is called 50 times a second
    private void FixedUpdate()
    {
        // You don't need to multiply the movement by Time.deltaTime because the physics calculations are already frame-rate independent
        rb.linearVelocity = movement * moveSpeed;
    }
    #endregion

    #region Custom Methods
    /// <summary>
    /// Handles player input
    /// </summary>
    private void PlayerInput()
    {
        //movement = new Vector2(0f, Input.GetAxis("Vertical")); // Gets the vertical input
        //old script
    
        float moveX = Input.GetAxis("Horizontal"); // A/D or Left/Right arrow
        float moveY = Input.GetAxis("Vertical");   // W/S or Up/Down arrow
        movement = new Vector2(moveX, moveY);
    

    }
    //new method to force the player to always face the location of the mouse
    private void RotateTowardsMouse()
    {
        // Get mouse position in world space
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        // Direction from player to mouse
        Vector2 direction = (mousePos - transform.position).normalized;

        // Calculate angle in degrees
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        // Apply rotation (subtract 90 if your sprite points up by default)
        transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle - 90));
    }
    #endregion
}

