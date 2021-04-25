using System.Collections;
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

    void GameOver()
    {
        IsGameOver = true;
        MenuController.Instance.ShowGameOverMenu();
        Time.timeScale = 0f;
    }

    public void Continue()
    {
        UnregisterEventHandlers();
        IsGameOver = false;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
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
