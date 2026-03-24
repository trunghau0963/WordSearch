using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

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
        _isCompletedLevel = isCompletedLevel;

        var loadData = FindAnyObjectByType<LoadData>();
        if (loadData != null) loadData.DestroyAllExplanation();

        // Save to play history
        SavePlayHistory();

        // Configure buttons
        if (nextLevelButton != null) nextLevelButton.gameObject.SetActive(!isCompletedLevel);
        if (returnToMenuButton != null) returnToMenuButton.gameObject.SetActive(true);

        // Animate in
        winPopup.SetActive(true);
        _canvasGroup.alpha = 0f;
        _popupRect.localScale = Vector3.one * 0.5f;

        LeanTween.cancel(winPopup);
        LeanTween.alphaCanvas(_canvasGroup, 1f, animDuration).setEaseOutQuad();
        LeanTween.scale(_popupRect, Vector3.one, animDuration).setEaseOutBack();
    }

    private void SavePlayHistory()
    {
        if (PlayHistory.Instance == null) return;

        var holder = LevelPlayDataHolder.Instance;
        if (holder == null || holder.CurrentData == null) return;

        var data = holder.CurrentData;
        int totalLevels = 3; // default level count per group

        // Try to get actual topic data for full completion check
        List<TopicData> allTopics = TopicDataParser.ParseFromResources();

        // Find the actual level count from the group
        if (allTopics != null)
        {
            foreach (var topic in allTopics)
            {
                if (topic.topicName == data.topicName)
                {
                    foreach (var group in topic.groups)
                    {
                        if (group.groupName == data.groupName)
                        {
                            totalLevels = group.levelCount;
                            break;
                        }
                    }
                    break;
                }
            }
        }

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
            // Try to advance to next level
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

                    if (groupData != null && nextLevel <= groupData.levelCount)
                    {
                        LevelPlayData nextData = LevelWordGenerator.Generate(
                            config, topicData, groupData, nextLevel);
                        holder.SetData(nextData);
                        LoadScene(nextData.sceneName);
                        return;
                    }
                }
            }

            // Fallback: return to menu
            LoadMainMenu();
        });
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
