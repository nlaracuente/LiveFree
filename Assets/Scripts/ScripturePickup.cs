using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScripturePickup : Spawnable
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            GameManager.Instance.OnScripturePickedUp();
            obstacleMover.PopToDestination();
        }
            
    }
}
