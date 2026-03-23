using UnityEngine;
using System.Collections.Generic;
using System.IO;


public class VocabularyList : MonoBehaviour
{
    public static VocabularyList Instance { get; private set; }
    List<string> words = new List<string>();
    public DictionaryDB dictionaryDB = new DictionaryDB();
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
        dictionaryDB = LoadWordDictionary();
        if (dictionaryDB != null)
        {
            foreach (var word in dictionaryDB.keys)
            {
                words.Add(word);
            }
        }
    }

    private void OnEnable()
    {
        if (_isDuplicate) return;
        GameEvents.OnSaveWordDictionary += SaveVocabularyListToJson;
    }

    private void OnDisable()
    {
        if (_isDuplicate) return;
        GameEvents.OnSaveWordDictionary -= SaveVocabularyListToJson;
    }

    public DictionaryDB LoadWordDictionary()
    {
        string filePath = Path.Combine(Application.dataPath,"Resources","Dictionary.json");
        if (File.Exists(filePath))
        {
            string dataAsJson = File.ReadAllText(filePath);
            DictionaryDB loadedData = JsonUtility.FromJson<DictionaryDB>(dataAsJson);
            return loadedData;
        }
        else
        {
            Debug.LogError("Cannot load word dictionary!");
            return null;
        }
    }


    public void SaveVocabularyListToJson()
    {
        if (words.Count != dictionaryDB.keys.Count)
        {
            Debug.LogError("Vocabulary list is empty!");
            return;
        }
        // Save vocabulary list to json
        if (dictionaryDB == null)
        {
            Debug.LogError("Word dictionary is null!");
            return;
        }
        string filePath = Path.Combine(Application.dataPath,"Resources", "Dictionary.json");
        string dataAsJson = JsonUtility.ToJson(dictionaryDB);
        File.WriteAllText(filePath, dataAsJson);
        Debug.Log("Word dictionary saved!");
    }

    void OnApplicationQuit()
    {
        SaveVocabularyListToJson();
        Destroy(gameObject);
    }
}
