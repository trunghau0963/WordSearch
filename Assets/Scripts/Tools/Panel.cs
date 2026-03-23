using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Panel : MonoBehaviour
{
    [SerializeField] private string id = ""; public string ID { get { return id; } }
    [SerializeField] private RectTransform container = null;

    [Header("Animation Settings")]
    [SerializeField] private float animationDuration = 0.3f;
    [SerializeField] private bool useAnimation = true;

    private bool _initialized = false; public bool IsInitialized => _initialized;
    private bool _isOpen = false; public bool IsOpen => _isOpen;
    private bool _isAnimating = false;
    private Canvas _canvas = null; public Canvas Canvas { get { return _canvas; } set { _canvas = value; } }
    private CanvasGroup _canvasGroup;

    public virtual void Awake()
    {
        Initialize();
    }

    public virtual void Initialize()
    {
        if (_initialized) { return; }
        _initialized = true;

        if (container != null)
        {
            _canvasGroup = container.GetComponent<CanvasGroup>();
            if (_canvasGroup == null)
                _canvasGroup = container.gameObject.AddComponent<CanvasGroup>();
        }

        Close();
    }

    public virtual void Open()
    {
        if (_initialized == false) { Initialize(); }
        if (_isOpen || _isAnimating) return;

        transform.SetAsLastSibling();
        container.gameObject.SetActive(true);
        _isOpen = true;

        if (useAnimation && _canvasGroup != null)
        {
            _isAnimating = true;
            _canvasGroup.alpha = 0f;
            container.localScale = Vector3.one * 0.85f;

            LeanTween.cancel(container.gameObject);
            LeanTween.alphaCanvas(_canvasGroup, 1f, animationDuration)
                .setEaseOutQuad()
                .setIgnoreTimeScale(true);
            LeanTween.scale(container, Vector3.one, animationDuration)
                .setEaseOutBack()
                .setIgnoreTimeScale(true)
                .setOnComplete(() => _isAnimating = false);
        }
    }

    public virtual void Close()
    {
        if (_initialized == false) { Initialize(); }
        if (!_isOpen && _initialized) 
        {
            container.gameObject.SetActive(false);
            return;
        }

        if (useAnimation && _canvasGroup != null && container.gameObject.activeInHierarchy)
        {
            _isAnimating = true;
            _isOpen = false;

            LeanTween.cancel(container.gameObject);
            LeanTween.alphaCanvas(_canvasGroup, 0f, animationDuration * 0.7f)
                .setEaseInQuad()
                .setIgnoreTimeScale(true);
            LeanTween.scale(container, Vector3.one * 0.85f, animationDuration * 0.7f)
                .setEaseInBack()
                .setIgnoreTimeScale(true)
                .setOnComplete(() =>
                {
                    container.gameObject.SetActive(false);
                    container.localScale = Vector3.one;
                    _isAnimating = false;
                });
        }
        else
        {
            container.gameObject.SetActive(false);
            _isOpen = false;
        }
    }
}