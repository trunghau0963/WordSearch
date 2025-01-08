using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Unity.Collections;
// using Unity.VisualScripting;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

[CustomEditor(typeof(BoardData), false)]
[CanEditMultipleObjects]
[System.Serializable]
public class BoardDataDrawer : Editor
{
    private BoardData GameDataInstance => target as BoardData;
    private ReorderableList _dataList;

    private void OnEnable()
    {
        // throw new System.NotImplementedException();
        InitializeReorderableList(ref _dataList, "SearchWords", "Search Words");
    }

    public override void OnInspectorGUI()
    {
        // DrawDefaultInspector();
        serializedObject.Update();

        GameDataInstance.Name = EditorGUILayout.TextField("Name", GameDataInstance.Name);
        GameDataInstance.isCompleted = EditorGUILayout.Toggle("Is Completed", GameDataInstance.isCompleted);
        GameDataInstance.timeInSeconds = EditorGUILayout.FloatField("Time in Seconds", GameDataInstance.timeInSeconds);

        DrawColumnsRowsInputFields();
        EditorGUILayout.Space();
        ConvertToUpperButton();

        if (GameDataInstance.Boards != null && GameDataInstance.Columns > 0 && GameDataInstance.Rows > 0)
        {
            DrawBoardTable();
        }

        GUILayout.BeginHorizontal();
        
        ClearBoardButton();
        FillUpWithRandomLettersButton();

        GUILayout.EndHorizontal();


        EditorGUILayout.Space();
        _dataList.DoLayoutList();


        serializedObject.ApplyModifiedProperties();
        if (GUI.changed)
        {
            EditorUtility.SetDirty(GameDataInstance);
        }
    }

    private void DrawColumnsRowsInputFields()
    {
        var columnsTemp = GameDataInstance.Columns;
        var rowsTemp = GameDataInstance.Rows;

        GameDataInstance.Columns = EditorGUILayout.IntField("Columns", GameDataInstance.Columns);
        GameDataInstance.Rows = EditorGUILayout.IntField("Rows", GameDataInstance.Rows);

        if ((GameDataInstance.Columns != columnsTemp || GameDataInstance.Rows != rowsTemp) && GameDataInstance.Columns > 0 && GameDataInstance.Rows > 0)
        {
            GameDataInstance.CreateNewBoard(GameDataInstance.Rows, GameDataInstance.Columns);
        }
    }

    private void DrawBoardTable()
    {

        var tableStyle = new GUIStyle();
        tableStyle.padding = new RectOffset(10, 10, 10, 10);
        tableStyle.margin.left = 32;

        var headerComlumnsStyle = new GUIStyle();
        headerComlumnsStyle.fixedWidth = 32;

        var columnsStyle = new GUIStyle();
        columnsStyle.fixedWidth = 50;

        var rowsStyle = new GUIStyle();
        rowsStyle.fixedWidth = 40;
        rowsStyle.fixedHeight = 25;
        rowsStyle.alignment = TextAnchor.MiddleCenter;

        var textFieldsStyle = new GUIStyle();

        textFieldsStyle.normal.background = Texture2D.grayTexture;
        textFieldsStyle.normal.textColor = Color.white;
        textFieldsStyle.fontStyle = FontStyle.Bold;
        textFieldsStyle.alignment = TextAnchor.MiddleCenter;

        EditorGUILayout.BeginHorizontal(tableStyle);
        for (int i = 0; i < GameDataInstance.Columns; i++)
        {
            // EditorGUILayout.LabelField(i.ToString(), headerComlumnsStyle);
            EditorGUILayout.BeginVertical(i == -1 ? headerComlumnsStyle : columnsStyle);
            for (int j = 0; j < GameDataInstance.Rows; j++)
            {
                if (i >= 0 && j >= 0)
                {
                    EditorGUILayout.BeginHorizontal(rowsStyle);
                    var character = (string)EditorGUILayout.TextArea(GameDataInstance.Boards[i].Row[j], textFieldsStyle);
                    if (GameDataInstance.Boards[i].Row[j].Length > 1)
                    {
                        character = GameDataInstance.Boards[i].Row[j].Substring(0, 1);
                    }
                    GameDataInstance.Boards[i].Row[j] = character;
                    EditorGUILayout.EndHorizontal();
                }
            }
            EditorGUILayout.EndVertical();
        }
        EditorGUILayout.EndHorizontal();
    }


    private void InitializeReorderableList(ref ReorderableList list, string propertyName, string listLabel)
    {
        list = new ReorderableList(serializedObject, serializedObject.FindProperty(propertyName), true, true, true, true);
        list.drawHeaderCallback = rect => EditorGUI.LabelField(rect, listLabel);
        var l = list;

        list.drawElementCallback = (rect, index, isActive, isFocused) =>
        {
            var element = l.serializedProperty.GetArrayElementAtIndex(index);
            rect.y += 2;
            EditorGUI.PropertyField(new Rect(rect.x, rect.y, EditorGUIUtility.labelWidth, EditorGUIUtility.singleLineHeight), element.FindPropertyRelative("Word"), GUIContent.none);
        };

    }

    private void ConvertToUpperButton()
    {
        if (GUILayout.Button("Convert to Upper"))
        {
            for (int i = 0; i < GameDataInstance.Columns; i++)
            {
                for (int j = 0; j < GameDataInstance.Rows; j++)
                {

                    int errorCounter = Regex.Matches(GameDataInstance.Boards[i].Row[j], @"[a-z]").Count;
                    if (errorCounter > 0)
                    {
                        GameDataInstance.Boards[i].Row[j] = GameDataInstance.Boards[i].Row[j].ToUpper();
                    }
                }
            }

            foreach (var word in GameDataInstance.SearchWords)
            {
                int errorCounter = Regex.Matches(word.Word, @"[a-z]").Count;
                if (errorCounter > 0)
                {
                    word.Word = word.Word.ToUpper();
                }
            }
        }
    }

    private void ClearBoardButton(){
        if(GUILayout.Button("Clear Board")){
            for(int i = 0; i < GameDataInstance.Columns; i++){
                for(int j = 0; j < GameDataInstance.Rows; j++){
                    GameDataInstance.Boards[i].Row[j] = " ";
                }
            }
        }
    }

    private void FillUpWithRandomLettersButton(){
        if(GUILayout.Button("Fill Up With Random Letters")){
            for(int i = 0; i < GameDataInstance.Columns; i++){
                for(int j = 0; j < GameDataInstance.Rows; j++){
                    int errorCounter = Regex.Matches(GameDataInstance.Boards[i].Row[j], @"[a-z]").Count;
                    string letters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
                    int index = UnityEngine.Random.Range(0, letters.Length);
                    if(errorCounter == 0){
                        GameDataInstance.Boards[i].Row[j] = letters[index].ToString();
                    }
                }
            }
        }
    }
}
