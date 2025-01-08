using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Key : MonoBehaviour
{
    [Header("Elements")]
    [SerializeField] private TextMeshProUGUI text;

    [Header("Properties")]
    public static Action<string> OnKeyPressed;
    void Start()
    {
        GetComponent<Button>().onClick.AddListener(() => SendKeyPress());
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void SendKeyPress(){
        // print("Key pressed: " + text.text);
        OnKeyPressed?.Invoke(text.text);
    }

    public void SetLetter(char letter)
    {
        text.text = letter.ToString();
    }

    public void SetState(Tiles.State state)
    {
        text.color = state.fillColor;
    }

    public void SetInteractable(bool interactable)
    {
        GetComponent<Button>().interactable = interactable;
    }

}
