using System.Collections.Generic;
using System.IO;
using UnityEngine;

[System.Serializable]
public class ScoreData
{
    public int score;
}
[System.Serializable]
public class GameDataSave
{
    public List<Category> DataSet;
}

[System.Serializable]
// [CreateAssetMenu]
public class Category
{
    public string CategoryName;
    public bool isLock = true;
    public List<Section> Sections;
}

[System.Serializable]
public class Section
{
    public string SectionName;
    public bool isCompleted = false;
    public bool isLock = true;
    public List<Level> Levels;
}

[System.Serializable]
// [CreateAssetMenu]
public class Level
{
    public string Name;
    public bool isLock = true;
    public bool isCompleted = false;
    public int score;
    public List<BoardList> Boards;
}

[System.Serializable]
public class BoardList
{

    public string Name;
    // public bool isLock = true;
    public bool isCompleted = false;
    public int index;
    public BoardData boardData;
}

public class SavingFile : MonoBehaviour
{
    public GameDataSave data;

    public string nameFile;

    public ScoreData score;

    string fileScore = "score.json";

    void Start()
    {
        // data = ScriptableObject.CreateInstance<GameData>();
        // data.Initialize();
        data = LoadData(nameFile);
        score = LoadScoreFromJson();
        // SaveData();
    }

    void Update()
    {
        // if (Input.GetMouseButtonDown(0))
        // {
        //     SaveData(nameFile);
        //     SaveScoreToJson();
        // }
    }

    void OnApplicationQuit()
    {
        // print("Application ending after " + Time.time + " seconds");
        SaveData(nameFile);
        SaveScoreToJson();
    }



    public void Save(string categoryName, string sectionName, string levelName, string boardName, int score, int time, bool isLock)
    {
        Category category = data.DataSet.Find(c => c.CategoryName == categoryName);
        if (category == null)
        {
            category = new Category();
            category.CategoryName = categoryName;
            category.Sections = new List<Section>();
            data.DataSet.Add(category);
        }

        Section section = category.Sections.Find(s => s.SectionName == sectionName);
        if (section == null)
        {
            section = new Section();
            section.SectionName = sectionName;
            section.Levels = new List<Level>();
            category.Sections.Add(section);
        }

        Level level = section.Levels.Find(l => l.Name == levelName);
        if (level == null)
        {
            level = new Level();
            level.Name = levelName;
            level.Boards = new List<BoardList>();
            section.Levels.Add(level);
        }

        BoardList board = level.Boards.Find(b => b.Name == boardName);
        if (board == null)
        {
            board = new BoardList();
            board.Name = boardName;
            // board.isLock = isLock;
            level.Boards.Add(board);
        }

        Debug.Log("Data Saved");
    }

    public void GetScore(string categoryName, string sectionName, string levelName, string boardName)
    {
        Category category = data.DataSet.Find(c => c.CategoryName == categoryName);
        if (category == null)
        {
            Debug.Log("Category not found");
            return;
        }

        Section section = category.Sections.Find(s => s.SectionName == sectionName);
        if (section == null)
        {
            Debug.Log("Section not found");
            return;
        }

        Level level = section.Levels.Find(l => l.Name == levelName);
        if (level == null)
        {
            Debug.Log("Level not found");
            return;
        }

        BoardList board = level.Boards.Find(b => b.Name == boardName);
        if (board == null)
        {
            Debug.Log("Board not found");
            return;
        }

    }

    public Category LoadCategory(string categoryName)
    {
        Category category = data.DataSet.Find(c => c.CategoryName == categoryName);
        if (category == null)
        {
            Debug.Log("Category not found");
            return null;
        }
        return category;
    }

    public Section LoadSection(string categoryName, string sectionName)
    {
        Category category = LoadCategory(categoryName);
        if (category == null)
        {
            Debug.Log("Category not found");
            return null;
        }

        Section section = category.Sections.Find(s => s.SectionName == sectionName);
        if (section == null)
        {
            Debug.Log("Section not found");
            return null;
        }
        return section;
    }

    public Level LoadLevel(string categoryName, string sectionName, string levelName)
    {
        Section section = LoadSection(categoryName, sectionName);
        if (section == null)
        {
            Debug.Log("Section not found");
            return null;
        }

        Level level = section.Levels.Find(l => l.Name == levelName);
        if (level == null)
        {
            Debug.Log("Level not found");
            return null;
        }
        return level;
    }

    public BoardList LoadBoardList(string categoryName, string sectionName, string levelName, string boardName)
    {
        Level level = LoadLevel(categoryName, sectionName, levelName);
        if (level == null)
        {
            Debug.Log("Level not found");
            return null;
        }

        BoardList board = level.Boards.Find(b => b.Name == boardName);
        if (board == null)
        {
            Debug.Log("Board not found");
            return null;
        }
        return board;
    }

