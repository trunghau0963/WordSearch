using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Unity.Collections;
// using Unity.VisualScripting;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

[CustomEditor(typeof(AlphabetData))]
[CanEditMultipleObjects]
[System.Serializable]
public class AlphabetDataDrawer : Editor
{
    private ReorderableList AlphabetPlainList;
    private ReorderableList AlphabetNormalList;
    private ReorderableList AlphabetHighlightedList;
    private ReorderableList AlphabetWrongList;

    private void OnEnable()
    {
        InitializeReorderableList(ref AlphabetPlainList, "AlphabetPlain", "Alphabet Plain");
        InitializeReorderableList(ref AlphabetNormalList, "AlphabetNormal", "Alphabet Normal");
        InitializeReorderableList(ref AlphabetHighlightedList, "AlphabetHighlighted", "Alphabet Highlighted");
        InitializeReorderableList(ref AlphabetWrongList, "AlphabetWrong", "Alphabet Wrong");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        AlphabetPlainList.DoLayoutList();
        AlphabetNormalList.DoLayoutList();
        AlphabetHighlightedList.DoLayoutList();
        AlphabetWrongList.DoLayoutList();
        serializedObject.ApplyModifiedProperties();
    }
    public void InitializeReorderableList(ref ReorderableList list, string propertyName, string listLabel)
    {
        list = new ReorderableList(serializedObject, serializedObject.FindProperty(propertyName), true, true, true, true);
        list.drawHeaderCallback = rect => EditorGUI.LabelField(rect, listLabel);

        var l = list;
        list.drawElementCallback = (rect, index, isActive, isFocused) =>
        {
            var element = l.serializedProperty.GetArrayElementAtIndex(index);
            rect.y += 2;
            EditorGUI.PropertyField(new Rect(rect.x, rect.y, 60, EditorGUIUtility.singleLineHeight), element.FindPropertyRelative("Letter"), GUIContent.none);
            // rectangle
            EditorGUI.PropertyField(new Rect(rect.x + 70, rect.y, rect.width - 90, EditorGUIUtility.singleLineHeight), element.FindPropertyRelative("Image"), GUIContent.none);
        };
    }

}
