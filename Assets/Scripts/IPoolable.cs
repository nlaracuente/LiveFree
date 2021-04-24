using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IPoolable
{
    GameObject GameObject { get; }
    bool isAvailable();
}