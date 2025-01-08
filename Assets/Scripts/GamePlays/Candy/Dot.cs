using System.Collections;
using System.Collections.Generic;
using Unity.Burst.CompilerServices;
using UnityEngine;
using UnityEngine.UI;

public class Dot : MonoBehaviour
{

    [Header("Board Variables")]
    public int column;
    public int row;

    private FindingMatches findMatches;
    private Board board;
    private HintManager hintManager;
    public int targetX;
    public int targetY;
    public int previousColumn;
    public int previousRow;
    public bool isMatched = false;
    public GameObject otherDot;
    private Vector2 firstTouchPosition;
    private Vector2 finalTouchPosition;

    private Vector2 tempPosition;
    List<string> uniqueLetterList = new();


    [Header("Swipe Variables")]
    public float swipeAngle = 0;
    public float swipeResist = 1f;

    [Header("Powerup Stuff")]
    public bool isColumnBomb, isRowBomb, isColorBomb, isAdjacentBomb;
    public GameObject rowArrow, columnArrow, colorBomb, AdjacentBomb;
    public GameObject characterAppear;
    public Sprite[] characterAppearSprite;

    void Start()
    {

        isColumnBomb = false;
        isRowBomb = false;
        isColorBomb = false;
        isAdjacentBomb = false;
        hintManager = FindAnyObjectByType<HintManager>();
        board = FindAnyObjectByType<Board>();
        findMatches = FindAnyObjectByType<FindingMatches>();
        MakeCharacterAppear();
        // targetX = (int)transform.position.x;
        // targetY = (int)transform.position.y;
        // row = targetY;
        // column = targetX;
        // previousColumn = column;
        // previousRow = row;
    }

    // Update is called once per frame
    void Update()
    {
        // FindMatches();
        // if (isMatched)
        // {
        //     SpriteRenderer mySprite = GetComponent<SpriteRenderer>();
        //     mySprite.color = new Color(1f, 1f, 1f, .2f);
        // }
        targetX = column;
        targetY = row;
        if (Mathf.Abs(targetX - transform.position.x) > .1)
        {
            // Move towards the target
            tempPosition = new Vector2(targetX, transform.position.y);
            transform.position = Vector2.Lerp(transform.position, tempPosition, .6f);
            if (board.allDots[column, row] != this.gameObject)
            {
                board.allDots[column, row] = this.gameObject;
            }
            findMatches.FindAllMatches();
        }
        else
        {
            // Directly set the position
            tempPosition = new Vector2(targetX, transform.position.y);
            transform.position = tempPosition;
        }

        if (Mathf.Abs(targetY - transform.position.y) > .1)
        {
            // Move towards the target
            tempPosition = new Vector2(transform.position.x, targetY);
            transform.position = Vector2.Lerp(transform.position, tempPosition, .6f);
            if (board.allDots[column, row] != this.gameObject)
            {
                board.allDots[column, row] = this.gameObject;
            }
            findMatches.FindAllMatches();
        }
        else
        {
            // Directly set the position
            tempPosition = new Vector2(transform.position.x, targetY);
            transform.position = tempPosition;
        }
    }

    private void CollectUniqueLetters()
    {
        List<string> wordList = board.GetWordList();
        HashSet<string> uniqueLetters = new HashSet<string>();

        foreach (string word in wordList)
        {
            print("Word: " + word);
            foreach (char letter in word)
            {
                uniqueLetters.Add(letter.ToString().ToUpper());
            }
        }

        // Convert HashSet to List if needed
        uniqueLetterList = new(uniqueLetters);

        // Print the unique letters
        Debug.Log("Unique letters: " + string.Join(",", uniqueLetterList));
    }
    public void MakeCharacterAppear()
    {

        CollectUniqueLetters();
        if (characterAppearSprite.Length == 0 || uniqueLetterList.Count == 0)
        {
            return;
        }
        // int rand = Random.Range(0, characterAppearSprite.Length);
        GameObject character = Instantiate(characterAppear, transform.position, Quaternion.identity);

        // Check if the character has a SpriteRenderer component
        if (!character.TryGetComponent<SpriteRenderer>(out var spriteRenderer))
        {
            // Add a SpriteRenderer component if it's missing
            spriteRenderer = character.AddComponent<SpriteRenderer>();
        }

        // Set the sprite to a random sprite from characterAppearSprite
        Sprite selectedSprite = null;
        for (int i = 0; i < uniqueLetterList.Count; i++)
        {
            int rand = Random.Range(0, characterAppearSprite.Length);
            if (uniqueLetterList.Contains(characterAppearSprite[rand].name))
            {
                selectedSprite = characterAppearSprite[rand];
            }
        }
        spriteRenderer.sprite = selectedSprite;

        // Set the parent of the character GameObject
        character.transform.parent = this.transform;

        // Print the name of the sprite
        print("Character appear: " + selectedSprite.name);
        print("Character appear: " + selectedSprite.name[0]);
        if (uniqueLetterList.Contains(selectedSprite.name))
        {
            {
                print("Character appear match is : " + selectedSprite.name);
            }
        }
    }

