using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class Word
{
    public string word;
    [Header("leave it empty if you want random")]
    public string desiredWord;

    public Word(string word, string desiredWord)
    {
        this.word = word;
        this.desiredWord = desiredWord;
    }

    public string GetWord()
    {
        if (desiredWord == null || desiredWord.Equals(""))
        {
            string result = word;
            while (result == word)
            {
                result = "";
                List<char> chars = new(word.ToCharArray());
                while (chars.Count > 0)
                {
                    int index = UnityEngine.Random.Range(0, chars.Count - 1);
                    result += chars[index];
                    chars.RemoveAt(index);
                }
            }

            return result;
        }
        else
        {
            return desiredWord;
        }
    }
}

public class WordScamble : MonoBehaviour
{
    public List<Word> words;
    [Header("UI References")]
    public CharObj charPrefab;
    public float lerpSpeed = 5;
    public Transform container;
    public float space;
    List<CharObj> charObjects = new();
    CharObj firstSelected;
    public int currentWord;
    public static WordScamble main;

    DictionaryDB dictionaryDB = new DictionaryDB();

    void Awake()
    {
        main = this;
    }
    // Start is called before the first frame update
    void Start()
    {
        if (VocabularyList.Instance != null)
        {
            dictionaryDB = VocabularyList.Instance.dictionaryDB;
            words = new List<Word>();
            foreach (var word in dictionaryDB.keys)
            {
                words.Add(new Word(word, ""));
            }
        }
        ShowScrambleWord(currentWord);
    }

    // Update is called once per frame
    void Update()
    {
        // Add null check before accessing RectTransform

        RepositionObject();
    }
    void RepositionObject()
    {
        // for(int i = 0; i < charObjects.Count; i++){
        //     charObjects[i].reactTransform.anchoredPosition = new Vector2(i * space, 0);
        //     charObjects[i].index = i;
        // }
        if (charObjects.Count > 0)
        {
            float center = (charObjects.Count - 1) / 2;
            print("Center: " + center);
            for (int i = 0; i < charObjects.Count; i++)
            {
                if (charObjects[i].reactTransform != null)
                {
                    charObjects[i].reactTransform.anchoredPosition = Vector2.Lerp(charObjects[i].reactTransform.anchoredPosition, new Vector2((i - center) * space, 0), lerpSpeed * Time.deltaTime);
                    charObjects[i].index = i;
                }
            }
        }
    }

    public void ShowScrambleWord()
    {
        ShowScrambleWord(UnityEngine.Random.Range(0, words.Count));
    }

    public void ShowScrambleWord(int index)
    {
        currentWord = index;
        charObjects.Clear();
        foreach (Transform item in container)
        {
            Destroy(item.gameObject);
        }
        if (index >= words.Count)
        {
            Debug.LogError("Index out of range");
            return;
        }
        string word = words[index].GetWord();
        for (int i = 0; i < word.Length; i++)
        {
            CharObj clone = Instantiate(charPrefab, container);
            clone.SetChar(word[i]);
            charObjects.Add(clone);
            string temp = clone.ShowActive();
            print(i + " " + temp);
        }
        currentWord = index;
        RepositionObject();
    }

    public void Swap(int indexA, int indexB)
    {
        (charObjects[indexB], charObjects[indexA]) = (charObjects[indexA], charObjects[indexB]);
        charObjects[indexA].transform.SetSiblingIndex(indexB);
        charObjects[indexB].transform.SetSiblingIndex(indexA);
        RepositionObject();
        CheckWord();
    }

    public void Select(CharObj charObj)
    {
        if (firstSelected)
        {
            Swap(firstSelected.index, charObj.index);
            firstSelected.Select();
            charObj.Select();
        }
        else
        {
            firstSelected = charObj;
        }
    }

    public void UnSelect()
    {
        firstSelected = null;
    }

    public void CheckWord()
    {
        StartCoroutine(CoCheckWord());
    }

    IEnumerator CoCheckWord()
    {
        yield return new WaitForSeconds(1);
        CheckWord();

              string current = "";
        foreach (CharObj charObj in charObjects)
        {
            current += charObj.charName;
        }
        if (current == words[currentWord].word)
        {
            currentWord++;
            ShowScrambleWord(currentWord);
        }
    }

}
