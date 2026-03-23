using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

/// <summary>
/// A 3-tier level selection menu: Topic → Group Question → Level (1-3).
/// Shared by all gameplay types. When a level is chosen, generates a word set
/// filtered by gameplay constraints, stores it in LevelPlayDataHolder, and loads the scene.
/// </summary>
public class LevelSelectMenu : Panel
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI titleText = null;
    [SerializeField] private Button backButton = null;
    [SerializeField] private Transform contentParent = null;
    [SerializeField] private GameObject itemPrefab = null;

    [Header("Gameplay Configs")]
    [SerializeField] private List<GameplayConfig> gameplayConfigs = new List<GameplayConfig>();

    [Header("Colors")]
    [SerializeField] private Color topicColor = new Color(0.2f, 0.6f, 0.2f, 1f);
    [SerializeField] private Color groupColor = new Color(0.18f, 0.5f, 0.72f, 1f);
    [SerializeField] private Color levelColor = new Color(0.85f, 0.55f, 0.13f, 1f);
    [SerializeField] private Color disabledColor = new Color(0.45f, 0.45f, 0.45f, 1f);

    [Header("Scene Transition")]
    [SerializeField] private string transitionName = "CrossWipe";
    [SerializeField] private bool useLevelManager = true;

    private List<TopicData> _topics;
    private GameplayConfig _currentConfig;

    // Navigation state
    private enum Tier { Topic, Group, Level }
    private Tier _currentTier = Tier.Topic;
    private TopicData _selectedTopic;
    private GroupQuestionData _selectedGroup;

    // Callback when a level is finally selected (before scene load)
    public event Action<LevelPlayData> OnLevelSelected;

    public override void Initialize()
    {
        if (IsInitialized) return;

        backButton.onClick.AddListener(GoBack);
        _topics = TopicDataParser.ParseFromResources();

        base.Initialize();
    }

    /// <summary>
    /// Open the level select menu for a specific gameplay type.
    /// </summary>
    public void Open(string gameplayType)
    {
        _currentConfig = FindConfig(gameplayType);
        _currentTier = Tier.Topic;
        _selectedTopic = null;
        _selectedGroup = null;

        if (_currentConfig == null)
        {
            Debug.LogError($"[LevelSelectMenu] No GameplayConfig found for '{gameplayType}'");
            return;
        }

        base.Open();
        ShowTopics();
    }

    // ─── Tier displays ────────────────────────────────────────────────────────

    private void ShowTopics()
    {
        _currentTier = Tier.Topic;
        titleText.text = _currentConfig.displayName + " - Select Topic";
        ClearContent();

        foreach (var topic in _topics)
        {
            CreateItem(topic.topicName, topicColor, () => OnTopicClicked(topic));
        }
    }

    private void ShowGroups(TopicData topic)
    {
        _currentTier = Tier.Group;
        _selectedTopic = topic;
        titleText.text = topic.topicName;
        ClearContent();

        foreach (var group in topic.groups)
        {
            // Check how many compatible words exist
            int compatibleCount = CountCompatibleWords(group);
            bool hasWords = compatibleCount > 0;
            Color color = hasWords ? groupColor : disabledColor;
            string label = group.groupName + (hasWords ? $" ({compatibleCount} words)" : " (no compatible words)");

            CreateItem(label, color, hasWords ? () => OnGroupClicked(group) : (Action)null, !hasWords);
        }
    }

    private void ShowLevels(GroupQuestionData group)
    {
        _currentTier = Tier.Level;
        _selectedGroup = group;
        titleText.text = _selectedTopic.topicName + " > " + group.groupName;
        ClearContent();

        for (int i = 1; i <= group.levelCount; i++)
        {
            int level = i;
            int wordCount = _currentConfig.GetWordCountForLevel(level);
            int available = CountCompatibleWords(group);
            int actual = Mathf.Min(wordCount, available);
            string label = $"Level {level}  ({actual} words)";
            CreateItem(label, levelColor, () => OnLevelClicked(level));
        }
    }

    // ─── Click handlers ───────────────────────────────────────────────────────

    private void OnTopicClicked(TopicData topic)
    {
        ShowGroups(topic);
    }

    private void OnGroupClicked(GroupQuestionData group)
    {
        ShowLevels(group);
    }

    private void OnLevelClicked(int level)
    {
        // Generate word set for this level
        LevelPlayData playData = LevelWordGenerator.Generate(
            _currentConfig, _selectedTopic, _selectedGroup, level);

        if (playData.words.Count == 0)
        {
            Debug.LogWarning("[LevelSelectMenu] No words generated for this level.");
            return;
        }

        // Store data for the gameplay scene to read
        var holder = LevelPlayDataHolder.GetOrCreate();
        holder.SetData(playData);

        OnLevelSelected?.Invoke(playData);

        Debug.Log($"[LevelSelect] {_currentConfig.displayName} | " +
                  $"{_selectedTopic.topicName} > {_selectedGroup.groupName} > Level {level} " +
                  $"| {playData.words.Count} words → Loading '{playData.sceneName}'");

        // Load the gameplay scene
        LoadGameplayScene(playData.sceneName);
    }

    // ─── Navigation ───────────────────────────────────────────────────────────

    private void GoBack()
    {
        switch (_currentTier)
        {
            case Tier.Level:
                ShowGroups(_selectedTopic);
                break;
            case Tier.Group:
                ShowTopics();
                break;
            case Tier.Topic:
                Close();
                break;
        }
    }

    // ─── Helpers ──────────────────────────────────────────────────────────────

    private GameplayConfig FindConfig(string gameplayType)
    {
        string id = gameplayType.ToLower();
        foreach (var cfg in gameplayConfigs)
        {
            if (cfg != null && cfg.gameplayId.ToLower() == id)
                return cfg;
        }
        return null;
    }

    private int CountCompatibleWords(GroupQuestionData group)
    {
        int count = 0;
        foreach (var word in group.words)
        {
            if (_currentConfig.IsWordCompatible(word))
                count++;
        }
        return count;
    }

    private void LoadGameplayScene(string sceneName)
    {
        if (useLevelManager && LevelManager.Instance != null)
        {
            LevelManager.Instance.LoadScene(sceneName, transitionName);
        }
        else
        {
            SceneManager.LoadScene(sceneName);
        }
    }

    private void ClearContent()
    {
        for (int i = contentParent.childCount - 1; i >= 0; i--)
        {
            Destroy(contentParent.GetChild(i).gameObject);
        }
    }

    private void CreateItem(string label, Color color, Action onClick, bool disabled = false)
    {
        GameObject go = Instantiate(itemPrefab, contentParent);
        go.SetActive(true);

        // Set text
        TextMeshProUGUI text = go.GetComponentInChildren<TextMeshProUGUI>();
        if (text != null)
            text.text = label;

        // Set button color and interactability
        if (go.TryGetComponent<Button>(out var btn))
        {
            ColorBlock cb = btn.colors;
            cb.normalColor = color;
            cb.highlightedColor = new Color(
                Mathf.Clamp01(color.r * 1.15f),
                Mathf.Clamp01(color.g * 1.15f),
                Mathf.Clamp01(color.b * 1.15f), color.a);
            cb.pressedColor = new Color(
                Mathf.Clamp01(color.r * 0.8f),
                Mathf.Clamp01(color.g * 0.8f),
                Mathf.Clamp01(color.b * 0.8f), color.a);
            cb.selectedColor = color;
            cb.disabledColor = disabledColor;
            btn.colors = cb;

            btn.interactable = !disabled;
            if (onClick != null && !disabled)
                btn.onClick.AddListener(() => onClick.Invoke());
        }

        // Set image color for Image-based buttons
        if (go.TryGetComponent<Image>(out var img))
            img.color = color;
    }
}
