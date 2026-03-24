using UnityEngine;

/// <summary>
/// Forces screen orientation to Portrait when this component is active.
/// Restores Landscape when destroyed (e.g., on scene change).
/// Attach to a root object in Portrait-only scenes (Wordle).
/// Includes a smooth fade transition to mask the rotation.
/// </summary>
public class ScreenOrientationHandler : MonoBehaviour
{
    [SerializeField] private bool forcePortrait = true;
    [SerializeField] private float fadeDuration = 0.3f;

    private CanvasGroup _fadeOverlay;

    private void Awake()
    {
        if (!forcePortrait) return;

        // Create a temporary black overlay to smooth the orientation change
        CreateFadeOverlay();

        Screen.autorotateToLandscapeLeft = false;
        Screen.autorotateToLandscapeRight = false;
        Screen.autorotateToPortrait = true;
        Screen.autorotateToPortraitUpsideDown = true;
        Screen.orientation = ScreenOrientation.Portrait;

#if UNITY_EDITOR
        // In Editor, force the Game view to portrait resolution
        ForceEditorPortrait();
#endif

        // Fade out the overlay after a short delay to allow orientation to settle
        if (_fadeOverlay != null)
        {
            _fadeOverlay.alpha = 1f;
            LeanTween.alphaCanvas(_fadeOverlay, 0f, fadeDuration)
                .setDelay(0.4f)
                .setEaseOutQuad()
                .setIgnoreTimeScale(true)
                .setOnComplete(() =>
                {
                    if (_fadeOverlay != null)
                        Destroy(_fadeOverlay.gameObject);
                });
        }
    }

    private void OnDestroy()
    {
        if (!forcePortrait) return;

        // Restore landscape for other scenes
        Screen.autorotateToLandscapeLeft = true;
        Screen.autorotateToLandscapeRight = true;
        Screen.autorotateToPortrait = false;
        Screen.autorotateToPortraitUpsideDown = false;
        Screen.orientation = ScreenOrientation.LandscapeLeft;

#if UNITY_EDITOR
        ForceEditorLandscape();
#endif
    }

#if UNITY_EDITOR
    private static int _previousWidth;
    private static int _previousHeight;

    private void ForceEditorPortrait()
    {
        _previousWidth = Screen.width;
        _previousHeight = Screen.height;

        // Swap to portrait if currently landscape
        if (Screen.width > Screen.height)
        {
            Screen.SetResolution(Screen.height, Screen.width, false);
        }
    }

    private void ForceEditorLandscape()
    {
        // Restore previous resolution, or swap back to landscape
        if (_previousWidth > 0 && _previousHeight > 0)
        {
            Screen.SetResolution(_previousWidth, _previousHeight, false);
        }
        else if (Screen.height > Screen.width)
        {
            Screen.SetResolution(Screen.height, Screen.width, false);
        }
    }
#endif

    private void CreateFadeOverlay()
    {
        var canvas = FindAnyObjectByType<Canvas>();
        if (canvas == null) return;

        var go = new GameObject("OrientationFade");
        go.transform.SetParent(canvas.transform, false);

        var rect = go.AddComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        var img = go.AddComponent<UnityEngine.UI.Image>();
        img.color = Color.black;
        img.raycastTarget = false;

        _fadeOverlay = go.AddComponent<CanvasGroup>();
        _fadeOverlay.blocksRaycasts = false;

        // Ensure it's on top
        go.transform.SetAsLastSibling();
    }
}
