﻿using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : Singleton<GameManager>
{
    [SerializeField]
    float tileSize = 3;

    [SerializeField]
    float moveSpeed = 5f;

    [SerializeField]
    float hitRecoveryDelay = 1f;

    [SerializeField]
    float delayBetweenWaves = 5f;

    [SerializeField]
    float delayBetweenRows = 1f;

    [SerializeField]
    int totalScriptures = 3;

    [SerializeField]
    int totalScripturesCollected = 1;

    [SerializeField]
    int damageToFleshPerHit = 1;

    [SerializeField]
    float playerJumpSpeed = 12f;

    [SerializeField]
    float fleshMoveToPlayerSpeed = 20f;

    [SerializeField]
    float chooseWordDelay = 5f;

    [SerializeField]
    Verse[] verses;

    [SerializeField]
    int currentVerseIndex = 0;

    [SerializeField]
    Transform playerJumpDestination;

    [SerializeField]
    Transform fleshPunchTargetDestination;

    [SerializeField]
    Transform fleshHitTargetDestination;

    public float MoveSpeed { get { return moveSpeed; } }
    public bool PlayerCollided { get; private set; }
    public bool IsGameOver { get; private set; } = false;
    public int TotalScripturesCollected { get { return totalScripturesCollected; } }
    public int TotalScriptures { get { return totalScriptures; } }
    public float TileSize { get { return tileSize; } }
    public float DelayBetweenWaves { get { return delayBetweenWaves; } }
    public float DelayBetweenRows { get { return delayBetweenRows; } }

    Player player;
    public Player Player
    {
        get
        {
            if (player == null)
                player = FindObjectOfType<Player>();
            return player;
        }
    }

    Flesh flesh;
    public Flesh Flesh
    {
        get
        {
            if (flesh == null)
                flesh = FindObjectOfType<Flesh>();
            return flesh;
        }
    }

    int chosenWordIndex = -1;

    public delegate void CollisionEvent();
    private CollisionEvent collisionStartEvents;
    private CollisionEvent collisionCompletedEvents;

    public void RegisterOnCollisionStart(CollisionEvent collisionEvent)
    {
        collisionStartEvents += collisionEvent;
    }

    public void RegisterOnCollisionCompleted(CollisionEvent collisionEvent)
    {
        collisionCompletedEvents += collisionEvent;
    }

    public void OnPlayerCollidedWithObstacle(Obstacle obstacle)
    {
        if (!PlayerCollided)
        {
            PlayerCollided = true;
            StartCoroutine(PlayerCollisionRoutine(obstacle));
        }
    }

    public void OnScripturePickedUp()
    {
        totalScripturesCollected++;

        if (totalScripturesCollected < totalScriptures)
            return;

        StartCoroutine(PunchRoutine());
    }

    IEnumerator PlayerCollisionRoutine(Obstacle obstacle)
    {
        var speed = moveSpeed;
        moveSpeed = 0f;

        // Notify anyone interest about the player collision
        collisionStartEvents?.Invoke();
        Player.TakeDamage(obstacle.Damage);
        yield return new WaitForSeconds(hitRecoveryDelay);

        moveSpeed = speed;
        PlayerCollided = false;

        if (Player.HP < 1)
            GameOver();
        else
            collisionCompletedEvents?.Invoke();
    }

    IEnumerator PunchRoutine()
    {
        // Stop Everything
        var speed = moveSpeed;
        moveSpeed = 0f;

        collisionStartEvents?.Invoke();

        // Lift Player in the air
        var playerOrigin = Player.transform.position;
        StartCoroutine(MoveToDestination(Player.transform, playerJumpDestination.position, playerJumpSpeed));

        // Reel the flesh in
        var fleshOrigin = Flesh.transform.position;
        yield return StartCoroutine(MoveToDestination(Flesh.transform, fleshPunchTargetDestination.position, fleshMoveToPlayerSpeed));

        // Display verse
        chosenWordIndex = -1;
        var verse = verses[currentVerseIndex];
        var shuffledWords = Utility.ShuffleArray(verse.words);

        var correctWordIndex = Array.IndexOf(shuffledWords, verse.correctWord);
        MenuController.Instance.ShowVerseMenu(verse.verse, verse.words, (int)chooseWordDelay);

        // Wait until the player chooses a word
        while (chosenWordIndex < 0)
            yield return new WaitForEndOfFrame();

        MenuController.Instance.HideVerseMenu();

        // Play out choice or lack there of routine
        var isCorrect = chosenWordIndex == correctWordIndex;
        if (isCorrect)
            currentVerseIndex++;

        yield return StartCoroutine(WordChosenRoutine(playerOrigin, fleshOrigin, isCorrect));

        // Finished
        if (currentVerseIndex >= verses.Length)
            GameWon();

        moveSpeed = speed;
    }

    public void ChosenWord(int index) => chosenWordIndex = index;

    IEnumerator MoveToDestination(Transform target, Vector3 destination, float speed)
    {
        while(Vector3.Distance(target.position, destination) > 0.01f)
        {
            var position = Vector3.MoveTowards(target.position, destination, speed * Time.deltaTime);
            target.position = position;
            yield return new WaitForEndOfFrame();
        }

        target.position = destination;
    }

    IEnumerator WordChosenRoutine(Vector3 playerOrigin, Vector3 fleshOrigin, bool isCorrect)
    {
        // TODO: Add Hit or Miss animation
        yield return new WaitForEndOfFrame();

        // Inflict the damage now so that it looks like it is the reason 
        // why the flesh is being pushed back
        if (isCorrect)
            Flesh.TakeDamage(damageToFleshPerHit);            

        // Post said anuamtion
        StartCoroutine(MoveToDestination(Player.transform, playerOrigin, playerJumpSpeed));
        yield return StartCoroutine(MoveToDestination(Flesh.transform, fleshOrigin, fleshMoveToPlayerSpeed));

        // Get hurt now so that it looks like it was the fall that hurt
        if (!isCorrect)
            Player.TakeDamage(damageToFleshPerHit);

        yield return new WaitForEndOfFrame();

        if (Flesh.HP < 1)
            GameWon();
        else if (Player.HP < 1)
            GameOver();
        else
        {
            totalScripturesCollected = 0;
            collisionCompletedEvents?.Invoke();
        }   
    }

    void GameOver()
    {
        IsGameOver = true;
        MenuController.Instance.ShowGameOverMenu();
        Time.timeScale = 0f;
    }

    void GameWon()
    {
        IsGameOver = true;
        MenuController.Instance.ShowGameWonMenu();
        Time.timeScale = 0f;
    }

    public void Continue()
    {        
        IsGameOver = false;
        currentVerseIndex = 0;
        totalScripturesCollected = 0;
        Flesh.HP = Flesh.MaxHP;
        Player.HP = Player.MaxHP;
        collisionCompletedEvents?.Invoke();
        MenuController.Instance.CloseMenus();
        Time.timeScale = 1f;
    }

    void UnregisterEventHandlers()
    {
        foreach (CollisionEvent d in collisionStartEvents.GetInvocationList())
        {
            collisionStartEvents -= d;
        }

        foreach (CollisionEvent d in collisionCompletedEvents.GetInvocationList())
        {
            collisionCompletedEvents -= d;
        }
    }
}
