using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ScriptableObject config that defines word constraints for each gameplay type.
/// Create one asset per gameplay: WordSearch, Wordle, WordZee, WordCandy, WordTetris, WordConnect.
/// </summary>
[CreateAssetMenu(fileName = "GameplayConfig", menuName = "WordGame/Gameplay Config")]
public class GameplayConfig : ScriptableObject
{
    [Header("Gameplay Identity")]
    public string gameplayId;           // e.g. "wordsearch", "wordle"
    public string displayName;          // e.g. "Word Search"
    public string sceneName;            // e.g. "WordSearchGameScene"

    [Header("Word Constraints")]
    public int minWordLength = 3;
    public int maxWordLength = 8;
    public bool singleWordOnly = false; // Wordle = true (1 word per round)

    [Header("Level Word Counts")]
    [Tooltip("Number of words for Level 1")]
    public int wordsPerLevel1 = 3;
    [Tooltip("Number of words for Level 2")]
    public int wordsPerLevel2 = 5;
    [Tooltip("Number of words for Level 3")]
    public int wordsPerLevel3 = 8;

    [Header("Grid Settings (if applicable)")]
    public int defaultGridRows = 10;
    public int defaultGridCols = 10;

    /// <summary>
    /// Get how many words to use for a given level (1-3).
    /// </summary>
    public int GetWordCountForLevel(int level)
    {
        switch (level)
        {
            case 1: return wordsPerLevel1;
            case 2: return wordsPerLevel2;
            case 3: return wordsPerLevel3;
            default: return wordsPerLevel2;
        }
    }

    /// <summary>
    /// Check if a word fits this gameplay's constraints.
    /// </summary>
    public bool IsWordCompatible(string word)
    {
        if (string.IsNullOrEmpty(word)) return false;
        string clean = word.Trim().Replace(" ", "");
        return clean.Length >= minWordLength && clean.Length <= maxWordLength;
    }
}
