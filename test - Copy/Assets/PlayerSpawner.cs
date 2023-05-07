using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSpawner : MonoBehaviour
{
    public GameObject playerPrefab; // the prefab for the player object
    public Transform[] spawnPoints; // an array of spawn points

    private GameObject currentPlayer; // the currently active player object
    public int whichSpawnPoint;

    private void Start()
    {
        whichSpawnPoint = 0;
        SpawnPlayer(whichSpawnPoint); // spawn the player at the first spawn point
    }

    public void SpawnPlayer(int spawnIndex)
    {
        // destroy the previous player object, if there is one
        if (currentPlayer != null)
        {
            Destroy(currentPlayer);
        }

        // instantiate a new player object at the specified spawn point
        currentPlayer = Instantiate(playerPrefab, spawnPoints[spawnIndex].position, spawnPoints[spawnIndex].rotation);

        //increment so that next time this is called it spawns us at the next spawn point
        whichSpawnPoint++;
    }
}
