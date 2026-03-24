using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SearchingWord : MonoBehaviour
{
    public TMP_Text displayText;
    public Image crossLine;
    private string _word;

    [Header("Animation")]
    private static readonly float CompletePunchScale = 0.2f;
    private static readonly float CompleteAnimDuration = 0.4f;
    private static readonly float CrossLineFadeDuration = 0.3f;

    private bool _isCompleted;

    void Start()
    {
        var button = GetComponent<Button>();
        button.onClick.AddListener(OnButtonClick);
        crossLine.gameObject.SetActive(false);
    }

    private void OnEnable()
    {
        GameEvents.OnCorrectWord += CorrectWord;
        GameEvents.OnRevealWord += CorrectWord;
    }

    private void OnDisable()
    {
        GameEvents.OnCorrectWord -= CorrectWord;
        GameEvents.OnRevealWord -= CorrectWord;
    }

    public void Setword(string word)
    {
        _word = word;
        displayText.text = word;
    }

    private void CorrectWord(string word, List<int> squareIdx)
    {
        if (word == _word && !_isCompleted)
        {
            _isCompleted = true;
            PlayCompleteAnimation();
        }
    }

    private void PlayCompleteAnimation()
    {
        var rt = GetComponent<RectTransform>();
        var targetScale = rt.localScale;

        // 1) Punch scale
        LeanTween.scale(rt, targetScale * (1f + CompletePunchScale), CompleteAnimDuration * 0.4f)
            .setEase(LeanTweenType.easeOutBack)
            .setOnComplete(() =>
            {
                LeanTween.scale(rt, targetScale, CompleteAnimDuration * 0.6f)
                    .setEase(LeanTweenType.easeOutBounce);
            });

        // 2) Show crossLine with fade-in
        crossLine.gameObject.SetActive(true);
        var crossColor = crossLine.color;
        crossLine.color = new Color(crossColor.r, crossColor.g, crossColor.b, 0f);
        LeanTween.value(gameObject, 0f, 1f, CrossLineFadeDuration)
            .setDelay(CompleteAnimDuration * 0.2f)
            .setOnUpdate((float alpha) =>
            {
                crossLine.color = new Color(crossColor.r, crossColor.g, crossColor.b, alpha);
            });

        // 3) Fade the text color to a completed look (greenish tint)
        var originalColor = displayText.color;
        var completedColor = new Color(0.4f, 0.7f, 0.4f, 0.7f);
        LeanTween.value(gameObject, 0f, 1f, CompleteAnimDuration)
            .setDelay(CrossLineFadeDuration)
            .setOnUpdate((float t) =>
            {
                displayText.color = Color.Lerp(originalColor, completedColor, t);
            });
    }

    private void OnButtonClick()
    {
        FindAnyObjectByType<LoadData>().ShowExplanation(_word);
    }
}
