using UnityEngine;

[RequireComponent(typeof(Camera))]
public class HotlineMiamiCamera : MonoBehaviour
{
    public Transform player;            // Player to follow
    public Vector2 minBounds;           // Bottom-left world boundary (X,Z)
    public Vector2 maxBounds;           // Top-right world boundary (X,Z)
    public float smoothSpeed = 0.15f;   // How smooth the camera moves
    public float lookAheadStrength = 4f;// Camera lean toward mouse
    public Vector3 offset = new Vector3(0, 20, 0); // Camera height above player

    [Header("Camera Size")]
    public float cameraSize = 10f;      // 🔒 Set size in Inspector

    private Camera cam;
    private float halfHeight;
    private float halfWidth;
    private Quaternion fixedRotation;

    void Start()
    {
        cam = GetComponent<Camera>();

        // Force orthographic mode for Hotline Miami style
        cam.orthographic = true;
        cam.orthographicSize = cameraSize;

        UpdateCameraExtents();

        // 🔒 Lock rotation (top-down view)
        //fixedRotation = Quaternion.Euler(90f, 0f, 0f);
    }

    void Update()
    {
        // Update extents if size changes in Inspector during play
        if (Mathf.Abs(cam.orthographicSize - cameraSize) > Mathf.Epsilon)
        {
            cam.orthographicSize = cameraSize;
            UpdateCameraExtents();
        }
    }

    void LateUpdate()
    {
        if (player == null) return;

        // --- Mouse world position on XZ plane ---
        Plane plane = new Plane(Vector3.up, Vector3.zero);
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        Vector3 mouseWorld = player.position;
        if (plane.Raycast(ray, out float distance))
            mouseWorld = ray.GetPoint(distance);

        // --- Look-ahead toward mouse ---
        Vector3 lookAhead = (mouseWorld - player.position) / lookAheadStrength;

        // --- Target position (center on player + look-ahead) ---
        Vector3 targetPos = player.position + offset + new Vector3(lookAhead.x, 0, lookAhead.z);

        // --- Clamp inside bounds ---
        float clampedX = Mathf.Clamp(targetPos.x, minBounds.x + halfWidth, maxBounds.x - halfWidth);
        float clampedZ = Mathf.Clamp(targetPos.z, minBounds.y + halfHeight, maxBounds.y - halfHeight);

        Vector3 boundedPos = new Vector3(clampedX, targetPos.y, clampedZ);

        // --- Smooth camera move ---
        transform.position = Vector3.Lerp(transform.position, boundedPos, smoothSpeed);

        // --- Keep fixed rotation ---
        transform.rotation = fixedRotation;
    }

    private void UpdateCameraExtents()
    {
        halfHeight = cam.orthographicSize;
        halfWidth = halfHeight * cam.aspect;
    }
}


