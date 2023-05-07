using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class enemyAnim : MonoBehaviour
{
    Animator animator;
    EnemyAI enemyAI;
    Rigidbody enemyRigidbody;
    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();
        enemyAI = GetComponentInParent<EnemyAI>();
        Rigidbody enemyRigidbody = GetComponentInParent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        if (enemyAI.isWalking)
        {
            animator.SetBool("isWalking", true);
        }
        if (!enemyAI.isWalking)
        {
            animator.SetBool("isWalking", false);
        }
        if (enemyAI.isRunning)
        {
            animator.SetBool("isRunning", true);
        }
        if (!enemyAI.isRunning)
        {
            animator.SetBool("isRunning", false);
        }
        if (enemyAI.isMovingFiring)
        {
            animator.SetBool("isMovingFiring", true);
        }
        if (!enemyAI.isMovingFiring)
        {
            animator.SetBool("isMovingFiring", false);
        }
        if (enemyAI.isFiring)
        {
            animator.SetBool("isFiring", true);
        }
        if (!enemyAI.isFiring)
        {
            animator.SetBool("isFiring", false);
        }
        if (enemyAI.isSafe)
        {
            animator.SetBool("isSafe", true);
        }
        if (!enemyAI.isSafe)
        {
            animator.SetBool("isSafe", false);
        }

    }
}
