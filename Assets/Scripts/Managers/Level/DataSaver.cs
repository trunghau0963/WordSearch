using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class DataSaver : MonoBehaviour
{
    public static int ReadCategoryCurrentIndex(string name)
    {
        var value = -1;
        if (PlayerPrefs.HasKey(name))
        {
            value = PlayerPrefs.GetInt(name);
        }
        return value;
    }

    public static void SaveCategoryCurrentIndex(string name, int index)
    {
        PlayerPrefs.SetInt(name, index);
        PlayerPrefs.Save();
    }
    public static int ReadSectionCurrentIndex(string name)
    {
        var value = -1;
        if (PlayerPrefs.HasKey(name))
        {
            value = PlayerPrefs.GetInt(name);
        }
        return value;
    }

    public static void SaveSectionCurrentIndex(string name, int index)
    {
        PlayerPrefs.SetInt(name, index);
        PlayerPrefs.Save();
    }

    public static int ReadLevelData(string name)
    {
        var value = -1;
        if (PlayerPrefs.HasKey(name))
        {
            value = PlayerPrefs.GetInt(name);
        }
        return value;
    }

    public static void SaveLevelData(string name, int index)
    {
        PlayerPrefs.SetInt(name, index);
        PlayerPrefs.Save();
    }

    public static int ReadBoardData(string name)
    {
        var value = -1;
        if (PlayerPrefs.HasKey(name))
        {
            value = PlayerPrefs.GetInt(name);
        }
        return value;
    }

    public static void SaveBoardData(string name, int index)
    {
        PlayerPrefs.SetInt(name, index);
        PlayerPrefs.Save();
    }

    public static void InitGameData(Data_PlayerPrefs data)
    {
        foreach (var category in data.categories)
        {
            PlayerPrefs.SetInt(category.GetCategoryName(), -1);
            foreach (var section in category.GetSections())
            {
                string sectionName = category.GetCategoryName() + "-" + section.GetSectionName();
                PlayerPrefs.SetInt(sectionName, -1);
                foreach (var level in section.GetLevels())
                {
                    level.SetIsLock(true);
                    level.SetIsCompleted(false);
                    level.SetScore(0);
                    string levelName = section.GetSectionName() + "-" + level.GetLevelName();
                    PlayerPrefs.SetInt(levelName, -1);
                    foreach (var boardList in level.GetBoardList())
                    {
                        boardList.SetIsCompleted(false);
                        string boardDataName = level.GetLevelName() + "-" + boardList.GetBoardName();
                        PlayerPrefs.SetInt(boardDataName, -1);
                    }
                }
            }
        }

        //unlock first category and first section and first level and first board and first boardData
        PlayerPrefs.SetInt(data.categories[0].GetCategoryName(), 0);
        PlayerPrefs.SetInt(data.categories[0].GetCategoryName() + "-" + data.categories[0].GetSections()[0].GetSectionName(), 0);
        PlayerPrefs.SetInt(data.categories[0].GetSections()[0].GetSectionName() + "-" + data.categories[0].GetSections()[0].GetLevels()[0].GetLevelName(), 0);
        PlayerPrefs.SetInt(data.categories[0].GetSections()[0].GetLevels()[0].GetLevelName() + "-" + data.categories[0].GetSections()[0].GetLevels()[0].GetBoardList()[0].GetBoardName(), 0);
        PlayerPrefs.Save();
    }
}