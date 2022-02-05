using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimationManager : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private float playerMoveSpeed;

    private float sprintThreshold;
    private Vector3 lastPosition;

    private void Start()
    {
        sprintThreshold = playerMoveSpeed * 1.5f * Time.fixedDeltaTime;
    }

    public void AnimateBasedOnSpeed()
    {
        lastPosition.y = transform.position.y;
        float distanceMoved = Vector3.Distance(transform.position, lastPosition);
        animator.SetBool("IsMoving", distanceMoved > 0.01f);
        animator.SetBool("IsSprinting", distanceMoved > sprintThreshold);

        lastPosition = transform.position;
    }
}
