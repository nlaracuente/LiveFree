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
    float laneChangeTime = 0f;

    /// <summary>
    /// Time left before lane change when collision happened
    /// </summary>
    float laneChangeTimeRemainder = 0f;

    bool playerCollided = false;

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

        laneChangeTime = Time.time + laneChangeDelay;

        GameManager.Instance.RegisterOnCollisionStart(OnCollisionStart);
        GameManager.Instance.RegisterOnCollisionCompleted(OnCollisionCompleted);
    }

    private void Update()
    {
        if (playerCollided || Time.time <= laneChangeTime || currentRotuine != null)
            return;

        TriggerLaneChange();
    }

    void TriggerLaneChange()
    {
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

        // We want to keep moving even after "resuming"
        while (Vector3.Distance(target, transform.position) > distanceToTarget)
        {
            // Don't move while we wait for the player collision routine to end
            if (!playerCollided)
            {
                var position = Vector3.MoveTowards(transform.position, target, changeLaneSpeed * Time.deltaTime);
                rigidbody.MovePosition(position);
            }

            yield return new WaitForFixedUpdate();
        }

        // Give it a frame to see the movement completed
        yield return new WaitForEndOfFrame();

        // Wait until the player collision routine is done
        // then we can continue with spawning
        while (playerCollided)
            yield return new WaitForEndOfFrame();

        // Spawn a wait a bit to see things spawning
        SpawnObstacle();
        yield return new WaitForEndOfFrame();

        laneChangeTime = Time.time + laneChangeDelay;
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

    /// <summary>
    /// Save how much time there was left before the next "spawn cycle"
    /// </summary>
    public void OnCollisionStart()
    {
        playerCollided = true;

        laneChangeTimeRemainder = laneChangeTime - Time.time;
        if (laneChangeTimeRemainder < 0)
            laneChangeTimeRemainder = 0f; // We were at lane change time
    }

    public void OnCollisionCompleted()
    {
        // Update the wait time to be whatever time we had left
        if (laneChangeTimeRemainder > 0)
            laneChangeTime = Time.time + laneChangeTimeRemainder;

        playerCollided = false;
    }
}
