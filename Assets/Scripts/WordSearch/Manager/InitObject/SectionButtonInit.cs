using System.Collections;
using System.Collections.Generic;
// using Unity.Android.Gradle.Manifest;
using UnityEngine;
using UnityEngine.UI;

public class SectionInit : MonoBehaviour
{
    // Start is called before the first frame update

    Category_PlayerPrefs category;
    public GameData currentData;

    public GameObject sectionButtonPrefab;

    int totalLevelCount = 0;

    int finishedLevelCount = 0;

    int totalBoardCount = 0;

    int finishedBoardCount = 0;


    public void SetSelectedCategory(Category_PlayerPrefs category)
    {
        this.category = category;
    }

    public Category_PlayerPrefs GetSelectedCategory()
    {
        return category;
    }



    void OnEnable()
    {
        if (currentData.selectedCategory != null)
        {
            category = currentData.selectedCategory;
        }
        else
        {
            category = new Category_PlayerPrefs();
        }
        InitializeSectionList();
    }

    void OnDisable()
    {
        // Reset all the button status
        foreach (Transform child in transform)
        {

            Destroy(child.gameObject);
        }
    }

    public void InitializeSectionList()
    {
        Debug.Log("SectionInit: " + category.GetCategoryName());
        if (category != null)
        {
            foreach (Section_PlayerPrefs section in category.GetSections())
            {
                totalLevelCount = 0;
                finishedLevelCount = 0;
                foreach (Level_PlayerPrefs level in section.GetLevels())
                {
                    totalLevelCount++;
                    if (level.GetIsLock() == false)
                    {
                        finishedLevelCount++;
                    }

                    foreach (BoardData board in level.GetBoardList())
                    {
                        totalBoardCount++;
                        if (board.GetIsCompleted() == true)
                        {
                            finishedBoardCount++;
                        }
                    }

                }
                string textProgress = (((float)finishedLevelCount / totalLevelCount) * 100).ToString() + " %";
                // Debug.Log("Section Name: " + section.SectionName + " " + (float)finishedLevelCount / totalLevelCount);
                Button sectionButton = Instantiate(sectionButtonPrefab, transform).GetComponent<Button>();
                sectionButton.GetComponent<SectionButton>().Init(section.GetSectionName(), (float)finishedLevelCount / totalLevelCount, section.GetIsLock(), textProgress, section);
                sectionButton.interactable = !section.GetIsLock();
                // Debug.Log(section.SectionName + " " + (float)finishedLevelCount / totalLevelCount);

            }
        }
    }
}