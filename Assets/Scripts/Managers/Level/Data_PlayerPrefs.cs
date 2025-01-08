using System.Collections.Generic;
using System.IO;
using UnityEngine;

[System.Serializable]
[CreateAssetMenu]
public class Data_PlayerPrefs : ScriptableObject
{
    public List<Category_PlayerPrefs> categories;
}