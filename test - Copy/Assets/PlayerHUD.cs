using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerHUD : MonoBehaviour
{
    [Header("Player Stats")]
    public TMP_Text hpValue;
    public TMP_Text shieldValue;
    [Header("Weapon Stats")]
    public TMP_Text currentAmmo;
    public TMP_Text spareAmmo;

    PlayerStats playerStats;
    playerShooting playerShooting;

    // Start is called before the first frame update
    void Start()
    {
        playerStats = GetComponent<PlayerStats>();
        playerShooting = GameObject.FindWithTag("Pistol").GetComponent<playerShooting>();
    }

    void DisplayPlayerStats()
    {
        hpValue.text = playerStats.currentHealth.ToString();
        shieldValue.text = playerStats.currentShield.ToString();

        currentAmmo.text = playerShooting.remainingAmmo.ToString();
        spareAmmo.text = playerShooting.spareAmmo.ToString();
    }

    // Update is called once per frame
    void Update()
    {
        DisplayPlayerStats();
    }
}
