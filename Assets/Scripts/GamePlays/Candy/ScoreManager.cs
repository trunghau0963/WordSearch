using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScoreManager : MonoBehaviour
{
    private Board board;
    public Text scoreText;
    public int score;
    public Image scoreBar;
    // Start is called before the first frame update
    void Start()
    {
        board = FindFirstObjectByType<Board>();
    }

    // Update is called once per frame
    void Update()
    {
        scoreText.text = "" + score;
    }

    public void IncreaseScore(int scoreToAdd)
    {
        score += scoreToAdd;
        if (board != null && scoreBar != null)
        {
            // board.scoreGoal = score;
            int lenght = board.scoreGoals.Length;

            float fillAmount = (float)score / board.scoreGoals[lenght - 1];
            scoreBar.fillAmount = fillAmount;
        }
    }
}
