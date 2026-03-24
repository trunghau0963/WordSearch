using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class VocabularyWordItem : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI wordText = null;
    [SerializeField] private TextMeshProUGUI explanationText = null;
    [SerializeField] private Button removeButton = null;
    [SerializeField] private Button expandButton = null;
    [SerializeField] private LayoutElement layoutElement = null;

    [Header("Expand Settings")]
    [SerializeField] private float collapsedHeight = 80f;
    [SerializeField] private float expandAnimDuration = 0.25f;

    private string _word;
    private string _explanation;
    private Action<string> _onRemove;
    private bool _isExpanded = false;
    private RectTransform _rectTransform;
    private int _animTweenId = -1;

    public void Initialize(string word, string explanation, Action<string> onRemove)
    {
        _word = word;
        _explanation = explanation ?? "";
        _onRemove = onRemove;
        _rectTransform = GetComponent<RectTransform>();

        if (wordText != null) wordText.text = word;
        if (removeButton != null) removeButton.onClick.AddListener(OnRemoveClicked);

        // Setup collapsed preview
        if (explanationText != null)
        {
            explanationText.text = _explanation;
            explanationText.overflowMode = TextOverflowModes.Ellipsis;
            explanationText.maxVisibleLines = 1;
        }

        // Set collapsed height
        if (layoutElement != null)
            layoutElement.preferredHeight = collapsedHeight;

        // Expand/collapse on click — use expandButton if assigned, otherwise the whole item
        if (expandButton != null)
            expandButton.onClick.AddListener(ToggleExpand);
    }

    private void ToggleExpand()
    {
        if (_isExpanded)
            Collapse();
        else
            Expand();
    }

    private void Expand()
    {
        _isExpanded = true;

        if (explanationText != null)
        {
            explanationText.maxVisibleLines = int.MaxValue;
            explanationText.overflowMode = TextOverflowModes.Overflow;
        }

        // Calculate the expanded height based on actual text size
        float expandedHeight = CalculateExpandedHeight();
        AnimateHeight(expandedHeight);
    }

    private void Collapse()
    {
        _isExpanded = false;

        if (explanationText != null)
        {
            explanationText.maxVisibleLines = 1;
            explanationText.overflowMode = TextOverflowModes.Ellipsis;
        }

        AnimateHeight(collapsedHeight);
    }

    private float CalculateExpandedHeight()
    {
        if (explanationText == null || layoutElement == null) return collapsedHeight;

        // Force text to recalculate with no line limit
        explanationText.ForceMeshUpdate();
        float textHeight = explanationText.preferredHeight;

        // Expanded height = collapsed height + extra text lines beyond the first line
        float singleLineHeight = explanationText.fontSize * 1.2f;
        float extraHeight = Mathf.Max(0, textHeight - singleLineHeight);
        return collapsedHeight + extraHeight;
    }

    private void AnimateHeight(float targetHeight)
    {
        if (layoutElement == null) return;

        CancelAnim();
        float startHeight = layoutElement.preferredHeight;

        _animTweenId = LeanTween.value(gameObject, startHeight, targetHeight, expandAnimDuration)
            .setEaseOutCubic()
            .setIgnoreTimeScale(true)
            .setOnUpdate((float h) =>
            {
                layoutElement.preferredHeight = h;
                LayoutRebuilder.MarkLayoutForRebuild(_rectTransform);
            })
            .setOnComplete(() =>
            {
                layoutElement.preferredHeight = targetHeight;
                LayoutRebuilder.ForceRebuildLayoutImmediate(
                    transform.parent as RectTransform);
                _animTweenId = -1;
            }).id;
    }

    private void CancelAnim()
    {
        if (_animTweenId != -1)
        {
            LeanTween.cancel(_animTweenId);
            _animTweenId = -1;
        }
    }

    private void OnRemoveClicked()
    {
        CancelAnim();
        _onRemove?.Invoke(_word);
    }
}
