using System.Collections.Generic;
using System.IO;
using UnityEngine;


[System.Serializable]
[CreateAssetMenu]
public class Section_PlayerPrefs : ScriptableObject
{
    public string SectionName;
    public bool isCompleted = false;
    public bool isLock = true;
    public List<Level_PlayerPrefs> levels;

    // Constructor

    public Section_PlayerPrefs()
    {
        this.SectionName = "";
        // this.isCompleted = false;
        // this.isLock = true;
        this.levels = new List<Level_PlayerPrefs>();
    }
    public Section_PlayerPrefs(string SectionName, bool isCompleted, bool isLock, List<Level_PlayerPrefs> levels)
    {
        this.SectionName = SectionName;
        this.isCompleted = isCompleted;
        this.isLock = isLock;
        this.levels = levels;
    }

    // Getter and Setter for SectionName
    public string GetSectionName()
    {
        return SectionName;
    }

    public void SetSectionName(string value)
    {
        SectionName = value;
    }

    // Getter and Setter for IsCompleted

    public bool GetIsCompleted()
    {
        return isCompleted;
    }

    public void SetIsCompleted(bool value)
    {
        isCompleted = value;
    }

    // Getter

    public bool GetIsLock()
    {
        return isLock;
    }


    public void SetIsLock(bool value)
    {
        isLock = value;
    }

    // Getter and Setter for Levels

    public List<Level_PlayerPrefs> GetLevels()
    {
        return levels;
    }

    public Level_PlayerPrefs GetLevel(int index)
    {
        if (levels != null && index >= 0 && index < levels.Count)
        {
            return levels[index];
        }
        return null;
    }

    public void SetLevels(List<Level_PlayerPrefs> value)
    {
        levels = value;
    }

    // Add a Level

    public void AddLevel(Level_PlayerPrefs level)
    {
        if (levels == null)
        {
            levels = new List<Level_PlayerPrefs>();
        }
        levels.Add(level);
    }

    // Remove a Level

    public bool RemoveLevel(Level_PlayerPrefs level)
    {
        if (levels != null && levels.Contains(level))
        {
            return levels.Remove(level);
        }
        return false;
    }

}