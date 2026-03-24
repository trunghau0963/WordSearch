using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TMPro;
using Unity.Services.Authentication;
using UnityEngine.UI;
using Unity.Services.Friends;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;

public class MainMenu : Panel
{

    [SerializeField] public TextMeshProUGUI nameText = null;
    [SerializeField] private Button logoutButton = null;
    [SerializeField] private Button leaderboardsButton = null;
    [SerializeField] private Button friendsButton = null;
    [SerializeField] private Button renameButton = null;
    [SerializeField] private Button customizationButton = null;
    [SerializeField] private Button lobbyButton = null;
    [SerializeField] private Button vocabularyButton = null;
    
    [Header("Play Game / GamePlay Panel")]
    [SerializeField] private Button playGameButton = null;
    [SerializeField] private TextMeshProUGUI playGameButtonText = null;
    [SerializeField] private RectTransform featureBtnRect = null;
    [SerializeField] private RectTransform gamePlayBtnRect = null;
    [SerializeField] private CanvasGroup gamePlayBtnCanvasGroup = null;
    [SerializeField] private float slideAnimationDuration = 0.4f;
    [SerializeField] private Color playGameNormalColor = new Color(0.13f, 0.55f, 0.13f, 1f);
    [SerializeField] private Color playGameCloseColor  = new Color(1f, 0.5f, 0f, 1f);

    [Header("Gameplay Buttons")]
    [SerializeField] private Button wordSearchButton = null;
    [SerializeField] private Button wordZeeButton = null;
    [SerializeField] private Button wordleButton = null;
    [SerializeField] private Button wordCandyButton = null;
    [SerializeField] private Button wordTetrisButton = null;
    [SerializeField] private Button wordConnectButton = null;

    private bool isGamePlayOpen = false;
    private bool isGamePlayAnimating = false;
    private bool _positionsCached = false;
    private Vector2 _featureClosedPos;
    private Vector2 _featureOpenPos;
    private Vector2 _gamePlayOpenPos;
    private Vector2 _gamePlayHiddenPos;
    private HorizontalLayoutGroup _bodyLayoutGroup;
    private int _animTweenId = -1;
    private bool isFriendsServiceInitialized = false;
    private List<string> joinedLobbyIds = new List<string>();
    
    public override void Initialize()
    {
        if (IsInitialized)
        {
            return;
        }
        logoutButton.onClick.AddListener(SignOut);
        leaderboardsButton.onClick.AddListener(Leaderboards);
        friendsButton.onClick.AddListener(Friends);
        renameButton.onClick.AddListener(RenamePlayer);
        customizationButton.onClick.AddListener(Customization);
        lobbyButton.onClick.AddListener(Lobby);
        if (vocabularyButton != null) vocabularyButton.onClick.AddListener(Vocabulary);
        playGameButton.onClick.AddListener(ToggleGamePlay);

        // Gameplay buttons → open level select
        if (wordSearchButton != null)  wordSearchButton.onClick.AddListener(()  => OpenLevelSelect("wordsearch"));
        if (wordZeeButton != null)     wordZeeButton.onClick.AddListener(()     => OpenLevelSelect("wordzee"));
        if (wordleButton != null)      wordleButton.onClick.AddListener(()      => OpenLevelSelect("wordle"));
        if (wordCandyButton != null)   wordCandyButton.onClick.AddListener(()   => OpenLevelSelect("wordcandy"));
        if (wordTetrisButton != null)  wordTetrisButton.onClick.AddListener(()  => OpenLevelSelect("wordtetris"));
        if (wordConnectButton != null) wordConnectButton.onClick.AddListener(() => OpenLevelSelect("wordconnect"));

        // Cache HorizontalLayoutGroup on BODY parent
        if (featureBtnRect != null)
            _bodyLayoutGroup = featureBtnRect.parent.GetComponent<HorizontalLayoutGroup>();

        // Ensure CanvasGroup on GamePlayBtn
        if (gamePlayBtnCanvasGroup == null && gamePlayBtnRect != null)
        {
            gamePlayBtnCanvasGroup = gamePlayBtnRect.GetComponent<CanvasGroup>();
            if (gamePlayBtnCanvasGroup == null)
                gamePlayBtnCanvasGroup = gamePlayBtnRect.gameObject.AddComponent<CanvasGroup>();
        }

        // Hide GamePlayBtn at start
        if (gamePlayBtnRect != null)
            gamePlayBtnRect.gameObject.SetActive(false);

        base.Initialize();
    }
    
    public override void Open()
    {
        friendsButton.interactable = isFriendsServiceInitialized;
        UpdatePlayerNameUI();
        if (isFriendsServiceInitialized == false)
        {
            InitializeFriendsServiceAsync();
        }
        
        // Reset GamePlay panel state when menu opens
        ResetGamePlayState();
        
        base.Open();
    }

