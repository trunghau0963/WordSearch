using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DisplayScore : MonoBehaviour
{
    SavingFile savingFile;
    ScoreData scoreData;

    public Text scoreText;
    void Start()
    {
        savingFile = FindAnyObjectByType<SavingFile>();
        scoreData = savingFile.LoadScoreFromJson();
        scoreText.text = scoreData.score.ToString();
    }

    // Update is called once per frame
    void Update()
    {
        scoreText.text = scoreData.score.ToString();
    }
}
