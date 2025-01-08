using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Unity.Services.CloudSave;
using Unity.Services.CloudSave.Models.Data.Player;
using UnityEngine.UI;
using SaveOptions = Unity.Services.CloudSave.Models.Data.Player.SaveOptions;

public class CustomizationMenu : Panel
{

    [SerializeField] public TextMeshProUGUI characterText = null;
    [SerializeField] private Button characterButton = null;
    [SerializeField] private Button colorButton = null;
    [SerializeField] private Button closeButton = null;
    [SerializeField] private Button saveButton = null;
    
    private int _savedColor = 0;
    private int _savedCharacter = 0;
    
    private int _color = 0;
    private int _character = 0;

    private string[] _characters = { "Cube", "Capsule", "Sphere" };
    private Color[] _colors = { Color.green, Color.red, Color.blue, Color.magenta, Color.cyan };

    public override void Initialize()
    {
        if (IsInitialized)
        {
            return;
        }
        closeButton.onClick.AddListener(ClosePanel);
        characterButton.onClick.AddListener(ChangeCharacter);
        colorButton.onClick.AddListener(ChangeColor);
        saveButton.onClick.AddListener(Save);
        base.Initialize();
    }
    
    public override void Open()
    {
        base.Open();
        LoadData();
    }
    
    private async void LoadData()
    {
        characterText.text = "";
        characterButton.interactable = false;
        colorButton.interactable = false;
        saveButton.interactable = false;
        _character = 0;
        _color = 0;
        _savedCharacter = 0;
        _savedColor = 0;
        try
        {
            var playerData = await CloudSaveService.Instance.Data.Player.LoadAsync(new HashSet<string> { "character" }, new LoadOptions(new PublicReadAccessClassOptions()));
            if (playerData.TryGetValue("character", out var characterData))
            {
                var data = characterData.Value.GetAs<Dictionary<string, object>>();
                _savedCharacter = int.Parse(data["type"].ToString());
                _savedColor = int.Parse(data["color_index"].ToString());
                _character = _savedCharacter;
                _color = _savedColor;
            }
        }
        catch (Exception exception)
        {
            Debug.Log(exception.Message);
        }
        characterButton.interactable = true;
        colorButton.interactable = true;
        ApplyData();
    }

    private async void Save()
    {
        saveButton.interactable = false;
        characterButton.interactable = false;
        colorButton.interactable = false;
        try
        {
            var playerData = new Dictionary<string, object>
            {
                { "type", _character },
                { "color", "#" + ColorUtility.ToHtmlStringRGBA(_colors[_color]) },
                { "color_index", _color }
            };
            var data = new Dictionary<string, object> { { "character", playerData } };
            await CloudSaveService.Instance.Data.Player.SaveAsync(data, new SaveOptions(new PublicWriteAccessClassOptions()));
            _savedCharacter = _character;
            _savedColor = _color;
        }
        catch (Exception exception)
        {
            Debug.Log(exception.Message);
            saveButton.interactable = true;
        }
        characterButton.interactable = true;
        colorButton.interactable = true;
    }

    private void ChangeCharacter()
    {
        _character++;
        if (_character >= _characters.Length)
        {
            _character = 0;
        }
        ApplyData();
    }

    private void ChangeColor()
    {
        _color++;
        if (_color >= _colors.Length)
        {
            _color = 0;
        }
        ApplyData();
    }
    
    private void ApplyData()
    {
        characterText.text = _characters[_character];
        characterText.color = _colors[_color];
        saveButton.interactable = _character != _savedCharacter || _color != _savedColor;
    }
    
    private void ClosePanel()
    {
        Close();
    }
    
}