using System;
using System.Collections;
using System.Collections.Generic;
// using UnityEditor.SearchService;
using UnityEngine;
using UnityEngine.SceneManagement;

public class WordChecker : MonoBehaviour
{
    public GameData currentGameData;

    private GameDataSelector gameDataSelector;

    [SerializeField] private GameDataSave dataSave;
    private string _word;

    private int _assignedPoints = 0;
    private int _completedWords = 0;
    private Ray _rayUp, _rayDown, _rayLeft, _rayRight;
    private Ray _rayDiagonalUpLeft, _rayDiagonalUpRight, _rayDiagonalDownLeft, _rayDiagonalDownRight;
    private Ray _currentRay = new Ray();
    private Vector3 _rayStartPositon;

    private List<int> _correctSquaresList = new();

    private void OnEnable()
    {
        GameEvents.OnCheckSquare += SquareSelected;
        GameEvents.OnClearSelection += ClearSelection;
        GameEvents.OnBoardComplete += () => _completedWords++;
        GameEvents.OnLoadNextBoard += LoadNextBoard;
    }

    private void OnDisable()
    {
        GameEvents.OnCheckSquare -= SquareSelected;
        GameEvents.OnClearSelection -= ClearSelection;
        GameEvents.OnBoardComplete -= () => _completedWords++;
        GameEvents.OnLoadNextBoard -= LoadNextBoard;
    }

    private void LoadNextBoard()
    {
        SceneManager.LoadScene("WordSearchGameScene");
    }

    void Start()
    {
        _assignedPoints = 0;
        _completedWords = 0;
        gameDataSelector = FindAnyObjectByType<GameDataSelector>();
    }

    void Update()
    {
        if (_assignedPoints > 0 && Application.isEditor)
        {
            Debug.DrawRay(_rayUp.origin, _rayUp.direction * 4);
            Debug.DrawRay(_rayDown.origin, _rayDown.direction * 4);
            Debug.DrawRay(_rayLeft.origin, _rayLeft.direction * 4);
            Debug.DrawRay(_rayRight.origin, _rayRight.direction * 4);
            Debug.DrawRay(_rayDiagonalUpLeft.origin, _rayDiagonalUpLeft.direction * 4);
            Debug.DrawRay(_rayDiagonalUpRight.origin, _rayDiagonalUpRight.direction * 4);
            Debug.DrawRay(_rayDiagonalDownLeft.origin, _rayDiagonalDownLeft.direction * 4);
            Debug.DrawRay(_rayDiagonalDownRight.origin, _rayDiagonalDownRight.direction * 4);
        }
    }

    public void SquareSelected(string letter, Vector3 position, int index)
    {
        if (_assignedPoints == 0)
        {
            _rayStartPositon = position;
            _correctSquaresList.Add(index);
            _word += letter;

            _rayUp = new Ray(new Vector2(position.x, position.y), new Vector2(0, 1));
            _rayDown = new Ray(new Vector2(position.x, position.y), new Vector2(0, -1));
            _rayLeft = new Ray(new Vector2(position.x, position.y), new Vector2(-1, 0));
            _rayRight = new Ray(new Vector2(position.x, position.y), new Vector2(1, 0));
            _rayDiagonalUpLeft = new Ray(new Vector2(position.x, position.y), new Vector2(-1, 1));
            _rayDiagonalUpRight = new Ray(new Vector2(position.x, position.y), new Vector2(1, 1));
            _rayDiagonalDownLeft = new Ray(new Vector2(position.x, position.y), new Vector2(-1, -1));
            _rayDiagonalDownRight = new Ray(new Vector2(position.x, position.y), new Vector2(1, -1));
        }
        else if (_assignedPoints == 1)
        {
            _correctSquaresList.Add(index);
            _currentRay = SelectRay(_rayStartPositon, position);
            GameEvents.SelectSquareMethod(position);
            _word += letter;
            CheckWord();
        }
        else
        {
            if (IsPointOnTheRay(_currentRay, position))
            {
                _correctSquaresList.Add(index);
                GameEvents.SelectSquareMethod(position);
                _word += letter;
                CheckWord();
            }
            else
            {
                _word = string.Empty;
                _correctSquaresList.Clear();
                _assignedPoints = 0;
                return;
            }
        }

        _assignedPoints++;
    }

    private void CheckWord()
    {
        foreach (var word in currentGameData.selectedBoardData.SearchWords)
        {
            if (word.Word == _word)
            {
                GameEvents.CorrectWordMethod(_word, _correctSquaresList);
                _completedWords++;
                _word = string.Empty;
                _correctSquaresList.Clear();
                CheckBoardComplete();
                return;
            }
        }
    }

