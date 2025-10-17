using UnityEngine;

public class MediumEnemy : Enemy
{
    protected override void Awake()
    {
        base.Awake();
        moveSpeed = 5f;
        maxHealth = 3;
        bulletSpeed = 12f;
        minShootCooldown = 0.6f;
        maxShootCooldown = 1.2f;
    }
}

