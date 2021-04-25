using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Obstacle : Spawnable
{
    [SerializeField, Tooltip("How much damage to inflict")]
    int damage = 1;
    public int Damage { get { return damage; } }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
            GameManager.Instance.OnPlayerCollidedWithObstacle(this);
    }
}

public enum SpawnableType
{
    Wall,
    Scripture
}
