using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SectionButton : MonoBehaviour
{
    public Image isLock;
    public TMP_Text title;
    public TMP_Text progressText;
    public Image progressBarFilling;

    ReviewNavigation navigation;

    public GameData gameData;

    Section_PlayerPrefs section;

    SoundManagement audioManager;

    private void Awake()
    {
        audioManager = GameObject.FindGameObjectWithTag("Audio").GetComponent<SoundManagement>();
    }

    public void Init(string sectionName, float progress, bool isLocked, string progressTxt, Section_PlayerPrefs section)
    {
        gameObject.name = sectionName;
        title.text = sectionName;
        isLock.gameObject.SetActive(isLocked);
        progressBarFilling.fillAmount = progress;
        progressText.gameObject.SetActive(!isLocked);
        progressText.text = progressTxt;
        this.section = section;

    }

    void Start()
    {
        navigation = FindAnyObjectByType<ReviewNavigation>();
        var button = GetComponent<Button>();
        button.onClick.AddListener(OnButtonClick);
    }


    private void OnButtonClick()
    {
        //  audioManager.PlaySFX(audioManager.sfxClipsList[0]);
        // gameData.newSectionName = gameObject.name;
        gameData.selectedSection = section;
        // if (gameData.newCategoryName != "")
        // {

        //     gameData.selectedSectionName = gameData.newSectionName;
        //     gameData.selectedCategoryName = gameData.newCategoryName;
        // }
        // else
        // {
        //     gameData.selectedSectionName = gameData.newSectionName;
        // }

        navigation.GoToLevel();
    }

}
