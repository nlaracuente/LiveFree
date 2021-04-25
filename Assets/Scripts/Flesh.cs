using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

[RequireComponent(typeof(SpawnablePatterns))]
public class Flesh : MonoBehaviour
{
    enum Phase
    {
        Idle,
        WaitingForStartOfWave,
        SpawningWave,
    }

    Phase phase;

    [SerializeField]
    LaneId currentLaneId = LaneId.MiddleLane;

    [SerializeField]
    float changeLaneSpeed = 7f;

    [SerializeField]
    float distanceToTarget = .01f;

    [SerializeField]
    float laneChangeDelay = 2f;
    float laneChangeTime = 0f;

    float DelayBetweenWaves { get { return GameManager.Instance.DelayBetweenWaves; } }
    float DelayBetweenRows { get { return GameManager.Instance.DelayBetweenRows; } }
    float ChangeLaneDelay { get { return laneChangeDelay; } }

    float timeUntillNextWave = 0f;
    float timeUntilNextSpawn = 0f;
    float timeUntilLaneChange = 0f;
    
    /// <summary>
    /// Time left before lane change when collision happened
    /// </summary>
    float laneChangeTimeRemainder = 0f;

    bool playerCollided = false;

    [SerializeField]
    Rigidbody rigidbody;

    SpawnablePatterns spawnablePatterns;

    SpawnableType[] obstacleTypes;

    IEnumerator moveRoutine;

    private void Awake()
    {
        if (rigidbody == null)
            rigidbody = GetComponent<Rigidbody>();

        obstacleTypes = Enum.GetValues(typeof(SpawnableType)).Cast<SpawnableType>().ToArray();
        spawnablePatterns = GetComponent<SpawnablePatterns>();
    }

    private void Start()
    {
        var lane = LaneController.Instance.GetLaneById(currentLaneId);
        transform.position = new Vector3(
            lane.position.x,
            transform.position.y,
            transform.position.z
        );

        GameManager.Instance.RegisterOnCollisionStart(OnCollisionStart);
        GameManager.Instance.RegisterOnCollisionCompleted(OnCollisionCompleted);

        StartCoroutine(FleshRoutine());
    }

    /// <summary>
    /// Main logic for the flesh that runs until the game is over
    /// </summary>
    /// <returns></returns>
    IEnumerator FleshRoutine()
    {
        while (!GameManager.Instance.IsGameOver)
        {
            // Wait until we can spawn the next wave
            yield return StartCoroutine(WaitRoutine(DelayBetweenWaves));

            // Spawn the wave
            yield return StartCoroutine(SpawnWaveRoutine());
        }
    }

    IEnumerator WaitRoutine(float delay)
    {
        var waitTime = Time.time + delay;
        while(Time.time <= waitTime)
        {
            if (playerCollided)
            {
                // save the time we had left so that we can resume waiting once collision is over
                var remainingTime = Time.time - waitTime;
                if (remainingTime < 0)
                    remainingTime = 0;

                // Wait until collision is done
                while (playerCollided)
                    yield return new WaitForEndOfFrame();

                // Reset our wait time 
                waitTime = Time.time + remainingTime;
            }
            yield return new WaitForEndOfFrame();
        }
    }    

    IEnumerator SpawnWaveRoutine()
    {
        // Choose a pattern to spawn
        var pattern = spawnablePatterns.GetRandomPattern();
        var spawnables = new List<Spawnable>();

        // Spawn the Row
        for (int y = 0; y < pattern.GetLength(1); y++)
        {
            spawnables.Clear();
            for (int x = 0; x < pattern.GetLength(0); x++)
            {
                // Patterns only tells us which "prefab" to use
                var prefab = pattern[x, y];

                // Probably a "space"
                if (prefab == null)
                    continue;

                // The pool is the one that has instantiate a prefab of that type
                var spawnable = SpawnablePool.Instance.GetNextAvailable(prefab.Type);

                if(spawnable != null)
                {
                    // -1 because the middle lane is actually lane 0
                    // 0 - 1 = -1 (left lane) 
                    // 1 - 1 = 0 (middle lane) 
                    // 2 - 1 = 1 (right lane) 
                    var xPos = (x - 1) * GameManager.Instance.TileSize;

                    spawnable.Spawn(xPos, transform.position.z);
                    spawnables.Add(spawnable);
                }
            }

            // Now that the "row" is spanwed
            // We can tell everyone to move
            spawnables.ForEach(s => s.Move());

            // Wait until we can spawn again
            yield return StartCoroutine(WaitRoutine(DelayBetweenRows));
        }
    }

    IEnumerator ChangeLaneRoutine()
    {
        var id = LaneController.Instance.GetRandomLaneId();
        var lane = LaneController.Instance.GetLaneById(id);

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

        moveRoutine = null;
    }

    /// <summary>
    /// Save how much time there was left before the next "spawn cycle"
    /// </summary>
    public void OnCollisionStart()
    {
        playerCollided = true;

        //laneChangeTimeRemainder = laneChangeTime - Time.time;
        //if (laneChangeTimeRemainder < 0)
        //    laneChangeTimeRemainder = 0f; // We were at lane change time
    }

    public void OnCollisionCompleted()
    {
        // Update the wait time to be whatever time we had left
        //if (laneChangeTimeRemainder > 0)
        //    laneChangeTime = Time.time + laneChangeTimeRemainder;

        playerCollided = false;
    }
}
