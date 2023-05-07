using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerOutOfBounds : MonoBehaviour
{
    public GameObject respawnPoint;
    public float lowBound;
    //public float highBound;
    //public float fallDamage;

    void resetPlayer()
    {
        this.gameObject.transform.position = respawnPoint.transform.position;
    }

    void Start()
    {
        respawnPoint = GameObject.FindGameObjectWithTag("playerSpawn");

    }

    void FixedUpdate()
    {
        if (transform.position.y <= lowBound)
        {
            resetPlayer();
            Debug.Log("Player out of bounds...");
        }
    }

}
