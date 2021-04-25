using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObstacleMover : MonoBehaviour
{
    [SerializeField]
    float zTarget = -35;

    Vector3 destination;
    public bool IsMoving { get; private set; }
    public bool DestinationReached { get; private set; }

    public void Move()
    {
        destination = new Vector3(
            transform.position.x,
            transform.position.y,
            zTarget
        );

        IsMoving = true;
        DestinationReached = false;
    }

    private void LateUpdate()
    {
        if (!IsMoving)
            return;

        // Because we set the MoveSpeed to zero on collision
        // this should NOT move until collision routine is completed
        var target = Vector3.MoveTowards(transform.position, destination, GameManager.Instance.MoveSpeed * Time.deltaTime);
        transform.position = target;

        if(Vector3.Distance(destination, transform.position) < .01f)
        {
            IsMoving = false;
            DestinationReached = true;
        }
    }

    public void PopToDestination()
    {
        transform.position = destination;
    }
}
