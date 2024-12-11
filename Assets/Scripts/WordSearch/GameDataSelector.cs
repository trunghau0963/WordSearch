using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// dung de chon boardata phu hop voi category, section, level
/// </summary>
public class GameDataSelector : MonoBehaviour
{
    public GameData currentGameData;

    // public GameDataSave data;

    // public SavingFile savingFile;



    void Awake()
    {
        // savingFile = FindAnyObjectByType<SavingFile>();
        // data = savingFile.LoadData();
        SelectSequentalBoardData();
    }

    private void SelectSequentalBoardData()
    {

        Level_PlayerPrefs level = currentGameData.selectedLevel;
        int totalBoardCount = level.boardList.Count;

        for (int i = 0; i < level.boardList.Count; i++)
        {
            BoardData board = level.boardList[i];
            if (!currentGameData.selectedLevel.GetIsCompleted())
            {
                // Skip the board if it is completed
                if (board.isCompleted)
                {
                    continue;
                }
                // if (!board.isLock)
                // {
                if (i < totalBoardCount)
                {
                    // currentGameData.selectedBoardName = board.Name;
                    currentGameData.selectedBoardData = board;
                    return;
                }
                // }
            }
            else
            {
                // if the level is completed, select the next board
                if (i < totalBoardCount)
                {
                    // currentGameData.selectedBoardName = board.Name;
                    currentGameData.selectedBoardData = board;
                    return;
                }
            }
        }
        return;
    }
}

