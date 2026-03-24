using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Generic win popup template for all gameplay scenes.
/// Place this component on a GameObject in any gameplay scene (WordSearch, WordZee, Wordle, etc.)
/// and wire the UI references. It subscribes to GameEvents.OnShowPopup, which is scene-local
/// (destroyed with the scene), so there is no cross-gameplay interference.
/// </summary>
public class WinPopup : MonoBehaviour
{
    public GameObject winPopup;
    public Button nextLevelButton;
    public Button returnToMenuButton;

    [Header("Animation")]
    [SerializeField] private float animDuration = 0.4f;

    private CanvasGroup _canvasGroup;
    private RectTransform _popupRect;
    private bool _isCompletedLevel;

    void Start()
    {
        if (winPopup == null)
        {
            Debug.LogError("[WinPopup] winPopup panel is not assigned! Win popup will not work.");
            return;
        }

        winPopup.SetActive(false);

        _popupRect = winPopup.GetComponent<RectTransform>();
        _canvasGroup = winPopup.GetComponent<CanvasGroup>();
        if (_canvasGroup == null)
            _canvasGroup = winPopup.AddComponent<CanvasGroup>();

        if (nextLevelButton != null)
            nextLevelButton.onClick.AddListener(OnNextLevel);
        if (returnToMenuButton != null)
            returnToMenuButton.onClick.AddListener(OnReturnToMenu);
    }

    private void OnEnable()
    {
        GameEvents.OnShowPopup += ShowWinPopup;
    }

    private void OnDisable()
    {
        GameEvents.OnShowPopup -= ShowWinPopup;
    }

    void ShowWinPopup(bool isCompletedLevel)
    {
        if (winPopup == null)
        {
            Debug.LogError("[WinPopup] Cannot show popup — winPopup panel reference is missing!");
            return;
        }

        Debug.Log($"[WinPopup] ShowWinPopup called (isCompletedLevel={isCompletedLevel})");

        _isCompletedLevel = isCompletedLevel;

        var loadData = FindAnyObjectByType<LoadData>();
        if (loadData != null) loadData.DestroyAllExplanation();

        // Save to play history
        SavePlayHistory();

        // Even if all levels in this group are done, check if there's a next group to play
        bool hasNextAction = !isCompletedLevel || HasNextGroup();

        // Configure buttons
        if (nextLevelButton != null) nextLevelButton.gameObject.SetActive(hasNextAction);
        if (returnToMenuButton != null) returnToMenuButton.gameObject.SetActive(true);

        // Animate in
        winPopup.SetActive(true);
        _canvasGroup.alpha = 0f;
        _popupRect.localScale = Vector3.one * 0.5f;

        LeanTween.cancel(winPopup);
        LeanTween.alphaCanvas(_canvasGroup, 1f, animDuration).setEaseOutQuad();
        LeanTween.scale(_popupRect, Vector3.one, animDuration).setEaseOutBack();
    }

    /// <summary>
    /// Check if there's a next playable group after the current one.
    /// </summary>
    private bool HasNextGroup()
    {
        var holder = LevelPlayDataHolder.Instance;
        if (holder == null || holder.CurrentData == null) return false;

        var current = holder.CurrentData;
        var configs = Resources.LoadAll<GameplayConfig>("GameplayConfigs");
        GameplayConfig config = null;
        foreach (var c in configs)
        {
            if (c.gameplayId == current.gameplayId) { config = c; break; }
        }
        if (config == null) return false;

        var allTopics = TopicDataParser.ParseFromResources();
        TopicData topicData = null;
        GroupQuestionData groupData = null;
        foreach (var t in allTopics)
        {
            if (t.topicName == current.topicName)
            {
                topicData = t;
                foreach (var g in t.groups)
                {
                    if (g.groupName == current.groupName) { groupData = g; break; }
                }
                break;
            }
        }

        if (topicData == null || groupData == null) return false;
        return FindNextPlayableGroup(config, topicData, groupData) != null;
    }

    private void SavePlayHistory()
    {
        if (PlayHistory.Instance == null) return;

        var holder = LevelPlayDataHolder.Instance;
        if (holder == null || holder.CurrentData == null) return;

        var data = holder.CurrentData;
        int totalLevels = data.totalLevelsInGroup;

        List<TopicData> allTopics = TopicDataParser.ParseFromResources();

        PlayHistory.Instance.CompleteLevel(
            data.gameplayId, data.topicName, data.groupName,
            data.level, totalLevels, allTopics);
    }

