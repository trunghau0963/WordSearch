using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Dynamically adjusts GridLayoutGroup cell sizes based on screen dimensions
/// to ensure consistent appearance across different resolutions and aspect ratios.
/// Attach to any GameObject that has a GridLayoutGroup component.
/// </summary>
[RequireComponent(typeof(GridLayoutGroup))]
[ExecuteInEditMode]
public class ResponsiveGridLayout : MonoBehaviour
{
    [Header("Reference Settings")]
    [Tooltip("The reference width the original cell size was designed for")]
    [SerializeField] private float referenceWidth = 1080f;

    [Tooltip("The original cell size at reference resolution")]
    [SerializeField] private Vector2 baseCellSize = new Vector2(185f, 190f);

    [Tooltip("The original spacing at reference resolution")]
    [SerializeField] private Vector2 baseSpacing = Vector2.zero;

    [Header("Scaling Options")]
    [Tooltip("If true, maintains the cell's width-to-height ratio")]
    [SerializeField] private bool maintainAspectRatio = true;

    [Tooltip("Minimum scale factor to prevent cells from becoming too small")]
    [SerializeField] private float minScale = 0.5f;

    [Tooltip("Maximum scale factor to prevent cells from becoming too large")]
    [SerializeField] private float maxScale = 1.5f;

    private GridLayoutGroup _gridLayout;
    private RectTransform _rectTransform;
    private float _lastWidth;

    private void Awake()
    {
        _gridLayout = GetComponent<GridLayoutGroup>();
        _rectTransform = GetComponent<RectTransform>();
    }

    private void OnEnable()
    {
        UpdateLayout();
    }

    private void Update()
    {
        if (_rectTransform == null) return;

        float currentWidth = _rectTransform.rect.width;
        if (!Mathf.Approximately(currentWidth, _lastWidth))
        {
            UpdateLayout();
        }
    }

    private void UpdateLayout()
    {
        if (_gridLayout == null || _rectTransform == null) return;

        float currentWidth = _rectTransform.rect.width;
        if (currentWidth <= 0) return;

        _lastWidth = currentWidth;

        float scaleFactor = Mathf.Clamp(currentWidth / referenceWidth, minScale, maxScale);

        Vector2 newCellSize;
        if (maintainAspectRatio)
        {
            newCellSize = baseCellSize * scaleFactor;
        }
        else
        {
            int columns = _gridLayout.constraintCount > 0 ? _gridLayout.constraintCount : 1;
            float totalPadding = _gridLayout.padding.left + _gridLayout.padding.right;
            float totalSpacing = baseSpacing.x * (columns - 1) * scaleFactor;
            float availableWidth = currentWidth - totalPadding - totalSpacing;
            float cellWidth = availableWidth / columns;
            float cellHeight = baseCellSize.y * scaleFactor;
            newCellSize = new Vector2(cellWidth, cellHeight);
        }

        _gridLayout.cellSize = newCellSize;
        _gridLayout.spacing = baseSpacing * scaleFactor;
    }
}
