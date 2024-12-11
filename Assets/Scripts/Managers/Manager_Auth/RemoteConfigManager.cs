using System.Collections;
using System.Collections.Generic;
using Firebase.RemoteConfig;
using System.Threading.Tasks;
using UnityEngine;
using System;
using Firebase.Extensions;

public class RemoteConfigManager : MonoBehaviour
{

    private void Awake(){
        CheckRemoteConfigValue();
    }

    public Task CheckRemoteConfigValue()
    {
        Debug.Log("Fetching Data...");
        Task fetchDataTask = FirebaseRemoteConfig.DefaultInstance.FetchAsync(TimeSpan.Zero);
        return fetchDataTask.ContinueWithOnMainThread(FetchCompleted);
    }

    private void FetchCompleted(Task fetchTask)
    {
        // checking for error !
        if (!fetchTask.IsCompleted)
        {
            Debug.LogError("Fetch failed");
            return;
        }

        var remoteConfig = FirebaseRemoteConfig.DefaultInstance;
        var info = remoteConfig.Info;
        if (info.LastFetchStatus != LastFetchStatus.Success)
        {
            Debug.LogError($"{nameof(FetchCompleted)}: was unsuccessful\n{nameof(info.LastFetchStatus)}: {info.LastFetchStatus}");
            return;
        }

        //fetch successfully
        remoteConfig.ActivateAsync()
            .ContinueWithOnMainThread((Task task) => Debug.Log($"Remote Data loaded and ready for use. Last fetch time: {info.FetchTime}"));

            print("Total values loaded: " + remoteConfig.AllValues.Count);
            foreach (var value in remoteConfig.AllValues)
            {
                Debug.Log($"Key: {value.Key}, Value: {value.Value.StringValue}");
            }
    }
}
