using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Generates a WordSearch board dynamically.
/// Places words in random directions (horizontal, vertical, diagonal) then fills
/// remaining cells with random letters.
/// </summary>
public static class BoardGenerator
{
    // 8 directions: (colStep, rowStep)
    // col = x axis, row = y axis in our grid
    private static readonly Vector2Int[] Directions = new Vector2Int[]
    {
        new Vector2Int(1, 0),   // right
        new Vector2Int(-1, 0),  // left
        new Vector2Int(0, 1),   // down
        new Vector2Int(0, -1),  // up
        new Vector2Int(1, 1),   // diagonal down-right
        new Vector2Int(1, -1),  // diagonal up-right
        new Vector2Int(-1, 1),  // diagonal down-left
        new Vector2Int(-1, -1), // diagonal up-left
    };

    /// <summary>
    /// Generate a complete BoardData with given words placed on a grid.
    /// </summary>
    /// <param name="words">List of uppercase words to place</param>
    /// <param name="rows">Number of rows</param>
    /// <param name="cols">Number of columns</param>
    /// <param name="maxAttempts">Max retries for the entire board if placement fails</param>
    /// <returns>A runtime BoardData ready for the game, or null if generation failed</returns>
    public static BoardData Generate(List<string> words, int rows, int cols, int maxAttempts = 50)
    {
        // Sort words by length descending — place longest first for better success rate
        var sorted = new List<string>(words);
        sorted.Sort((a, b) => b.Length.CompareTo(a.Length));

        // Auto-size grid if too small for the longest word
        int longestWord = 0;
        foreach (var w in sorted)
        {
            if (w.Length > longestWord) longestWord = w.Length;
        }
        if (rows < longestWord) rows = longestWord;
        if (cols < longestWord) cols = longestWord;

        for (int attempt = 0; attempt < maxAttempts; attempt++)
        {
            string[,] grid = new string[cols, rows]; // grid[col, row]
            InitGrid(grid, cols, rows);

            bool allPlaced = true;
            var placedWords = new List<PlacedWord>();

            foreach (var word in sorted)
            {
                var placement = TryPlaceWord(grid, word, cols, rows);
                if (placement != null)
                {
                    placedWords.Add(placement);
                }
                else
                {
                    allPlaced = false;
                    break;
                }
            }

            if (allPlaced)
            {
                FillEmptyCells(grid, cols, rows);
                return BuildBoardData(grid, placedWords, rows, cols);
            }
        }

        Debug.LogWarning("[BoardGenerator] Failed to place all words after max attempts. " +
                         "Trying with expanded grid...");

        // Fallback: expand grid and try again
        return Generate(words, rows + 2, cols + 2, maxAttempts);
    }

    private static void InitGrid(string[,] grid, int cols, int rows)
    {
        for (int c = 0; c < cols; c++)
            for (int r = 0; r < rows; r++)
                grid[c, r] = "";
    }

    /// <summary>
    /// Try to place a word on the grid. Returns placement info or null if failed.
    /// </summary>
    private static PlacedWord TryPlaceWord(string[,] grid, string word, int cols, int rows)
    {
        // Shuffle directions for randomness
        var dirs = ShuffleArray(Directions);

        // Try many random starting positions per direction
        int maxPositionAttempts = cols * rows * 2;

        foreach (var dir in dirs)
        {
            for (int posAttempt = 0; posAttempt < maxPositionAttempts; posAttempt++)
            {
                int startCol = Random.Range(0, cols);
                int startRow = Random.Range(0, rows);

                if (CanPlace(grid, word, startCol, startRow, dir, cols, rows))
                {
                    PlaceWord(grid, word, startCol, startRow, dir);
                    return new PlacedWord
                    {
                        word = word,
                        startCol = startCol,
                        startRow = startRow,
                        direction = dir
                    };
                }
            }
        }

        return null;
    }

    private static bool CanPlace(string[,] grid, string word, int startCol, int startRow,
        Vector2Int dir, int cols, int rows)
    {
        for (int i = 0; i < word.Length; i++)
        {
            int c = startCol + dir.x * i;
            int r = startRow + dir.y * i;

            // Out of bounds
            if (c < 0 || c >= cols || r < 0 || r >= rows)
                return false;

            // Cell occupied by a different letter
            string existing = grid[c, r];
            if (existing != "" && existing != word[i].ToString())
                return false;
        }
        return true;
    }

    private static void PlaceWord(string[,] grid, string word, int startCol, int startRow, Vector2Int dir)
    {
        for (int i = 0; i < word.Length; i++)
        {
            int c = startCol + dir.x * i;
            int r = startRow + dir.y * i;
            grid[c, r] = word[i].ToString();
        }
    }

    private static void FillEmptyCells(string[,] grid, int cols, int rows)
    {
        const string letters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        for (int c = 0; c < cols; c++)
        {
            for (int r = 0; r < rows; r++)
            {
                if (grid[c, r] == "")
                {
                    grid[c, r] = letters[Random.Range(0, letters.Length)].ToString();
                }
            }
        }
    }

    /// <summary>
    /// Build a BoardData ScriptableObject instance from the generated grid.
    /// </summary>
    private static BoardData BuildBoardData(string[,] grid, List<PlacedWord> placedWords, int rows, int cols)
    {
        var boardData = ScriptableObject.CreateInstance<BoardData>();
        boardData.Name = "Generated";
        boardData.isCompleted = false;
        boardData.timeInSeconds = 120f;
        boardData.Columns = cols;
        boardData.Rows = rows;

        // BoardData.Boards is BoardRow[columns], each BoardRow has string[rows]
        boardData.Boards = new BoardData.BoardRow[cols];
        for (int c = 0; c < cols; c++)
        {
            boardData.Boards[c] = new BoardData.BoardRow(rows);
            for (int r = 0; r < rows; r++)
            {
                boardData.Boards[c].Row[r] = grid[c, r];
            }
        }

        // SearchWords
        boardData.SearchWords = new List<BoardData.SearchingWord>();
        foreach (var pw in placedWords)
        {
            boardData.SearchWords.Add(new BoardData.SearchingWord
            {
                Word = pw.word,
                Column = pw.startCol,
                Row = pw.startRow,
                Direction = DirectionToInt(pw.direction)
            });
        }

        return boardData;
    }

    private static int DirectionToInt(Vector2Int dir)
    {
        for (int i = 0; i < Directions.Length; i++)
        {
            if (Directions[i] == dir) return i;
        }
        return 0;
    }

    private static T[] ShuffleArray<T>(T[] array)
    {
        var shuffled = new T[array.Length];
        System.Array.Copy(array, shuffled, array.Length);
        for (int i = shuffled.Length - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (shuffled[i], shuffled[j]) = (shuffled[j], shuffled[i]);
        }
        return shuffled;
    }

    private class PlacedWord
    {
        public string word;
        public int startCol;
        public int startRow;
        public Vector2Int direction;
    }
}
