using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class IntrolControl : MonoBehaviour
{

    public string gameSceneName;
    public bool isMainMenu = false;
    // SoundManagement audioManager;

    [Header("UI")]
    public GameObject mainMenu;
    public GameObject optionMenu;

    public AudioMixer audioMixer;
    public Slider musicSlider;
    public Slider sfxSlider;
    // Start is called before the first frame update
    void Awake()
    {
        // audioManager = GameObject.FindGameObjectWithTag("Audio").GetComponent<SoundManagement>();
        mainMenu.SetActive(true);
        optionMenu.SetActive(false);
    }

    // Update is called once per frame
    void Start()
    {
        if (isMainMenu)
        {
            MusicManager.Instance.PlayMusic("MainMenu");
        }
        else
        {
            MusicManager.Instance.PlayMusic("Intro");
        }
        // SoundManagement.Instance.PlayMusic(1);
    }

    public void Play()
    {
        LevelManager.Instance.LoadScene(gameSceneName, "CrossWipe");
        MusicManager.Instance.PlayMusic("MainMenu");
        // SoundManagement.Instance.PlayMusic(0);
        // SceneManager.LoadScene(gameSceneName);
    }

    // public void Options()
    // {
    //     mainMenu.SetActive(false);
    //     optionMenu.SetActive(true);
    // }

    // public void Back()
    // {
    //     mainMenu.SetActive(true);
    //     optionMenu.SetActive(false);
    // }
    public void Quit()
    {
        Application.Quit();
    }

    public void LogIn()
    {
        LevelManager.Instance.LoadScene("Authenticate", "CrossWipe");
        MusicManager.Instance.PlayMusic("MainMenu");
    }

    public void UpdateMusicVolume(float volume)
    {
        audioMixer.SetFloat("MusicVolume", volume);
    }

    public void UpdateSoundVolume(float volume)
    {
        audioMixer.SetFloat("SFXVolume", volume);
    }

    public void SaveVolume()
    {
        audioMixer.GetFloat("MusicVolume", out float musicVolume);
        PlayerPrefs.SetFloat("MusicVolume", musicVolume);

        audioMixer.GetFloat("SFXVolume", out float sfxVolume);
        PlayerPrefs.SetFloat("SFXVolume", sfxVolume);
    }

    public void LoadVolume()
    {
        musicSlider.value = PlayerPrefs.GetFloat("MusicVolume");
        sfxSlider.value = PlayerPrefs.GetFloat("SFXVolume");
    }
}