    private void AnimateOut(System.Action onComplete)
    {
        LeanTween.cancel(winPopup);
        LeanTween.alphaCanvas(_canvasGroup, 0f, animDuration * 0.6f).setEaseInQuad();
        LeanTween.scale(_popupRect, Vector3.one * 0.5f, animDuration * 0.6f)
            .setEaseInBack()
            .setOnComplete(() =>
            {
                winPopup.SetActive(false);
                onComplete?.Invoke();
            });
    }

    private void OnNextLevel()
    {
        AnimateOut(() =>
        {
            // Try to advance to next level or next group
            var holder = LevelPlayDataHolder.Instance;
            if (holder != null && holder.CurrentData != null)
            {
                var current = holder.CurrentData;
                int nextLevel = current.level + 1;

                // Rebuild data for the next level
                var configs = Resources.LoadAll<GameplayConfig>("GameplayConfigs");
                GameplayConfig config = null;
                foreach (var c in configs)
                {
                    if (c.gameplayId == current.gameplayId) { config = c; break; }
                }

                if (config != null)
                {
                    var allTopics = TopicDataParser.ParseFromResources();
                    TopicData topicData = null;
                    GroupQuestionData groupData = null;
                    foreach (var t in allTopics)
                    {
                        if (t.topicName == current.topicName)
                        {
                            topicData = t;
                            foreach (var g in t.groups)
                            {
                                if (g.groupName == current.groupName) { groupData = g; break; }
                            }
                            break;
                        }
                    }

                    // If there are still levels to play in this group
                    if (groupData != null && nextLevel <= current.totalLevelsInGroup)
                    {
                        LevelPlayData nextData = LevelWordGenerator.Generate(
                            config, topicData, groupData, nextLevel);
                        nextData.totalLevelsInGroup = current.totalLevelsInGroup;
                        holder.SetData(nextData);
                        LoadScene(nextData.sceneName);
                        return;
                    }

                    // Levels exhausted — try to advance to the next group in the topic
                    if (topicData != null)
                    {
                        GroupQuestionData nextGroup = FindNextPlayableGroup(
                            config, topicData, groupData);

                        if (nextGroup != null)
                        {
                            // Determine effective level count for the next group
                            int compatible = CountCompatibleWords(config, nextGroup);
                            bool skipLevels = ShouldSkipLevels(config, compatible);
                            int effectiveLevels = skipLevels ? 1 : nextGroup.levelCount;

                            LevelPlayData nextData = LevelWordGenerator.Generate(
                                config, topicData, nextGroup, 1);
                            nextData.totalLevelsInGroup = effectiveLevels;
                            holder.SetData(nextData);
                            LoadScene(nextData.sceneName);
                            return;
                        }
                    }
                }
            }

            // Fallback: return to menu
            LoadMainMenu();
        });
    }

    /// <summary>
    /// Find the next group in the topic that has compatible words and hasn't been completed yet.
    /// </summary>
    private GroupQuestionData FindNextPlayableGroup(
        GameplayConfig config, TopicData topic, GroupQuestionData currentGroup)
    {
        bool foundCurrent = false;

        foreach (var group in topic.groups)
        {
            if (group.groupName == currentGroup.groupName)
            {
                foundCurrent = true;
                continue;
            }

            if (!foundCurrent) continue;

            // Check if this group has compatible words and is not completed
            int compatible = CountCompatibleWords(config, group);
            if (compatible <= 0) continue;

            bool done = PlayHistory.Instance != null &&
                        PlayHistory.Instance.IsGroupCompleted(
                            config.gameplayId, topic.topicName, group.groupName);
            if (done) continue;

            return group;
        }
        return null;
    }

    private int CountCompatibleWords(GameplayConfig config, GroupQuestionData group)
    {
        int count = 0;
        foreach (var word in group.words)
        {
            if (config.IsWordCompatible(word)) count++;
        }
        return count;
    }

    private bool ShouldSkipLevels(GameplayConfig config, int compatible)
    {
        if (config.singleWordOnly)
            return compatible <= 1;

        int minLevelWords = config.wordsPerLevel1;
        return compatible <= minLevelWords || compatible < config.wordsPerLevel2;
    }

    private void OnReturnToMenu()
    {
        AnimateOut(() => LoadMainMenu());
    }

    private void LoadMainMenu()
    {
        if (LevelManager.Instance != null)
            LevelManager.Instance.LoadScene("MainMenu", "CrossWipe");
        else
            SceneManager.LoadScene("MainMenu");
    }

    private void LoadScene(string sceneName)
    {
        if (LevelManager.Instance != null)
            LevelManager.Instance.LoadScene(sceneName, "CrossWipe");
        else
            SceneManager.LoadScene(sceneName);
    }
}
