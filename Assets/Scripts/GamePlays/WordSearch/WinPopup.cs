using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class WinPopup : MonoBehaviour
{
    public GameObject winPopup;
    public Button exitButton;
    bool checkCompletedLevel = false;
    void Start()
    {
        // winPopup = GameObject.Find("WinPopup");
        winPopup.SetActive(false);
    }

    private void OnEnable()
    {
        GameEvents.OnShowPopup += ShowWinPopup;
    }

    private void OnDisable()
    {
        GameEvents.OnShowPopup -= ShowWinPopup;
    }

    void ShowWinPopup(bool isCompletedLevel)
    {
        FindAnyObjectByType<LoadData>().DestroyAllExplanation();
        winPopup.SetActive(true);
        if (isCompletedLevel)
        {
            checkCompletedLevel = true;
            exitButton.gameObject.SetActive(false);
        }
        else
        {
            checkCompletedLevel = false;
            exitButton.gameObject.SetActive(true);
        }
    }

    void CloseWinPopup()
    {
        winPopup.SetActive(false);
    }

    public void loadNextBoard()
    {
        if (checkCompletedLevel)
        {
            SceneManager.LoadScene("MainMenu");
        }
        else
        {
            GameEvents.LoadNextBoardMethod();
        }
    }

    public void loadMainMenu()
    {
    }
}
