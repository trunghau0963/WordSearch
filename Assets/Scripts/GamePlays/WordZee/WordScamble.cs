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
        if (string.IsNullOrEmpty(desiredWord))
        {
            string result = word;
            int maxAttempts = 100;
            while (result == word && maxAttempts-- > 0)
            {
                result = "";
                List<char> chars = new(word.ToCharArray());
                while (chars.Count > 0)
                {
                    int index = UnityEngine.Random.Range(0, chars.Count);
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

    [Header("Animation")]
    [SerializeField] private float spawnStagger = 0.08f;
    [SerializeField] private float correctDelay = 0.8f;
    [SerializeField] private float transitionDelay = 0.5f;

    DictionaryDB dictionaryDB = new DictionaryDB();
    private bool _isChecking;
    private bool _isTransitioning;

    /// <summary> True when input should be blocked (during check/transition animations). </summary>
    public bool IsInputBlocked => _isChecking || _isTransitioning;

    void Awake()
    {
        main = this;
    }
    void Start()
    {
        // Priority 1: LevelPlayDataHolder (from level select menu)
        var holder = LevelPlayDataHolder.Instance;
        if (holder != null && holder.CurrentData != null && holder.CurrentData.words.Count > 0)
        {
            words = new List<Word>();
            foreach (var entry in holder.CurrentData.words)
            {
                words.Add(new Word(entry.word, ""));
            }
        }
        // Priority 2: VocabularyList (legacy / editor testing)
        else if (VocabularyList.Instance != null)
        {
            dictionaryDB = VocabularyList.Instance.dictionaryDB;
            words = new List<Word>();
            foreach (var w in dictionaryDB.keys)
            {
                words.Add(new Word(w, ""));
            }
        }

        if (words != null && words.Count > 0)
        {
            currentWord = 0;
            ShowScrambleWord(0);
        }
        else
        {
            Debug.LogWarning("[WordScamble] No word data available.");
        }
    }

    void Update()
    {
        if (!_isTransitioning)
            RepositionObject();
    }

    void RepositionObject()
    {
        if (charObjects.Count > 0)
        {
            float center = (charObjects.Count - 1) / 2f;
            for (int i = 0; i < charObjects.Count; i++)
            {
                if (charObjects[i] != null && charObjects[i].reactTransform != null)
                {
                    Vector2 target = new Vector2((i - center) * space, 0);
                    charObjects[i].reactTransform.anchoredPosition = Vector2.Lerp(
                        charObjects[i].reactTransform.anchoredPosition,
                        target, lerpSpeed * Time.deltaTime);
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
            // All words completed → fire win event
            bool isLastLevel = CheckIsLastLevel();
            GameEvents.ShowPopupMethod(isLastLevel);
            return;
        }

        string scrambled = words[index].GetWord();
        for (int i = 0; i < scrambled.Length; i++)
        {
            CharObj clone = Instantiate(charPrefab, container);
            clone.SetChar(scrambled[i]);
            charObjects.Add(clone);

            // Spawn pop-in animation
            clone.transform.localScale = Vector3.zero;
            LeanTween.scale(clone.gameObject, Vector3.one, 0.3f)
                .setDelay(i * spawnStagger)
                .setEaseOutBack();
        }

        _isChecking = false;
        _isTransitioning = false;
    }

    public void Swap(int indexA, int indexB)
    {
        if (_isChecking || _isTransitioning) return;

        (charObjects[indexB], charObjects[indexA]) = (charObjects[indexA], charObjects[indexB]);
        charObjects[indexA].transform.SetSiblingIndex(indexB);
        charObjects[indexB].transform.SetSiblingIndex(indexA);
        RepositionObject();
        CheckWord();
    }

    public void Select(CharObj charObj)
    {
        if (_isChecking || _isTransitioning) return;

        if (firstSelected)
        {
            Swap(firstSelected.index, charObj.index);
            firstSelected.Select();
            charObj.Select();
        }
        else
        {
            firstSelected = charObj;
            charObj.AnimateSelect();
        }
    }

    public void UnSelect()
    {
        firstSelected = null;
    }

    public void CheckWord()
    {
        if (!_isChecking)
            StartCoroutine(CoCheckWord());
    }

    IEnumerator CoCheckWord()
    {
        _isChecking = true;
        yield return new WaitForSeconds(0.5f);

        string current = "";
        foreach (CharObj charObj in charObjects)
        {
            current += charObj.charName;
        }

        if (current == words[currentWord].word)
        {
            _isTransitioning = true;

            // Correct! Celebrate each letter with stagger
            for (int i = 0; i < charObjects.Count; i++)
            {
                charObjects[i].AnimateCorrect(i * 0.1f);
            }
            yield return new WaitForSeconds(correctDelay);

            // Fade out current word
            for (int i = 0; i < charObjects.Count; i++)
            {
                LeanTween.scale(charObjects[i].gameObject, Vector3.zero, 0.2f)
                    .setDelay(i * 0.05f)
                    .setEaseInBack();
            }
            yield return new WaitForSeconds(transitionDelay);

            currentWord++;
            ShowScrambleWord(currentWord);
        }

        _isChecking = false;
    }

    private bool CheckIsLastLevel()
    {
        var holder = LevelPlayDataHolder.Instance;
        if (holder == null || holder.CurrentData == null) return true;

        var data = holder.CurrentData;
        var allTopics = TopicDataParser.ParseFromResources();
        foreach (var topic in allTopics)
        {
            if (topic.topicName == data.topicName)
            {
                foreach (var group in topic.groups)
                {
                    if (group.groupName == data.groupName)
                    {
                        return data.level >= group.levelCount;
                    }
                }
            }
        }
        return true;
    }
}
