using UnityEngine;

/// <summary>
/// Simple dash trigger. Top-down: any direction allowed. Only cooldown limits dashing.
/// Attach to the same GameObject as Movement2D. Press the configured key to dash.
/// </summary>
public class DodgeRoll : MonoBehaviour
{
    private Movement2D move;
    [Header("Dodge Settings")]
    [Tooltip("Key used for dash")]
    public KeyCode dashKey = KeyCode.Space;

    void Start()
    {
        move = GetComponent<Movement2D>();
        if (move == null)
            Debug.LogWarning("DodgeRoll: Movement2D component not found on the same GameObject.");
    }

    void Update()
    {   
        if (Input.GetKeyDown(dashKey) && move != null)
        {
            // TryStartDash handles cooldown and dash state internally.
            move.TryStartDash();
        }
    }
}
