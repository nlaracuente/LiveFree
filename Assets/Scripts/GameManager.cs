using System.Collections;
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
    float playerPunchDelay = .25f;

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
    public int TotalVerses { get { return verses.Length; } }


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
    private CollisionEvent fightStartEvents;
    private CollisionEvent fightCompletedEvents;

    public void RegisterOnCollisionStart(CollisionEvent collisionEvent)
    {
        collisionStartEvents += collisionEvent;
    }

    public void RegisterOnCollisionCompleted(CollisionEvent collisionEvent)
    {
        collisionCompletedEvents += collisionEvent;
    }

    public void RegisterOnFightStart(CollisionEvent collisionEvent)
    {
        fightStartEvents += collisionEvent;
    }

    public void RegisterOnFightCompleted(CollisionEvent collisionEvent)
    {
        fightCompletedEvents += collisionEvent;
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
        AudioManager.Instance.PlayRandomPitchClip(AudioLibrary.Instance.scripturePickedUpClip);
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

        // Trigger collision effect
        obstacle.OnPlayerCollision();
        Player.TakeDamage(obstacle.Damage);
        AudioManager.Instance.PlayRandomPitchClip(AudioLibrary.Instance.playerCrashedClip);

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

        fightStartEvents?.Invoke();

        // Snap the enemy to within punching distance
        var fleshOrigin = Flesh.transform.position;
        Flesh.transform.position = fleshPunchTargetDestination.position;

        // Make the player jump and wait
        player.Jump();
        var playerOrigin = Player.transform.position;
        yield return StartCoroutine(MoveToDestination(Player.transform, playerJumpDestination.position, playerJumpSpeed));

        // Reel the flesh in
        
        // yield return StartCoroutine(MoveToDestination(Flesh.transform, fleshPunchTargetDestination.position, fleshMoveToPlayerSpeed));

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
        // Wait for the punch to complete
        var src = AudioManager.Instance.PlayClip(AudioLibrary.Instance.playerPunchCryClip);
        // Wait a bit for the "Take" part
        yield return new WaitForSeconds(src.clip.length * 0.4f);

        Player.Punch();
        yield return new WaitForSeconds(playerPunchDelay);

        // Inflict the damage now so that it looks like it is the reason 
        // why the flesh is being pushed back
        if (isCorrect)
        {
            Flesh.TakeDamage(damageToFleshPerHit);
            AudioManager.Instance.PlayClip(AudioLibrary.Instance.punchHitClip);
        } else
        {
            Player.TakeDamage(damageToFleshPerHit);
            AudioManager.Instance.PlayerRandomClip(AudioLibrary.Instance.punchMissedClips);
        }
        
        // Reset positions
        StartCoroutine(MoveToDestination(Player.transform, playerOrigin, playerJumpSpeed));
        yield return StartCoroutine(MoveToDestination(Flesh.transform, fleshOrigin, fleshMoveToPlayerSpeed));

        if (Flesh.HP < 1)
            GameWon();
        else if (Player.HP < 1)
            GameOver();
        else
        {
            totalScripturesCollected = 0;
            fightCompletedEvents?.Invoke();
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
