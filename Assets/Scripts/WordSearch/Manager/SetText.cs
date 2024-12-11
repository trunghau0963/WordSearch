using System.Collections.Generic;
using System.Xml.Serialization;
using NUnit.Framework;
using TMPro;
using UnityEngine;

public class SetText : MonoBehaviour
{
    public bool isHeaderCanClose;
    public bool isCondition;
    public bool isTitleInSection;
    public TMP_Text tMP_Text;
    public GameData gameData;

    void OnEnable()
    {
        if (isHeaderCanClose)
        {
            if (isCondition)
            {
                tMP_Text.text = "Select a Section";
            }
            else
            {
                tMP_Text.text = "Select a category";
            }
        }
        else
        {
            if (isCondition)
            {
                tMP_Text.text = gameData.selectedSection.GetSectionName();
            }
            else
            {
                tMP_Text.text = "Leaderboard";
            }
        }
        if (isTitleInSection)
        {
            tMP_Text.text = gameData.selectedCategory.GetCategoryName();
        }
    }

    void OnDisable()
    {
        tMP_Text.text = "";
    }
}



