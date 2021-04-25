using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField]
    Vector3 offset;

    [SerializeField]
    float followSpeed = 3f;

    Player Target { get { return GameManager.Instance.Player; } }
   

    private void Start()
    {
        offset = transform.position - Target.transform.position;
    }

    /// <summary>
    /// Track PLayer
    /// </summary>
    void LateUpdate()
    {
        if (Target == null)
            return;

        var destination = Target.transform.position + offset;
        //transform.position = Vector3.MoveTowards(
        //    transform.position,
        //    destination, 
        //    followSpeed * Time.deltaTime
        //);

        // "Tracking" is disorianting
        // Testing snap movement
        transform.position = destination;
    }
}
