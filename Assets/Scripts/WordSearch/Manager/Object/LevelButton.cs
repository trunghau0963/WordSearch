using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LevelButton : MonoBehaviour
{

    [Header("UI Elements")]
    public GameObject evenStyle;
    public GameObject oddStyle;
    public GameObject startStyle;
    public GameObject unlockStyle;
    public Button button;
    public Text Name;
    public GameData gameData;
    private string gameSceneName = "WordSearchGameScene";

    Level_PlayerPrefs level;

    // SoundManagement audioManager;

    // private void Awake()
    // {
    //     // audioManager = GameObject.FindGameObjectWithTag("Audio").GetComponent<SoundManagement>();
    //     // unlock.gameObject.SetActive(true);
    //     // isLock.gameObject.SetActive(false);
    // }
    public void Init(string levelName, bool isLocked, bool isEvenLevel, bool isEnd, bool isCompleted, Level_PlayerPrefs level)
    {

        if (isEvenLevel)
        {
            evenStyle.SetActive(true);
            if (isCompleted)
            {
                unlockStyle.SetActive(true);
                evenStyle.transform.Find("IsCompleted").GetComponent<Image>().gameObject.SetActive(true);
            }
            else
            {
                unlockStyle.SetActive(false);
                evenStyle.transform.Find("LockImage").GetComponent<Image>().gameObject.SetActive(isLocked);
            }
            startStyle.SetActive(false);
            oddStyle.SetActive(false);
        }
        else
        {
            oddStyle.SetActive(true);
            if (isCompleted)
            {
                unlockStyle.SetActive(true);
                oddStyle.transform.Find("IsCompleted").GetComponent<Image>().gameObject.SetActive(true);
            }
            else
            {
                unlockStyle.SetActive(false);
                oddStyle.transform.Find("LockImage").GetComponent<Image>().gameObject.SetActive(isLocked);
            }
            startStyle.SetActive(false);
            evenStyle.SetActive(false);
        }
        if (isEnd)
        {
            startStyle.SetActive(true);
            evenStyle.SetActive(false);
            oddStyle.SetActive(false);
            startStyle.transform.Find("LockImage").GetComponent<Image>().gameObject.SetActive(isLocked);
        }
        button.interactable = !isLocked;
        Name.text = levelName;
        gameObject.name = levelName;
        this.level = level;
    }

    void Start()
    {
        button.onClick.AddListener(OnButtonClick);
    }

    private void OnButtonClick()
    {
        // audioManager.PlaySFX(audioManager.sfxClipsList[2]);
        gameData.selectedLevel = level;
        // gameData.selectedLevelName = gameObject.name;
        SceneManager.LoadScene(gameSceneName);
    }


}
