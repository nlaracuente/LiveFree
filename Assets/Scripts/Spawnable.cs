using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(ObstacleMover))]
public class Spawnable : MonoBehaviour
{
    [SerializeField]
    protected SpawnableType type;
    public SpawnableType Type { get { return type; } }

    [SerializeField]
    protected new Renderer renderer;

    protected ObstacleMover obstacleMover;
    public ObstacleMover ObstacleMover { get { return obstacleMover; } }
    protected bool isMoving = false;
    public bool IsAvailable { get; protected set; } = true;

    private void Awake()
    {
        obstacleMover = GetComponent<ObstacleMover>();
        if(renderer == null)
            renderer = GetComponentInChildren<Renderer>();
    }

    private void LateUpdate()
    {
        // Object is available for respawning
        if (isMoving && obstacleMover.DestinationReached)
        {
            IsAvailable = true;
            isMoving = false;
        }
    }

    public void Spawn(float xPosition, float zPosition)
    {
        IsAvailable = false;

        transform.position = new Vector3(
            xPosition,
            transform.position.y,
            zPosition
        );

        renderer.enabled = true;
        gameObject.SetActive(true);        
    }

    public void Move()
    {
        isMoving = true;
        obstacleMover.Move();
    }
}
