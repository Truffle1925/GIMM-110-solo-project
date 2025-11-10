using UnityEngine;

public class FastEnemy : Enemy
{
    protected override void Awake()
    {
        base.Awake(); // Initialize Rigidbody2D, Animator, currentHealth, player, waypoints

        // Set FastEnemy-specific stats
        moveSpeed = 60f;
        maxHealth = 2;
        currentHealth = maxHealth; // important to sync health
        bulletSpeed = 13f;
        minShootCooldown = 0.3f;
        maxShootCooldown = 0.6f;
        dashCooldown = 1.5f;
        roomChaseRange = 6f;
        baseScoreValue = 75f;
    }
}
