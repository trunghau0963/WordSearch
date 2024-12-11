using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Pulse : MonoBehaviour
{
    public bool coroutineAllowed = false;
    public float scaleSpeed;
    public float scaleSize;
    public float duration;
    public float waitDuration;

    // Start is called before the first frame update
    void Start()
    {
        coroutineAllowed = true;
    }

    void OnEnable()
    {
        InvokeRepeating("StartPulseCoroutine", 0f, duration); // Call StartPulseCoroutine every 'duration' seconds
    }

    void OnDisable()
    {
        CancelInvoke("StartPulseCoroutine");
    }

    // Update is called once per frame
    void Update()
    {
        if (gameObject.activeSelf)
        {
            InvokeRepeating("StartPulseCoroutine", 0f, duration);
        }
    }


    public void StartPulsePublic(){
        InvokeRepeating("StartPulseCoroutine", 0f, duration);
    }

#pragma warning disable IDE0051 // Remove unused private members
    private void StartPulseCoroutine()
#pragma warning restore IDE0051 // Remove unused private members
    {
        if (coroutineAllowed)
        {
            StartCoroutine(StartPulse());
        }
    }

    private IEnumerator StartPulse()
    {
        coroutineAllowed = false;
        for (float i = 0f; i < 1f; i += scaleSpeed) // Slower increment
        {
            transform.localScale = new Vector3(
                Mathf.Lerp(transform.localScale.x, transform.localScale.x + scaleSize, Mathf.SmoothStep(0f, 1f, i)),
                Mathf.Lerp(transform.localScale.y, transform.localScale.y + scaleSize, Mathf.SmoothStep(0f, 1f, i)),
                Mathf.Lerp(transform.localScale.z, transform.localScale.z + scaleSize, Mathf.SmoothStep(0f, 1f, i))
            );
            yield return new WaitForSeconds(waitDuration); // Longer wait time
        }

        for (float i = 0; i < 1f; i += scaleSpeed) // Slower increment
        {
            transform.localScale = new Vector3(
                Mathf.Lerp(transform.localScale.x, transform.localScale.x - scaleSize, Mathf.SmoothStep(0f, 1f, i)),
                Mathf.Lerp(transform.localScale.y, transform.localScale.y - scaleSize, Mathf.SmoothStep(0f, 1f, i)),
                Mathf.Lerp(transform.localScale.z, transform.localScale.z - scaleSize, Mathf.SmoothStep(0f, 1f, i))
            );
            yield return new WaitForSeconds(waitDuration); // Longer wait time
        }

        coroutineAllowed = true;
    }
}