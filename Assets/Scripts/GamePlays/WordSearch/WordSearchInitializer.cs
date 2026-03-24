using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Placed on the WordSearch scene root. On Awake (before other Start() methods),
/// reads LevelPlayDataHolder and generates a BoardData dynamically,
/// then populates GameData.selectedBoardData so all existing scripts
/// (WordsGrid, WordChecker, SearchingWordList) work unchanged.
/// </summary>
[DefaultExecutionOrder(-100)]
public class WordSearchInitializer : MonoBehaviour
{
    [Tooltip("The GameData ScriptableObject used by all WordSearch scripts")]
    public GameData currentGameData;

    [Header("Grid Defaults")]
    [Tooltip("Default rows if not specified in LevelPlayData")]
    public int defaultRows = 8;
    [Tooltip("Default columns if not specified in LevelPlayData")]
    public int defaultCols = 8;
    [Tooltip("Time in seconds for the generated board")]
    public float defaultTimeInSeconds = 120f;

    void Awake()
    {
        var holder = LevelPlayDataHolder.Instance;
        if (holder == null || holder.CurrentData == null)
        {
            Debug.LogWarning("[WordSearchInitializer] No LevelPlayData found. " +
                             "Keeping existing BoardData (editor/debug mode).");
            return;
        }

        var playData = holder.CurrentData;

        // Extract word strings
        var wordStrings = new List<string>();
        foreach (var entry in playData.words)
        {
            string clean = entry.word.Trim().ToUpper();
            if (!string.IsNullOrEmpty(clean))
                wordStrings.Add(clean);
        }

        if (wordStrings.Count == 0)
        {
            Debug.LogWarning("[WordSearchInitializer] No words in LevelPlayData.");
            return;
        }

        int rows = playData.gridRows > 0 ? playData.gridRows : defaultRows;
        int cols = playData.gridCols > 0 ? playData.gridCols : defaultCols;

        // Generate the board
        BoardData generated = BoardGenerator.Generate(wordStrings, rows, cols);

        if (generated == null)
        {
            Debug.LogError("[WordSearchInitializer] Board generation failed!");
            return;
        }

        generated.timeInSeconds = defaultTimeInSeconds;

        // Populate GameData so all existing scripts read from it
        currentGameData.selectedBoardData = generated;

        Debug.Log($"[WordSearchInitializer] Generated {cols}x{rows} board with " +
                  $"{wordStrings.Count} words: {string.Join(", ", wordStrings)}");
    }
}
