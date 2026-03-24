using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Shows a confirmation popup when the player wants to quit mid-game.
/// Pauses the game (timer + input) while the popup is visible.
/// "Go Home!" returns to MainMenu without saving.
/// "Cancel" resumes the game.
/// </summary>
public class ConfirmQuitPopup : MonoBehaviour
{
    [Header("UI References")]
    public GameObject popupPanel;
    public Button backButton;       // The "<" arrow button that triggers the popup
    public Button goHomeButton;     // "Go Home!" confirms quit
    public Button cancelButton;     // "Cancel" resumes game

    [Header("Animation")]
    [SerializeField] private float animDuration = 0.35f;

    private CanvasGroup _canvasGroup;
    private RectTransform _popupRect;
    private bool _isPaused;

    void Start()
    {
        if (popupPanel != null)
        {
            popupPanel.SetActive(false);

            _popupRect = popupPanel.GetComponent<RectTransform>();
            _canvasGroup = popupPanel.GetComponent<CanvasGroup>();
            if (_canvasGroup == null)
                _canvasGroup = popupPanel.AddComponent<CanvasGroup>();
        }

        if (backButton != null)
            backButton.onClick.AddListener(ShowPopup);
        if (goHomeButton != null)
            goHomeButton.onClick.AddListener(OnGoHome);
        if (cancelButton != null)
            cancelButton.onClick.AddListener(OnCancel);
    }

    private void ShowPopup()
    {
        if (_isPaused || popupPanel == null) return;

        _isPaused = true;

        // Pause game
        GameEvents.PauseGameMethod();

        // Animate in
        popupPanel.SetActive(true);
        _canvasGroup.alpha = 0f;
        _popupRect.localScale = Vector3.one * 0.5f;

        LeanTween.cancel(popupPanel);
        LeanTween.alphaCanvas(_canvasGroup, 1f, animDuration).setEaseOutQuad().setIgnoreTimeScale(true);
        LeanTween.scale(_popupRect, Vector3.one, animDuration).setEaseOutBack().setIgnoreTimeScale(true);
    }

    private void OnCancel()
    {
        if (!_isPaused) return;

        LeanTween.cancel(popupPanel);
        LeanTween.alphaCanvas(_canvasGroup, 0f, animDuration * 0.6f).setEaseInQuad().setIgnoreTimeScale(true);
        LeanTween.scale(_popupRect, Vector3.one * 0.5f, animDuration * 0.6f)
            .setEaseInBack()
            .setIgnoreTimeScale(true)
            .setOnComplete(() =>
            {
                popupPanel.SetActive(false);
                _isPaused = false;

                // Resume game
                GameEvents.ResumeGameMethod();
            });
    }

    private void OnGoHome()
    {
        if (!_isPaused) return;

        LeanTween.cancel(popupPanel);
        LeanTween.alphaCanvas(_canvasGroup, 0f, animDuration * 0.6f).setEaseInQuad().setIgnoreTimeScale(true);
        LeanTween.scale(_popupRect, Vector3.one * 0.5f, animDuration * 0.6f)
            .setEaseInBack()
            .setIgnoreTimeScale(true)
            .setOnComplete(() =>
            {
                popupPanel.SetActive(false);
                _isPaused = false;

                // Resume time scale before loading
                Time.timeScale = 1f;

                if (LevelManager.Instance != null)
                    LevelManager.Instance.LoadScene("MainMenu", "CrossWipe");
                else
                    SceneManager.LoadScene("MainMenu");
            });
    }
}
