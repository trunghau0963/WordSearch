using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridSquare : MonoBehaviour
{
    public int SquareIndex { get; set; }

    private AlphabetData.LetterData _normalLetterData;
    private AlphabetData.LetterData _selectedLetterData;
    private AlphabetData.LetterData _correctLetterData;

    private SpriteRenderer _displayedImage;

    private bool _selected;
    private bool _clicked;
    public int _index = -1;
    public bool _correct;

    // Animation settings
    private static readonly float SelectPunchScale = 0.15f;
    private static readonly float SelectAnimDuration = 0.15f;
    private static readonly float CorrectPunchScale = 0.25f;
    private static readonly float CorrectAnimDuration = 0.35f;
    private static readonly float ShakeIntensity = 0.08f;
    private static readonly float ShakeDuration = 0.3f;

    private Vector3 _originalScale;
    private Vector3 _originalPosition;
    private static bool _gamePaused;

    public void SetIndex(int index)
    {
        _index = index;
    }

    public int GetIndex()
    {
        return _index;
    }

    void Start()
    {
        _clicked = false;
        _selected = false;
        _correct = false;
        _displayedImage = GetComponent<SpriteRenderer>();
        _originalPosition = transform.localPosition;
    }

    private void OnEnable()
    {
        GameEvents.OnEnableSquareSelection += OnEnableSquareSelection;
        GameEvents.OnDisableSquareSelection += OnDisableSquareSelection;
        GameEvents.OnSelectSquare += SelectSquare;
        GameEvents.OnCorrectWord += CorrectWord;
        GameEvents.OnRevealWord += RevealWord;
        GameEvents.OnClearSelection += OnWrongSelection;
        GameEvents.OnPauseGame += OnPauseGame;
        GameEvents.OnResumeGame += OnResumeGame;
    }

    private void OnDisable()
    {
        GameEvents.OnEnableSquareSelection -= OnEnableSquareSelection;
        GameEvents.OnDisableSquareSelection -= OnDisableSquareSelection;
        GameEvents.OnSelectSquare -= SelectSquare;
        GameEvents.OnCorrectWord -= CorrectWord;
        GameEvents.OnRevealWord -= RevealWord;
        GameEvents.OnClearSelection -= OnWrongSelection;
        GameEvents.OnPauseGame -= OnPauseGame;
        GameEvents.OnResumeGame -= OnResumeGame;
    }

    private void OnPauseGame() { _gamePaused = true; }
    private void OnResumeGame() { _gamePaused = false; }

    public void CorrectWord(string word, List<int> squareIdx)
    {
        if (squareIdx.Contains(_index) && _selected)
        {
            _correct = true;
            _displayedImage.sprite = _correctLetterData.Image;
            PlayCorrectAnimation();
        }

        _selected = false;
        _clicked = false;
    }

    public void OnEnableSquareSelection()
    {
        _clicked = true;
        _selected = false;
    }

    public void OnDisableSquareSelection()
    {
        _clicked = false;
        _selected = false;
        if (_correct)
        {
            _displayedImage.sprite = _correctLetterData.Image;
        }
        else
        {
            _displayedImage.sprite = _normalLetterData.Image;
        }
    }

    private void SelectSquare(Vector3 position)
    {
        if (this.gameObject.transform.position == position)
        {
            _displayedImage.sprite = _selectedLetterData.Image;
            PlaySelectAnimation();
        }
    }

    public void SetSprite(AlphabetData.LetterData normalLetterData, AlphabetData.LetterData selectedLetterData,
        AlphabetData.LetterData correctLetterData)
    {
        _normalLetterData = normalLetterData;
        _selectedLetterData = selectedLetterData;
        _correctLetterData = correctLetterData;

        GetComponent<SpriteRenderer>().sprite = _normalLetterData.Image;
    }

    private void OnMouseDown()
    {
        if (_gamePaused) return;
        OnEnableSquareSelection();
        GameEvents.EnableSquareSelectionMethod();
        CheckSquare();
        _displayedImage.sprite = _selectedLetterData.Image;
        PlaySelectAnimation();
    }

    private void OnMouseEnter()
    {
        if (_gamePaused) return;
        CheckSquare();
    }

    private void OnMouseUp()
    {
        if (_gamePaused) return;
        GameEvents.ClearSelectionMethod();
        GameEvents.DisableSquareSelectionMethod();
    }

    public void CheckSquare()
    {
        if (_clicked && !_selected)
        {
            _selected = true;
            GameEvents.CheckSquareMethod(_normalLetterData.Letter, gameObject.transform.position, _index);
        }
    }

    // --- Animations ---

    private void PlaySelectAnimation()
    {
        LeanTween.cancel(gameObject);
        LeanTween.scale(gameObject, transform.localScale * (1f + SelectPunchScale), SelectAnimDuration * 0.5f)
            .setEase(LeanTweenType.easeOutQuad)
            .setOnComplete(() =>
            {
                LeanTween.scale(gameObject, transform.localScale / (1f + SelectPunchScale), SelectAnimDuration * 0.5f)
                    .setEase(LeanTweenType.easeInQuad);
            });
    }

    private void PlayCorrectAnimation()
    {
        LeanTween.cancel(gameObject);
        var targetScale = transform.localScale;

        // Punch scale up then back
        LeanTween.scale(gameObject, targetScale * (1f + CorrectPunchScale), CorrectAnimDuration * 0.4f)
            .setEase(LeanTweenType.easeOutBack)
            .setOnComplete(() =>
            {
                LeanTween.scale(gameObject, targetScale, CorrectAnimDuration * 0.6f)
                    .setEase(LeanTweenType.easeOutBounce);
            });
    }

    private void OnWrongSelection()
    {
        // Only shake squares that were selected but not correct
        if (_selected && !_correct)
        {
            PlayShakeAnimation();
            TryVibrate();
        }
    }

    private void PlayShakeAnimation()
    {
        LeanTween.cancel(gameObject);
        var startPos = transform.localPosition;

        LeanTween.value(gameObject, 0f, 1f, ShakeDuration)
            .setOnUpdate((float t) =>
            {
                float decay = 1f - t;
                float offsetX = Mathf.Sin(t * Mathf.PI * 8f) * ShakeIntensity * decay;
                transform.localPosition = startPos + new Vector3(offsetX, 0f, 0f);
            })
            .setOnComplete(() =>
            {
                transform.localPosition = startPos;
            })
            .setEase(LeanTweenType.linear);
    }

    public void RevealWord(string word, List<int> squareIdx)
    {
        if (!squareIdx.Contains(_index)) return;
        if (_correct) return; // already found by player

        _correct = true;
        _displayedImage.sprite = _correctLetterData.Image;

        // Yellow tint
        _displayedImage.color = new Color(1f, 0.92f, 0.016f, 1f);

        // Shake animation
        PlayRevealShakeAnimation();
    }

    private void PlayRevealShakeAnimation()
    {
        LeanTween.cancel(gameObject);
        var startPos = transform.localPosition;

        LeanTween.value(gameObject, 0f, 1f, 0.5f)
            .setOnUpdate((float t) =>
            {
                float decay = 1f - t;
                float offsetX = Mathf.Sin(t * Mathf.PI * 10f) * 0.12f * decay;
                transform.localPosition = startPos + new Vector3(offsetX, 0f, 0f);
            })
            .setOnComplete(() =>
            {
                transform.localPosition = startPos;
            })
            .setEase(LeanTweenType.linear);
    }

    private void TryVibrate()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        Handheld.Vibrate();
#endif
    }
}
