using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[DefaultExecutionOrder(-1)]
public class BoardWD : MonoBehaviour
{
    private Row[] rows;
    private int rowIndex;
    private int columnIndex;

    private string[] solutions;
    private string[] validWords;
    private string word;

    [Header("Tiles")]
    public Tiles.State emptyState;
    public Tiles.State occupiedState;
    public Tiles.State correctState;
    public Tiles.State wrongSpotState;
    public Tiles.State incorrectState;

    [Header("UI")]
    public GameObject ExplanationPrefab;
    public Button tryAgainButton;
    public Button newWordButton;
    public GameObject invalidWordText;
    public DictionaryDB dictionary = new DictionaryDB();

    [Header("Animation")]
    [SerializeField] private float flipDuration = 0.15f;
    [SerializeField] private float flipStagger = 0.25f;
    [SerializeField] private float typeBounceScale = 1.2f;
    [SerializeField] private float typeBounceDuration = 0.08f;
    [SerializeField] private float winBounceHeight = 20f;
    [SerializeField] private float rowShakeIntensity = 10f;
    [SerializeField] private float rowShakeDuration = 0.4f;

    [Header("Keyboard")]
    [Tooltip("Parent transform containing all Key rows (auto-found if null)")]
    [SerializeField] private Transform keyboardParent;

    // Level data
    private List<WordEntry> _levelWords;
    private int _currentWordIndex;
    private bool _isSubmitting;
    private bool _isPaused;
    private bool _isGameOver;
    private Button _submitButton;
    private Key[] _allKeys;

    // Start is called before the first frame update
    private void Awake()
    {
        rows = GetComponentsInChildren<Row>();
    }

    private void Start()
    {
        Key.OnKeyPressed += KeyPressCallback;
        CacheKeyboardReferences();

        // Priority 1: LevelPlayDataHolder (from level select menu)
        var holder = LevelPlayDataHolder.Instance;
        if (holder != null && holder.CurrentData != null && holder.CurrentData.words.Count > 0)
        {
            dictionary = new DictionaryDB();
            foreach (var entry in holder.CurrentData.words)
            {
                dictionary.Add(entry.word, entry.question ?? "");
            }
            solutions = dictionary.keys.ToArray();
            NewGame();
        }
        // Priority 2: VocabularyList (legacy / editor testing)
        else if (VocabularyList.Instance != null)
        {
            dictionary = VocabularyList.Instance.dictionaryDB;
            solutions = dictionary.keys.ToArray();
            NewGame();
        }
        else
        {
            Debug.LogError("[BoardWD] No word data available (LevelPlayDataHolder and VocabularyList both null).");
        }
    }

    private void CacheKeyboardReferences()
    {
        _allKeys = FindObjectsByType<Key>(FindObjectsSortMode.None);
        foreach (var k in _allKeys)
        {
            if (k.Action == Key.KeyAction.Submit)
            {
                _submitButton = k.GetComponent<Button>();
                break;
            }
        }

        // Fallback: search by TMP text if no key has Submit action set
        if (_submitButton == null)
        {
            foreach (var k in _allKeys)
            {
                var tmp = k.GetComponentInChildren<TextMeshProUGUI>();
                if (tmp != null && tmp.text.ToUpper().Trim() == "SUBMIT")
                {
                    _submitButton = k.GetComponent<Button>();
                    break;
                }
            }
        }

        Debug.Log($"[BoardWD] Cached {_allKeys.Length} keys, submitButton={(_submitButton != null ? "found" : "NOT FOUND")}");
    }

    private void LoadData()
    {
        TextAsset textFile = Resources.Load("official_wordle_common") as TextAsset;
        solutions = textFile.text.Split('\n');

        textFile = Resources.Load("official_wordle_all") as TextAsset;
        validWords = textFile.text.Split('\n');
    }

    public void NewGame()
    {
        ClearBoard();
        ResetKeyboardColors();
        SetRandomWord();
        _isGameOver = false;
        _isSubmitting = false;
        enabled = true;
        AnimateBoardEntry();
        UpdateSubmitInteractable();
    }

    public void TryAgain()
    {
        ClearBoard();
        ResetKeyboardColors();
        _isGameOver = false;
        _isSubmitting = false;
        enabled = true;
        AnimateBoardEntry();
        UpdateSubmitInteractable();
    }

