using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
[CreateAssetMenu]
public class AlphabetData : ScriptableObject
{
    [System.Serializable]
    public class LetterData
    {
        public string Letter;
        public Sprite Image;
        public int Score;
    }

    public List<LetterData> AlphabetPlain = new();
    public List<LetterData> AlphabetNormal = new();
    public List<LetterData> AlphabetHighlighted = new();
    public List<LetterData> AlphabetWrong = new();
}
