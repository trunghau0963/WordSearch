using UnityEngine;
using System.Collections.Generic;
using System.IO;

[System.Serializable]
public class GameDataLoad
{
    public List<CategoryDB> DataSet;
}

[System.Serializable]
public class CategoryDB
{
    public string CategoryName;
    public List<SectionDB> Sections;
}

[System.Serializable]
public class SectionDB
{
    public string SectionName;
    public List<Words> Words;
}

[System.Serializable]
public class Words
{
    public string word_name;
    public string word_explanation;
}

[System.Serializable]

public class LoadData : MonoBehaviour
{
    public GameDataLoad data;
    public DictionaryDB dictionaryDB;
    string nameFile = "Data";
    // public string nameWordCheck;

    public GameObject wordExplanationPrefab;
    private Dictionary<string, string> wordDictionary; // Dictionary to store word names and meanings

    void Start()
    {
        wordDictionary = new Dictionary<string, string>(); // Initialize the dictionary
        data = LoadGameData();
        dictionaryDB = LoadWordDictionary2();
    }

    void Update()
    {
        // Example usage: print meaning of a specific word if it exists in the dictionary
        // CheckWordDictionary();
    }

    void OnApplicationQuit()
    {
        // SaveGameData();
    }


    public GameDataLoad LoadGameData()
    {
        string file = nameFile + ".json";
        string filePath = Path.Combine(Application.dataPath,"Resources", file);
        if (File.Exists(filePath))
        {
            string dataAsJson = File.ReadAllText(filePath);
            GameDataLoad loadedData = JsonUtility.FromJson<GameDataLoad>(dataAsJson);

            // Populate the dictionary with words and their explanations
            foreach (var category in loadedData.DataSet)
            {
                foreach (var section in category.Sections)
                {
                    foreach (var word in section.Words)
                    {
                        if (!wordDictionary.ContainsKey(word.word_name.ToUpper())) // Ensure no duplicates
                        {
                            wordDictionary.Add(word.word_name.ToUpper(), word.word_explanation);
                        }
                    }
                }
            }
            // SaveGameData();
            SaveWordDictionary(); // Save the word dictionary
            return loadedData;
        }
        else
        {
            Debug.LogError("Cannot load game data!");
            return null;
        }
    }

    // public Dictionary<string, string> LoadWordDictionary()
    // {
    //     Debug.Log("Loading word dictionary...");
    //     string filePath = Path.Combine(Application.dataPath,"Resources", "WordDictionary.json");
    //     if (File.Exists(filePath))
    //     {
    //         string dataAsJson = File.ReadAllText(filePath);
    //         Serialization<string, string> loadedData = JsonUtility.FromJson<Serialization<string, string>>(dataAsJson);
    //         Dictionary<string, string> dictionary = loadedData.ToDictionary();
    //         // DictionaryDB loadedData = JsonUtility.FromJson<DictionaryDB>(dataAsJson);
    //         // return loadedData;
    //         return dictionary;
    //     }
    //     else
    //     {
    //         Debug.LogError("Cannot load word dictionary!");
    //         return null;
    //     }
    // }

    public DictionaryDB LoadWordDictionary2()
    {
        Debug.Log("Loading word dictionary...");
        string filePath = Path.Combine(Application.dataPath,"Resources", "WordDictionary.json");
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


    public void SaveGameData()
    {
        string dataAsJson = JsonUtility.ToJson(data);
        string filePath = Path.Combine(Application.dataPath,"Resources", nameFile + "Save.json");
        File.WriteAllText(filePath, dataAsJson);
    }

    public void SaveWordDictionary()
    {
        if (wordDictionary == null)
        {
            Debug.LogError("Word dictionary is null!");
            return;
        }
        string filePath = Path.Combine(Application.dataPath,"Resources", "WordDictionary.json");
        if (File.Exists(filePath))
        {
            Debug.Log("Word dictionary already exists. Skipping save.");
            return;
        }
        string dataAsJson = JsonUtility.ToJson(new Serialization<string, string>(wordDictionary));
        File.WriteAllText(filePath, dataAsJson);
        Debug.Log("Word dictionary saved!");
    }

    // public void CheckWordDictionary()
    // {
    //     // Example usage: print meaning of a specific word if it exists in the dictionary
    //     if (wordDictionary.TryGetValue(nameWordCheck, out string meaning))
    //     {
    //         Debug.Log($"Meaning of {nameWordCheck}: {meaning}");
    //     }
    //     else
    //     {
    //         Debug.Log($"Word {nameWordCheck} not found in the dictionary!");
    //     }
    // }

    public void ShowExplanation(string word)
    {
        if (wordExplanationPrefab != null)
        {
            DestroyAllExplanation();
        }
        if (wordDictionary.TryGetValue(word, out string meaning))
        {
            // wordExplanationPrefab.SetActive(true);
            Instantiate(wordExplanationPrefab, transform).GetComponent<ExplanationWord>().SetText(word, meaning);
        }
        else
        {
            Debug.Log($"Word {word} not found in the dictionary!");
        }
    }

    public void DestroyAllExplanation()
    {
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }
    }

    // public void HideExplanation()
    // {
    //     wordExplanationPrefab.SetActive(false);
    // }
}
