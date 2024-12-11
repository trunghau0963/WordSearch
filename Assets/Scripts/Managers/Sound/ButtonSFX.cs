using UnityEngine;
using UnityEngine.UI;

public class ButtonSFX : MonoBehaviour
{
    private SoundManagement audioSource;
    public int sfxClipIndex = 0; // Index of the SFX clip to play

    void Awake()
    {
        audioSource = SoundManagement.Instance;
        if (TryGetComponent<Button>(out var button))
        {
            button.onClick.AddListener(PlaySFX);
        }
    }

    void PlaySFX()
    {
        if (audioSource != null && audioSource.sfxClipsList.Count >= sfxClipIndex + 1)
        {
            audioSource.PlaySFX(audioSource.sfxClipsList[sfxClipIndex]);
        }
    }
}