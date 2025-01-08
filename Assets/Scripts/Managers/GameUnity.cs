using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameUnity : MonoBehaviour
{
    public void loadScene(string sceneName)
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);
    }
}
