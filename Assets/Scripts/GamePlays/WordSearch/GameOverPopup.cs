using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameOverPopup : MonoBehaviour
{
    public GameObject gameOverPopup;
    public Button retryButton;
    public Button returnToMenuButton;

    [Header("Animation")]
    [SerializeField] private float animDuration = 0.4f;

    private CanvasGroup _canvasGroup;
    private RectTransform _popupRect;

    void Start()
    {
        if (gameOverPopup != null)
            gameOverPopup.SetActive(false);

        if (gameOverPopup != null)
        {
            _popupRect = gameOverPopup.GetComponent<RectTransform>();
            _canvasGroup = gameOverPopup.GetComponent<CanvasGroup>();
            if (_canvasGroup == null)
                _canvasGroup = gameOverPopup.AddComponent<CanvasGroup>();
        }

        if (retryButton != null)
            retryButton.onClick.AddListener(OnRetry);
        if (returnToMenuButton != null)
            returnToMenuButton.onClick.AddListener(OnReturnToMenu);

        GameEvents.OnGameOver += ShowGameOverPopup;
    }

    private void OnDisable()
    {
        GameEvents.OnGameOver -= ShowGameOverPopup;
    }

    public void ShowGameOverPopup()
    {
        var loadData = FindAnyObjectByType<LoadData>();
        if (loadData != null) loadData.DestroyAllExplanation();

        if (gameOverPopup == null) return;

        gameOverPopup.SetActive(true);
        _canvasGroup.alpha = 0f;
        _popupRect.localScale = Vector3.one * 0.5f;

        LeanTween.cancel(gameOverPopup);
        LeanTween.alphaCanvas(_canvasGroup, 1f, animDuration).setEaseOutQuad();
        LeanTween.scale(_popupRect, Vector3.one, animDuration).setEaseOutBack();
    }

    private void AnimateOut(System.Action onComplete)
    {
        LeanTween.cancel(gameOverPopup);
        LeanTween.alphaCanvas(_canvasGroup, 0f, animDuration * 0.6f).setEaseInQuad();
        LeanTween.scale(_popupRect, Vector3.one * 0.5f, animDuration * 0.6f)
            .setEaseInBack()
            .setOnComplete(() =>
            {
                gameOverPopup.SetActive(false);
                onComplete?.Invoke();
            });
    }

    private void OnRetry()
    {
        AnimateOut(() =>
        {
            // Reload the same scene to retry
            var holder = LevelPlayDataHolder.Instance;
            if (holder != null && holder.CurrentData != null)
            {
                LoadScene(holder.CurrentData.sceneName);
            }
            else
            {
                // Legacy fallback: reload current scene
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            }
        });
    }

    private void OnReturnToMenu()
    {
        AnimateOut(() =>
        {
            if (LevelManager.Instance != null)
                LevelManager.Instance.LoadScene("MainMenu", "CrossWipe");
            else
                SceneManager.LoadScene("MainMenu");
        });
    }

    private void LoadScene(string sceneName)
    {
        if (LevelManager.Instance != null)
            LevelManager.Instance.LoadScene(sceneName, "CrossWipe");
        else
            SceneManager.LoadScene(sceneName);
    }
}
