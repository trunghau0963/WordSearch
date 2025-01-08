using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
[CreateAssetMenu]
public class BoardData : ScriptableObject
{
    [System.Serializable]
    public class SearchingWord
    {
        public string Word;
        public int Row;
        public int Column;
        public int Direction;

        public int timeInSeconds;
    }

    [System.Serializable]
    public class BoardRow
    {
        public int Size;
        public string[] Row;

        public BoardRow() { }

        public BoardRow(int size)
        {
            CreateRow(size);
        }

        public void CreateRow(int size)
        {
            Size = size;
            Row = new string[size];
            ClearRow();
        }

        public void ClearRow()
        {
            for (int i = 0; i < Size; i++)
            {
                Row[i] = "";
            }
        }
    }
    public string Name;
    public bool isCompleted = false;
    public int index;
    public float timeInSeconds;
    public int Columns = 0;
    public int Rows = 0;
    public BoardRow[] Boards;
    public List<SearchingWord> SearchWords = new();

    // Constructor

    public BoardData()
    {
        this.Name = "";
        this.isCompleted = false;
        this.Columns = 0;
        this.Columns = 0;
        this.Rows = 0;
        this.Boards = new BoardRow[Columns];
    }
    public BoardData(string Name, bool isCompleted, int idx, int columns, int rows, BoardRow[] boards)
    {
        this.Name = Name;
        this.isCompleted = isCompleted;
        this.index = idx;
        this.Columns = columns;
        this.Rows = rows;
        this.Boards = boards;
    }

    public string GetBoardName()
    {
        return Name;
    }

    public void SetBoardName(string value)
    {
        Name = value;
    }

    // Getter

    public bool GetIsCompleted()
    {
        return isCompleted;
    }

    public void SetIsCompleted(bool value)
    {
        isCompleted = value;
    }
    public void ClearWithEmptyString()
    {
        for (int i = 0; i < Rows; i++)
        {
            Boards[i].ClearRow();
        }
    }

    public void CreateNewBoard()
    {
        Boards = new BoardRow[Columns];
        for (int i = 0; i < Columns; i++)
        {
            Boards[i] = new BoardRow(Rows);
        }
    }
    public void CreateNewBoard(int rows, int columns)
    {
        Rows = rows;
        Columns = columns;
        Boards = new BoardRow[Columns];
        for (int i = 0; i < Columns; i++)
        {
            Boards[i] = new BoardRow(Rows);
        }
    }

}
