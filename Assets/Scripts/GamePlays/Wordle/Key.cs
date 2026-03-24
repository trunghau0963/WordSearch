using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Key : MonoBehaviour
{
    public enum KeyAction { Letter, Delete, Submit, Other }

    [Header("Elements")]
    [SerializeField] private TextMeshProUGUI text;

    [Header("Key Type")]
    [Tooltip("Set to Delete for backspace key, Submit for submit key, Other for non-gameplay buttons")]
    [SerializeField] private KeyAction keyAction = KeyAction.Letter;

    [Header("Animation")]
    [SerializeField] private float pressScale = 0.85f;
    [SerializeField] private float pressDuration = 0.06f;

    [Header("Properties")]
    public static Action<string> OnKeyPressed;

    private RectTransform _rectTransform;
    private Vector3 _originalScale;

    public KeyAction Action => keyAction;

    void Awake()
    {
        _rectTransform = GetComponent<RectTransform>();
        _originalScale = transform.localScale;
    }

    void Start()
    {
        GetComponent<Button>().onClick.AddListener(() => SendKeyPress());
    }

    private void SendKeyPress()
    {
        if (keyAction == KeyAction.Other) return;

        // Press animation: quick shrink → bounce back
        LeanTween.cancel(gameObject);
        LeanTween.scale(gameObject, _originalScale * pressScale, pressDuration)
            .setEaseInQuad()
            .setOnComplete(() =>
            {
                LeanTween.scale(gameObject, _originalScale, pressDuration * 1.5f)
                    .setEaseOutBack();
            });

        // Send standardized key string based on action type
        string keyString = keyAction switch
        {
            KeyAction.Delete => "DELETE",
            KeyAction.Submit => "SUBMIT",
            _ => text != null ? text.text : ""
        };

        OnKeyPressed?.Invoke(keyString);
    }

    /// <summary>
    /// Called by BoardWD to animate keyboard entry. Slides up from below with pop.
    /// </summary>
    public void AnimateEntry(float delay)
    {
        transform.localScale = Vector3.zero;
        LeanTween.scale(gameObject, _originalScale, 0.25f)
            .setDelay(delay)
            .setEaseOutBack();
    }

    public void SetLetter(char letter)
    {
        text.text = letter.ToString();
    }

    public void SetState(Tiles.State state)
    {
        if (state == null || text == null) return;
        text.color = state.fillColor;
    }

    public void SetInteractable(bool interactable)
    {
        GetComponent<Button>().interactable = interactable;
    }
}