    public IEnumerator CheckMoveCo()
    {
        if (isColorBomb)
        {
            findMatches.MatchPiecesOfColor(otherDot.tag);
            isMatched = true;
        }
        else if (otherDot.GetComponent<Dot>().isColorBomb)
        {
            findMatches.MatchPiecesOfColor(this.gameObject.tag);
            otherDot.GetComponent<Dot>().isMatched = true;
            // isMatched = true;
        }

        yield return new WaitForSeconds(.5f);
        if (otherDot != null)
        {
            if (!isMatched && !otherDot.GetComponent<Dot>().isMatched)
            {
                // otherDot.GetComponent<Dot>().row = row;
                // otherDot.GetComponent<Dot>().column = column;
                // row = previousRow;
                // column = previousColumn;
                yield return new WaitForSeconds(.5f);
                board.currentDot = null;
                board.currentState = GameState.move;
            }
            else
            {
                board.DestroyMatches();
            }
            // otherDot = null;
        }
    }

    private void OnMouseDown()
    {
        if (hintManager != null)
        {
            hintManager.DestroyHint();
        }
        if (board.currentState == GameState.move)
        {

            firstTouchPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        }
        // Debug.Log("First touch position: " + firstTouchPosition);
    }

    private void OnMouseUp()
    {
        if (board.currentState == GameState.move)
        {

            finalTouchPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            CalculateAngle();
        }
    }

    // private void OnMouseOver()
    // {
    //     Debug.Log("Mouse is over the object"); // Debug xem OnMouseOver có hoạt động không

    //     if (Input.GetMouseButtonDown(0))
    //     {
    //         isAdjacentBomb = true;

    //         if (AdjacentBomb != null)
    //         {
    //             GameObject marker = Instantiate(AdjacentBomb, transform.position, Quaternion.identity);
    //             marker.transform.parent = this.transform;
    //         }
    //         else
    //         {
    //             Debug.LogWarning("AdjacentBomb prefab is not assigned.");
    //         }
    //     }
    //     else
    //     {
    //     }
    // }
    void CalculateAngle()
    {
        if (finalTouchPosition.y - firstTouchPosition.y > swipeResist || finalTouchPosition.y - firstTouchPosition.y < -swipeResist || finalTouchPosition.x - firstTouchPosition.x > swipeResist || finalTouchPosition.x - firstTouchPosition.x < -swipeResist)
        {
            board.currentState = GameState.wait;
            swipeAngle = Mathf.Atan2(finalTouchPosition.y - firstTouchPosition.y, finalTouchPosition.x - firstTouchPosition.x) * 180 / Mathf.PI;
            MovePieces();
            board.currentDot = this;
        }
        else
        {
            board.currentState = GameState.move;
        }
        // swipeAngle = Mathf.Atan2(finalTouchPosition.y - firstTouchPosition.y, finalTouchPosition.x - firstTouchPosition.x) * 180 / Mathf.PI;
        // MovePieces();
        // Debug.Log("Swipe angle: " + swipeAngle);
    }

    // private void OnMouseOver(){
    //     if(Input.GetMouseButtonDown(0)){
    //         isColorBomb = true;
    //         GameObject color = Instantiate(colorBomb, transform.position, Quaternion.identity);
    //         color.transform.parent = this.transform;
    //     }
    // }

