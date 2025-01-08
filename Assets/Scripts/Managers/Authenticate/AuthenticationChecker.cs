// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;
// using Firebase;
// using Firebase.Auth;
// using Firebase.Database;
// using TMPro;
// using System.Linq;
// using System.Threading.Tasks;
// using UnityEngine.SceneManagement;
// using UnityEngine.UI;


// public class AuthenticationChecker : MonoBehaviour
// {

//     [Header("Firebase")]
//     private bool isLogin = false;
//     DependencyStatus dependencyStatus;
//     FirebaseAuth auth;
//     FirebaseUser User;
//     DatabaseReference DBreference;

//     IEnumerator Start()
//     {
//         Debug.Log("Authentication");
//         yield return new WaitForSeconds(2);
//         InitializeFirebase();
//     }
//     void InitializeFirebase()
//     {
//         auth = FirebaseAuth.DefaultInstance;
//         DBreference = FirebaseDatabase.DefaultInstance.RootReference;
//         auth.StateChanged += (sender, args) => StartCoroutine(AuthStateChanged(sender, args));
//         StartCoroutine(AuthStateChanged(this, null));
//     }

//     IEnumerator AuthStateChanged(object sender, System.EventArgs eventArgs)
//     {
//         Debug.Log("AuthStateChanged");
//         Debug.Log("currentUser " + auth.CurrentUser);
//         Debug.Log("User " + User);
//         // Debug.Log("User id" + User.UserId);
//         if (auth.CurrentUser != User)
//         {
//             bool signedIn = User != auth.CurrentUser && auth.CurrentUser != null;
//             if (!signedIn && User != null)
//             {
//                 Debug.Log("Signed out " + User.UserId);
//                 LevelManager.Instance.LoadScene("IntroScene", "CrossWipe");

//             }
//             User = auth.CurrentUser;
//             if (signedIn)
//             {
//                 Debug.Log("Signed in " + User.UserId);
//                 if (isLogin)
//                 {
//                     yield return new WaitForSeconds(1);
//                     LevelManager.Instance.LoadScene("MainMenu", "CrossWipe");
//                 }
//             }
//         }
//         if(User != null)
//         {
//             yield return new WaitForSeconds(1);
//             LevelManager.Instance.LoadScene("MainMenu", "CrossWipe");
//         }
//         else {
//             yield return new WaitForSeconds(1);
//             LevelManager.Instance.LoadScene("IntroScene", "CrossWipe");
//         }
        
//     }
//     // void OnDestroy()
//     // {
//     //     auth.StateChanged -= (sender, args) => StartCoroutine(AuthStateChanged(sender, args));
//     //     auth = null;
//     // }

//     public void Reauthenticate(string password)
//     {

//         // such as GoogleAuthProvider or FacebookAuthProvider.
//         Credential credential = EmailAuthProvider.GetCredential(User.Email, password);

//         User?.ReauthenticateAsync(credential).ContinueWith(task =>
//             {
//                 if (task.IsCanceled)
//                 {
//                     Debug.LogError("ReauthenticateAsync was canceled.");
//                     return;
//                 }
//                 if (task.IsFaulted)
//                 {
//                     Debug.LogError("ReauthenticateAsync encountered an error: " + task.Exception);
//                     return;
//                 }

//                 Debug.Log("User reauthenticated successfully.");
//             });
//     }
// }
