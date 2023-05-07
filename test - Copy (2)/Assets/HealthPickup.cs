using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthPickup : MonoBehaviour
{
    public int healthRestored;
    public GameObject playerObject;
    PlayerStats playerStats;

    void Awake()
    {
        playerObject = GameObject.Find("PlayerObj");
        playerStats = playerObject.GetComponent<PlayerStats>();
    }

    private void OnTriggerEnter(Collider playerCollider)
    {
        if (playerCollider.CompareTag("Player"))
        {
            // Only consume the health pickup if player health is less than 100
            if (playerStats.currentHealth < 100)
            {
                playerStats.Heal(healthRestored);
                Destroy(gameObject);
            }
        }
    }
}