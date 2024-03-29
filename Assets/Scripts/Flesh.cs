﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

[RequireComponent(typeof(SpawnablePatterns))]
public class Flesh : MonoBehaviour
{
    [SerializeField]
    LaneId currentLaneId = LaneId.MiddleLane;

    [SerializeField]
    float changeLaneSpeed = 7f;

    [SerializeField]
    float distanceToTarget = .01f;

    [SerializeField]
    float spawnableStartingZPos = 70f;

    //[SerializeField]
    //int maxHP = 3;

    [SerializeField]
    float laneChangeDelay = 2f;

    [SerializeField]
    ParticleSystem particleSystem;

    [SerializeField]
    new Renderer renderer;
    

    float DelayBetweenWaves { get { return GameManager.Instance.DelayBetweenWaves; } }
    float DelayBetweenRows { get { return GameManager.Instance.DelayBetweenRows; } }
    float ChangeLaneDelay { get { return laneChangeDelay; } }
    public int HP { get; set; }
    public int MaxHP { get { return GameManager.Instance.TotalVerses; } }

    bool pauseControl = false;

    [SerializeField]
    new Rigidbody rigidbody;

    SpawnablePatterns spawnablePatterns;

    private void Awake()
    {
        if (rigidbody == null)
            rigidbody = GetComponent<Rigidbody>();

        if (renderer == null)
            renderer = GetComponentInChildren<Renderer>();

        if (particleSystem == null)
            particleSystem = GetComponentInChildren<ParticleSystem>();

        spawnablePatterns = GetComponent<SpawnablePatterns>();
        HP = MaxHP;
    }

    private void Start()
    {
        var lane = LaneController.Instance.GetLaneById(currentLaneId);
        transform.position = new Vector3(
            lane.position.x,
            transform.position.y,
            transform.position.z
        );

        GameManager.Instance.RegisterOnCollisionStart(TriggerPauseControl);
        GameManager.Instance.RegisterOnCollisionCompleted(TriggerResumeControl);

        GameManager.Instance.RegisterOnFightStart(TriggerPauseControl);
        GameManager.Instance.RegisterOnFightCompleted(TriggerResumeControl);

        StartCoroutine(FleshRoutine());
    }

    /// <summary>
    /// Main logic for the flesh that runs until the game is over
    /// </summary>
    /// <returns></returns>
    IEnumerator FleshRoutine()
    {
        // This will keep running until game over
        // Which is what we want to make the Flesh look like it's active and moving
        // StartCoroutine(ChangeLaneRoutine());

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
            if (pauseControl)
            {
                // save the time we had left so that we can resume waiting once collision is over
                var remainingTime = Time.time - waitTime;
                if (remainingTime < 0)
                    remainingTime = 0;

                // Wait until collision is done
                while (pauseControl)
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
                // Wait before we spawn again if we have to wait
                while (pauseControl)
                    yield return new WaitForEndOfFrame();

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

                    spawnable.Spawn(xPos, spawnableStartingZPos);
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
        // Initial wait before we change lanes
        yield return StartCoroutine(WaitRoutine(ChangeLaneDelay));

        while (!GameManager.Instance.IsGameOver)
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
                if (!pauseControl)
                {
                    var position = Vector3.MoveTowards(transform.position, target, changeLaneSpeed * Time.deltaTime);
                    rigidbody.MovePosition(position);
                }

                yield return new WaitForFixedUpdate();
            }

            yield return StartCoroutine(WaitRoutine(ChangeLaneDelay));
        }
    }

    /// <summary>
    /// Save how much time there was left before the next "spawn cycle"
    /// </summary>
    public void TriggerPauseControl()
    {
        pauseControl = true;
    }

    public void TriggerResumeControl()
    {
        pauseControl = false;
        renderer.enabled = true;
    }

    public void TakeDamage(int damage)
    {
        HP = Mathf.Clamp(HP - damage, 0, MaxHP);
    }

    public void Explode()
    {
        renderer.enabled = false;
        particleSystem?.Play();
    }
}
