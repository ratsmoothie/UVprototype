using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PreventTilt : MonoBehaviour
{
    EnemyStats enemyStats;
    Rigidbody rb;
    public float turnSpeed;

    // Start is called before the first frame update
    void Start()
    {
        enemyStats = GetComponent<EnemyStats>();
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        /*if (enemyStats.currentHealth > 0 && rb.velocity.magnitude > 0.1f)
        {
            Quaternion targetRotation = Quaternion.Euler(0f, transform.rotation.eulerAngles.y, 0f);
            rb.MoveRotation(Quaternion.Lerp(rb.rotation, targetRotation, Time.fixedDeltaTime * turnSpeed));
            //Debug.Log("Countering tilt");
        }*/
        
        // Set the rigidbody's rotation to zero degrees around the x-axis
        rb.MoveRotation(Quaternion.Euler(0f, transform.rotation.eulerAngles.y, 0f));
    }
}
