using System.Collections.Generic;
using UnityEngine;

public class HandleVocabularyList : MonoBehaviour
{
    List<string> words = new List<string>();
    DictionaryDB dictionary = new DictionaryDB();
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // VocabularyList.Instance.fileName = "Dictionary.json";
        // VocabularyList.Instance.LoadWordDictionary();
        dictionary = VocabularyList.Instance.dictionaryDB;
        if (dictionary != null)
        {
            foreach (var word in dictionary.keys)
            {
                words.Add(word);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
        private void OnEnable()
    {
        GameEvents.OnAddWordToList += AddWordToVocabularyList;
        GameEvents.OnRemoveWordFromList += RemoveWordFromVocabularyList;
        GameEvents.OnCheckWordIsInList += CheckIsAlreadyInVocabularyList;
    }

    private void OnDisable()
    {
        GameEvents.OnAddWordToList -= AddWordToVocabularyList;
        GameEvents.OnRemoveWordFromList -= RemoveWordFromVocabularyList;
        GameEvents.OnCheckWordIsInList -= CheckIsAlreadyInVocabularyList;
    }

    public void LoadVocabularyList()
    {
        // Load vocabulary list from database
    }

    public void GetVocabularyList()
    {
        // Get vocabulary list from database
    }

    public void AddWordToVocabularyList(string word, string explanation)
    {
        words.Add(word);
        dictionary.Add(word, explanation);
    }

    public void RemoveWordFromVocabularyList(string word)
    {
        words.Remove(word);
        dictionary.Remove(word);
    }

    public void CheckIsAlreadyInVocabularyList(string word, GameObject addWordButton, GameObject removeWordButton)
    {
        if (dictionary == null)
        {
            Debug.LogError("Word dictionary is null!");
            return;
        }
        if (words.Contains(word) || words.Contains(word.ToUpper()))
        {
            addWordButton.SetActive(false);
            removeWordButton.SetActive(true);
        }
        else
        {
            addWordButton.SetActive(true);
            removeWordButton.SetActive(false);
        }
    }
}
