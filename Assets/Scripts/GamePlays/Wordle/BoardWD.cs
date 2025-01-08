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
        Debug.Log("NewGame");
        ClearBoard();
        SetRandomWord();

        enabled = true;
    }

    public void TryAgain()
    {
        Debug.Log("TryAgain");
        ClearBoard();
        enabled = true;
    }

    private void SetRandomWord()
    {
        do
        {
            word = solutions[Random.Range(0, solutions.Length)];
        } while (word.Length != 5);
        string meaning = dictionary.GetValue(word);
        word = word.ToLower().Trim();
        ExplanationPrefab.GetComponent<ExplanationWord>().SetText(word, meaning);
        Debug.Log("Word: " + word);
    }

    private void KeyPressCallback(string letter)
    {
        Debug.Log("KeyPressCallback: " + letter);
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
                // if (rowIndex < rows.Length)
                // {
                //     rowIndex++;
                //     columnIndex = 0;
                // }
                // if (letter == "SUBMIT")
                // {
                // }
                SubmitRow(currentRow);
                return;
            }
            else
            {
                return;
            }

        }
        if (rowIndex < rows.Length && columnIndex < rows[rowIndex].tiles.Length && letter != "Enter" && letter != "Delete")
        {
            currentRow.tiles[columnIndex].SetLetter(letter.ToString().ToUpper());
            currentRow.tiles[columnIndex].SetState(occupiedState);
            columnIndex++;
            // Debug.Log("Letter set: " + letter);
        }
    }

    private void SubmitRow(Row row)
    {
        string remaining = word;

        Debug.Log("Word: " + word);

        // Check correct/incorrect letters first
        for (int i = 0; i < row.tiles.Length; i++)
        {
            Tiles tile = row.tiles[i];
            string letter = tile.letter.ToLower();
            if (letter == word[i].ToString())
            {
                tile.SetState(correctState);
            }
            else if (!word.Contains(letter))
            {
                tile.SetState(incorrectState);
            }
        }

        for (int i = 0; i < row.tiles.Length; i++)
        {
            Tiles tile = row.tiles[i];
            if (tile.state != correctState && tile.state != incorrectState)
            {
                string letter = tile.letter.ToLower();
                if (remaining.Contains(letter))
                {
                    tile.SetState(wrongSpotState);
                    // int index = remaining.IndexOf(letter);
                    // remaining = remaining.Remove(index, 1);
                    // remaining = remaining.Insert(index, " ");
                }
                else
                {
                    tile.SetState(incorrectState);
                }
            }
        }

        if (HasWon(row))
        {
            enabled = false;
        }

        rowIndex++;
        columnIndex = 0;

        if (rowIndex >= rows.Length)
        {
            enabled = false;
        }
    }

    private bool IsValidWord(string word)
    {
        for (int i = 0; i < validWords.Length; i++)
        {
            if (string.Equals(word, validWords[i], System.StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
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
        tryAgainButton.interactable = false;
        newWordButton.interactable = false;
    }

    private void OnDisable()
    {
        tryAgainButton.interactable = true;
        newWordButton.interactable = true;
    }


}
