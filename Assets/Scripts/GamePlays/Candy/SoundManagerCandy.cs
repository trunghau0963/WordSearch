using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManagerCandy : MonoBehaviour
{
    // Start is called before the first frame update
    public AudioSource[] destroyNoise;
    // Update is called once per frame
    public void PlayRandomDestroyNoise()
    {
        int clipToPlay = Random.Range(0, destroyNoise.Length);
        destroyNoise[clipToPlay].Play();
    }   
}
