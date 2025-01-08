using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class CategoryButton : MonoBehaviour
{
    // Start is called before the first frame update
    public Text text;
    public Image progressBarFilling;
    public GameData gameData;
    public TMP_Text textProgress;
    // [SerializeField] private GameObject panel;
    ReviewNavigation navigation;

    Category_PlayerPrefs category;

    SoundManagement audioManager;



    private void Awake()
    {
        audioManager = GameObject.FindGameObjectWithTag("Audio").GetComponent<SoundManagement>();
    }



    public void Init(string categoryName, float progress, string progressText, Category_PlayerPrefs category)
    {
        text.text = categoryName;
        progressBarFilling.fillAmount = progress;
        textProgress.text = progressText;
        gameObject.name = categoryName;
        this.category = category;
    }

    void Start()
    {
        navigation = FindAnyObjectByType<ReviewNavigation>();
        var button = GetComponent<Button>();
        button.onClick.AddListener(OnButtonClick);
    }

    private void OnButtonClick()
    {
        // audioManager.PlaySFX(audioManager.sfxClipsList[0]);
        // Debug.Log("CategoryButton: " + gameObject.name);
        // gameData.newCategoryName = gameObject.name;
        Debug.Log("category selected: " + category.GetCategoryName());
        gameData.selectedCategory = category;
        // gameData.selectedCategoryName = gameData.newCategoryName;
        Debug.Log("Moving...");
        navigation.GoToSection();
    }
}
