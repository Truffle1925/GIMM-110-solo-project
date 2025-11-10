using UnityEngine;

public class HeavyEnemy : Enemy
{
    protected override void Awake()
    {
        base.Awake();

        // Set HeavyEnemy-specific stats
        moveSpeed = 30f;
        maxHealth = 8;
        currentHealth = maxHealth;
        bulletSpeed = 8f;
        minShootCooldown = 1f;
        maxShootCooldown = 2f;
        dashCooldown = 3f;
        roomChaseRange = 5f;
        baseScoreValue = 100f;
    }
}
