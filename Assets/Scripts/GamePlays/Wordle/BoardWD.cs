using System.Collections;
using System.Collections.Generic;
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

    // Level data
    private List<WordEntry> _levelWords;
    private int _currentWordIndex;
    private bool _isSubmitting;

    // Start is called before the first frame update
    private void Awake()
    {
        rows = GetComponentsInChildren<Row>();
    }

    private void Start()
    {
        // LoadData();
        // NewGame();
        Key.OnKeyPressed += KeyPressCallback;
        // Debug.Log("BoardWD Start");
        // LoadData();
        // solutions = VocabularyList.Instance.words.ToArray();
        if (VocabularyList.Instance != null)
        {
            // VocabularyList.Instance.fileName = "WordDictionary.json";
            // VocabularyList.Instance.LoadWordDictionary();
            dictionary = VocabularyList.Instance.dictionaryDB;
            solutions = dictionary.keys.ToArray();
            NewGame();
        }
        else
        {
            Debug.LogError("VocabularyList chưa được khởi tạo.");
        }
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
        SetRandomWord();
        enabled = true;
        AnimateBoardEntry();
    }

    public void TryAgain()
    {
        ClearBoard();
        enabled = true;
        AnimateBoardEntry();
    }

    private void SetRandomWord()
    {
        do
        {
            word = solutions[Random.Range(0, solutions.Length)];
        } while (word.Length != 5);
        string meaning = dictionary.GetValue(word);
        word = word.ToLower().Trim();
        if (ExplanationPrefab != null)
        {
            var exp = ExplanationPrefab.GetComponent<ExplanationWord>();
            if (exp != null) exp.SetText(word, meaning);
        }
        Debug.Log("Word: " + word);
    }

    /// <summary>
    /// Board tiles pop-in animation with staggered delay.
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
    }

    private void KeyPressCallback(string letter)
    {
        if (_isSubmitting) return;

        Row currentRow = rows[rowIndex];
        if (letter == "Delete")
        {
            if (columnIndex > 0)
            {
                columnIndex--;
                currentRow.tiles[columnIndex].SetLetter("");
                currentRow.tiles[columnIndex].SetState(emptyState);
            }
            return;
        }

        if (letter == "SUBMIT")
        {
            if (columnIndex >= currentRow.tiles.Length)
            {
                StartCoroutine(SubmitRowAnimated(currentRow));
            }
            return;
        }

        if (rowIndex < rows.Length && columnIndex < rows[rowIndex].tiles.Length
            && letter != "Enter" && letter != "Delete")
        {
            var tile = currentRow.tiles[columnIndex];
            tile.SetLetter(letter.ToString().ToUpper());
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
        }
    }

    /// <summary>
    /// Animated row submission: tiles flip one-by-one revealing color states.
    /// </summary>
    private IEnumerator SubmitRowAnimated(Row row)
    {
        _isSubmitting = true;

        // Calculate states first
        Tiles.State[] states = new Tiles.State[row.tiles.Length];

        // Pass 1: correct positions
        for (int i = 0; i < row.tiles.Length; i++)
        {
            string letter = row.tiles[i].letter.ToLower();
            if (i < word.Length && letter == word[i].ToString())
                states[i] = correctState;
        }

        // Pass 2: wrong spot or incorrect
        for (int i = 0; i < row.tiles.Length; i++)
        {
            if (states[i] == correctState) continue;
            string letter = row.tiles[i].letter.ToLower();
            if (word.Contains(letter))
                states[i] = wrongSpotState;
            else
                states[i] = incorrectState;
        }

        // Flip tiles one by one
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
        }

        yield return new WaitForSeconds(0.2f);

        bool won = HasWon(row);
        rowIndex++;
        columnIndex = 0;

        if (won)
        {
            enabled = false;

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
            bool isLastLevel = CheckIsLastLevel();
            GameEvents.ShowPopupMethod(isLastLevel);
            yield break;
        }

        if (rowIndex >= rows.Length)
        {
            enabled = false;
            _isSubmitting = false;
            yield return new WaitForSeconds(0.5f);
            GameEvents.GameOverlMethod();
            yield break;
        }

        _isSubmitting = false;
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
    }

    private void OnDisable()
    {
        if (tryAgainButton != null) tryAgainButton.interactable = true;
        if (newWordButton != null) newWordButton.interactable = true;
    }

    private void OnDestroy()
    {
        Key.OnKeyPressed -= KeyPressCallback;
    }
}
