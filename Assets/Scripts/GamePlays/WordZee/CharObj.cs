using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class CharObj : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public char charName;
    public TMP_Text text;
    public Image image;
    public RectTransform reactTransform;
    public int index;

    [Header("Appearance")]
    public Color normalColor;
    public Color selectedColor;
    public Color wrongColor;
    public Color correctColor;

    [Header("Drag Settings")]
    [SerializeField] private float liftScale = 1.15f;
    [SerializeField] private float liftDuration = 0.15f;
    [SerializeField] private float dropDuration = 0.15f;

    private bool _isDragging;
    private Vector2 _dragOffset;
    private CanvasGroup _canvasGroup;

    public bool IsDragging => _isDragging;

    private void Awake()
    {
        _canvasGroup = GetComponent<CanvasGroup>();
        if (_canvasGroup == null)
            _canvasGroup = gameObject.AddComponent<CanvasGroup>();
    }

    public CharObj SetChar(char c)
    {
        charName = c;
        image.color = normalColor;
        text.text = c.ToString();
        gameObject.SetActive(true);
        return this;
    }

    // ── Drag Handlers ──────────────────────────────────────────────

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (WordScamble.main != null && WordScamble.main.IsInputBlocked) return;

        _isDragging = true;

        // Calculate drag offset so card doesn't jump to pointer center
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            reactTransform.parent as RectTransform,
            eventData.position,
            eventData.pressEventCamera,
            out Vector2 localPoint);
        _dragOffset = reactTransform.anchoredPosition - localPoint;

        // Bring to front
        transform.SetAsLastSibling();

        // Lift animation: scale up + slight shadow via reduced alpha on others
        image.color = selectedColor;
        _canvasGroup.blocksRaycasts = false; // allow raycasts to pass through to slots below
        LeanTween.cancel(gameObject);
        LeanTween.scale(gameObject, Vector3.one * liftScale, liftDuration).setEaseOutBack();

        if (WordScamble.main != null) WordScamble.main.OnCharDragBegin(this);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!_isDragging) return;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            reactTransform.parent as RectTransform,
            eventData.position,
            eventData.pressEventCamera,
            out Vector2 localPoint);
        reactTransform.anchoredPosition = localPoint + _dragOffset;

        if (WordScamble.main != null) WordScamble.main.OnCharDragging(this);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (!_isDragging) return;
        _isDragging = false;

        // Drop animation: scale back, restore color
        _canvasGroup.blocksRaycasts = true;
        image.color = normalColor;
        LeanTween.cancel(gameObject);
        LeanTween.scale(gameObject, Vector3.one, dropDuration).setEaseOutQuad();

        if (WordScamble.main != null) WordScamble.main.OnCharDragEnd(this);
    }

    // ── Visual Feedback ────────────────────────────────────────────

    /// <summary>
    /// Highlight/unhighlight when another card hovers over this slot.
    /// </summary>
    public void SetHighlight(bool highlight)
    {
        image.color = highlight ? selectedColor : normalColor;
    }

    /// <summary>
    /// Smooth slide to a target position (used after swap).
    /// </summary>
    public void AnimateToPosition(Vector2 targetPos, float duration = 0.25f, float delay = 0f)
    {
        LeanTween.cancel(gameObject);
        transform.localScale = Vector3.one;
        LeanTween.value(gameObject, reactTransform.anchoredPosition, targetPos, duration)
            .setDelay(delay)
            .setEaseOutQuad()
            .setOnUpdate((Vector2 pos) =>
            {
                reactTransform.anchoredPosition = pos;
            });
    }

    // ── Game Animations ────────────────────────────────────────────

    /// <summary>
    /// Plays a celebration animation when the character is in the correct position.
    /// </summary>
    public void AnimateCorrect(float delay = 0f)
    {
        image.color = correctColor;
        LeanTween.cancel(gameObject);
        LeanTween.scale(gameObject, Vector3.one * 1.3f, 0.15f)
            .setDelay(delay)
            .setEaseOutQuad()
            .setOnComplete(() =>
            {
                LeanTween.scale(gameObject, Vector3.one, 0.15f).setEaseInQuad();
            });
    }

    /// <summary>
    /// Plays a shake animation when the word is wrong.
    /// </summary>
    public void AnimateWrong()
    {
        image.color = wrongColor;
        var startPos = reactTransform.anchoredPosition;
        LeanTween.value(gameObject, 0f, 1f, 0.4f)
            .setOnUpdate((float t) =>
            {
                float offset = Mathf.Sin(t * Mathf.PI * 5) * 8f * (1f - t);
                reactTransform.anchoredPosition = startPos + new Vector2(offset, 0f);
            })
            .setOnComplete(() =>
            {
                reactTransform.anchoredPosition = startPos;
                image.color = normalColor;
            });
    }

    /// <summary>
    /// Reveal animation: slide to correct position, turn yellow, then shake.
    /// </summary>
    public void AnimateReveal(Vector2 targetPos, float slideDuration, float delay)
    {
        LeanTween.cancel(gameObject);
        transform.localScale = Vector3.one;

        // 1) Slide to correct slot
        LeanTween.value(gameObject, reactTransform.anchoredPosition, targetPos, slideDuration)
            .setDelay(delay)
            .setEaseOutQuad()
            .setOnUpdate((Vector2 pos) =>
            {
                reactTransform.anchoredPosition = pos;
            })
            .setOnComplete(() =>
            {
                // 2) Turn yellow
                image.color = new Color(1f, 0.92f, 0.016f, 1f);

                // 3) Shake
                var startPos = reactTransform.anchoredPosition;
                LeanTween.value(gameObject, 0f, 1f, 0.5f)
                    .setOnUpdate((float t) =>
                    {
                        float decay = 1f - t;
                        float offset = Mathf.Sin(t * Mathf.PI * 10f) * 8f * decay;
                        reactTransform.anchoredPosition = startPos + new Vector2(offset, 0f);
                    })
                    .setOnComplete(() =>
                    {
                        reactTransform.anchoredPosition = startPos;
                    });
            });
    }

    public string ShowActive()
    {
        if (gameObject.activeSelf && text.gameObject.activeSelf && image.gameObject.activeSelf)
            return "Active";
        return "Inactive";
    }
}