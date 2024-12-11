using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CountDownTimer : MonoBehaviour
{
    public GameData currentGameData;

    public TMP_Text timerText;

    private float _timeLeft;
    private float _minus;
    private float _seconds;
    private float _oneSecondDown;

    private bool _timeOut;
    private bool _stopTimer;

    void Start()
    {
        _timeLeft = currentGameData.selectedBoardData.timeInSeconds;
        _oneSecondDown = _timeLeft - 1f;
        _timeOut = false;
        _stopTimer = false;

        GameEvents.OnBoardComplete += StopTimer;
        GameEvents.OnUnlockNextBoard += StopTimer;

    }

    void Update()
    {
        if (!_stopTimer)
        {
            _timeLeft -= Time.deltaTime;
        }
        if(_timeLeft <= _oneSecondDown)
        {
            _oneSecondDown = _timeLeft - 1f;
        }
    }

    private void OnDisable()
    {
        GameEvents.OnBoardComplete -= StopTimer;
        GameEvents.OnUnlockNextBoard -= StopTimer;
    }

    public void StopTimer()
    {
        _stopTimer = true;
    }

    void OnGUI()
    {
        if (!_timeOut)
        {
            if (_timeLeft > 0)
            {
                // _timeLeft -= Time.deltaTime;
                _seconds = Mathf.RoundToInt(_timeLeft % 60);
                _minus = Mathf.Floor(_timeLeft / 60);
                timerText.text = _minus.ToString("00") + ":" + _seconds.ToString("00");
            }
            else
            {
                // _timeOut = true;
                // timerText.text = "00:00";
                // GameEvents.OnTimeOut();
                _stopTimer = true;
                ActivateGameOverGUI();
            }
        }
    }

    private void ActivateGameOverGUI()
    {
        GameEvents.GameOverlMethod();
        GameEvents.SaveWordDictionaryMethod();
        _timeOut = true;

    }

}
