using UnityEngine;

/// <summary>
/// Handles switching between multiple weapon scripts (Shoot, ShootShotgun, MeleeAttack)
/// on the same player using number keys (1, 2, 3) or mouse scroll.
/// </summary>
public class WeaponManager : MonoBehaviour
{
    [Header("Weapon Components")]
    public MonoBehaviour[] weapons; // Assign: [0]=Shoot, [1]=ShootShotgun, [2]=MeleeAttack

    private int selectedWeapon = 0;

    void Start()
    {
        SelectWeapon(selectedWeapon);
    }

    void Update()
    {
        HandleInput();
    }

    void HandleInput()
    {
        int previousSelectedWeapon = selectedWeapon;

        // Scroll wheel input
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll > 0f)
        {
            selectedWeapon++;
            if (selectedWeapon >= weapons.Length)
                selectedWeapon = 0;
        }
        else if (scroll < 0f)
        {
            selectedWeapon--;
            if (selectedWeapon < 0)
                selectedWeapon = weapons.Length - 1;
        }

        // Number keys
        if (Input.GetKeyDown(KeyCode.Alpha1)) selectedWeapon = 0;
        if (Input.GetKeyDown(KeyCode.Alpha2)) selectedWeapon = 1;
        if (Input.GetKeyDown(KeyCode.Alpha3)) selectedWeapon = 2;

        // Only switch if selection changed
        if (previousSelectedWeapon != selectedWeapon)
            SelectWeapon(selectedWeapon);
    }

    void SelectWeapon(int index)
    {
        for (int i = 0; i < weapons.Length; i++)
        {
            if (weapons[i] != null)
                weapons[i].enabled = (i == index);
        }
    }
}
