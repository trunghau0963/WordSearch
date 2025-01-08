using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Unity.Services.Lobbies.Models;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class LobbySettingsMenu : Panel
{

    [SerializeField] private Button confirmButton = null;
    [SerializeField] private Button cancelButton = null;
    [SerializeField] private TMP_InputField nameInput = null;
    [SerializeField] private TMP_InputField maxPlayersInput = null;
    [SerializeField] private TMP_Dropdown visibilityDropdown = null;
    [SerializeField] private TMP_Dropdown modeDropdown = null;
    [SerializeField] private TMP_Dropdown mapDropdown = null;
    [SerializeField] private TMP_Dropdown languageDropdown = null;
    
    private Lobby lobby = null;
    
    public override void Initialize()
    {
        if (IsInitialized)
        {
            return;
        }
        confirmButton.onClick.AddListener(Confirm);
        cancelButton.onClick.AddListener(Cancel);
        nameInput.contentType = TMP_InputField.ContentType.Standard;
        maxPlayersInput.contentType = TMP_InputField.ContentType.IntegerNumber;
        nameInput.characterLimit = 20;
        maxPlayersInput.characterLimit = 2;
        base.Initialize();
    }
    
    public void Open(Lobby lobby)
    {
        this.lobby = lobby;
        if (lobby == null)
        {
            nameInput.name = "";
            maxPlayersInput.name = "5";
            visibilityDropdown.SetValueWithoutNotify(0);
            modeDropdown.SetValueWithoutNotify(0);
            mapDropdown.SetValueWithoutNotify(0);
            languageDropdown.SetValueWithoutNotify(0);
        }
        else
        {
            nameInput.name = lobby.Name;
            maxPlayersInput.name = lobby.MaxPlayers.ToString();
            visibilityDropdown.SetValueWithoutNotify(lobby.IsPrivate ? 1 : 0);
            for (int i = 0; i < visibilityDropdown.options.Count; i++)
            {
                if ((lobby.IsPrivate && visibilityDropdown.options[i].text.ToLower() == "private") || (lobby.IsPrivate == false && visibilityDropdown.options[i].text.ToLower() == "public"))
                {
                    visibilityDropdown.SetValueWithoutNotify(i);
                    break;
                }
            }
            
            if (lobby.Data.ContainsKey("mode"))
            {
                var gameMode = lobby.Data["mode"].Value.ToLower();
                for (int i = 0; i < modeDropdown.options.Count; i++)
                {
                    if (modeDropdown.options[i].text.ToLower() == gameMode)
                    {
                        modeDropdown.SetValueWithoutNotify(i);
                        break;
                    }
                }
            }
            
            if (lobby.Data.ContainsKey("map"))
            {
                var gameMap = lobby.Data["map"].Value.ToLower();
                for (int i = 0; i < mapDropdown.options.Count; i++)
                {
                    if (mapDropdown.options[i].text.ToLower() == gameMap)
                    {
                        mapDropdown.SetValueWithoutNotify(i);
                        break;
                    }
                }
            }
            
            if (lobby.Data.ContainsKey("language"))
            {
                var language = lobby.Data["language"].Value.ToLower();
                for (int i = 0; i < languageDropdown.options.Count; i++)
                {
                    if (languageDropdown.options[i].text.ToLower() == language)
                    {
                        languageDropdown.SetValueWithoutNotify(i);
                        break;
                    }
                }
            }
        }
        Open();
    }

    private void Confirm()
    {
        string lobbyName = nameInput.text.Trim();
        int maxPlayer = 0;
        int.TryParse(maxPlayersInput.text.Trim(), out maxPlayer);
        bool isPrivate = visibilityDropdown.captionText.text.Trim().ToLower() == "private" ? true : false;
        string mode = modeDropdown.captionText.text.Trim();
        string map = mapDropdown.captionText.text.Trim();
        string language = languageDropdown.captionText.text.Trim();
        if (maxPlayer > 0 && string.IsNullOrEmpty(lobbyName) == false)
        {
            LobbyMenu panel = (LobbyMenu)PanelManager.GetSingleton("lobby");
            if (lobby == null)
            {
                panel.CreateLobby(lobbyName, maxPlayer, isPrivate, mode, map, language);
            }
            else
            {
                panel.UpdateLobby(lobby.Id, lobbyName, maxPlayer, isPrivate, mode, map, language);
            }
            Close();
        }
    }
    
    private void Cancel()
    {
        Close();
    }
    
    public override void Close()
    {
        base.Close();
        lobby = null;
    }
    
}