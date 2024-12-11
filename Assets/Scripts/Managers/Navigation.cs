using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Navigation : MonoBehaviour
{
    [SerializeField] private GameObject[] panels;
    [SerializeField] private GameObject[] buttons;

    private void Start()
    {
        Console.WriteLine("size of panels: " + panels.Length);
        for (int i = 1; i < panels.Length; i++)
        {
            panels[i].SetActive(false);
        }
        panels[0].SetActive(true);
        EventSystem.current.SetSelectedGameObject(buttons[0].gameObject);
    }

    public void ShowPanel(GameObject activePanel)
    {
        foreach (GameObject panel in panels)
        {
            if (panel == activePanel)
            {
                panel.SetActive(true);
            }
            else
            {
                panel.SetActive(false);
            }
        }
    }

}
