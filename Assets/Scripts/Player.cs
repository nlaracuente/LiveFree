using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    [SerializeField]
    LaneId currentLaneId = LaneId.MiddleLane;

    [SerializeField]
    float changeLaneSpeed = 7f;

    [SerializeField]
    float distanceToTarget = .01f;

    [SerializeField]
    int maxHP = 3;

    [SerializeField]
    Rigidbody rigidbody;

    IEnumerator currentRotuine;
    public int HP { get; set; }
    public int MaxHP { get { return maxHP; } }

    bool collided = false;

    private void Awake()
    {
        if (rigidbody == null)
            rigidbody = GetComponent<Rigidbody>();

        HP = maxHP;
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
    }

    private void Update()
    {
        var direction = (int) Input.GetAxisRaw("Horizontal");
        if (collided || direction == 0 || currentRotuine != null)
            return;

        currentLaneId = LaneController.Instance.ChangeLane(currentLaneId, direction);
        var lane = LaneController.Instance.GetLaneById(currentLaneId);
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

        while(Vector3.Distance(target, transform.position) > distanceToTarget)
        {
            // Freeze movement while in collision routine
            if(!collided)
            {
                var position = Vector3.MoveTowards(transform.position, target, changeLaneSpeed * Time.deltaTime);
                rigidbody.MovePosition(position);
            }

            yield return new WaitForFixedUpdate();
        }

        currentRotuine = null;
    }

    public void TakeDamage(int damage)
    {
        HP = Mathf.Clamp(HP - damage, 0, maxHP);
    }

    public void OnCollisionStart()
    {
        collided = true;
    }

    public void OnCollisionCompleted()
    {
        collided = false;
    }
}
