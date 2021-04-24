using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    [SerializeField]
    LaneId currentLaneId = LaneId.MiddleLane;

    [SerializeField]
    float changeLaneSpeed = 7f;

    [SerializeField]
    float distanceToTarget = .01f;

    [SerializeField]
    int maxHP = 3;

    [SerializeField]
    Rigidbody rigidbody;

    IEnumerator currentRotuine;
    public int HP { get; protected set; }

    private void Awake()
    {
        if (rigidbody == null)
            rigidbody = GetComponent<Rigidbody>();

        HP = maxHP;
    }

    private void Start()
    {
        var lane = LaneController.Instance.GetLaneById(currentLaneId);
        transform.position = new Vector3(
            lane.position.x,
            transform.position.y,
            transform.position.z
        );
    }

    private void Update()
    {
        var direction = (int) Input.GetAxisRaw("Horizontal");
        if (direction == 0 || currentRotuine != null)
            return;

        currentLaneId = LaneController.Instance.ChangeLane(currentLaneId, direction);
        var lane = LaneController.Instance.GetLaneById(currentLaneId);
        currentRotuine = ChangeLaneRoutine(lane);
        StartCoroutine(currentRotuine);
    }

    IEnumerator ChangeLaneRoutine(Transform lane)
    {
        var target = new Vector3(
            lane.position.x,
            transform.position.y,
            transform.position.z
        );

        while(Vector3.Distance(target, transform.position) > distanceToTarget)
        {
            var position = Vector3.MoveTowards(transform.position, target, changeLaneSpeed * Time.deltaTime);
            rigidbody.MovePosition(position);
            yield return new WaitForFixedUpdate();
        }

        currentRotuine = null;
    }
}