    public void SaveCategoryData(Category category)
    {
        Category categoryData = data.DataSet.Find(c => c.CategoryName == category.CategoryName);
        if (categoryData == null)
        {
            data.DataSet.Add(category);
        }
        else
        {
            categoryData = category;
        }
        SaveData(nameFile);
    }

    public void SaveSectionData(string categoryName, Section section)
    {
        Category category = LoadCategory(categoryName);
        if (category == null)
        {
            Debug.Log("Category not found");
            return;
        }

        Section sectionData = category.Sections.Find(s => s.SectionName == section.SectionName);
        if (sectionData == null)
        {
            category.Sections.Add(section);
        }
        else
        {
            sectionData = section;
        }
        SaveData(nameFile);
    }

    public void SaveLevelData(string categoryName, string sectionName, Level level)
    {
        Section section = LoadSection(categoryName, sectionName);
        if (section == null)
        {
            Debug.Log("Section not found");
            return;
        }

        Level levelData = section.Levels.Find(l => l.Name == level.Name);
        if (levelData == null)
        {
            section.Levels.Add(level);
        }
        else
        {
            levelData = level;
        }
        SaveData(nameFile);
    }

    public void SaveBoardData(string categoryName, string sectionName, string levelName, BoardList board)
    {
        Debug.Log("SaveBoardData....");
        Level level = LoadLevel(categoryName, sectionName, levelName);
        if (level == null)
        {
            Debug.Log("Level not found");
            return;
        }

        BoardList boardData = level.Boards.Find(b => b.Name == board.Name);
        if (boardData == null)
        {
            level.Boards.Add(board);
        }
        else
        {
            boardData = board;
        }
        SaveData(nameFile);
    }

    public GameDataSave LoadData()
    {
        string file = nameFile + ".json";
        string filePath = Path.Combine(Application.dataPath,"Resources", file);
        GameDataSave data = new GameDataSave();

        if (File.Exists(filePath))
        {
            string json = File.ReadAllText(filePath);
            data = JsonUtility.FromJson<GameDataSave>(json);
            // Debug.Log("Data Loaded");
        }
        else
        {
            // GameDataSave data = new GameDataSave();
            // data.DataSet = new List<Category>();
            Debug.Log("No Data Found");
        }

        Debug.Log("Data Saved to: " + filePath);

        return data;
    }

    public GameDataSave LoadData(string nameFile)
    {
        string file = nameFile + ".json";
        string filePath = Path.Combine(Application.dataPath,"Resources", file);
        GameDataSave data = new GameDataSave();

        if (File.Exists(filePath))
        {
            string json = File.ReadAllText(filePath);
            data = JsonUtility.FromJson<GameDataSave>(json);
            // Debug.Log("Data Loaded");
        }
        else
        {
            // GameDataSave data = new GameDataSave();
            // data.DataSet = new List<Category>();
            Debug.Log("No Data Found");
        }

        Debug.Log("Data Loaded from: " + filePath);

        return data;
    }


    public void SaveData()
    {
        string file = nameFile + ".json";
        string filePath = Path.Combine(Application.dataPath, "Resources", file);

        string json = JsonUtility.ToJson(data);  // true for pretty-printing the JSON
        File.WriteAllText(filePath, json);

        Debug.Log("Data Saved to: " + filePath);
    }

    public void SaveData(string nameFile)
    {
        string file = nameFile + ".json";
        string filePath = Path.Combine(Application.dataPath,"Resources", file);

        string json = JsonUtility.ToJson(data);  // true for pretty-printing the JSON
        File.WriteAllText(filePath, json);

        Debug.Log("Data Saved to: " + filePath);
    }

    public ScoreData LoadScoreFromJson()
    {
        string filePath = Path.Combine(Application.dataPath,"Resources", fileScore);
        if (File.Exists(filePath))
        {
            try
            {
                string json = File.ReadAllText(filePath);
                score = JsonUtility.FromJson<ScoreData>(json);
            }
            catch (IOException e)
            {
                Debug.LogError("Error reading score file: " + e.Message);
            }
        }
        else
        {
            Debug.LogWarning("Score file not found, initializing score to 0.");
            score.score = 0;
        }
        return new ScoreData { score = score.score };
    }

    public void SaveScoreToJson()
    {
        int totalScore = 0;
        string filePath = Path.Combine(Application.dataPath,"Resources", fileScore);
        foreach (Category category in data.DataSet)
        {
            foreach (Section section in category.Sections)
            {
                foreach (Level level in section.Levels)
                {
                    totalScore += level.score;
                }
            }
        }
        score.score = totalScore;
        ScoreData scoreData = new ScoreData { score = totalScore };
        string json = JsonUtility.ToJson(scoreData);
        try
        {
            File.WriteAllText(filePath, json);
            Debug.Log("Score Saved on path " + filePath);
        }
        catch (IOException e)
        {
            Debug.LogError("Error writing score file: " + e.Message);
        }
    }
}