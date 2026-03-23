using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

public static class TopicDataParser
{
    /// <summary>
    /// Parse the "Scientific words" CSV from Resources and return the full topic hierarchy.
    /// </summary>
    public static List<TopicData> ParseFromResources(string resourceName = "Scientific words")
    {
        TextAsset csv = Resources.Load<TextAsset>(resourceName);
        if (csv == null)
        {
            Debug.LogError($"TopicDataParser: could not load '{resourceName}' from Resources.");
            return new List<TopicData>();
        }
        return Parse(csv.text);
    }

    public static List<TopicData> Parse(string csvText)
    {
        var topics = new List<TopicData>();

        // Split into rows respecting quoted fields (multiline)
        var rows = SplitCsvRows(csvText);

        // Skip header row
        for (int i = 1; i < rows.Count; i++)
        {
            var fields = SplitCsvFields(rows[i]);
            if (fields.Count < 3) continue;

            string topicName = fields[0].Trim();
            string questionBlock = fields[2].Trim();

            if (string.IsNullOrEmpty(topicName) || string.IsNullOrEmpty(questionBlock))
                continue;

            TopicData topic = new TopicData { topicName = topicName };
            topic.groups = ParseGroups(questionBlock);
            topics.Add(topic);
        }

        return topics;
    }

    /// <summary>
    /// Parse the Question column into groups. Group headers are lines that do NOT
    /// start with "- " and are not blank.
    /// </summary>
    private static List<GroupQuestionData> ParseGroups(string questionBlock)
    {
        var groups = new List<GroupQuestionData>();
        var lines = questionBlock.Split('\n');

        GroupQuestionData current = null;

        foreach (var rawLine in lines)
        {
            string line = rawLine.Trim();
            if (string.IsNullOrEmpty(line)) continue;

            if (!line.StartsWith("-"))
            {
                // This is a group header
                current = new GroupQuestionData
                {
                    groupName = line.TrimEnd(':'),
                    words = new List<string>(),
                    questions = new List<string>(),
                    levelCount = 3
                };
                groups.Add(current);
            }
            else if (current != null)
            {
                // Question line — extract the word (after last colon)
                string questionText = line.TrimStart('-').Trim();
                current.questions.Add(questionText);

                string word = ExtractWord(questionText);
                if (!string.IsNullOrEmpty(word) && !current.words.Contains(word.ToLower()))
                {
                    current.words.Add(word.ToLower());
                }
            }
        }

        return groups;
    }

    /// <summary>
    /// Extract the answer word from a question line. The word is typically after the
    /// last colon or inside parentheses.
    /// </summary>
    private static string ExtractWord(string question)
    {
        // Try "answer after last colon"
        int colonIndex = question.LastIndexOf(':');
        if (colonIndex >= 0 && colonIndex < question.Length - 1)
        {
            string word = question.Substring(colonIndex + 1).Trim().TrimEnd('.', ',', '!', '?', ' ');
            // Clean up multi-word like "food producer"
            word = word.Replace("/ ", "/").Trim();
            if (!string.IsNullOrEmpty(word))
                return word;
        }

        // Try parentheses
        var match = Regex.Match(question, @"\(([^)]+)\)");
        if (match.Success)
        {
            return match.Groups[1].Value.Trim();
        }

        return null;
    }

    // ─── CSV parsing helpers ──────────────────────────────────────────────────

    /// <summary>
    /// Split the full CSV text into rows, respecting double-quoted multiline fields.
    /// </summary>
    private static List<string> SplitCsvRows(string text)
    {
        var rows = new List<string>();
        bool inQuote = false;
        int start = 0;

        for (int i = 0; i < text.Length; i++)
        {
            char c = text[i];
            if (c == '"')
            {
                inQuote = !inQuote;
            }
            else if ((c == '\n' || c == '\r') && !inQuote)
            {
                string row = text.Substring(start, i - start).Trim();
                if (!string.IsNullOrEmpty(row))
                    rows.Add(row);
                // Skip \r\n pair
                if (c == '\r' && i + 1 < text.Length && text[i + 1] == '\n')
                    i++;
                start = i + 1;
            }
        }
        // Last row
        if (start < text.Length)
        {
            string row = text.Substring(start).Trim();
            if (!string.IsNullOrEmpty(row))
                rows.Add(row);
        }

        return rows;
    }

    /// <summary>
    /// Split a single CSV row into fields, respecting double quotes.
    /// </summary>
    private static List<string> SplitCsvFields(string row)
    {
        var fields = new List<string>();
        bool inQuote = false;
        var current = new System.Text.StringBuilder();

        for (int i = 0; i < row.Length; i++)
        {
            char c = row[i];
            if (c == '"')
            {
                if (inQuote && i + 1 < row.Length && row[i + 1] == '"')
                {
                    current.Append('"');
                    i++;
                }
                else
                {
                    inQuote = !inQuote;
                }
            }
            else if (c == ',' && !inQuote)
            {
                fields.Add(current.ToString());
                current.Clear();
            }
            else
            {
                current.Append(c);
            }
        }
        fields.Add(current.ToString());

        return fields;
    }
}
