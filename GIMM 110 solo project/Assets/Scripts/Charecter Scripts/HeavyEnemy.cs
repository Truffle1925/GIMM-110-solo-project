using UnityEngine;

public class HeavyEnemy : Enemy
{
    protected override void Awake()
    {
        base.Awake();
        moveSpeed = 2f;
        maxHealth = 4;
        bulletSpeed = 8f;
        minShootCooldown = 1.2f;
        maxShootCooldown = 2.5f;
        dashCooldown = 5f;
    }
}

