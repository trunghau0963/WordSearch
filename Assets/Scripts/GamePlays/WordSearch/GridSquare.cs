using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridSquare : MonoBehaviour
{
    public int SquareIndex { get; set; }

    private AlphabetData.LetterData _normalLetterData;
    private AlphabetData.LetterData _selectedLetterData;
    private AlphabetData.LetterData _correctLetterData;

    private SpriteRenderer _displayedImage;

    private bool _selected;
    private bool _clicked;
    public int _index = -1;
    public bool _correct;

    public void SetIndex(int index)
    {
        _index = index;
    }

    public int GetIndex()
    {
        return _index;
    }

    void Start()
    {
        _clicked = false;
        _selected = false;
        _correct = false;
        _displayedImage = GetComponent<SpriteRenderer>();
    }

    private void OnEnable()
    {
        GameEvents.OnEnableSquareSelection += OnEnableSquareSelection;
        GameEvents.OnDisableSquareSelection += OnDisableSquareSelection;
        GameEvents.OnSelectSquare += SelectSquare;
        GameEvents.OnCorrectWord += CorrectWord;
    }

    private void OnDisable()
    {
        GameEvents.OnEnableSquareSelection -= OnEnableSquareSelection;
        GameEvents.OnDisableSquareSelection -= OnDisableSquareSelection;
        GameEvents.OnSelectSquare -= SelectSquare;
        GameEvents.OnCorrectWord -= CorrectWord;
    }

    public void CorrectWord(string word, List<int> squareIdx)
    {
        if (squareIdx.Contains(_index) && _selected)
        {
            _correct = true;
            _displayedImage.sprite = _correctLetterData.Image;
        }

        _selected = false;
        _clicked = false;
    }

    public void OnEnableSquareSelection()
    {
        _clicked = true;
        _selected = false;
    }

    public void OnDisableSquareSelection()
    {
        _clicked = false;
        _selected = false;
        if (_correct == true)
        {
            _displayedImage.sprite = _correctLetterData.Image;
        }
        else
        {
            _displayedImage.sprite = _normalLetterData.Image;
        }
    }

    private void SelectSquare(UnityEngine.Vector3 position)
    {
        if (this.gameObject.transform.position == position)
        {
            _displayedImage.sprite = _selectedLetterData.Image;
        }
    }
    public void SetSprite(AlphabetData.LetterData normalLetterData, AlphabetData.LetterData selectedLetterData, 
        AlphabetData.LetterData correctLetterData)
    {
        _normalLetterData = normalLetterData;
        _selectedLetterData = selectedLetterData;
        _correctLetterData = correctLetterData;

        GetComponent<SpriteRenderer>().sprite = _normalLetterData.Image;

    }

    private void OnMouseDown()
    {
        OnEnableSquareSelection();
        GameEvents.EnableSquareSelectionMethod();
        CheckSquare();
        _displayedImage.sprite = _selectedLetterData.Image;
    }

    private void OnMouseEnter()
    {
        CheckSquare();
    }

    private void OnMouseUp()
    {
        GameEvents.ClearSelectionMethod();
        GameEvents.DisableSquareSelectionMethod();
    }

    public void CheckSquare()
    {
        if (_clicked == true && _selected == false)
        {
            _selected = true;
            GameEvents.CheckSquareMethod(_normalLetterData.Letter, gameObject.transform.position, _index);
        }
    }
}
