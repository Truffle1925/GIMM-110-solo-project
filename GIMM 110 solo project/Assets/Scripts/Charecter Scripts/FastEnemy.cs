using UnityEngine;

public class FastEnemy : Enemy
{
    protected override void Awake()
    {
        base.Awake();
        moveSpeed = 8f;
        maxHealth = 2;
        bulletSpeed = 13f;
        minShootCooldown = 0.3f;
        maxShootCooldown = 0.6f;
        dashCooldown = 1.5f;
    }
}

