using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManagement : MonoBehaviour
{
    private static SoundManagement instance;

    [Header("Audio Source------------------------------------------------")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioSource sfxSource;

    [Header("Audio Clips------------------------------------------------")]
    [Tooltip("Add your audio clips here")]  // Tooltip will show up in the Unity inspector as a helpful hint
    [SerializeField] public List<AudioClip> audioClipsList;
    [Tooltip("Add your sfx clips here")]
    [SerializeField] public List<AudioClip> sfxClipsList;

    public static SoundManagement Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindAnyObjectByType<SoundManagement>();
                if (instance == null)
                {
                    GameObject singletonObject = new();
                    instance = singletonObject.AddComponent<SoundManagement>();
                    singletonObject.name = typeof(SoundManagement).ToString() + " (Singleton)";
                    DontDestroyOnLoad(singletonObject);
                }
            }
            return instance;
        }
    }

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }

    // void Start()
    // {
    //     PlayMainMenuBg();
    // }

    // public void PlayMainMenuBg(){
    //     audioSource.clip = audioClipsList[0];
    //     audioSource.loop = dqsddqe;d
    //     audioSource.Play();
    // }

    public void PlayMusic(int idx){
        audioSource.clip = audioClipsList[idx];
        audioSource.loop = true;
        audioSource.Play();
    }

    public void PlaySFX(AudioClip clip)
    {
        sfxSource.PlayOneShot(clip);
    }
}