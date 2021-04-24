using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

public class Flesh : MonoBehaviour
{
    [SerializeField]
    LaneId currentLaneId = LaneId.MiddleLane;

    [SerializeField]
    float changeLaneSpeed = 7f;

    [SerializeField]
    float distanceToTarget = .01f;

    [SerializeField]
    float laneChangeDelay = 2f;
    float langeChangeTime = 0f;

    [SerializeField]
    Rigidbody rigidbody;

    ObstacleType[] obstacleTypes;

    IEnumerator currentRotuine;

    private void Awake()
    {
        if (rigidbody == null)
            rigidbody = GetComponent<Rigidbody>();

        obstacleTypes = Enum.GetValues(typeof(ObstacleType)).Cast<ObstacleType>().ToArray();
    }

    private void Start()
    {
        var lane = LaneController.Instance.GetLaneById(currentLaneId);
        transform.position = new Vector3(
            lane.position.x,
            transform.position.y,
            transform.position.z
        );

        langeChangeTime = Time.time + laneChangeDelay;
    }

    private void Update()
    {
        if (Time.time <= langeChangeTime || currentRotuine != null)
            return;

        var id = LaneController.Instance.GetRandomLaneId();
        var lane = LaneController.Instance.GetLaneById(id);

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

        while (Vector3.Distance(target, transform.position) > distanceToTarget)
        {
            var position = Vector3.MoveTowards(transform.position, target, changeLaneSpeed * Time.deltaTime);
            rigidbody.MovePosition(position);
            yield return new WaitForFixedUpdate();
        }

        yield return new WaitForEndOfFrame();
        SpawnObstacle();
        yield return new WaitForEndOfFrame();

        langeChangeTime = Time.time + laneChangeDelay;
        currentRotuine = null;
    }

    void SpawnObstacle()
    {
        var randType = Utility.ShuffleArray(obstacleTypes).First();
        var obstacle = ObstaclePool.Instance.GetNextAvailable(randType);

        if(obstacle == null)
        {
            Debug.LogError($"There is not available {randType} obstacle");
            return;
        }
        obstacle.Spawn(transform.position.x, transform.position.z);
    }
}
