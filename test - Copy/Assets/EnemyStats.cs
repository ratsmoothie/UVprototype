using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyStats : MonoBehaviour
{
    playerShooting playerShooting;

    public int maxHealth;
    public int currentHealth;

    // Start is called before the first frame update
    void Start()
    {
        playerShooting = GetComponent<playerShooting>();

        currentHealth = maxHealth;
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
    }

    public void Die()
    {
        Destroy(this.gameObject);       
    }

    // Update is called once per frame
    void Update()
    {
        if (currentHealth < 0)
        {
            Die();
            currentHealth = 0;
        }
        if (currentHealth > maxHealth)
        {
            currentHealth = maxHealth;
        }
    }
}
