using TMPro;
// using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class ExplanationWord : MonoBehaviour
{
    private string _word = "";
    public TMP_Text displayText;
    public GameObject ExplanationPopup;
    public Button closeButton;
    public Button AddWordButton;
    public Button RemoveWordButton;

    // void Awake()
    // {
    //     GameEvents.CheckWordIsInListMethod(_word, AddWordButton.gameObject, RemoveWordButton.gameObject);
    // }
    void Start()
    {
        // GameEvents.CheckWordIsInListMethod(_word, AddWordButton.gameObject, RemoveWordButton.gameObject);
        closeButton.onClick.AddListener(CloseExplanation);
        AddWordButton.onClick.AddListener(AddWordToVocabularyList);
        RemoveWordButton.onClick.AddListener(RemoveWordFromVocabularyList);
    }

    // Update is called once per frame
    // void Update()
    // {

    // }

    // void OnEnable()
    // {
    //     GameEvents.CheckWordIsInListMethod(_word, AddWordButton.gameObject, RemoveWordButton.gameObject);
    // }


    public void SetText(string _word, string explanations)
    {
        this._word = _word;
        GameEvents.CheckWordIsInListMethod(_word, AddWordButton.gameObject, RemoveWordButton.gameObject);
        displayText.text = explanations;
    }
    private void CloseExplanation()
    {
        // ExplanationPopup.SetActive(false);
        FindAnyObjectByType<ExplanationWord>().Destroy();
    }

    private void Destroy()
    {
        Destroy(gameObject);
    }

    private void AddWordToVocabularyList()
    {
        if (_word == null)
        {
            Debug.LogError("Word is null");
            return;
        }
        GameEvents.AddWordToListMethod(_word, displayText.text);
        AddWordButton.gameObject.SetActive(false);
        RemoveWordButton.gameObject.SetActive(true);
    }

    private void RemoveWordFromVocabularyList()
    {
        if (_word == null)
        {
            Debug.LogError("Word is null");
            return;
        }
        GameEvents.RemoveWordFromListMethod(_word);
        AddWordButton.gameObject.SetActive(true);
        RemoveWordButton.gameObject.SetActive(false);
    }
}
