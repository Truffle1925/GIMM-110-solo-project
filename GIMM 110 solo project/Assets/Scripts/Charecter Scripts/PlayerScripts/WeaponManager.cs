using UnityEngine;

/// <summary>
/// Handles switching between Glock, Shotgun, and Knife weapons
/// and updates the player animator to match the active weapon.
/// </summary>
public class WeaponManager : MonoBehaviour
{
    [Header("Weapon Components")]
    public MonoBehaviour glockWeapon;     // Assign your Shoot script
    public MonoBehaviour shotgunWeapon;   // Assign your ShootShotgun script
    public MonoBehaviour knifeWeapon;     // Assign your MeleeAttack script

    [Header("Animation Settings")]
    public Animator playerAnimator; // Assign your player Animator in Inspector
    private static readonly int PlayerGlock = Animator.StringToHash("playerGlock");
    private static readonly int PlayerShotgun = Animator.StringToHash("playerShotgun");
    private static readonly int PlayerKnife = Animator.StringToHash("playerKnife");
    private static readonly int MeleeAttack = Animator.StringToHash("meleeAttack");

    private int selectedWeapon = 0; // 0 = Glock, 1 = Shotgun, 2 = Knife

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
            if (selectedWeapon > 2)
                selectedWeapon = 0;
        }
        else if (scroll < 0f)
        {
            selectedWeapon--;
            if (selectedWeapon < 0)
                selectedWeapon = 2;
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
        // Disable all weapons
        if (glockWeapon != null) glockWeapon.enabled = false;
        if (shotgunWeapon != null) shotgunWeapon.enabled = false;
        if (knifeWeapon != null) knifeWeapon.enabled = false;

        // Enable selected weapon
        switch (index)
        {
            case 0:
                if (glockWeapon != null) glockWeapon.enabled = true;
                break;
            case 1:
                if (shotgunWeapon != null) shotgunWeapon.enabled = true;
                break;
            case 2:
                if (knifeWeapon != null) knifeWeapon.enabled = true;
                break;
        }

        Debug.Log("Weapon switched to index " + index);
        UpdateAnimatorState(index);
    }

    void UpdateAnimatorState(int index)
    {
        if (playerAnimator == null)
            return;

        // Reset all weapon states
        playerAnimator.SetBool(PlayerGlock, false);
        playerAnimator.SetBool(PlayerShotgun, false);
        playerAnimator.SetBool(PlayerKnife, false);

        // Enable only the active one
        switch (index)
        {
            case 0:
                playerAnimator.SetBool(PlayerGlock, true);
                break;
            case 1:
                playerAnimator.SetBool(PlayerShotgun, true);
                break;
            case 2:
                playerAnimator.SetBool(PlayerKnife, true);
                break;
        }
    }

    /// <summary>
    /// Called externally (e.g., by MeleeAttack script) to trigger the attack animation.
    /// </summary>
    public void PlayMeleeAttackAnimation()
    {
        if (playerAnimator != null)
        {
            playerAnimator.SetTrigger(MeleeAttack);
        }
    }
}
