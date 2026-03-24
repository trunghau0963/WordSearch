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
    }

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
