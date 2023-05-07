using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class WinCollider : MonoBehaviour
{
    public GameObject winScreen;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // Show the win screen
            winScreen.SetActive(true);
            // Stop the game time
            Time.timeScale = 0f;
        }
    }

    public void RestartGame()
    {
        // Reload the current scene to restart the game
        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.buildIndex);     
    }

    void Start()
    {
        winScreen.SetActive(false);
    }

    void Update()
    {
        if (Input.GetKey("u"))
        {
            RestartGame();
        }
    }
}
