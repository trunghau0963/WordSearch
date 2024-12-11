using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Row : MonoBehaviour
{
    public Tiles[] tiles { get; private set; }


    public string word
    {
        get
        {
            string word = "";

            for (int i = 0; i < tiles.Length; i++) {
                word += tiles[i].letter;
            }

            return word;
        }
    }
    private void Awake()
    {
        tiles = GetComponentsInChildren<Tiles>();
    }
    

    public void SetLetter(int index, string letter)
    {
        tiles[index].SetLetter(letter);
    }

    public void SetState(int index, Tiles.State state)
    {
        tiles[index].SetState(state);
    }
}
