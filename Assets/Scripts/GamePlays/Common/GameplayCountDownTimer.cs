using TMPro;
using UnityEngine;

/// <summary>
/// Generic countdown timer for any gameplay scene (WordSearch, WordZee, Wordle, etc.).
/// - Reads time from GameSessionData.CurrentBoard.timeInSeconds if available (WordSearch),
///   otherwise falls back to defaultTime (WordZee, Wordle, ...).
/// - Fires GameEvents.GameOverlMethod() when time runs out.
/// - Responds to PauseGame / ResumeGame events.
/// - Stops on ShowPopup (win), BoardComplete, UnlockNextBoard, or GameOver.
/// </summary>
public class GameplayCountDownTimer : MonoBehaviour
{
    public TMP_Text timerText;

    [Header("Time Settings")]
    [Tooltip("Fallback time (seconds) when GameSessionData has no board data")]
    [SerializeField] private float defaultTime = 120f;

    private float _timeLeft;
    private bool _timeOut;
    private bool _stopTimer;

    void Start()
    {
        // WordSearch: read from board data if available; other gameplays: use defaultTime
        if (GameSessionData.CurrentBoard != null && GameSessionData.CurrentBoard.timeInSeconds > 0f)
            _timeLeft = GameSessionData.CurrentBoard.timeInSeconds;
        else
            _timeLeft = defaultTime;

        _timeOut = false;
        _stopTimer = false;
    }

    private void OnEnable()
    {
        GameEvents.OnShowPopup += OnWin;
        GameEvents.OnGameOver += StopTimer;
        GameEvents.OnPauseGame += PauseTimer;
        GameEvents.OnResumeGame += ResumeTimer;

        // WordSearch-specific stop events (harmless for other scenes — never fired there)
        GameEvents.OnBoardComplete += StopTimer;
        GameEvents.OnUnlockNextBoard += StopTimer;
    }

    private void OnDisable()
    {
        GameEvents.OnShowPopup -= OnWin;
        GameEvents.OnGameOver -= StopTimer;
        GameEvents.OnPauseGame -= PauseTimer;
        GameEvents.OnResumeGame -= ResumeTimer;

        GameEvents.OnBoardComplete -= StopTimer;
        GameEvents.OnUnlockNextBoard -= StopTimer;
    }

    void Update()
    {
        if (_stopTimer || _timeOut) return;

        _timeLeft -= Time.deltaTime;

        if (_timeLeft <= 0f)
        {
            _timeLeft = 0f;
            _timeOut = true;
            _stopTimer = true;
            UpdateDisplay();
            GameEvents.GameOverlMethod();
            GameEvents.SaveWordDictionaryMethod(); // WordSearch saves vocab on game over
            return;
        }

        UpdateDisplay();
    }

    private void UpdateDisplay()
    {
        if (timerText == null) return;
        float mins = Mathf.Floor(_timeLeft / 60f);
        float secs = Mathf.RoundToInt(_timeLeft % 60f);
        timerText.text = mins.ToString("00") + ":" + secs.ToString("00");
    }

    private void OnWin(bool isCompletedLevel) => _stopTimer = true;

    private void StopTimer() => _stopTimer = true;

    private void PauseTimer() => _stopTimer = true;

    private void ResumeTimer()
    {
        if (!_timeOut)
            _stopTimer = false;
    }
}

