using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class VocabularyMenu : Panel
{
    [SerializeField] private VocabularyWordItem wordItemPrefab = null;
    [SerializeField] private RectTransform wordListContainer = null;
    [SerializeField] private GameObject emptyMessage = null;
    [SerializeField] private Button closeButton = null;

    public override void Initialize()
    {
        if (IsInitialized) return;

        closeButton.onClick.AddListener(Close);
        base.Initialize();
    }

    public override void Open()
    {
        base.Open();
        RefreshWordList();
    }

    private void RefreshWordList()
    {
        ClearWordList();

        if (VocabularyList.Instance == null || VocabularyList.Instance.WordCount == 0)
        {
            if (emptyMessage != null) emptyMessage.SetActive(true);
            return;
        }

        if (emptyMessage != null) emptyMessage.SetActive(false);

        List<string> words = VocabularyList.Instance.GetAllWords();
        foreach (string word in words)
        {
            string explanation = VocabularyList.Instance.GetExplanation(word);
            VocabularyWordItem item = Instantiate(wordItemPrefab, wordListContainer);
            item.Initialize(word, explanation, OnRemoveWord);
        }
    }

    private void OnRemoveWord(string word)
    {
        VocabularyList.Instance.RemoveWord(word);
        VocabularyList.Instance.Save();
        RefreshWordList();
    }

    private void ClearWordList()
    {
        for (int i = wordListContainer.childCount - 1; i >= 0; i--)
        {
            Destroy(wordListContainer.GetChild(i).gameObject);
        }
    }
}
