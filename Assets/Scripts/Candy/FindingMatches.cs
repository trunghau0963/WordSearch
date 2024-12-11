using System.Collections;
using System.Collections.Generic;
using System.Linq;
// using Unity.VisualScripting;
using UnityEngine;

public class FindingMatches : MonoBehaviour
{
    public Board board;
    public List<GameObject> currentMatches = new();

    void Start()
    {
        board = FindFirstObjectByType<Board>();
    }

    public void FindAllMatches()
    {
        StartCoroutine(FindAllMatchesCo());
    }

    private List<GameObject> IsRowBomb(Dot dot1, Dot dot2, Dot dot3)
    {
        List<GameObject> currentDots = new();
        if (dot1.isRowBomb)
        {
            currentMatches.Union(GetRowPieces(dot1.row));
        }
        if (dot2.isRowBomb)
        {
            currentMatches.Union(GetRowPieces(dot2.row));
        }
        if (dot3.isRowBomb)
        {
            currentMatches.Union(GetRowPieces(dot3.row));
        }
        return currentDots;
    }

    private List<GameObject> IsColumnBomb(Dot dot1, Dot dot2, Dot dot3)
    {
        List<GameObject> currentDots = new();
        if (dot1.isColumnBomb)
        {
            currentMatches.Union(GetColumnPieces(dot1.column));
        }
        if (dot2.isColumnBomb)
        {
            currentMatches.Union(GetColumnPieces(dot2.column));
        }
        if (dot3.isColumnBomb)
        {
            currentMatches.Union(GetColumnPieces(dot3.column));
        }
        return currentDots;
    }

        private List<GameObject> IsAdjacentBomb(Dot dot1, Dot dot2, Dot dot3)
    {
        List<GameObject> currentDots = new();
        if (dot1.isAdjacentBomb)
        {
            currentMatches.Union(GetAdjacentPieces(dot1.column, dot1.row));
        }
        if (dot2.isAdjacentBomb)
        {
            currentMatches.Union(GetAdjacentPieces(dot2.column, dot2.row));
        }
        if (dot3.isAdjacentBomb)
        {
            currentMatches.Union(GetAdjacentPieces(dot3.column, dot3.row));
        }
        return currentDots;
    }

    private void AddToListAndMatch(GameObject dot){
        if(!currentMatches.Contains(dot)){
            currentMatches.Add(dot);
        }
        dot.GetComponent<Dot>().isMatched = true;
    }

    private void GetNearbyPieces(GameObject dot1, GameObject dot2, GameObject dot3)
    {
        AddToListAndMatch(dot1);
        AddToListAndMatch(dot2);
        AddToListAndMatch(dot3);
    }
    private IEnumerator FindAllMatchesCo()
    {
        yield return new WaitForSeconds(.2f);
        for (int i = 0; i < board.width; i++)
        {
            for (int j = 0; j < board.height; j++)
            {
                GameObject currentDot = board.allDots[i, j];
                if (currentDot != null)
                {
                    if (i > 0 && i < board.width - 1)
                    {
                        GameObject leftDot = board.allDots[i - 1, j];
                        GameObject rightDot = board.allDots[i + 1, j];
                        if (leftDot != null && rightDot != null)
                        {
                            if(currentDot.CompareTag(leftDot.tag) && currentDot.CompareTag(rightDot.tag))
                            {
                                currentMatches.Union(IsRowBomb(leftDot.GetComponent<Dot>(), currentDot.GetComponent<Dot>(), rightDot.GetComponent<Dot>()));

                                currentMatches.Union(IsColumnBomb(leftDot.GetComponent<Dot>(), currentDot.GetComponent<Dot>(), rightDot.GetComponent<Dot>()));

                                currentMatches.Union(IsAdjacentBomb(leftDot.GetComponent<Dot>(), currentDot.GetComponent<Dot>(), rightDot.GetComponent<Dot>()));

                                GetNearbyPieces(leftDot, currentDot, rightDot);
                            }
                        }
                    }
                    if (j > 0 && j < board.height - 1)
                    {
                        GameObject upDot = board.allDots[i, j + 1];
                        GameObject downDot = board.allDots[i, j - 1];
                        if (upDot != null && downDot != null)
                        {
                            if(currentDot.CompareTag(upDot.tag) && currentDot.CompareTag(downDot.tag))
                            {
                                currentMatches.Union(IsColumnBomb(upDot.GetComponent<Dot>(), currentDot.GetComponent<Dot>(), downDot.GetComponent<Dot>()));

                                currentMatches.Union(IsRowBomb(upDot.GetComponent<Dot>(), currentDot.GetComponent<Dot>(), downDot.GetComponent<Dot>()));

                                currentMatches.Union(IsAdjacentBomb(upDot.GetComponent<Dot>(), currentDot.GetComponent<Dot>(), downDot.GetComponent<Dot>()));

                                GetNearbyPieces(upDot, currentDot, downDot);
                            }
                        }
                    }
                }
            }
        }
    }

