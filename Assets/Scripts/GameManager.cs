using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : Singleton<GameManager>
{
    [SerializeField]
    float moveSpeed = 5f;
    public float MoveSpeed { get { return moveSpeed; } }
}