    private bool IsPointOnTheRay(Ray currentRay, Vector3 point)
    {
        var hits = Physics.RaycastAll(currentRay, 100f);
        foreach (var hit in hits)
        {
            if (hit.transform.position == point)
            {
                return true;
            }
        }
        return false;
    }

    private Ray SelectRay(Vector2 firstPosion, Vector2 secondPosition)
    {

        var direction = (secondPosition - firstPosion).normalized;
        float tolerance = 0.01f;

        if (Math.Abs(direction.x) < tolerance && Math.Abs(direction.y - 1f) < tolerance)
        {
            return _rayUp;
        }

        if (Math.Abs(direction.x) < tolerance && Math.Abs(direction.y + 1f) < tolerance)
        {
            return _rayDown;
        }

        if (Math.Abs(direction.x + 1f) < tolerance && Math.Abs(direction.y) < tolerance)
        {
            return _rayLeft;
        }

        if (Math.Abs(direction.x - 1f) < tolerance && Math.Abs(direction.y) < tolerance)
        {
            return _rayRight;
        }

        if (direction.x > 0 && direction.y > 0)
        {
            return _rayDiagonalUpRight;
        }

        if (direction.x > 0 && direction.y < 0)
        {
            return _rayDiagonalDownRight;
        }

        if (direction.x < 0 && direction.y > 0)
        {
            return _rayDiagonalUpLeft;
        }

        if (direction.x < 0 && direction.y < 0)
        {
            return _rayDiagonalDownLeft;
        }
        return _rayUp;
    }

    private void ClearSelection()
    {
        _assignedPoints = 0;
        _correctSquaresList.Clear();
        _word = string.Empty;
    }

    private void CheckBoardComplete()
    {
        bool loadNextBoard = false;
        if (currentGameData.selectedBoardData.SearchWords.Count == _completedWords)
        {
            Section_PlayerPrefs section = currentGameData.selectedSection;
            Level_PlayerPrefs level = currentGameData.selectedLevel;

            int totalBoardCount = level.boardList.Count;

            if (level == null)
            {
                return;
            }
            int currentBoardIndex = 0;
            for (int i = 0; i < totalBoardCount; i++)
            {
                BoardData board = level.boardList[i];
                if (currentGameData.selectedBoardData == board)
                {
                    currentBoardIndex = i;
                    int nextBoardIndex = i + 1;
                    int score = (int)(level.boardList[currentBoardIndex].timeInSeconds * 10);
                    level.boardList[currentBoardIndex].isCompleted = true;
                    int currentScore = level.GetScore();
                    level.SetScore(currentScore + score);
                    if (nextBoardIndex < totalBoardCount)
                    {
                        currentGameData.selectedBoardData = level.boardList[nextBoardIndex];
                        level.boardList[nextBoardIndex].index = nextBoardIndex;
                        loadNextBoard = true;
                    }
                    else
                    {
                        GameEvents.ShowPopupMethod(true);
                        GameEvents.SaveWordDictionaryMethod();
                    }
                    break;
                }
            }

            if ((currentBoardIndex + 1) >= totalBoardCount)
            {
                int currentLevelIndex = 0;
                for (int i = 0; i < section.GetLevels().Count; i++)
                {
                    if (section.GetLevel(i).Name == currentGameData.selectedLevel.Name)
                    {
                        currentLevelIndex = i;
                        break;
                    }
                }

                if ((currentLevelIndex + 1) < section.GetLevels().Count)
                {
                    Level_PlayerPrefs currentLevel = section.GetLevel(currentLevelIndex);
                    currentLevel.SetIsCompleted(true);
                    Level_PlayerPrefs nextLevel = section.GetLevel(currentLevelIndex + 1);
                    nextLevel.SetIsLock(false);
                    loadNextBoard = true;
                }
                else
                {
                    Debug.Log("Section complete and Show popup event"); 
                    GameEvents.ShowPopupMethod(true);
                    GameEvents.SaveWordDictionaryMethod();
                }
            }
            else
            {
                Debug.Log("Board complete");
                GameEvents.BoardCompleteMethod();
                GameEvents.SaveWordDictionaryMethod();
                GameEvents.ShowPopupMethod(false);
            }
            if (loadNextBoard)
            {
                GameEvents.SaveWordDictionaryMethod();
                GameEvents.OnUnlockNextBoardMethod();
            }
        }
    }
}
