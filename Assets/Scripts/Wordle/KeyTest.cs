using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeyTest : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        Key.OnKeyPressed += DebugLetter;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void DebugLetter(string letter)
    {
        Debug.Log(letter);
    }
}
