using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class winScreenText : MonoBehaviour
{
    public TextMeshProUGUI scoreText;
    int playerScore = 0;

    void updateScore(int score)
    {
        playerScore += score; 
        scoreText.text = playerScore.ToString();
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

        
    }
}
