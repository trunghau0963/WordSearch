using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Generates a set of words + questions for a specific gameplay + topic + group + level combination.
/// Filters words based on gameplay constraints (max length, word count per level).
/// </summary>
public static class LevelWordGenerator
{
    /// <summary>
    /// Generate data for a level. Returns a LevelPlayData with filtered words + questions.
    /// </summary>
    public static LevelPlayData Generate(
        GameplayConfig config,
        TopicData topic,
        GroupQuestionData group,
        int level)
    {
        var data = new LevelPlayData
        {
            gameplayId = config.gameplayId,
            sceneName = config.sceneName,
            topicName = topic.topicName,
            groupName = group.groupName,
            level = level,
            gridRows = config.defaultGridRows,
            gridCols = config.defaultGridCols,
            timeInSeconds = config.GetTimeForLevel(level)
        };

        int targetWordCount = config.GetWordCountForLevel(level);

        // Build candidate list: words that fit this gameplay's constraints
        var candidates = new List<WordEntry>();
        for (int i = 0; i < group.words.Count; i++)
        {
            string word = group.words[i];
            string question = (i < group.questions.Count) ? group.questions[i] : "";

            if (config.IsWordCompatible(word))
            {
                candidates.Add(new WordEntry
                {
                    word = word.Trim().ToUpper(),
                    question = question
                });
            }
        }

        if (candidates.Count == 0)
        {
            Debug.LogWarning($"[LevelWordGenerator] No compatible words for {config.gameplayId} " +
                             $"in '{group.groupName}' (min={config.minWordLength}, max={config.maxWordLength})");
            data.words = new List<WordEntry>();
            return data;
        }

        // Sort by word length for level difficulty progression:
        //   Level 1: shortest words first (easiest)
        //   Level 2: medium words
        //   Level 3: longest words first (hardest)
        List<WordEntry> sorted;
        switch (level)
        {
            case 1:
                sorted = candidates.OrderBy(w => w.word.Length).ToList();
                break;
            case 3:
                sorted = candidates.OrderByDescending(w => w.word.Length).ToList();
                break;
            default: // level 2 — mix
                sorted = candidates.OrderBy(w => w.word.Length).ToList();
                // Take from middle outward
                var mid = new List<WordEntry>();
                int center = sorted.Count / 2;
                int radius = 0;
                while (mid.Count < sorted.Count)
                {
                    int lo = center - radius;
                    int hi = center + radius;
                    if (lo >= 0 && lo < sorted.Count && !mid.Contains(sorted[lo]))
                        mid.Add(sorted[lo]);
                    if (hi >= 0 && hi < sorted.Count && !mid.Contains(sorted[hi]))
                        mid.Add(sorted[hi]);
                    radius++;
                }
                sorted = mid;
                break;
        }

        // Single-word gameplay (e.g. Wordle): pick one word appropriate for level
        if (config.singleWordOnly)
        {
            targetWordCount = 1;
        }

        // Take up to targetWordCount
        data.words = sorted.Take(Mathf.Min(targetWordCount, sorted.Count)).ToList();

        return data;
    }
}
