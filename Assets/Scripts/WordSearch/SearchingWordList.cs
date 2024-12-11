using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;

public class SearchingWordList : MonoBehaviour
{
    public GameData currentGameData;
    public GameObject searchingWordPrefab;
    public float offset = 0.0f;
    public int maxColumns = 5;
    public int maxRows = 4;
    private int _columns = 2;
    private int _rows;
    private int _wordNumber = 0;
    private List<GameObject> _words = new(); 
    private void Start()
    {
        _wordNumber = currentGameData.selectedBoardData.SearchWords.Count;

        if(_wordNumber < _columns){
            _rows = 1;
        }
        else {
            CalculateColumnsAndRowsNumber();
        }

        CreateWordObject();
        SetWordsPostion();

    }

    private void CalculateColumnsAndRowsNumber(){
        do {
            _columns++;
            _rows = _wordNumber / _columns;
        } while (_rows >= maxColumns);

        if(_columns > maxColumns){
            _columns = maxColumns;
            _rows = _wordNumber / _columns;
        }
    }

    private bool TryIncreaseColumnNumber(){
        _columns++;
        _rows = _wordNumber / _columns;

        if(_columns > maxColumns){
            _columns = maxColumns;
            _rows = _wordNumber / _columns;

            return false;
        }

        if(_wordNumber % _columns > 0){
            _rows++;
        }
        return true;
    }

    private void CreateWordObject(){
        var squareScale = GetSquareScale(new UnityEngine.Vector3(1f, 1f, 0.1f));

        for(var idx = 0; idx < _wordNumber; idx++){
            _words.Add(Instantiate(searchingWordPrefab) as GameObject);
            _words[idx].transform.SetParent(transform);
            _words[idx].GetComponent<RectTransform>().localScale = squareScale;
            _words[idx].GetComponent<RectTransform>().localPosition = new UnityEngine.Vector3(0, 0, 0);
            _words[idx].GetComponent<SearchingWord>().Setword(currentGameData.selectedBoardData.SearchWords[idx].Word);
        }
    }

    private UnityEngine.Vector3 GetSquareScale(UnityEngine.Vector3 defaultScale){
        var finalScale = defaultScale;
        var adjustment = 0.01f;
        while(ShouldScaleDown(finalScale)){
            finalScale.x -= adjustment;
            finalScale.y -= adjustment;

            if(finalScale.x <= 0 || finalScale.y <= 0){
                finalScale.x = adjustment;  
                finalScale.y = adjustment;

                return finalScale;
            }
        }

        return finalScale;
    }

    private bool ShouldScaleDown(UnityEngine.Vector3 targetScale){
        var squareRect = searchingWordPrefab.GetComponent<RectTransform>();
        var parentRect = GetComponent<RectTransform>();

        var squareSize = new UnityEngine.Vector2(0f, 0f)
        {
            x = squareRect.rect.width * targetScale.x + offset,
            y = squareRect.rect.height * targetScale.y + offset
        };

        var totalSquareHeight = squareSize.y * _rows;

        if(totalSquareHeight > parentRect.rect.height){
            while(totalSquareHeight > parentRect.rect.height){
                if(TryIncreaseColumnNumber()){
                    totalSquareHeight = squareSize.y * _rows;
                }
                else {
                    return true;
                }
            }
        }

        var totalSquareWidth = squareSize.x * _columns;

        if(totalSquareWidth > parentRect.rect.width){
            return true;
        }

        return false;
    }

    private void SetWordsPostion(){
        var squareRect = searchingWordPrefab.GetComponent<RectTransform>();
        var wordOffset = new UnityEngine.Vector2(0f, 0f)
        {
            x = squareRect.rect.width * squareRect.localScale.x + offset,
            y = squareRect.rect.height * squareRect.localScale.y + offset
        };

        int columnNumber = 0;
        int rowNumber = 0;
        var startPostion = GetFirstSquarePostion();

        foreach(var word in _words){
            if(columnNumber + 1 > _columns){
                columnNumber = 0;
                rowNumber++;
            }

            var positionX = startPostion.x + wordOffset.x * columnNumber;
            var positionY = startPostion.y - wordOffset.y * rowNumber;

            word.GetComponent<RectTransform>().localPosition = new UnityEngine.Vector2(positionX, positionY);
            columnNumber++;
        }
    }

    private UnityEngine.Vector2 GetFirstSquarePostion(){
        var startPostion = new UnityEngine.Vector2(0f, transform.position.y);
        var squareRect = _words[0].GetComponent<RectTransform>();
        var parentRect = GetComponent<RectTransform>();
        var squareSize = new UnityEngine.Vector2(0f, 0f)
        {
            x = squareRect.rect.width * squareRect.localScale.x + offset,
            y = squareRect.rect.height * squareRect.localScale.y + offset
        };

        var shiftBy = (parentRect.rect.width - squareSize.x * _columns) / 2;

        startPostion.x = (parentRect.rect.width - squareSize.x) / 2 * (-1);
        startPostion.x += shiftBy;
        startPostion.y = (parentRect.rect.height - squareSize.y) / 2;

        return startPostion;
    }
}
