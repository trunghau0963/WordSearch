using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WordsGrid : MonoBehaviour
{
    public GameObject gridSquarePrefab;
    public AlphabetData alphabetData;

    public float squareOffset = 0.0f;
    public float topPosition;

    [Header("Animation Settings")]
    [Tooltip("Time for each square to pop in")]
    public float spawnAnimDuration = 0.25f;
    [Tooltip("Delay between each square spawn")]
    public float spawnStaggerDelay = 0.02f;
    [Tooltip("Scale multiplier for grid (larger = bigger grid)")]
    public float gridScaleMultiplier = 5.0f;

    [Header("Position Settings")]
    [Tooltip("Horizontal offset for grid center (negative = left)")]
    public float gridHorizontalOffset = -3.0f;
    [Tooltip("Fraction of screen width the grid is allowed to use (0-1)")]
    public float gridWidthFraction = 0.66f;
    [Tooltip("Fraction of screen height the grid is allowed to use (0-1)")]
    public float gridHeightFraction = 0.9f;

    private List<GameObject> _squareList = new List<GameObject>();
    private Vector3 _targetSquareScale;

    void Start()
    {
        var board = GameSessionData.CurrentBoard;
        if (board == null)
        {
            Debug.LogError("[WordsGrid] GameSessionData.CurrentBoard is null!");
            return;
        }
        _targetSquareScale = GetSquareScale(new Vector3(1.5f * gridScaleMultiplier, 1.5f * gridScaleMultiplier, 0.1f));
        SpawnGridSquares();
        SetSquarePosition();
        StartCoroutine(AnimateGridAppearance());
    }

    private void SetSquarePosition()
    {
        var squareRect = _squareList[0].GetComponent<SpriteRenderer>().sprite.rect;

        // Use the pre-computed target scale (not the current zero scale)
        var offset = new Vector2
        {
            x = (squareRect.width * _targetSquareScale.x + squareOffset) * 0.01f,
            y = (squareRect.height * _targetSquareScale.y + squareOffset) * 0.01f
        };

        var startPosition = GetFirstSquarePosition();

        int columnNumber = 0;
        int rowNumber = 0;

        foreach (var square in _squareList)
        {
            if (rowNumber + 1 > GameSessionData.CurrentBoard.Rows)
            {
                columnNumber++;
                rowNumber = 0;
            }
            var positionX = startPosition.x + offset.x * columnNumber;
            var positionY = startPosition.y - offset.y * rowNumber;

            square.GetComponent<Transform>().position = new Vector2(positionX, positionY);
            rowNumber++;
        }
    }

    private Vector2 GetFirstSquarePosition()
    {
        var startPosition = new Vector2(gridHorizontalOffset, transform.position.y);
        var squareRect = _squareList[0].GetComponent<SpriteRenderer>().sprite.rect;

        // Use pre-computed target scale
        var squareSize = new Vector2
        {
            x = squareRect.width * _targetSquareScale.x,
            y = squareRect.height * _targetSquareScale.y
        };

        var midWidthPosition = (((GameSessionData.CurrentBoard.Columns - 1) * squareSize.x) / 2) * 0.01f;
        var midWidthHeight = (((GameSessionData.CurrentBoard.Rows - 1) * squareSize.y) / 2) * 0.01f;

        startPosition.x += (midWidthPosition != 0) ? -midWidthPosition : midWidthPosition;
        startPosition.y += midWidthHeight;

        return startPosition;
    }

    private void SpawnGridSquares()
    {
        var board = GameSessionData.CurrentBoard;
        if (board != null)
        {
            foreach (var square in board.Boards)
            {
                foreach (var squareLetter in square.Row)
                {
                    var normalLetter = alphabetData.AlphabetNormal.Find(x => x.Letter == squareLetter);
                    var selectedLetter = alphabetData.AlphabetWrong.Find(x => x.Letter == squareLetter);
                    var correctLetter = alphabetData.AlphabetHighlighted.Find(x => x.Letter == squareLetter);

                    if (normalLetter.Image == null || selectedLetter.Image == null)
                    {
                        Debug.LogError("Missing image for letter: " + squareLetter);

#if UNITY_EDITOR
                        if (UnityEditor.EditorApplication.isPlaying)
                        {
                            UnityEditor.EditorApplication.isPlaying = false;
                        }
#endif
                    }
                    else
                    {
                        var go = Instantiate(gridSquarePrefab);
                        go.GetComponent<GridSquare>().SetSprite(normalLetter, selectedLetter, correctLetter);
                        go.transform.SetParent(transform);
                        go.transform.position = Vector3.zero;
                        go.GetComponent<GridSquare>().SetIndex(_squareList.Count);

                        // Start at zero scale for entrance animation
                        go.transform.localScale = Vector3.zero;

                        _squareList.Add(go);
                    }
                }
            }
        }
    }

    private IEnumerator AnimateGridAppearance()
    {
        int rows = GameSessionData.CurrentBoard.Rows;

        for (int i = 0; i < _squareList.Count; i++)
        {
            var square = _squareList[i];
            int col = i / rows;
            int row = i % rows;

            float diagonalDelay = (col + row) * spawnStaggerDelay;

            LeanTween.scale(square, _targetSquareScale, spawnAnimDuration)
                .setDelay(diagonalDelay)
                .setEase(LeanTweenType.easeOutBack);
        }

        yield return new WaitForSeconds((_squareList.Count * spawnStaggerDelay) + spawnAnimDuration);
    }

    private Vector3 GetSquareScale(Vector3 defaultScale)
    {
        var finalScale = defaultScale;
        var adjustment = 0.01f;

        while (ShouldScaleDown(finalScale))
        {
            finalScale.x -= adjustment;
            finalScale.y -= adjustment;

            if (finalScale.x <= 0 || finalScale.y <= 0)
            {
                finalScale.x = adjustment;
                finalScale.y = adjustment;
                return finalScale;
            }
        }
        return finalScale;
    }

    private bool ShouldScaleDown(Vector3 targetScale)
    {
        var squareRect = gridSquarePrefab.GetComponent<SpriteRenderer>().sprite.rect;

        var squareSize = new Vector2
        {
            x = (squareRect.width * targetScale.x) + squareOffset,
            y = (squareRect.height * targetScale.y) + squareOffset
        };

        var midWidthPosition = ((GameSessionData.CurrentBoard.Columns * squareSize.x) / 2) * 0.01f;
        var midHeightPosition = ((GameSessionData.CurrentBoard.Rows * squareSize.y) / 2) * 0.01f;

        // Allow grid to use only a fraction of the screen width/height
        float allowedHalfWidth = GetHalfScreenWidth() * gridWidthFraction;
        float allowedHalfHeight = Camera.main.orthographicSize * gridHeightFraction;
        return midWidthPosition > allowedHalfWidth || midHeightPosition > allowedHalfHeight;
    }

    private float GetHalfScreenWidth()
    {
        float height = Camera.main.orthographicSize * 2;
        float width = 1.7f * height * Screen.width / Screen.height;
        return width / 2;
    }
}
