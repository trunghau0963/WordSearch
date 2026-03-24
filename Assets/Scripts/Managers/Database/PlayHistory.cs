using UnityEngine;
using System.Collections.Generic;
using System.IO;

/// <summary>
/// Singleton (DontDestroyOnLoad) that tracks which levels the player has completed.
/// Key format: "gameplayId|topicName|groupName|level"
/// Saves to persistentDataPath as JSON.
/// </summary>
public class PlayHistory : MonoBehaviour
{
    public static PlayHistory Instance { get; private set; }

    private const string FileName = "play_history.json";

    private HashSet<string> _completedLevels = new HashSet<string>();
    private HashSet<string> _completedGroups = new HashSet<string>();
    private HashSet<string> _completedTopics = new HashSet<string>();

    [System.Serializable]
    private class SaveData
    {
        public List<string> completedLevels = new List<string>();
        public List<string> completedGroups = new List<string>();
        public List<string> completedTopics = new List<string>();
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        Load();
    }

    // ─── Keys ──────────────────────────────────────────────────────────────

    public static string LevelKey(string gameplayId, string topic, string group, int level)
    {
        return $"{gameplayId}|{topic}|{group}|{level}";
    }

    public static string GroupKey(string gameplayId, string topic, string group)
    {
        return $"{gameplayId}|{topic}|{group}";
    }

    public static string TopicKey(string gameplayId, string topic)
    {
        return $"{gameplayId}|{topic}";
    }

    // ─── Queries ───────────────────────────────────────────────────────────

    public bool IsLevelCompleted(string gameplayId, string topic, string group, int level)
    {
        return _completedLevels.Contains(LevelKey(gameplayId, topic, group, level));
    }

    public bool IsGroupCompleted(string gameplayId, string topic, string group)
    {
        return _completedGroups.Contains(GroupKey(gameplayId, topic, group));
    }

    public bool IsTopicCompleted(string gameplayId, string topic)
    {
        return _completedTopics.Contains(TopicKey(gameplayId, topic));
    }

    // ─── Mark Complete ─────────────────────────────────────────────────────

    /// <summary>
    /// Mark a level as completed. Automatically checks if the group/topic are fully done.
    /// </summary>
    public void CompleteLevel(string gameplayId, string topic, string group, int level,
                              int totalLevelsInGroup, List<TopicData> allTopics = null)
    {
        _completedLevels.Add(LevelKey(gameplayId, topic, group, level));

        // Check if all levels in this group are done
        bool allLevelsDone = true;
        for (int i = 1; i <= totalLevelsInGroup; i++)
        {
            if (!_completedLevels.Contains(LevelKey(gameplayId, topic, group, i)))
            {
                allLevelsDone = false;
                break;
            }
        }
        if (allLevelsDone)
        {
            _completedGroups.Add(GroupKey(gameplayId, topic, group));
        }

        // Check if all groups in this topic are done (if topic data provided)
        if (allTopics != null)
        {
            TopicData topicData = null;
            foreach (var t in allTopics)
            {
                if (t.topicName == topic) { topicData = t; break; }
            }
            if (topicData != null)
            {
                bool allGroupsDone = true;
                foreach (var g in topicData.groups)
                {
                    if (!_completedGroups.Contains(GroupKey(gameplayId, topic, g.groupName)))
                    {
                        allGroupsDone = false;
                        break;
                    }
                }
                if (allGroupsDone)
                {
                    _completedTopics.Add(TopicKey(gameplayId, topic));
                }
            }
        }

        Save();
    }

    // ─── Persistence ───────────────────────────────────────────────────────

    private string FilePath => Path.Combine(Application.persistentDataPath, FileName);

    private void Save()
    {
        var data = new SaveData
        {
            completedLevels = new List<string>(_completedLevels),
            completedGroups = new List<string>(_completedGroups),
            completedTopics = new List<string>(_completedTopics)
        };
        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(FilePath, json);
    }

    private void Load()
    {
        if (!File.Exists(FilePath)) return;

        string json = File.ReadAllText(FilePath);
        var data = JsonUtility.FromJson<SaveData>(json);
        if (data == null) return;

        _completedLevels = new HashSet<string>(data.completedLevels);
        _completedGroups = new HashSet<string>(data.completedGroups);
        _completedTopics = new HashSet<string>(data.completedTopics);
    }
}
