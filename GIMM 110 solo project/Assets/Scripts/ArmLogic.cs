using UnityEngine;

public class ArmRotation : MonoBehaviour
{
    [SerializeField] Transform firingPoint; 

    private Camera mainCamera;

    void Start()
    {
        mainCamera = Camera.main;

        if (firingPoint == null)
        {
            Debug.LogError("Firing Point not assigned in the inspector.");
        }
    }

    void Update()
    {
        RotateArmToMouse();
    }

    void RotateArmToMouse()
    {
        Vector3 mouseWorldPos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPos.z = 0f;

        Vector3 direction = mouseWorldPos - transform.position;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        transform.rotation = Quaternion.Euler(0f, 0f, angle);
    }
}