    public void MatchPiecesOfColor(string color)
    {
        for (int i = 0; i < board.width; i++)
        {
            for (int j = 0; j < board.height; j++)
            {
                // check if that piece exists
                if (board.allDots[i, j] != null)
                {
                    if (board.allDots[i, j].CompareTag(color))
                    {
                        // set that dot to be matched
                        board.allDots[i, j].GetComponent<Dot>().isMatched = true;
                    }
                }
            }
        }
    }

    List<GameObject> GetColumnPieces(int column){
        List<GameObject> dots = new();
        for (int i = 0; i < board.height; i++)
        {
            if(board.allDots[column, i] != null){
                Dot dot = board.allDots[column, i].GetComponent<Dot>();
                if(dot.isRowBomb){
                    dots.Union(GetRowPieces(i)).ToList();
                }
                dots.Add(board.allDots[column, i]);
                dot.isMatched = true;
            }
        }
        return dots;
    }

    List<GameObject> GetRowPieces(int row){
        List<GameObject> dots = new();
        for (int i = 0; i < board.width; i++)
        {
            if(board.allDots[i, row] != null){
                Dot dot = board.allDots[i, row].GetComponent<Dot>();
                if(dot.isColumnBomb){
                    dots.Union(GetColumnPieces(i)).ToList();
                }
                dots.Add(board.allDots[i, row]);
                dot.isMatched = true;
            }
        }
        return dots;
    }

    List<GameObject> GetAdjacentPieces(int column, int row)
    {
        List<GameObject> dots = new();
        for (int i = column - 1; i <= column + 1; i++)
        {
            for (int j = row - 1; j <= row + 1; j++)
            {
                if (i >= 0 && i < board.width && j >= 0 && j < board.height)
                {
                    if(board.allDots[i, j] != null)
                    {
                        Dot dot = board.allDots[i, j].GetComponent<Dot>();
                        dots.Add(board.allDots[i, j]);
                        dot.isMatched = true;
                    }
                }
            }
        }
        return dots;
    }

    // Update is called once per frame

    public void CheckBombs()
    {
        // is the bomb spawned?
        if (board.currentDot != null)
        {
            if (board.currentDot.isMatched)
            {
                board.currentDot.isMatched = false;
                if((board.currentDot.swipeAngle > -45 && board.currentDot.swipeAngle <= 45 )|| (board.currentDot.swipeAngle < -135 || board.currentDot.swipeAngle >= 135))
                {
                    board.currentDot.MakeRowBomb();
                }
                else
                {
                    board.currentDot.MakeColumnBomb();
                }
            }
            else if(board.currentDot.otherDot != null)
            {
                Dot otherDot = board.currentDot.otherDot.GetComponent<Dot>();
                if(otherDot.isMatched)
                {
                    otherDot.isMatched = false;
                    if((board.currentDot.swipeAngle > -45 && board.currentDot.swipeAngle <= 45 )|| (board.currentDot.swipeAngle < -135 || board.currentDot.swipeAngle >= 135))
                    {
                        otherDot.MakeRowBomb();
                    }
                    else
                    {
                        otherDot.MakeColumnBomb();
                    }
                }
            }
        }
    }
}
