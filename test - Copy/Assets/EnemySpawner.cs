using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public GameObject enemyPrefab;
    public Transform[] spawnPoints; // an array of spawn points

    public int maxEnemies;
    public int enemiesKilled;

    public int spawnDelayTimer;
    public int maxEnemiesAllowed;
    private int enemiesAlive;

    // Start is called before the first frame update
    void Start()
    {
        
        
    }

    void Update()
    {
        StartCoroutine(SpawnEnemies());
    }

    IEnumerator SpawnEnemies()
    {
        while (enemiesAlive < maxEnemiesAllowed)
        {
            int spawnIndex = Random.Range(0, spawnPoints.Length);
            Instantiate(enemyPrefab, spawnPoints[spawnIndex].position, spawnPoints[spawnIndex].rotation);
            enemiesAlive++;
            yield return new WaitForSeconds(spawnDelayTimer);
        }
    }

    public void EnemyKilled()
    {
        enemiesKilled++;
        enemiesAlive--;

        if (enemiesKilled == maxEnemies)
        {
            Debug.Log("All enemies killed!");
            // Do something fun here
        }
    }
}