    private void SetRandomWord()
    {
        int tileCount = (rows.Length > 0 && rows[0].tiles != null) ? rows[0].tiles.Length : 5;
        int attempts = 0;
        do
        {
            word = solutions[Random.Range(0, solutions.Length)];
            attempts++;
            if (attempts > 1000)
            {
                Debug.LogWarning($"[BoardWD] No word with {tileCount} chars found. Using first word: '{solutions[0]}'");
                word = solutions[0];
                break;
            }
        } while (word.Trim().Length != tileCount);
        string meaning = dictionary.GetValue(word);
        word = word.ToLower().Trim();
        if (ExplanationPrefab != null)
        {
            var exp = ExplanationPrefab.GetComponent<ExplanationWord>();
            if (exp != null) exp.SetText(word, meaning);
        }
        Debug.Log($"[BoardWD] Word: '{word}' (length={word.Length}, tiles={tileCount})");
    }

    /// <summary>
    /// Board tiles pop-in animation with staggered delay, followed by keyboard slide-in.
    /// </summary>
    private void AnimateBoardEntry()
    {
        for (int r = 0; r < rows.Length; r++)
        {
            for (int c = 0; c < rows[r].tiles.Length; c++)
            {
                var tile = rows[r].tiles[c];
                tile.transform.localScale = Vector3.zero;
                float delay = r * 0.06f + c * 0.03f;
                LeanTween.scale(tile.gameObject, Vector3.one, 0.25f)
                    .setDelay(delay)
                    .setEaseOutBack();
            }
        }

        AnimateKeyboardEntry();
    }

    /// <summary>
    /// Animates keyboard keys popping in row-by-row with stagger.
    /// </summary>
    private void AnimateKeyboardEntry()
    {
        Transform kbRoot = keyboardParent;
        if (kbRoot == null)
        {
            // Try finding a Key in the scene and walk up to find rows
            var anyKey = FindAnyObjectByType<Key>();
            if (anyKey != null)
            {
                var parent = anyKey.transform.parent;
                kbRoot = parent != null ? parent.parent : null; // key -> row -> keyboard
            }
        }
        if (kbRoot == null) return;

        float baseDelay = rows.Length * 0.06f + 0.1f; // start after board tiles finish
        int keyIndex = 0;
        for (int r = 0; r < kbRoot.childCount; r++)
        {
            var rowTransform = kbRoot.GetChild(r);
            var keys = rowTransform.GetComponentsInChildren<Key>();
            foreach (var key in keys)
            {
                key.AnimateEntry(baseDelay + keyIndex * 0.02f);
                keyIndex++;
            }
        }
    }

