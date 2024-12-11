// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;
// using UnityEngine.UI;

// public class SetText : MonoBehaviour
// {

//     public GameData gameData;

//     public Text text;

//     public string type;

//     public void OnEnable()
//     {
//         text.text = type switch
//         {
//             "Category" => gameData.selectedCategoryName,
//             "Section" => gameData.selectedSectionName,
//             "Level" => gameData.selectedLevelName,
//             _ => "Error",
//         };
//     }
// }
