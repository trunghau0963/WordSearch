using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Central manager that handles global responsive UI settings at runtime.
/// Enables TMP auto-sizing, adjusts scroll sensitivity, and optimizes
/// rendering quality based on device capabilities.
/// Place on a persistent GameObject in the scene.
/// </summary>
public class UIResponsiveManager : MonoBehaviour
{
    [Header("Auto-Size Text Settings")]
    [Tooltip("Enable auto-sizing on all TMP text elements in the scene")]
    [SerializeField] private bool enableTextAutoSizing = true;

    [Tooltip("Minimum font size for auto-sized text (relative to reference resolution)")]
    [SerializeField] private float globalMinFontSize = 14f;

    [Header("Scroll Settings")]
    [Tooltip("Base scroll sensitivity at reference resolution 1080x1920")]
    [SerializeField] private float baseScrollSensitivity = 30f;

    [Header("Quality Settings")]
    [Tooltip("Target frame rate (-1 = platform default)")]
    [SerializeField] private int targetFrameRate = 60;

    [Tooltip("Enable high resolution rendering on capable devices")]
    [SerializeField] private bool enableHighResolution = true;

    [Tooltip("Maximum resolution scale (1.0 = native, 0.75 = 75% of native)")]
    [SerializeField] private float maxResolutionScale = 1.0f;

    private static UIResponsiveManager _instance;
    public static UIResponsiveManager Instance => _instance;

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;

        ApplyQualitySettings();
    }

    private void Start()
    {
        if (enableTextAutoSizing)
        {
            EnableAutoSizingOnAllTMP();
        }

        AdjustScrollSensitivity();
    }

    private void ApplyQualitySettings()
    {
        if (targetFrameRate > 0)
        {
            Application.targetFrameRate = targetFrameRate;
        }

        if (enableHighResolution)
        {
            float dpi = Screen.dpi;
            if (dpi > 0)
            {
                // High DPI devices (>400 DPI) get slightly reduced resolution for performance
                // Lower DPI devices render at full native resolution
                float scale = dpi > 400f
                    ? Mathf.Min(maxResolutionScale, 400f / dpi * 1.2f)
                    : maxResolutionScale;

                QualitySettings.resolutionScalingFixedDPIFactor = scale;
            }
        }

        // Ensure VSync is appropriate for mobile
        QualitySettings.vSyncCount = 0; // Let targetFrameRate control timing
    }

    private void EnableAutoSizingOnAllTMP()
    {
        var tmpTexts = FindObjectsByType<TMP_Text>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (var tmp in tmpTexts)
        {
            if (!tmp.enableAutoSizing)
            {
                float currentSize = tmp.fontSize;
                tmp.enableAutoSizing = true;
                tmp.fontSizeMin = Mathf.Max(globalMinFontSize, currentSize * 0.4f);
                tmp.fontSizeMax = currentSize;
            }
        }
    }

    private void AdjustScrollSensitivity()
    {
        float referenceHeight = 1920f;
        float scaleFactor = Screen.height / referenceHeight;
        float adjustedSensitivity = baseScrollSensitivity * scaleFactor;

        var scrollRects = FindObjectsByType<ScrollRect>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (var scrollRect in scrollRects)
        {
            scrollRect.scrollSensitivity = adjustedSensitivity;
            // Enable inertia for smoother scrolling feel
            scrollRect.inertia = true;
            scrollRect.decelerationRate = 0.135f;
            scrollRect.movementType = ScrollRect.MovementType.Elastic;
            scrollRect.elasticity = 0.1f;
        }
    }
}
