using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum GameState
{
    wait,
    move
}

public enum TileKind
{
    Breakable,
    Blank,
    Normal
}

[System.Serializable]
public class TileType
{
    public int x, y;
    public TileKind tileKind;
}



public class Board : MonoBehaviour
{
    private bool[,] blankSpaces;
    private FindingMatches findMatches;
    private BackgroundTile[,] breakableTiles;
    private ScoreManager scoreManager;
    private SoundManager soundManager;
    private string[] solutions;
    private string[] validWords;
    public List<string> wordList = new();
    private int streakValue = 1;
    public int width;
    public int height;
    public int offSet;
    public int basePieceValue = 100;
    public float refillDelay = .5f;
    public GameState currentState = GameState.move;
    public GameObject tilePrefab;
    public GameObject destroyEffect;
    public GameObject[] dots;
    public GameObject[,] allDots;
    public GameObject breakableTilePrefab;
    // public BackgroundTile[,] allTiles;
    public TileType[] boardLayout;
    public Dot currentDot;
    public int[] scoreGoals;


    void Start()
    {
        scoreManager = FindFirstObjectByType<ScoreManager>();
        soundManager = FindFirstObjectByType<SoundManager>();
        breakableTiles = new BackgroundTile[width, height];
        findMatches = FindAnyObjectByType<FindingMatches>();
        blankSpaces = new bool[width, height];
        allDots = new GameObject[width, height];
        CreateBoard();
        LoadData();
        SetRandomWord();
    }

    private void LoadData()
    {
        TextAsset textFile = Resources.Load("official_wordle_common") as TextAsset;
        solutions = textFile.text.Split('\n');

        textFile = Resources.Load("official_wordle_all") as TextAsset;
        validWords = textFile.text.Split('\n');
    }

    private void SetRandomWord()
    {
        int numberOfWords = Random.Range(1, 5);
        print("Number of words: " + numberOfWords);
        for (int i = 0; i < numberOfWords; i++)
        {
            string word = solutions[Random.Range(0, solutions.Length)];
            word = word.ToLower().Trim();
            wordList.Add(word);
        }

    }

    public List<string> GetWordList()
    {
        return wordList;
    }

    public void GenerateBlankSpace()
    {
        for (int i = 0; i < boardLayout.Length; i++)
        {
            if (boardLayout[i].tileKind == TileKind.Blank)
            {
                print("GenerateBlankSpace " + i + " " + boardLayout[i].x + " " + boardLayout[i].y);
                blankSpaces[boardLayout[i].x, boardLayout[i].y] = true;
            }
        }
    }

    public void GenerateBreakableTiles()
    {
        for (int i = 0; i < boardLayout.Length; i++)
        {
            if (boardLayout[i].tileKind == TileKind.Breakable)
            {
                Vector2 tempPosition = new(boardLayout[i].x, boardLayout[i].y + offSet);
                GameObject tile = Instantiate(breakableTilePrefab, tempPosition, Quaternion.identity);
                breakableTiles[boardLayout[i].x, boardLayout[i].y] = tile.GetComponent<BackgroundTile>();
            }
        }
    }

