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
    public int currentWord;
    public static WordScamble main;

    [Header("Word Constraints")]
    [SerializeField] private int maxWordLength = 6;

    [Header("Animation")]
    [SerializeField] private float spawnStagger = 0.08f;
    [SerializeField] private float correctDelay = 0.8f;
    [SerializeField] private float transitionDelay = 0.5f;
    [SerializeField] private float swapSlideTime = 0.25f;

    DictionaryDB dictionaryDB = new DictionaryDB();
    private bool _isChecking;
    private bool _isTransitioning;
    private bool _isPaused;
    private bool _isGameOver;

    // Drag state
    private CharObj _draggedChar;
    private CharObj _hoveredChar;
    private int _dragOriginalIndex;

    /// <summary> True when input should be blocked (during check/transition/pause/gameover). </summary>
    public bool IsInputBlocked => _isChecking || _isTransitioning || _isPaused || _isGameOver;

    void Awake()
    {
        main = this;
    }

    private void OnEnable()
    {
        GameEvents.OnPauseGame += OnPause;
        GameEvents.OnResumeGame += OnResume;
        GameEvents.OnGameOver += OnGameOver;
        GameEvents.OnRevealAnswers += RevealCorrectOrder;
    }

    private void OnDisable()
    {
        GameEvents.OnPauseGame -= OnPause;
        GameEvents.OnResumeGame -= OnResume;
        GameEvents.OnGameOver -= OnGameOver;
        GameEvents.OnRevealAnswers -= RevealCorrectOrder;
    }

    private void OnPause() { _isPaused = true; }
    private void OnResume() { _isPaused = false; }
    private void OnGameOver() { _isGameOver = true; }

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

        // Filter out words that exceed maxWordLength
        if (words != null)
            words.RemoveAll(w => w.word.Length > maxWordLength);

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
                // Skip the card being dragged — it follows the pointer
                if (charObjects[i] == null || charObjects[i].reactTransform == null) continue;
                if (charObjects[i].IsDragging) continue;

                Vector2 target = GetSlotPosition(i);
                charObjects[i].reactTransform.anchoredPosition = Vector2.Lerp(
                    charObjects[i].reactTransform.anchoredPosition,
                    target, lerpSpeed * Time.deltaTime);
                charObjects[i].index = i;
            }
        }
    }

    /// <summary>
    /// Calculate the slot position for a given index.
    /// </summary>
    public Vector2 GetSlotPosition(int i)
    {
        float center = (charObjects.Count - 1) / 2f;
        return new Vector2((i - center) * space, 0);
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

    // ── Drag & Drop Handlers ─────────────────────────────────────────

    public void OnCharDragBegin(CharObj dragged)
    {
        _draggedChar = dragged;
        _dragOriginalIndex = dragged.index;
        _hoveredChar = null;
    }

    public void OnCharDragging(CharObj dragged)
    {
        // Find the nearest other CharObj that the dragged card is hovering over
        CharObj nearest = null;
        float minDist = float.MaxValue;

        for (int i = 0; i < charObjects.Count; i++)
        {
            if (charObjects[i] == dragged) continue;
            float dist = Vector2.Distance(dragged.reactTransform.anchoredPosition, GetSlotPosition(i));
            if (dist < space * 0.7f && dist < minDist)
            {
                minDist = dist;
                nearest = charObjects[i];
            }
        }

        // Update highlight
        if (nearest != _hoveredChar)
        {
            if (_hoveredChar != null) _hoveredChar.SetHighlight(false);
            _hoveredChar = nearest;
            if (_hoveredChar != null) _hoveredChar.SetHighlight(true);
        }
    }

    public void OnCharDragEnd(CharObj dragged)
    {
        // Clear highlight
        if (_hoveredChar != null)
        {
            _hoveredChar.SetHighlight(false);

            int indexA = dragged.index;
            int indexB = _hoveredChar.index;

            if (indexA != indexB)
            {
                // Swap in the list
                (charObjects[indexA], charObjects[indexB]) = (charObjects[indexB], charObjects[indexA]);
                charObjects[indexA].index = indexA;
                charObjects[indexB].index = indexB;

                // Animate the swapped card to its new slot
                charObjects[indexA].AnimateToPosition(GetSlotPosition(indexA), swapSlideTime);
                // Animate the dragged card to its new slot
                charObjects[indexB].AnimateToPosition(GetSlotPosition(indexB), swapSlideTime);

                // Check word after swap animation completes
                LeanTween.delayedCall(swapSlideTime + 0.05f, () => CheckWord());
            }
            else
            {
                // Dropped on same slot — snap back
                dragged.AnimateToPosition(GetSlotPosition(indexA), swapSlideTime * 0.5f);
            }
        }
        else
        {
            // No valid target — snap back to original position
            dragged.AnimateToPosition(GetSlotPosition(dragged.index), swapSlideTime * 0.5f);
        }

        _draggedChar = null;
        _hoveredChar = null;
    }

    // ── Word Check ────────────────────────────────────────────────

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
        return data.level >= data.totalLevelsInGroup;
    }

    private void RevealCorrectOrder()
    {
        if (currentWord >= words.Count || charObjects.Count == 0) return;
        _isTransitioning = true;

        string correct = words[currentWord].word;
        // Build target mapping: for each position i in the correct word,
        // find the CharObj that currently holds correct[i] and swap into slot i.
        // Use a simple selection approach so each char is used only once.
        bool[] used = new bool[charObjects.Count];

        CharObj[] ordered = new CharObj[correct.Length];
        for (int i = 0; i < correct.Length; i++)
        {
            for (int j = 0; j < charObjects.Count; j++)
            {
                if (!used[j] && charObjects[j].charName == correct[i])
                {
                    ordered[i] = charObjects[j];
                    used[j] = true;
                    break;
                }
            }
        }

        // Animate each CharObj: slide to correct slot → turn yellow → shake
        for (int i = 0; i < ordered.Length; i++)
        {
            if (ordered[i] == null) continue;
            ordered[i].AnimateReveal(GetSlotPosition(i), 0.5f, i * 0.15f);
        }

        // Update the list so RepositionObject doesn't fight the animation
        for (int i = 0; i < ordered.Length; i++)
        {
            if (ordered[i] != null)
            {
                charObjects[i] = ordered[i];
                charObjects[i].index = i;
            }
        }
    }
}
