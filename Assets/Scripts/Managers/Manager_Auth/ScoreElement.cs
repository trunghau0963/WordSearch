using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ScoreElement : MonoBehaviour
{
    [Header("UI")]
    public TMP_Text usernameText;
    public TMP_Text levelsText;
    public TMP_Text heartsText;
    [Header("Rank")]
    public Image rankImage;
    public TMP_Text rankText;

    public void NewScoreElementInTop(string _username, int _levels, int _hearts, Sprite _rankImage)
    {
        usernameText.text = _username;
        levelsText.text = _levels.ToString();
        heartsText.text = _hearts.ToString();
        rankText.gameObject.SetActive(false);
        rankImage.gameObject.SetActive(true);
        rankImage.sprite = _rankImage;
    }

    public void NewScoreElement(string _username, int _levels, int _hearts, int _rank)
    {
        usernameText.text = _username;
        levelsText.text = _levels.ToString();
        heartsText.text = _hearts.ToString();
        rankImage.gameObject.SetActive(false);
        rankText.gameObject.SetActive(true);
        rankText.text = _rank.ToString();
    }

}
