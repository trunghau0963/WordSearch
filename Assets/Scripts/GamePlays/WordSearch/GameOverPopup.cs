using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameOverPopup : MonoBehaviour
{
    public GameObject gameOverPopup;
    public GameObject continueGameAftersAdsButton;
    void Start()
    {
        continueGameAftersAdsButton.GetComponent<Button>().interactable = false;
        gameOverPopup.SetActive(false);

        GameEvents.OnGameOver += ShowGameOverPopup;
    }

    private void OnDisable()
    {
        GameEvents.OnGameOver -= ShowGameOverPopup;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ShowGameOverPopup()
    {
        FindAnyObjectByType<LoadData>().DestroyAllExplanation();
        gameOverPopup.SetActive(true);
        continueGameAftersAdsButton.GetComponent<Button>().interactable = true;
    }
}
