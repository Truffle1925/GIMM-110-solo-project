using UnityEngine;

public class LightEnemy : Enemy
{
    protected override void Awake()
    {
        base.Awake();
        moveSpeed = 30f;
        maxHealth = 2;
        currentHealth = maxHealth;
        bulletSpeed = 14f;
        minShootCooldown = 0.4f;
        maxShootCooldown = 0.8f;
        dashCooldown = 1f;
        roomChaseRange = 6f;
    }
}
