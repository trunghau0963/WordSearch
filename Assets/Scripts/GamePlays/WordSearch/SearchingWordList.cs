using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SearchingWordList : MonoBehaviour
{
    public GameObject searchingWordPrefab;
    public float offset = 10.0f;

    [Header("Layout")]
    [Tooltip("Scale multiplier for word items")]
    public float wordScaleMultiplier = 1.3f;

    [Header("Animation")]
    public float spawnAnimDuration = 0.3f;
    public float spawnStaggerDelay = 0.06f;

    private int _wordNumber = 0;
    private List<GameObject> _words = new();

    private void Start()
    {
        var board = GameSessionData.CurrentBoard;
        if (board == null)
        {
            Debug.LogError("[SearchingWordList] GameSessionData.CurrentBoard is null!");
            return;
        }
        _wordNumber = board.SearchWords.Count;
        CreateWordObjects();
        SetWordsPositionVertical();
        StartCoroutine(AnimateWordsAppearance());
    }

    private void CreateWordObjects()
    {
        var baseScale = new Vector3(wordScaleMultiplier, wordScaleMultiplier, 1f);
        var finalScale = GetSquareScale(baseScale);

        for (var idx = 0; idx < _wordNumber; idx++)
        {
            var go = Instantiate(searchingWordPrefab, transform);
            var rt = go.GetComponent<RectTransform>();
            rt.localScale = Vector3.zero; // start invisible for animation
            rt.localPosition = Vector3.zero;
            go.GetComponent<SearchingWord>().Setword(GameSessionData.CurrentBoard.SearchWords[idx].Word);
            _words.Add(go);
        }

        // Store target scale for animation
        _targetScale = finalScale;
    }

    private Vector3 _targetScale;

    private void SetWordsPositionVertical()
    {
        if (_words.Count == 0) return;

        var parentRect = GetComponent<RectTransform>();
        var squareRect = searchingWordPrefab.GetComponent<RectTransform>();

        float itemHeight = squareRect.rect.height * _targetScale.y + offset;
        float totalHeight = itemHeight * _wordNumber;

        // Center the list vertically in parent
        float startY = (totalHeight - itemHeight) / 2f;

        for (int i = 0; i < _words.Count; i++)
        {
            var rt = _words[i].GetComponent<RectTransform>();
            float posY = startY - (itemHeight * i);
            rt.localPosition = new Vector2(0f, posY);
        }
    }

    private IEnumerator AnimateWordsAppearance()
    {
        for (int i = 0; i < _words.Count; i++)
        {
            var go = _words[i];
            LeanTween.scale(go, _targetScale, spawnAnimDuration)
                .setDelay(i * spawnStaggerDelay)
                .setEase(LeanTweenType.easeOutBack);
        }

        yield return new WaitForSeconds((_words.Count * spawnStaggerDelay) + spawnAnimDuration);
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
        var squareRect = searchingWordPrefab.GetComponent<RectTransform>();
        var parentRect = GetComponent<RectTransform>();

        float itemHeight = squareRect.rect.height * targetScale.y + offset;
        float totalHeight = itemHeight * _wordNumber;

        if (totalHeight > parentRect.rect.height)
            return true;

        float itemWidth = squareRect.rect.width * targetScale.x;
        if (itemWidth > parentRect.rect.width)
            return true;

        return false;
    }
}
