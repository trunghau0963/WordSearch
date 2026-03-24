using System.Collections.Generic;

/// <summary>
/// Data passed from level selection to the gameplay scene.
/// Stored in LevelPlayDataHolder (DontDestroyOnLoad singleton) so it
/// survives scene transitions.
/// </summary>
[System.Serializable]
public class LevelPlayData
{
    public string gameplayId;
    public string sceneName;
    public string topicName;
    public string groupName;
    public int level;

    public List<WordEntry> words = new List<WordEntry>();

    // Grid dimensions (for grid-based games)
    public int gridRows;
    public int gridCols;

    // Time limit in seconds (from GameplayConfig)
    public float timeInSeconds;

    // Effective number of levels for this group (1 when levels were skipped)
    public int totalLevelsInGroup = 3;
}

[System.Serializable]
public class WordEntry
{
    public string word;
    public string question;
}
