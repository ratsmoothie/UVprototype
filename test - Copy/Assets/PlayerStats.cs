using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    PlayerHUD playerHUD;
    enemyShooting enemyShooting;
    private Rigidbody playerRigidbody;

    [Header("Health & Shields")]
    public int maxHealth;
    public float maxShield;
    public int currentHealth;
    public float currentShield;

    //charge shield by moving, rate based on movement speed
    [Header("Shield Charging")]
    public float shieldChargeRate;
    

    // Start is called before the first frame update
    void Start()
    {
        playerHUD = GetComponent<PlayerHUD>();
        enemyShooting = GetComponent<enemyShooting>();

        currentHealth = maxHealth;
        currentShield = maxShield;

        playerRigidbody = GetComponentInParent<Rigidbody>();
    }

    public void TakeDamage(int damage)
    {
        if (currentShield > 0)
        {
            currentShield -= damage;
        }
        else
        {
            currentHealth -= damage;
        }
        
    }

    public void Heal(int healthRestored)
    {
        currentHealth += healthRestored;
    }

    public void ChargeShield()
    {
        float speed = playerRigidbody.velocity.magnitude;
        float chargeAmount = speed * shieldChargeRate * Time.deltaTime;
        currentShield = Mathf.Clamp(currentShield + chargeAmount, 0, maxShield);
    }

    public void Die()
    {
        Destroy(this.gameObject);
    }

    // Update is called once per frame
    void Update()
    {
        if (currentHealth <= 0)
        {
            Die();
        }
        if(currentHealth > maxHealth)//make sure we dont go over
        {
            currentHealth = maxHealth;
        }
        if (currentShield > maxShield)//same here
        {
            currentShield = maxShield;
        }
        if (currentHealth < 0)
        {
            currentHealth = 0;
        }
        if (currentShield < 0)
        {
            currentShield = 0;
        }
        if (playerRigidbody.velocity.magnitude > 0)
        {
            ChargeShield();
        }
    }
}
