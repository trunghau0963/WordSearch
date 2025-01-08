using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GooglePlayGames;
using GooglePlayGames.BasicApi;
using TMPro;
public class GooglePlayGame : MonoBehaviour
{
    public TMP_Text DetailsText;
    public TMP_Text UserName;
    public TMP_Text UserId;
    [SerializeField] private Transform loginPanel, userPanel;
    // Start is called before the first frame update
    void Start()
    {
        SignIn();
    }

    public void SignIn()
    {
        PlayGamesPlatform.Instance.Authenticate(ProcessAuthentication);
    }
    internal void ProcessAuthentication(SignInStatus status)
    {
        DetailsText.text = "Status: " + status;
        if (status == SignInStatus.Success)
        {
            // Continue with Play Games Services
            loginPanel.gameObject.SetActive(false);
            userPanel.gameObject.SetActive(true);
            string name = PlayGamesPlatform.Instance.GetUserDisplayName();
            string id = PlayGamesPlatform.Instance.GetUserId();
            string ImgUrl = PlayGamesPlatform.Instance.GetUserImageUrl();


            UserName.text = "Success \n " + name;
            UserId.text = "ID: " + id;

        }
        else
        {
            DetailsText.text = "Sign in Failed!!";

            // Disable your integration with Play Games Services or show a login button
            // to ask users to sign-in. Clicking it should call
            // PlayGamesPlatform.Instance.ManuallyAuthenticate(ProcessAuthentication).
        }
    }

}