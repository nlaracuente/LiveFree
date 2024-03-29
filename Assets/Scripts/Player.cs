﻿using System.Collections;
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
    Animator animator;

    [SerializeField]
    new Rigidbody rigidbody;

    IEnumerator currentRotuine;
    public int HP { get; set; }
    public int MaxHP { get { return maxHP; } }

    bool disableControl = false;

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

        GameManager.Instance.RegisterOnCollisionStart(TriggerDisableControl);
        GameManager.Instance.RegisterOnCollisionCompleted(TriggerResumeControl);

        GameManager.Instance.RegisterOnFightStart(TriggerDisableControl);
        GameManager.Instance.RegisterOnFightCompleted(TriggerResumeControl);
    }

    private void Update()
    {
        // Ensures the button has to be released/re-pressed before processing another input
        var buttonPressed = Input.GetButtonDown("Horizontal");
        var direction = (int)Input.GetAxisRaw("Horizontal");

        animator.SetBool("Running", !disableControl);

        if (disableControl || !buttonPressed || currentRotuine != null)
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

        AudioManager.Instance.PlayRandomPitchClip(AudioLibrary.Instance.playerMoveClip);
        while(Vector3.Distance(target, transform.position) > distanceToTarget)
        {
            // Freeze movement while in collision routine
            if (!disableControl)
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

    public void TriggerDisableControl()
    {
        disableControl = true;
    }

    public void TriggerResumeControl()
    {
        disableControl = false;
    }

    public void Jump() => animator.SetTrigger("Jump");
    public void Punch() => animator.SetTrigger("Punch");
}