    void MovePiecesActual(Vector2 direction)
    {
        otherDot = board.allDots[column + (int)direction.x, row + (int)direction.y];
        previousRow = row;
        previousColumn = column;
        if (otherDot != null)
        {
            otherDot.GetComponent<Dot>().column += -1 * (int)direction.x;
            otherDot.GetComponent<Dot>().row += -1 * (int)direction.y;
            column += (int)direction.x;
            row += (int)direction.y;
            StartCoroutine(CheckMoveCo());
        }
        else
        {
            board.currentState = GameState.move;
        }
    }
    void MovePieces()
    {
        if (swipeAngle > -45 && swipeAngle <= 45 && column < board.width - 1)
        {
            // // Right swipe
            // otherDot = board.allDots[column + 1, row]; // Get the other dot
            // previousColumn = column; // Set the previous column to the current column
            // previousRow = row; // Set the previous row to the current row
            // otherDot.GetComponent<Dot>().column -= 1; // Set the other dot's column to the current column
            // column += 1; // Set the current column to the other dot's column
            // StartCoroutine(CheckMoveCo()); // Wait for the coroutine to finish before moving
            MovePiecesActual(Vector2.right);
        }
        else if (swipeAngle > 45 && swipeAngle <= 135 && row < board.height - 1)
        {
            // Up swipe
            // otherDot = board.allDots[column, row + 1];
            // previousColumn = column;
            // previousRow = row;
            // otherDot.GetComponent<Dot>().row -= 1;
            // row += 1;
            // StartCoroutine(CheckMoveCo());
            MovePiecesActual(Vector2.up);
        }
        else if ((swipeAngle > 135 || swipeAngle <= -135) && column > 0)
        {
            // Left swipe\
            // otherDot = board.allDots[column - 1, row]; 
            // previousColumn = column;
            // previousRow = row;
            // otherDot.GetComponent<Dot>().column += 1;
            // column -= 1;
            // StartCoroutine(CheckMoveCo()); 
            MovePiecesActual(Vector2.left);
        }
        else if (swipeAngle < -45 && swipeAngle >= -135 && row > 0)
        {
            // Down swipe
            // print("Down swipe");
            // otherDot = board.allDots[column, row - 1];
            // previousColumn = column;
            // previousRow = row;
            // otherDot.GetComponent<Dot>().row += 1;
            // row -= 1;
            // StartCoroutine(CheckMoveCo()); // Wait for the coroutine to finish before moving
            MovePiecesActual(Vector2.down);
        }
        else
        {
            board.currentState = GameState.move; // If the swipe is not valid, set the state back to move
        }
    }

    void FindMatches()
    {
        if (column > 0 && column < board.width - 1)
        {
            GameObject leftDot1 = board.allDots[column - 1, row];
            GameObject rightDot1 = board.allDots[column + 1, row];
            if (leftDot1 != null && rightDot1 != null)
            {
                if (leftDot1.tag == this.gameObject.tag && rightDot1.tag == this.gameObject.tag)
                {
                    leftDot1.GetComponent<Dot>().isMatched = true;
                    rightDot1.GetComponent<Dot>().isMatched = true;
                    isMatched = true;
                }
            }
        }
        if (row > 0 && row < board.height - 1)
        {
            GameObject upDot1 = board.allDots[column, row + 1];
            GameObject downDot1 = board.allDots[column, row - 1];
            if (upDot1 != null && downDot1 != null)
            {
                if (upDot1.tag == this.gameObject.tag && downDot1.tag == this.gameObject.tag)
                {
                    upDot1.GetComponent<Dot>().isMatched = true;
                    downDot1.GetComponent<Dot>().isMatched = true;
                    isMatched = true;
                }
            }
        }
    }

    public void MakeRowBomb()
    {

        print("Make row bomb");
        isRowBomb = true;
        GameObject arrow = Instantiate(rowArrow, transform.position, Quaternion.identity);
        arrow.transform.parent = this.transform;
    }

    public void MakeColumnBomb()
    {
        print("Make column bomb");
        isColumnBomb = true;
        GameObject arrow = Instantiate(columnArrow, transform.position, Quaternion.identity);
        arrow.transform.parent = this.transform;
    }

    public void MakeColorBomb()
    {

        print("Make color bomb");
        isColorBomb = true;
        GameObject bomb = Instantiate(colorBomb, transform.position, Quaternion.identity);
        bomb.transform.parent = this.transform;
        this.gameObject.tag = "Color";
    }

    public void MakeAdjacentBomb()
    {
        print("Make adjacent bomb");
        isAdjacentBomb = true;
        GameObject marker = Instantiate(AdjacentBomb, transform.position, Quaternion.identity);
        marker.transform.parent = this.transform;
    }

}
