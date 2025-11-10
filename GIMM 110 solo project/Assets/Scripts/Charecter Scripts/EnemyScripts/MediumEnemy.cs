using UnityEngine;

public class MediumEnemy : Enemy
{
    protected override void Awake()
    {
        base.Awake();
        moveSpeed = 40f;
        maxHealth = 3;
        currentHealth = maxHealth;
        bulletSpeed = 12f;
        minShootCooldown = 0.6f;
        maxShootCooldown = 1.2f;
        dashCooldown = 2f;
        roomChaseRange = 6f;
        baseScoreValue = 75f;
    }
}
