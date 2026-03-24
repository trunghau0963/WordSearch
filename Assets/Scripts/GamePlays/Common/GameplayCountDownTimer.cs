using TMPro;
using UnityEngine;
using System.Collections;

/// <summary>
/// Generic countdown timer for any gameplay scene (WordSearch, WordZee, Wordle, etc.).
/// - Reads time from LevelPlayData.timeInSeconds (per-level config) if available,
///   then from GameSessionData.CurrentBoard.timeInSeconds (WordSearch legacy),
///   otherwise falls back to defaultTime.
/// - Fires RevealAnswers first, waits for reveal, then fires GameOver.
/// - Responds to PauseGame / ResumeGame events.
/// - Stops on ShowPopup (win), BoardComplete, UnlockNextBoard, or GameOver.
/// </summary>
public class GameplayCountDownTimer : MonoBehaviour
{
    public TMP_Text timerText;

    [Header("Time Settings")]
    [Tooltip("Fallback time (seconds) when GameSessionData has no board data")]
    [SerializeField] private float defaultTime = 120f;

    [Header("Reveal Settings")]
    [Tooltip("Seconds to wait after revealing answers before showing GameOver popup")]
    [SerializeField] private float revealDuration = 3f;

    private float _timeLeft;
    private bool _timeOut;
    private bool _stopTimer;

    void Start()
    {
        // Priority 1: LevelPlayData.timeInSeconds (from GameplayConfig per-level time)
        var holder = LevelPlayDataHolder.Instance;
        if (holder != null && holder.CurrentData != null && holder.CurrentData.timeInSeconds > 0f)
            _timeLeft = holder.CurrentData.timeInSeconds;
        // Priority 2: WordSearch board data
        else if (GameSessionData.CurrentBoard != null && GameSessionData.CurrentBoard.timeInSeconds > 0f)
            _timeLeft = GameSessionData.CurrentBoard.timeInSeconds;
        // Priority 3: Inspector fallback
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
            StartCoroutine(RevealThenGameOver());
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

    private IEnumerator RevealThenGameOver()
    {
        // 1) Tell each gameplay to reveal the correct answer
        GameEvents.RevealAnswersMethod();

        // 2) Wait for reveal animation to complete
        yield return new WaitForSeconds(revealDuration);

        // 3) Now fire game over + save
        GameEvents.GameOverlMethod();
        GameEvents.SaveWordDictionaryMethod();
    }
}

