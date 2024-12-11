using System.Collections;
using System.Collections.Generic;
// using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class CategoryButtonInit : MonoBehaviour
{
    public Data_PlayerPrefs data;

    public GameObject categoryButtonPrefab;

    int finishedSectionCount = 0;

    int totalSectionCount = 0;

    void Awake()
    {
        InitializeCategoryList();
    }

    void OnEnable()
    {
        InitializeCategoryList();
    }

    void OnDisable()
    {
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }
    }

    public void InitializeCategoryList()
    {
        if (data.categories != null)
        {
            foreach (Category_PlayerPrefs category in data.categories)
            {
                finishedSectionCount = 0;
                totalSectionCount = 0;
                foreach (Section_PlayerPrefs section in category.GetSections())
                {
                    totalSectionCount++;
                    if (section.GetIsLock() == false)
                    {
                        finishedSectionCount++;
                    }
                }

                string textProgress = finishedSectionCount + "/" + totalSectionCount;
                Button categoryButton = Instantiate(categoryButtonPrefab, transform).GetComponent<Button>();
                categoryButton.GetComponent<CategoryButton>().Init(category.GetCategoryName(), (float)finishedSectionCount / totalSectionCount, textProgress, category);
                categoryButton.interactable = !category.GetIsLock();
                // Debug.Log(category.CategoryName + " " + (float)finishedSectionCount / totalSectionCount);
            }
        }
    }
}
// public class CategoryButtonInit : MonoBehaviour
// {
//     public GameDataSave data;

//     public GameObject categoryButtonPrefab;

//     int finishedSectionCount = 0;

//     int totalSectionCount = 0;

//     SavingFile savingFile;
//     void Awake()
//     {
//         savingFile = FindAnyObjectByType<SavingFile>();
//         data = savingFile.LoadData();
//         // Debug.Log("CategoryButtonInit: " + data.DataSet.Count);
//         InitializeCategoryList();
//     }

//     public void InitializeCategoryList()
//     {
//         if (data.DataSet != null)
//         {
//             foreach (Category category in data.DataSet)
//             {
//                 finishedSectionCount = 0;
//                 totalSectionCount = 0;
//                 foreach (Section section in category.Sections)
//                 {
//                     totalSectionCount++;
//                     if (section.isLock == false)
//                     {
//                         finishedSectionCount++;
//                     }
//                 }

//                 string textProgress = finishedSectionCount + "/" + totalSectionCount;
//                 Button categoryButton = Instantiate(categoryButtonPrefab, transform).GetComponent<Button>();
//                 categoryButton.GetComponent<CategoryButton>().Init(category.CategoryName, (float)finishedSectionCount / totalSectionCount, textProgress);
//                 categoryButton.interactable = !category.isLock;
//                 // Debug.Log(category.CategoryName + " " + (float)finishedSectionCount / totalSectionCount);
//             }
//         }
//     }
// }