    private void KeyPressCallback(string letter)
    {
        // Debug: log every key press and current state
        Debug.Log($"[BoardWD] Key='{letter}' | row={rowIndex}/{rows.Length} col={columnIndex} | submitting={_isSubmitting} paused={_isPaused} gameOver={_isGameOver}");

        if (_isSubmitting || _isPaused)
        {
            Debug.Log($"[BoardWD] Input blocked: submitting={_isSubmitting}, paused={_isPaused}");
            return;
        }
        if (_isGameOver)
        {
            Debug.LogWarning("[BoardWD] Input blocked: Game Over! (timer hết hoặc đã dùng hết lượt)");
            return;
        }
        if (rowIndex >= rows.Length)
        {
            Debug.LogWarning($"[BoardWD] Input blocked: rowIndex={rowIndex} >= rows.Length={rows.Length}");
            return;
        }

        Row currentRow = rows[rowIndex];

        // === DELETE ===
        if (letter == "DELETE" || letter == "Delete" || letter == "⌫" || letter == "DEL" || letter == "←")
        {
            if (columnIndex > 0)
            {
                columnIndex--;
                var delTile = currentRow.tiles[columnIndex];
                // Shrink-out then clear
                LeanTween.cancel(delTile.gameObject);
                LeanTween.scale(delTile.gameObject, Vector3.one * 0.6f, 0.06f)
                    .setEaseInQuad()
                    .setOnComplete(() =>
                    {
                        delTile.SetLetter("");
                        delTile.SetState(emptyState);
                        LeanTween.scale(delTile.gameObject, Vector3.one, 0.1f)
                            .setEaseOutBack();
                    });
                Debug.Log($"[BoardWD] Deleted tile at col={columnIndex}");
            }
            UpdateSubmitInteractable();
            return;
        }

        // === SUBMIT ===
        if (letter == "SUBMIT" || letter == "Submit" || letter == "Enter")
        {
            if (columnIndex >= currentRow.tiles.Length)
            {
                Debug.Log($"[BoardWD] >>> Submitting row {rowIndex}: '{currentRow.word}' vs answer '{word}'");
                StartCoroutine(SubmitRowAnimated(currentRow));
            }
            else
            {
                Debug.Log($"[BoardWD] Row not full: col={columnIndex}/{currentRow.tiles.Length} — shaking");
                // Row not full – shake to indicate incomplete
                StartCoroutine(ShakeRow(currentRow));
                if (invalidWordText != null)
                {
                    invalidWordText.SetActive(true);
                    LeanTween.delayedCall(1f, () =>
                    {
                        if (invalidWordText != null) invalidWordText.SetActive(false);
                    });
                }
            }
            return;
        }

        // === REGULAR LETTER ===
        if (columnIndex < currentRow.tiles.Length
            && letter.Length == 1 && char.IsLetter(letter[0]))
        {
            var tile = currentRow.tiles[columnIndex];
            tile.SetLetter(letter.ToUpper());
            tile.SetState(occupiedState);
            columnIndex++;

            // Type bounce animation
            LeanTween.cancel(tile.gameObject);
            tile.transform.localScale = Vector3.one;
            LeanTween.scale(tile.gameObject, Vector3.one * typeBounceScale, typeBounceDuration)
                .setEaseOutQuad()
                .setOnComplete(() =>
                {
                    LeanTween.scale(tile.gameObject, Vector3.one, typeBounceDuration)
                        .setEaseInQuad();
                });

            UpdateSubmitInteractable();
        }
        else
        {
            Debug.Log($"[BoardWD] Key '{letter}' ignored (not a letter, or row full)");
        }
    }

    private void UpdateSubmitInteractable()
    {
        if (_submitButton == null) return;
        bool canSubmit = rowIndex < rows.Length
                         && columnIndex >= rows[rowIndex].tiles.Length
                         && !_isSubmitting && !_isGameOver;
        _submitButton.interactable = canSubmit;
    }