    private void CreateBoard()
    {
        GenerateBlankSpace();
        GenerateBreakableTiles();
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (!blankSpaces[x, y])
                {

                    // Điều chỉnh vị trí của tile dựa trên kích thước của nó
                    Vector2 tempPosition = new(x, y + offSet);
                    Vector2 tilePosition = new(x, y);
                    GameObject newTile = Instantiate(tilePrefab, tilePosition, Quaternion.identity);

                    // Đặt parent cho tile để giữ cho scene gọn gàng
                    newTile.transform.parent = this.transform;
                    newTile.name = "Tile BG (" + x + "," + y + ")";
                    // allTiles[x, y] = newTile.GetComponent<BackgroundTile>();
                    int dotToUse = Random.Range(0, dots.Length);
                    int maxIterations = 0;
                    // Đảm bảo không có matches khi khởi tạo
                    while (MatchesAt(x, y, dots[dotToUse]) && maxIterations < 100)
                    {
                        dotToUse = Random.Range(0, dots.Length);
                        maxIterations++;
                        Debug.Log(maxIterations);
                    }
                    maxIterations = 0;
                    GameObject dot = Instantiate(dots[dotToUse], tempPosition, Quaternion.identity);
                    dot.GetComponent<Dot>().row = y;
                    dot.GetComponent<Dot>().column = x;
                    dot.transform.parent = this.transform;
                    dot.name = "Tile Dot (" + x + "," + y + ")";
                    allDots[x, y] = dot;
                }

            }
        }
    }

    private bool MatchesOnBoard()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (allDots[x, y] != null)
                {
                    if (allDots[x, y].GetComponent<Dot>().isMatched)
                    {
                        return true;
                    }
                }
            }
        }
        return false;
    }

    private void RefillBoard()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (allDots[x, y] == null && !blankSpaces[x, y]) // if there is a blank space
                {
                    Vector2 tempPosition = new(x, y + offSet);
                    int dotToUse = Random.Range(0, dots.Length);
                    int maxIterations = 0;
                    while (MatchesAt(x, y, dots[dotToUse]) && maxIterations < 100)
                    {
                        dotToUse = Random.Range(0, dots.Length);
                        maxIterations++;
                    }
                    maxIterations = 0;
                    GameObject piece = Instantiate(dots[dotToUse], tempPosition, Quaternion.identity);
                    allDots[x, y] = piece;
                    piece.GetComponent<Dot>().row = y;
                    piece.GetComponent<Dot>().column = x;
                }
            }
        }
    }

    public IEnumerator FillBoardCo()
    {
        RefillBoard();
        yield return new WaitForSeconds(refillDelay);
        while (MatchesOnBoard())
        {
            streakValue++;
            DestroyMatches();
            yield return new WaitForSeconds(2 * refillDelay);
        }
        findMatches.currentMatches.Clear();
        currentDot = null;
        if (IsDeadLock())
        {
            ShuffleBoard();
            print("Deadlock");
        }
        yield return new WaitForSeconds(refillDelay);
        currentState = GameState.move;
        streakValue = 1;
    }

    private IEnumerable ShuffleBoard()
    {
        yield return new WaitForSeconds(.5f);
        List<GameObject> newBoard = new();
        // Add all the pieces to the list
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (allDots[x, y] != null)
                {
                    newBoard.Add(allDots[x, y]);
                }
            }
        }
        // for each spot on the board
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                // if this spot shouldn't be blank
                if (!blankSpaces[x, y] && !breakableTiles[x, y])
                {
                    // pick a random number
                    int pieceToUse = Random.Range(0, newBoard.Count);
                    // make a container for the piece
                    int maxIterations = 0;
                    while (MatchesAt(x, y, newBoard[pieceToUse]) && maxIterations < 100)
                    {
                        pieceToUse = Random.Range(0, newBoard.Count);
                        maxIterations++;
                    }
                    // make a container for the piece
                    Dot piece = newBoard[pieceToUse].GetComponent<Dot>();
                    maxIterations = 0;
                    piece.column = x;
                    piece.row = y;
                    // fill in the dots array with this new piece
                    allDots[x, y] = newBoard[pieceToUse];
                    // remove it from the list
                    newBoard.Remove(newBoard[pieceToUse]);
                }
            }
        }
        // check if it's still deadlocked
        if (IsDeadLock())
        {
            ShuffleBoard();
        }
    }

    public IEnumerator DecreaseRowCo()
    {
        int nullCount = 0;
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (allDots[x, y] == null)
                {
                    nullCount++;
                }
                else if (nullCount > 0)
                {
                    allDots[x, y].GetComponent<Dot>().row -= nullCount;
                    allDots[x, y] = null;
                }
            }
            nullCount = 0;
        }
        yield return new WaitForSeconds(.4f);
        StartCoroutine(FillBoardCo());
    }

    public IEnumerator DecreaseRowCo2()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (!blankSpaces[x, y] && allDots[x, y] == null)
                { // if the space is blank and there is a null
                    for (int k = y + 1; k < height; k++)
                    { // start from the next row
                        if (allDots[x, k] != null)
                        { // if there is a dot in this row
                            allDots[x, k].GetComponent<Dot>().row = y; // move that dot to the null space
                            allDots[x, k] = null; // set the space to null
                            break;
                        }
                    }
                }

            }
        }
        yield return new WaitForSeconds(refillDelay * 0.5f);
        StartCoroutine(FillBoardCo());
    }

    private bool MatchesAt(int column, int row, GameObject piece)
    {
        if (column > 1 && row > 1)
        {

            if (allDots[column - 1, row] != null && allDots[column - 2, row] != null)
            {
                if (piece.CompareTag(allDots[column - 1, row].tag) && piece.CompareTag(allDots[column - 2, row].tag))
                {
                    return true;
                }
            }
            if (allDots[column, row - 1] != null && allDots[column, row - 2] != null)
            {
                if (piece.CompareTag(allDots[column, row - 1].tag) && piece.CompareTag(allDots[column, row - 2].tag))
                {
                    return true;
                }
            }
        }
        else if (column <= 1 || row <= 1)
        {
            if (row > 1)
            {
                if (allDots[column, row - 1] != null && allDots[column, row - 2] != null)
                {
                    if (piece.CompareTag(allDots[column, row - 1].tag) && piece.CompareTag(allDots[column, row - 2].tag))
                    {
                        return true;
                    }
                }
            }
            if (column > 1)
            {
                if (allDots[column - 1, row] != null && allDots[column - 2, row] != null)
                {
                    if (piece.CompareTag(allDots[column - 1, row].tag) && piece.CompareTag(allDots[column - 2, row].tag))
                    {
                        return true;
                    }
                }
            }
        }
        return false;
    }
    private bool ColumnOrRow()
    {
        int numberHorizontal = 0;
        int numberVertical = 0;
        if (findMatches.currentMatches[0].TryGetComponent<Dot>(out var firstPiece))
        {
            foreach (GameObject currentPiece in findMatches.currentMatches)
            {
                Dot dot = currentPiece.GetComponent<Dot>();
                if (dot.row == firstPiece.row)
                {
                    numberHorizontal++;
                }
                if (dot.column == firstPiece.column)
                {
                    numberVertical++;
                }
            }
        }
        return numberVertical == 5 || numberHorizontal == 5;
    }

    private void CheckToMakeBombs()
    {
        print("dklasdnldlkadk : " + findMatches.currentMatches.Count);
        if (findMatches.currentMatches.Count == 4 || findMatches.currentMatches.Count == 7)
        {
            findMatches.CheckBombs();
        }
        if (findMatches.currentMatches.Count == 5 || findMatches.currentMatches.Count == 8)
        {
            print("Make a bomb");
            if (ColumnOrRow())
            {
                // Make a color bomb
                if (currentDot != null)
                {
                    if (currentDot.isMatched)
                    {
                        if (!currentDot.isColorBomb)
                        {
                            currentDot.isMatched = false;
                            currentDot.MakeColorBomb();
                        }
                    }
                    else
                    {
                        if (currentDot.otherDot != null)
                        {
                            Dot otherDot = currentDot.otherDot.GetComponent<Dot>();
                            if (otherDot.isMatched)
                            {
                                if (!otherDot.isColorBomb)
                                {
                                    otherDot.isMatched = false;
                                    otherDot.MakeColorBomb();
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                // Make a adjacent bomb
                if (currentDot != null)
                {
                    if (currentDot.isMatched)
                    {
                        if (!currentDot.isAdjacentBomb)
                        {
                            currentDot.isMatched = false;
                            currentDot.MakeAdjacentBomb();
                        }
                    }
                    else
                    {
                        if (currentDot.otherDot != null)
                        {
                            Dot otherDot = currentDot.otherDot.GetComponent<Dot>();
                            if (otherDot.isMatched)
                            {
                                if (!otherDot.isAdjacentBomb)
                                {
                                    otherDot.isMatched = false;
                                    otherDot.MakeAdjacentBomb();
                                }
                            }
                        }
                    }
                }
            }
        }
    }

    private void DestroyMatchesAt(int column, int row)
    {
        if (allDots[column, row].GetComponent<Dot>().isMatched)
        {
            // how many elements are in the matched pieces list from findmatches?
            if (findMatches.currentMatches.Count >= 4)
            {
                CheckToMakeBombs();
            }

            // does a tile need to break?
            if (breakableTiles[column, row] != null)
            {
                print("Take damge Breakable tile");
                breakableTiles[column, row].TakeDamage(1);
                if (breakableTiles[column, row].hitPoints <= 0)
                {
                    breakableTiles[column, row] = null;
                }
            }
            GameObject particle = Instantiate(destroyEffect, allDots[column, row].transform.position, Quaternion.identity);
            Destroy(particle, .5f);
            Destroy(allDots[column, row]);
            scoreManager.IncreaseScore(basePieceValue * streakValue);
            allDots[column, row] = null;
        }
    }

    public void DestroyMatches()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (allDots[x, y] != null)
                {
                    DestroyMatchesAt(x, y);
                }
            }
        }
        findMatches.currentMatches.Clear();
        StartCoroutine(DecreaseRowCo2());
    }


    private void SwitchPieces(int column, int row, Vector2 direction)
    {
        // Take the second piece and save it in a holder
        GameObject holder = allDots[column + (int)direction.x, row + (int)direction.y] as GameObject;
        // Switching the first dot to be the second position
        allDots[column + (int)direction.x, row + (int)direction.y] = allDots[column, row];
        // Set the first dot to be the second dot
        allDots[column, row] = holder;
    }
    private bool CheckForMatches()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (allDots[x, y] != null)
                {
                    // Make sure that one and two to the right are in the board
                    if (x < width - 2)
                    {
                        // Check if the dots to the right and two to the right exist
                        if (allDots[x + 1, y] != null && allDots[x + 2, y] != null)
                        {
                            if (allDots[x, y].CompareTag(allDots[x + 1, y].tag) && allDots[x, y].CompareTag(allDots[x + 2, y].tag))
                            {
                                return true;
                            }
                        }
                    }
                    if (y < height - 2)
                    {
                        // Check if the dots above exist
                        if (allDots[x, y + 1] != null && allDots[x, y + 2] != null)
                        {
                            if (allDots[x, y].CompareTag(allDots[x, y + 1].tag) && allDots[x, y].CompareTag(allDots[x, y + 2].tag))
                            {
                                return true;
                            }
                        }
                    }
                }
            }
        }
        return false;
    }

    public bool SwitchAndCheck(int column, int row, Vector2 direction)
    {
        SwitchPieces(column, row, direction);
        if (CheckForMatches())
        {
            SwitchPieces(column, row, direction);
            return true;
        }
        SwitchPieces(column, row, direction);
        return false;
    }
    private bool IsDeadLock()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (allDots[x, y] != null)
                {
                    if (x < width - 1)
                    {
                        if (SwitchAndCheck(x, y, Vector2.right))
                        {
                            return false;
                        }
                    }
                    if (y < height - 1)
                    {
                        if (SwitchAndCheck(x, y, Vector2.up))
                        {
                            return false;
                        }
                    }
                }
            }
        }
        return true;
    }
}
