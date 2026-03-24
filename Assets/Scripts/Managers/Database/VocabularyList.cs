using UnityEngine;
using System.Collections.Generic;
using System.IO;

/// <summary>
/// Global singleton (DontDestroyOnLoad) that manages the user's saved vocabulary list.
/// All gameplay scenes can use VocabularyList.Instance to Add/Remove/Check words.
/// Subscribes to GameEvents for CRUD operations so scene scripts don't need to.
/// </summary>
public class VocabularyList : MonoBehaviour
{
    public static VocabularyList Instance { get; private set; }

    public DictionaryDB dictionaryDB = new DictionaryDB();
    private HashSet<string> _wordSet = new HashSet<string>();
    private bool _isDuplicate;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            _isDuplicate = true;
            DestroyImmediate(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        dictionaryDB = LoadWordDictionary() ?? new DictionaryDB();
        RebuildWordSet();
    }

    private void OnEnable()
    {
        if (_isDuplicate) return;
        GameEvents.OnSaveWordDictionary += Save;
        GameEvents.OnAddWordToList += AddWord;
        GameEvents.OnRemoveWordFromList += RemoveWord;
        GameEvents.OnCheckWordIsInList += CheckWordInList;
    }

    private void OnDisable()
    {
        if (_isDuplicate) return;
        GameEvents.OnSaveWordDictionary -= Save;
        GameEvents.OnAddWordToList -= AddWord;
        GameEvents.OnRemoveWordFromList -= RemoveWord;
        GameEvents.OnCheckWordIsInList -= CheckWordInList;
    }

    // ─── CRUD ──────────────────────────────────────────────────────────────

    public void AddWord(string word, string explanation)
    {
        string key = word.ToUpper();
        if (_wordSet.Contains(key)) return;

        _wordSet.Add(key);
        dictionaryDB.Add(key, explanation);
    }

    public void RemoveWord(string word)
    {
        string key = word.ToUpper();
        if (!_wordSet.Contains(key)) return;

        _wordSet.Remove(key);
        dictionaryDB.Remove(key);
    }

    public bool ContainsWord(string word)
    {
        return _wordSet.Contains(word.ToUpper());
    }

    public void CheckWordInList(string word, GameObject addWordButton, GameObject removeWordButton)
    {
        if (ContainsWord(word))
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

    public string GetExplanation(string word)
    {
        string key = word.ToUpper();
        if (dictionaryDB.ContainsKey(key))
            return dictionaryDB.GetValue(key);
        return null;
    }

    public List<string> GetAllWords()
    {
        return new List<string>(_wordSet);
    }

    public int WordCount => _wordSet.Count;

    // ─── Persistence ───────────────────────────────────────────────────────

    public void Save()
    {
        if (dictionaryDB == null)
        {
            Debug.LogError("[VocabularyList] Dictionary is null, cannot save.");
            return;
        }

        string filePath = Path.Combine(Application.persistentDataPath, "Dictionary.json");
        string dataAsJson = JsonUtility.ToJson(dictionaryDB);
        File.WriteAllText(filePath, dataAsJson);
        Debug.Log($"[VocabularyList] Saved {_wordSet.Count} words to {filePath}");
    }

    private DictionaryDB LoadWordDictionary()
    {
        // Try persistent data first (user's saved copy)
        string persistentPath = Path.Combine(Application.persistentDataPath, "Dictionary.json");
        if (File.Exists(persistentPath))
        {
            string json = File.ReadAllText(persistentPath);
            return JsonUtility.FromJson<DictionaryDB>(json);
        }

        // Fallback to Resources (initial data)
        string resourcesPath = Path.Combine(Application.dataPath, "Resources", "Dictionary.json");
        if (File.Exists(resourcesPath))
        {
            string json = File.ReadAllText(resourcesPath);
            return JsonUtility.FromJson<DictionaryDB>(json);
        }

        Debug.Log("[VocabularyList] No dictionary file found, starting fresh.");
        return null;
    }

    private void RebuildWordSet()
    {
        _wordSet.Clear();
        if (dictionaryDB?.keys == null) return;
        foreach (var key in dictionaryDB.keys)
        {
            _wordSet.Add(key.ToUpper());
        }
    }

    void OnApplicationQuit()
    {
        Save();
    }
}