    /// <summary>
    /// Animated row submission: tiles flip one-by-one revealing color states.
    /// Properly handles duplicate letters in the word.
    /// </summary>
    private IEnumerator SubmitRowAnimated(Row row)
    {
        _isSubmitting = true;
        UpdateSubmitInteractable();

        int len = row.tiles.Length;
        Tiles.State[] states = new Tiles.State[len];

        // Build a frequency map of remaining chars in the answer
        Dictionary<char, int> remaining = new Dictionary<char, int>();
        foreach (char c in word)
        {
            if (remaining.ContainsKey(c)) remaining[c]++;
            else remaining[c] = 1;
        }

        // Pass 1: exact matches (green) – consume those letters first
        for (int i = 0; i < len && i < word.Length; i++)
        {
            string tileLetter = row.tiles[i].letter;
            if (string.IsNullOrEmpty(tileLetter)) continue;
            char c = char.ToLower(tileLetter[0]);
            if (c == word[i])
            {
                states[i] = correctState;
                remaining[c]--;
            }
        }

        // Pass 2: wrong spot or incorrect
        for (int i = 0; i < len; i++)
        {
            if (states[i] != null) continue; // already correct
            string tileLetter = row.tiles[i].letter;
            if (string.IsNullOrEmpty(tileLetter))
            {
                states[i] = incorrectState;
                continue;
            }
            char c = char.ToLower(tileLetter[0]);
            if (remaining.ContainsKey(c) && remaining[c] > 0)
            {
                states[i] = wrongSpotState;
                remaining[c]--;
            }
            else
            {
                states[i] = incorrectState;
            }
        }

        // Null-safety fallback: ensure no state is null
        for (int i = 0; i < states.Length; i++)
        {
            if (states[i] == null)
            {
                Debug.LogWarning($"[BoardWD] State[{i}] is null! Check Inspector: correctState/wrongSpotState/incorrectState must be assigned.");
                states[i] = incorrectState ?? emptyState;
            }
        }

        // Determine overall result for post-flip effects
        bool hasCorrect = false;
        bool hasWrongSpot = false;
        for (int i = 0; i < states.Length; i++)
        {
            if (states[i] == correctState) hasCorrect = true;
            if (states[i] == wrongSpotState) hasWrongSpot = true;
        }
        bool allIncorrect = !hasCorrect && !hasWrongSpot;

        // Flip tiles one by one with color reveal
        for (int i = 0; i < row.tiles.Length; i++)
        {
            var tile = row.tiles[i];
            var state = states[i];

            // Flip down (scale Y → 0)
            LeanTween.scaleY(tile.gameObject, 0f, flipDuration).setEaseInQuad();
            yield return new WaitForSeconds(flipDuration);

            // Apply color at midpoint
            tile.SetState(state);

            // Flip up (scale Y → 1)
            LeanTween.scaleY(tile.gameObject, 1f, flipDuration).setEaseOutQuad();
            yield return new WaitForSeconds(flipStagger);

            // Post-flip feedback per tile
            if (state == correctState)
            {
                LeanTween.scale(tile.gameObject, Vector3.one * 1.15f, 0.1f)
                    .setEaseOutQuad()
                    .setOnComplete(() =>
                        LeanTween.scale(tile.gameObject, Vector3.one, 0.1f).setEaseInQuad());
            }
            else if (state == wrongSpotState)
            {
                LeanTween.rotateZ(tile.gameObject, 5f, 0.05f)
                    .setEaseInOutSine()
                    .setLoopPingPong(1)
                    .setOnComplete(() => tile.transform.localRotation = Quaternion.identity);
            }
        }

        // Shake the entire row if all tiles are incorrect
        if (allIncorrect)
        {
            yield return StartCoroutine(ShakeRow(row));
        }

        // Update keyboard key colors based on this guess
        UpdateKeyboardColors(row, states);

        yield return new WaitForSeconds(0.2f);

        bool won = HasWon(row);
        rowIndex++;
        columnIndex = 0;

        if (won)
        {
            // Win bounce animation on the winning row
            for (int i = 0; i < row.tiles.Length; i++)
            {
                var tile = row.tiles[i];
                float delay = i * 0.1f;
                LeanTween.moveLocalY(tile.gameObject, tile.transform.localPosition.y + winBounceHeight, 0.2f)
                    .setDelay(delay)
                    .setEaseOutQuad()
                    .setLoopPingPong(1);
            }
            yield return new WaitForSeconds(1f);

            _isSubmitting = false;
            enabled = false;
            bool isLastLevel = CheckIsLastLevel();
            GameEvents.ShowPopupMethod(isLastLevel);
            yield break;
        }

        if (rowIndex >= rows.Length)
        {
            // Reveal the correct word on the last row before showing game over
            Row lastRow = rows[rows.Length - 1];
            yield return StartCoroutine(RevealCorrectWordAnimated(lastRow));
            yield return new WaitForSeconds(0.5f);

            _isSubmitting = false;
            enabled = false;
            GameEvents.GameOverlMethod();
            yield break;
        }

        _isSubmitting = false;
        UpdateSubmitInteractable();
    }

    private bool CheckIsLastLevel()
    {
        var holder = LevelPlayDataHolder.Instance;
        if (holder == null || holder.CurrentData == null) return true;

        var data = holder.CurrentData;
        return data.level >= data.totalLevelsInGroup;
    }

    private bool HasWon(Row row)
    {
        for (int i = 0; i < row.tiles.Length; i++)
        {
            if (row.tiles[i].state != correctState)
            {
                return false;
            }
        }

        return true;
    }

    private void ClearBoard()
    {
        for (int row = 0; row < rows.Length; row++)
        {
            for (int col = 0; col < rows[row].tiles.Length; col++)
            {
                rows[row].tiles[col].SetLetter("");
                rows[row].tiles[col].SetState(emptyState);
            }
        }

        rowIndex = 0;
        columnIndex = 0;
    }

    private void OnEnable()
    {
        if (tryAgainButton != null) tryAgainButton.interactable = false;
        if (newWordButton != null) newWordButton.interactable = false;

        GameEvents.OnPauseGame += OnPause;
        GameEvents.OnResumeGame += OnResume;
        GameEvents.OnGameOver += OnGameOverEvent;
        GameEvents.OnRevealAnswers += RevealCorrectWord;
    }

