using UnityEngine;

public class LightEnemy : Enemy
{
    protected override void Awake()
    {
        base.Awake();
        moveSpeed = 7f;
        maxHealth = 2;
        bulletSpeed = 14f;
        minShootCooldown = 0.4f;
        maxShootCooldown = 0.8f;
    }
}

