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
        isSelected = !isSelected;
        image.color = isSelected ? selectedColor : normalColor;
        print("Selected: " + charName);
        print("Selected: " + isSelected);   
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
}