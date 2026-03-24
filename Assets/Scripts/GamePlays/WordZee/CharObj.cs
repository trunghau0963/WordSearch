using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CharObj : MonoBehaviour
{
    public char charName;
    public TMP_Text text;
    public Image image;
    public RectTransform reactTransform;
    public int index;

    bool isSelected = false;

    [Header("Appearance")]
    public Color normalColor;
    public Color selectedColor;
    public Color wrongColor;
    public Color correctColor;

    public CharObj SetChar(char c)
    {
        charName = c;
        image.color = normalColor;
        text.text = c.ToString();
        gameObject.SetActive(true);
        return this;
    }

    public void Select()
    {
        // Block input during check/transition animations
        if (WordScamble.main != null && WordScamble.main.IsInputBlocked) return;

        isSelected = !isSelected;
        image.color = isSelected ? selectedColor : normalColor;
        if (isSelected)
        {
            WordScamble.main.Select(this);
        }
        else
        {
            WordScamble.main.UnSelect();
        }
    }

    public string ShowActive(){
        if(gameObject.activeSelf && text.gameObject.activeSelf && image.gameObject.activeSelf){
            return "Active";
        }
        return "Inactive";
    }

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
    /// Plays the selection bounce animation.
    /// </summary>
    public void AnimateSelect()
    {
        LeanTween.cancel(gameObject);
        LeanTween.scale(gameObject, Vector3.one * 1.15f, 0.1f)
            .setEaseOutQuad()
            .setOnComplete(() =>
            {
                LeanTween.scale(gameObject, Vector3.one, 0.1f).setEaseInQuad();
            });
    }
}