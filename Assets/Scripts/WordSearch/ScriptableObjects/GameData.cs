using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
[CreateAssetMenu]
public class GameData : ScriptableObject
{
    public BoardData selectedBoardData;
    public Category_PlayerPrefs selectedCategory;
    public Section_PlayerPrefs selectedSection;
    public Level_PlayerPrefs selectedLevel;
}