    private void OnDisable()
    {
        if (tryAgainButton != null) tryAgainButton.interactable = true;
        if (newWordButton != null) newWordButton.interactable = true;

        GameEvents.OnPauseGame -= OnPause;
        GameEvents.OnResumeGame -= OnResume;
        GameEvents.OnGameOver -= OnGameOverEvent;
        GameEvents.OnRevealAnswers -= RevealCorrectWord;
    }

    private void OnPause() { _isPaused = true; }
    private void OnResume() { _isPaused = false; }
    private void OnGameOverEvent() { _isGameOver = true; }

    private void RevealCorrectWord()
    {
        if (string.IsNullOrEmpty(word)) return;

        // Pick the target row: first empty row, or the last row if all are used
        int targetRow = rowIndex < rows.Length ? rowIndex : rows.Length - 1;
        Row row = rows[targetRow];

        StartCoroutine(RevealCorrectWordAnimated(row));
    }

    private IEnumerator RevealCorrectWordAnimated(Row row)
    {
        for (int i = 0; i < row.tiles.Length && i < word.Length; i++)
        {
            var tile = row.tiles[i];

            // Flip down
            LeanTween.scaleY(tile.gameObject, 0f, flipDuration).setEaseInQuad();
            yield return new WaitForSeconds(flipDuration);

            // Set correct letter and state at midpoint
            tile.SetLetter(word[i].ToString().ToUpper());
            tile.SetState(correctState);

            // Flip up
            LeanTween.scaleY(tile.gameObject, 1f, flipDuration).setEaseOutQuad();
            yield return new WaitForSeconds(flipStagger);
        }
    }

    /// <summary>
    /// Shakes a row left-right to indicate wrong guess.
    /// </summary>
    private IEnumerator ShakeRow(Row row)
    {
        var rt = row.GetComponent<RectTransform>();
        if (rt == null) yield break;

        Vector3 origin = rt.localPosition;
        float elapsed = 0f;
        while (elapsed < rowShakeDuration)
        {
            float x = Mathf.Sin(elapsed / rowShakeDuration * Mathf.PI * 6f) * rowShakeIntensity
                      * (1f - elapsed / rowShakeDuration);
            rt.localPosition = origin + new Vector3(x, 0f, 0f);
            elapsed += Time.deltaTime;
            yield return null;
        }
        rt.localPosition = origin;
    }

    /// <summary>
    /// Updates keyboard key colors after a guess submission.
    /// Green > Yellow > Gray priority (a key only "upgrades", never downgrades).
    /// </summary>
    private void UpdateKeyboardColors(Row row, Tiles.State[] states)
    {
        if (_allKeys == null) return;

        for (int i = 0; i < row.tiles.Length && i < states.Length; i++)
        {
            string tileLetter = row.tiles[i].letter;
            if (string.IsNullOrEmpty(tileLetter)) continue;
            char c = char.ToUpper(tileLetter[0]);
            var state = states[i];

            foreach (var key in _allKeys)
            {
                var tmp = key.GetComponentInChildren<TextMeshProUGUI>();
                if (tmp == null || tmp.text.Length != 1) continue;
                if (char.ToUpper(tmp.text[0]) != c) continue;

                // Only upgrade: correct > wrongSpot > incorrect
                // If key already shows correct, don't change it
                var img = key.GetComponent<Image>();
                if (img == null) break;

                if (state == correctState)
                {
                    img.color = correctState.fillColor;
                    key.SetState(correctState);
                }
                else if (state == wrongSpotState)
                {
                    // Only set if not already correct
                    if (img.color != correctState?.fillColor)
                    {
                        img.color = wrongSpotState.fillColor;
                        key.SetState(wrongSpotState);
                    }
                }
                else if (state == incorrectState)
                {
                    // Only set if not already correct or wrongSpot
                    if (img.color != correctState?.fillColor && img.color != wrongSpotState?.fillColor)
                    {
                        img.color = incorrectState.fillColor;
                        key.SetState(incorrectState);
                    }
                }
                break;
            }
        }
    }

    private void ResetKeyboardColors()
    {
        if (_allKeys == null) return;
        foreach (var key in _allKeys)
        {
            var img = key.GetComponent<Image>();
            if (img != null && occupiedState != null)
                img.color = Color.white;
            if (emptyState != null)
                key.SetState(emptyState);
        }
    }

    private void OnDestroy()
    {
        Key.OnKeyPressed -= KeyPressCallback;
    }
}
