using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChangeNameText : MonoBehaviour
{
    public Text text;

    // Update is called once per fram
    public void ChangeName(string name){
        text.text = name;
    }
}
