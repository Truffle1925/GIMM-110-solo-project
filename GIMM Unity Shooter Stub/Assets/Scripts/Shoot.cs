using UnityEngine;

public class Shoot : MonoBehaviour
{
    // Regions help to visually organize your code into sections.
    #region Variables
    // Headers are like titles for the Unity Inspector.
    [Header("References")]

    // SerializeField allows you to see private variables in the inspector while keeping them private
    [SerializeField] GameObject bulletPrefab;
    [SerializeField] Transform firingPoint;
    [SerializeField] GameObject bulletContainer;
    #endregion // Marks the end of the region

    #region Unity Methods
    // Update is called once per frame
    private void Update()
    {
        // Calls the ShootBullet method on every frame
        ShootBullet();


        
        #endregion

        #region Custom Methods
        /// <summary>
        /// Spawns a bullet at the set firing point
        /// </summary>
        void ShootBullet()
        {
            // If the player presses the left mouse button
            if (Input.GetButtonDown("Fire1"))
            {
                // Create a bullet at the firing point. Set the bullet's parent to the bullet container so it isn't following the player.
                //GameObject bullet = Instantiate(bulletPrefab, firingPoint.position, firingPoint.rotation, bulletContainer.transform);
                //Old script

                // Get mouse position in world space
                Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                mousePos.z = 0f;
                // Get direction from firing point to mouse
                Vector3 direction = (mousePos - firingPoint.position).normalized;

                // Create a rotation that looks at the direction
                Quaternion rotation = Quaternion.LookRotation(Vector3.forward, direction);

                // Spawn the bullet facing the cursor
                GameObject bullet = Instantiate(bulletPrefab, firingPoint.position, rotation, bulletContainer.transform);
            }
        }
        #endregion
    }
}
