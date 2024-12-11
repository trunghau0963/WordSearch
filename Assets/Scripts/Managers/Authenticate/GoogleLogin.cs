// using System.Collections;
// using System.Collections.Generic;
// using System.Threading.Tasks;
// using Firebase.Extensions;
// using Google;
// using UnityEngine;
// using UnityEngine.UI;

// public class GoogleLogin : MonoBehaviour
// {
//     public string GoogleAPI = "1000133012177-57kol01kehd0bojl1esk2njr6vqvh7f9.apps.googleusercontent.com";

//     private GoogleSignInConfiguration configuration;

//     Firebase.DependencyStatus dependencyStatus = Firebase.DependencyStatus.UnavailableOther;
//     Firebase.Auth.FirebaseAuth auth;
//     Firebase.Auth.FirebaseUser user;

//     public Text Username, UserEmail, Alert;

//     public GameObject LoginScreen, ProfileScreen;

//     private void Awake()
//     {
//         configuration = new GoogleSignInConfiguration
//         {
//             WebClientId = GoogleAPI,
//             RequestIdToken = true,
//         };
//         Alert.text = "Ready to login";
//     }

//     private void Start()
//     {
//         InitFirebase();
//     }

//     void InitFirebase()
//     {
//         auth = Firebase.Auth.FirebaseAuth.DefaultInstance;
//     }

//     public void GoogleSignInClick() {
//         GoogleSignIn.Configuration = configuration;
//         GoogleSignIn.Configuration.UseGameSignIn = false;
//         GoogleSignIn.Configuration.RequestIdToken = true;
//         GoogleSignIn.Configuration.RequestEmail = true;

//         Alert.text = "Signing in...";

//         GoogleSignIn.DefaultInstance.SignIn().ContinueWith(OnGoogleAuthenticatedFinished);

//     }

//     void OnGoogleAuthenticatedFinished(Task<GoogleSignInUser> task)
//     {

//         Alert.text = "Check Authen...";
//         if (task.IsFaulted)
//         {
//             Alert.text = "Google Sign-In encountered an error";
//             Debug.LogError("Faulted");
//         }
//         else if (task.IsCanceled)
//         {
//             Alert.text = "Google Sign-in was canceled.";
//             Debug.LogError("Cancelled");
//         }
//         else
//         {
//             Firebase.Auth.Credential credential = Firebase.Auth.GoogleAuthProvider.GetCredential(task.Result.IdToken, null);

//             auth.SignInWithCredentialAsync(credential).ContinueWithOnMainThread(task =>
//             {
//                 if (task.IsCanceled)
//                 {
//                     return;
//                 }

//                 if (task.IsFaulted)
//                 {
//                     Debug.LogError("SignInWithCredentialAsync encountered an error: " + task.Exception);
//                     return;
//                 }

//                 user = auth.CurrentUser;

//                 Username.text = user.DisplayName;
//                 UserEmail.text = user.Email;

//                 LoginScreen.SetActive(false);
//                 ProfileScreen.SetActive(true);

//                 // StartCoroutine(LoadImage(CheckImageUrl(user.PhotoUrl.ToString())));
//             });
//         }
//     }

//     // private string CheckImageUrl(string url) {
//     //     if (!string.IsNullOrEmpty(url)) {
//     //         return url;
//     //     }
//     //     return imageUrl;
//     // }

//     // IEnumerator LoadImage(string imageUri) {
//     //     WWW www = new WWW(imageUri);
//     //     yield return www;

//     //     UserProfilePic.sprite = Sprite.Create(www.texture, new Rect(0, 0, www.texture.width, www.texture.height), new Vector2(0, 0));
//     // }
// }