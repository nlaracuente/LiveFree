using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(ObstacleMover))]
public class Obstacle : MonoBehaviour
{
    [SerializeField]
    ObstacleType type;
    public ObstacleType Type { get { return type; } }
    public GameObject GameObject { get { return gameObject; } }
    public bool IsAvailable { get; protected set; } = true;

    ObstacleMover obstacleMover;
    bool wasSpawned = false;

    private void Awake()
    {
        obstacleMover = GetComponent<ObstacleMover>();
    }

    private void LateUpdate()
    {
        // Object is available for respawning
        if (wasSpawned && obstacleMover.DestinationReached)
        {
            IsAvailable = true;
            wasSpawned = false;
        }   
    }

    public void Spawn(float xPosition, float zPosition)
    {
        wasSpawned = true;
        IsAvailable = false;
        transform.position = new Vector3(
            xPosition,
            transform.position.y,
            zPosition
        );
        obstacleMover.Move();
    }
}

public enum ObstacleType
{
    Wall
}
