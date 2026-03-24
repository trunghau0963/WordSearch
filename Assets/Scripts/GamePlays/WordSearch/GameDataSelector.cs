using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// dung de chon boardata phu hop voi category, section, level
/// </summary>
public class GameDataSelector : MonoBehaviour
{
    public GameData currentGameData;

    [Header("Dynamic Board Defaults")]
    [Tooltip("Default rows if not specified in LevelPlayData")]
    public int defaultRows = 8;
    [Tooltip("Default columns if not specified in LevelPlayData")]
    public int defaultCols = 8;
    [Tooltip("Time in seconds for the generated board")]
    public float defaultTimeInSeconds = 120f;

    void Awake()
    {
        // If dynamic data from LevelPlayDataHolder exists, generate board from it
        var holder = LevelPlayDataHolder.Instance;
        if (holder != null && holder.CurrentData != null)
        {
            Debug.Log("[GameDataSelector] Dynamic data detected, generating board...");
            GenerateDynamicBoard(holder.CurrentData);
            return;
        }

        // Legacy flow: select board from ScriptableObject hierarchy
        if (currentGameData.selectedLevel == null)
        {
            Debug.LogWarning("[GameDataSelector] No selectedLevel set. Skipping.");
            return;
        }

        SelectSequentalBoardData();
    }

    private void GenerateDynamicBoard(LevelPlayData playData)
    {
        var wordStrings = new List<string>();
        foreach (var entry in playData.words)
        {
            string clean = entry.word.Trim().ToUpper();
            if (!string.IsNullOrEmpty(clean))
                wordStrings.Add(clean);
        }

        if (wordStrings.Count == 0)
        {
            Debug.LogWarning("[GameDataSelector] No words in LevelPlayData.");
            return;
        }

        int rows = playData.gridRows > 0 ? playData.gridRows : defaultRows;
        int cols = playData.gridCols > 0 ? playData.gridCols : defaultCols;

        BoardData generated = BoardGenerator.Generate(wordStrings, rows, cols);

        if (generated == null)
        {
            Debug.LogError("[GameDataSelector] Board generation failed!");
            return;
        }

        generated.timeInSeconds = playData.timeInSeconds > 0f ? playData.timeInSeconds : defaultTimeInSeconds;
        GameSessionData.CurrentBoard = generated;

        // Also set legacy GameData if assigned (for backward compatibility)
        if (currentGameData != null)
            currentGameData.selectedBoardData = generated;

        Debug.Log($"[GameDataSelector] Generated {cols}x{rows} board with " +
                  $"{wordStrings.Count} words: {string.Join(", ", wordStrings)}");
    }

    private void SelectSequentalBoardData()
    {

        Level_PlayerPrefs level = currentGameData.selectedLevel;
        int totalBoardCount = level.boardList.Count;

        for (int i = 0; i < level.boardList.Count; i++)
        {
            BoardData board = level.boardList[i];
            if (!currentGameData.selectedLevel.GetIsCompleted())
            {
                // Skip the board if it is completed
                if (board.isCompleted)
                {
                    continue;
                }
                // if (!board.isLock)
                // {
                if (i < totalBoardCount)
                {
                    currentGameData.selectedBoardData = board;
                    GameSessionData.CurrentBoard = board;
                    return;
                }
                // }
            }
            else
            {
                // if the level is completed, select the next board
                if (i < totalBoardCount)
                {
                    currentGameData.selectedBoardData = board;
                    GameSessionData.CurrentBoard = board;
                    return;
                }
            }
        }
        return;
    }
}

