using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Obstacle : Spawnable
{
    [SerializeField, Tooltip("How much damage to inflict")]
    int damage = 1;

    [SerializeField]
    ParticleSystem particleSystem;

    public int Damage { get { return damage; } }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
            GameManager.Instance.OnPlayerCollidedWithObstacle(this);
    }

    public virtual void OnPlayerCollision()
    {
        particleSystem?.Play();
        renderer.enabled = false;
    }
}

public enum SpawnableType
{
    Wall,
    Scripture
}
