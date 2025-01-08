using System.Collections.Generic;
using System.IO;
using UnityEngine;


[System.Serializable]
[CreateAssetMenu]
public class Category_PlayerPrefs : ScriptableObject
{
    public string categoryName;
    public bool isLock = true;
    public List<Section_PlayerPrefs> sections;

    // Constructor

    public Category_PlayerPrefs()
    {
        this.categoryName = "";
        // this.isLock = true;
        this.sections = new List<Section_PlayerPrefs>();
    }
    public Category_PlayerPrefs(string categoryName, bool isLock, List<Section_PlayerPrefs> sections)
    {
        this.categoryName = categoryName;
        this.isLock = isLock;
        this.sections = sections;
    }

    // Getter and Setter for CategoryName
    public string GetCategoryName()
    {
        return categoryName;
    }

    public void SetCategoryName(string value)
    {
        categoryName = value;
    }

    // Getter and Setter for IsLock
    public bool GetIsLock()
    {
        return isLock;
    }

    public void SetIsLock(bool value)
    {
        isLock = value;
    }

    // Getter and Setter for Sections
    public List<Section_PlayerPrefs> GetSections()
    {
        return sections;
    }

    public void SetSections(List<Section_PlayerPrefs> value)
    {
        sections = value;
    }

    // Add a Section
    public void AddSection(Section_PlayerPrefs section)
    {
        if (sections == null)
        {
            sections = new List<Section_PlayerPrefs>();
        }
        sections.Add(section);
    }

    // Remove a Section
    public bool RemoveSection(Section_PlayerPrefs section)
    {
        if (sections != null && sections.Contains(section))
        {
            return sections.Remove(section);
        }
        return false;
    }
}