    private async void Lobby()
    {
        PanelManager.Open("loading");
        try
        {
            var lobbyIds = await LobbyService.Instance.GetJoinedLobbiesAsync();
            joinedLobbyIds = lobbyIds;
        }
        catch (Exception exception)
        {
            Debug.Log(exception.Message);
        }

        Lobby lobby = null;
        if (joinedLobbyIds.Count > 0)
        {
            try
            {
                lobby = await LobbyService.Instance.GetLobbyAsync(joinedLobbyIds.Last());
            }
            catch (Exception exception)
            {
                Debug.Log(exception.Message);
            }
        }
        
        if (lobby == null)
        {
            LobbyMenu panel = (LobbyMenu)PanelManager.GetSingleton("lobby");
            if (panel.JoinedLobby != null && joinedLobbyIds.Count > 0 && panel.JoinedLobby.Id == joinedLobbyIds.Last())
            {
                lobby = panel.JoinedLobby;
            }
        }
        
        if (lobby != null)
        {
            LobbyMenu panel = (LobbyMenu)PanelManager.GetSingleton("lobby");
            panel.Open(lobby);
        }
        else
        {
            PanelManager.Open("lobby_search");
        }
        
        PanelManager.Close("loading");
    }
    
    // ── Cache layout positions (called once, lazily) ───────────────────────────
    private void CachePositions()
    {
        // 1. Record FeatureBtn position when GamePlayBtn is hidden (centered)
        if (_bodyLayoutGroup != null) _bodyLayoutGroup.enabled = true;
        gamePlayBtnRect.gameObject.SetActive(false);
        LayoutRebuilder.ForceRebuildLayoutImmediate(featureBtnRect.parent as RectTransform);
        _featureClosedPos = featureBtnRect.anchoredPosition;

        // 2. Show GamePlayBtn so HLG computes the two-column layout
        gamePlayBtnRect.gameObject.SetActive(true);
        LayoutRebuilder.ForceRebuildLayoutImmediate(featureBtnRect.parent as RectTransform);
        _featureOpenPos  = featureBtnRect.anchoredPosition;
        _gamePlayOpenPos = gamePlayBtnRect.anchoredPosition;

        // 3. Compute an off-screen position for GamePlayBtn (slides in from right)
        RectTransform bodyRect = featureBtnRect.parent as RectTransform;
        float bodyWidth = bodyRect.rect.width;
        _gamePlayHiddenPos = new Vector2(bodyWidth, _gamePlayOpenPos.y);

        // 4. Restore hidden state
        gamePlayBtnRect.gameObject.SetActive(false);
        LayoutRebuilder.ForceRebuildLayoutImmediate(featureBtnRect.parent as RectTransform);

        _positionsCached = true;
    }

    private void ToggleGamePlay()
    {
        if (isGamePlayAnimating) return;

        if (isGamePlayOpen)
            CloseGamePlay();
        else
            OpenGamePlay();
    }

    private void OpenGamePlay()
    {
        if (!_positionsCached) CachePositions();

        isGamePlayAnimating = true;
        isGamePlayOpen = true;

        if (playGameButtonText != null) playGameButtonText.text = "Close";
        SetPlayGameButtonColor(playGameCloseColor);

        // Disable HLG — we drive positions manually
        if (_bodyLayoutGroup != null) _bodyLayoutGroup.enabled = false;

        // Activate GamePlayBtn at its hidden (off-screen) position
        gamePlayBtnRect.gameObject.SetActive(true);
        featureBtnRect.anchoredPosition  = _featureClosedPos;
        gamePlayBtnRect.anchoredPosition = _gamePlayHiddenPos;
        if (gamePlayBtnCanvasGroup != null) gamePlayBtnCanvasGroup.alpha = 0f;

        // Single tween 0→1 drives BOTH panels in perfect sync
        CancelAnimTween();
        _animTweenId = LeanTween.value(gameObject, 0f, 1f, slideAnimationDuration)
            .setEaseOutCubic()
            .setIgnoreTimeScale(true)
            .setOnUpdate((float t) =>
            {
                featureBtnRect.anchoredPosition  = Vector2.Lerp(_featureClosedPos, _featureOpenPos, t);
                gamePlayBtnRect.anchoredPosition = Vector2.Lerp(_gamePlayHiddenPos, _gamePlayOpenPos, t);
                if (gamePlayBtnCanvasGroup != null)
                    gamePlayBtnCanvasGroup.alpha = Mathf.Clamp01(t * 2f); // fade in first half
            })
            .setOnComplete(() =>
            {
                featureBtnRect.anchoredPosition  = _featureOpenPos;
                gamePlayBtnRect.anchoredPosition = _gamePlayOpenPos;
                if (gamePlayBtnCanvasGroup != null) gamePlayBtnCanvasGroup.alpha = 1f;
                isGamePlayAnimating = false;
            }).id;
    }

