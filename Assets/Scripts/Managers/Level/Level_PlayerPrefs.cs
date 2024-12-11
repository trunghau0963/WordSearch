using System.Collections.Generic;
using System.IO;
using UnityEngine;


[System.Serializable]
[CreateAssetMenu]
public class Level_PlayerPrefs : ScriptableObject
{
    public string Name;

    public bool isLock = true;
    public bool isCompleted = false;
    public int score;
    public List<BoardData> boardList;

    // Constructor
    public Level_PlayerPrefs()
    {
        this.Name = "";
        // this.isLock = true;
        // this.isCompleted = false;
        this.score = 0;
        this.boardList = new List<BoardData>();
    }

    public Level_PlayerPrefs(string Name, bool isLock, bool isCompleted, int score, List<BoardData> boardList)
    {
        this.Name = Name;
        this.isLock = isLock;
        this.isCompleted = isCompleted;
        this.score = score;
        this.boardList = boardList;
    }

    // Getter and Setter for Name

    public string GetLevelName()
    {
        return Name;
    }

    public void SetLevelName(string value)
    {
        Name = value;
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

    // Getter and Setter for IsCompleted

    public bool GetIsCompleted()
    {
        return isCompleted;
    }

    public void SetIsCompleted(bool value)
    {
        isCompleted = value;
    }

    // Getter and Setter for Score

    public int GetScore()
    {
        return score;
    }

    public void SetScore(int value)
    {
        score = value;
    }

    // Getter and Setter for BoardList

    public List<BoardData> GetBoardList()
    {
        return boardList;
    }

    public void SetBoardList(List<BoardData> value)
    {
        boardList = value;
    }

    // Add a BoardList

    public void AddBoardList(BoardData boardList)
    {
        if (this.boardList == null)
        {
            this.boardList = new List<BoardData>();
        }
        this.boardList.Add(boardList);
    }

    // Remove a BoardList

    public bool RemoveBoardList(BoardData boardList)
    {
        if (this.boardList != null && this.boardList.Contains(boardList))
        {
            return this.boardList.Remove(boardList);
        }
        return false;
    }
}