    private void CloseGamePlay()
    {
        if (!_positionsCached) CachePositions();

        isGamePlayAnimating = true;
        isGamePlayOpen = false;

        if (playGameButtonText != null) playGameButtonText.text = "Play Game";
        SetPlayGameButtonColor(playGameNormalColor);

        // Disable HLG — we drive positions manually
        if (_bodyLayoutGroup != null) _bodyLayoutGroup.enabled = false;

        // Single tween 1→0 drives BOTH panels in perfect sync
        CancelAnimTween();
        _animTweenId = LeanTween.value(gameObject, 1f, 0f, slideAnimationDuration)
            .setEaseInCubic()
            .setIgnoreTimeScale(true)
            .setOnUpdate((float t) =>
            {
                featureBtnRect.anchoredPosition  = Vector2.Lerp(_featureClosedPos, _featureOpenPos, t);
                gamePlayBtnRect.anchoredPosition = Vector2.Lerp(_gamePlayHiddenPos, _gamePlayOpenPos, t);
                if (gamePlayBtnCanvasGroup != null)
                    gamePlayBtnCanvasGroup.alpha = Mathf.Clamp01(t * 2f);
            })
            .setOnComplete(() =>
            {
                gamePlayBtnRect.gameObject.SetActive(false);
                featureBtnRect.anchoredPosition = _featureClosedPos;
                if (_bodyLayoutGroup != null) _bodyLayoutGroup.enabled = true;
                LayoutRebuilder.ForceRebuildLayoutImmediate(featureBtnRect.parent as RectTransform);
                isGamePlayAnimating = false;
            }).id;
    }

    private void CancelAnimTween()
    {
        if (_animTweenId != -1)
        {
            LeanTween.cancel(_animTweenId);
            _animTweenId = -1;
        }
    }

    private void SetPlayGameButtonColor(Color color)
    {
        if (playGameButton == null) return;
        ColorBlock cb = playGameButton.colors;
        cb.normalColor      = color;
        cb.highlightedColor = new Color(
            Mathf.Clamp01(color.r * 1.15f),
            Mathf.Clamp01(color.g * 1.15f),
            Mathf.Clamp01(color.b * 1.15f), color.a);
        cb.pressedColor = new Color(
            Mathf.Clamp01(color.r * 0.8f),
            Mathf.Clamp01(color.g * 0.8f),
            Mathf.Clamp01(color.b * 0.8f), color.a);
        cb.selectedColor = color;
        playGameButton.colors = cb;
    }

    private void ResetGamePlayState()
    {
        CancelAnimTween();
        if (gamePlayBtnRect != null)
        {
            gamePlayBtnRect.gameObject.SetActive(false);
            if (gamePlayBtnCanvasGroup != null) gamePlayBtnCanvasGroup.alpha = 0f;
        }
        if (_bodyLayoutGroup != null) _bodyLayoutGroup.enabled = true;
        _positionsCached = false;
        isGamePlayOpen = false;
        isGamePlayAnimating = false;
        if (playGameButtonText != null) playGameButtonText.text = "Play Game";
        SetPlayGameButtonColor(playGameNormalColor);
    }

    private void OpenLevelSelect(string gameplayType)
    {
        LevelSelectMenu panel = (LevelSelectMenu)PanelManager.GetSingleton("level_select");
        if (panel != null)
        {
            panel.Open(gameplayType);
        }
    }
    
    private void Customization()
    {
        PanelManager.Open("customization");
    }

    private void Vocabulary()
    {
        // Ensure VocabularyList singleton exists before opening menu
        if (VocabularyList.Instance == null)
        {
            var go = new GameObject("VocabularyList");
            go.AddComponent<VocabularyList>();
        }
        PanelManager.Open("vocabulary");
    }
    
    private async void InitializeFriendsServiceAsync()
    {
        try
        {
            await FriendsService.Instance.InitializeAsync();
            isFriendsServiceInitialized = true;
            friendsButton.interactable = true;
        }
        catch (Exception exception)
        {
            Debug.Log(exception.Message);
        }
    }
    
    private void SignOut()
    {
        ActionConfirmMenu panel = (ActionConfirmMenu)PanelManager.GetSingleton("action_confirm");
        panel.Open(SignOutResult, "Are you sure that you want to sign out?", "Yes", "No");
    }
    
    private void SignOutResult(ActionConfirmMenu.Result result)
    {
        if (result == ActionConfirmMenu.Result.Positive)
        {
            MenuManager.Singleton.SignOut();
            isFriendsServiceInitialized = false;
        }
    }
    
    private void UpdatePlayerNameUI()
    {
        nameText.text = AuthenticationService.Instance.PlayerName;
    }
    
    private void Leaderboards()
    {
        PanelManager.Open("leaderboards");
    }
    
    private void Friends()
    {
        PanelManager.Open("friends");
    }
    
    private void RenamePlayer()
    {
        GetInputMenu panel = (GetInputMenu)PanelManager.GetSingleton("input");
        panel.Open(RenamePlayerConfirm, GetInputMenu.Type.String, 20, "Enter a name for your account.", "Send", "Cancel");
    }
    
    private async void RenamePlayerConfirm(string input)
    {
        renameButton.interactable = false;
        try
        {
            await AuthenticationService.Instance.UpdatePlayerNameAsync(input);
            UpdatePlayerNameUI();
        }
        catch
        {
            ErrorMenu panel = (ErrorMenu)PanelManager.GetSingleton("error");
            panel.Open(ErrorMenu.Action.None, "Failed to change the account name.", "OK");
        }
        renameButton.interactable = true;
    }
    
}