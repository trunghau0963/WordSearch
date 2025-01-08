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


// public class Authentication : MonoBehaviour
// {

//     [Header("Firebase")]
//     private bool isLogin = false;
//     public DependencyStatus dependencyStatus;
//     public FirebaseAuth auth;
//     public FirebaseUser User;
//     public DatabaseReference DBreference;

//     [Header("UserData")]
//     // public TMP_InputField usernameField;
//     public TMP_Text usernameField;
//     public TMP_InputField xpField;
//     public TMP_InputField killsField;
//     public TMP_InputField deathsField;

//     public Sprite[] scoreElementImage;
//     public GameObject scoreElement;
//     public Transform scoreboardContent;

//     [Header("UI")]
//     public GameObject userDataUI;
//     public GameObject anonymousDataUI;
//     public GameObject scoreboardUI;
//     public GameObject anonymousScoreboardUI;

//     void Start()
//     {
//         Debug.Log("Authentication");
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
//         if (auth.CurrentUser != User)
//         {
//             bool signedIn = User != auth.CurrentUser && auth.CurrentUser != null;
//             if (!signedIn && User != null)
//             {
//                 isLogin = false;
//                 Debug.Log("Signed out " + User.UserId);
//                 anonymousDataUI.SetActive(true);
//                 anonymousScoreboardUI.SetActive(true);
//                 userDataUI.SetActive(false);
//                 scoreboardUI.SetActive(false);
//             }
//             User = auth.CurrentUser;
//             if (signedIn)
//             {
//                 isLogin = true;
//                 Debug.Log("Signed in " + User.UserId);
//                 StartCoroutine(LoadUserData());
//                 // StartCoroutine(LoadScoreboardData());
//                 yield return new WaitForSeconds(2);
//                 userDataUI.SetActive(true);
//                 scoreboardUI.SetActive(true);
//                 anonymousDataUI.SetActive(false);
//                 anonymousScoreboardUI.SetActive(false);

//                 usernameField.text = User.DisplayName;
//                 Debug.Log("Signed in " + User.UserId);
//             }
//         }
//         // if (User == null)
//         // {
//         //     Debug.Log("User is null");
//         //     anonymousDataUI.SetActive(true);
//         //     anonymousScoreboardUI.SetActive(true);
//         //     userDataUI.SetActive(false);
//         //     scoreboardUI.SetActive(false);
//         // }
//     }

//     // Handle removing subscription and reference to the Auth instance.
//     // Automatically called by a Monobehaviour after Destroy is called on it.
//     void OnDestroy()
//     {
//         auth.StateChanged -= (sender, args) => StartCoroutine(AuthStateChanged(sender, args));
//         auth = null;
//     }

//     public void Reauthenticate(string password)
//     {

//         // Get auth credentials from the user for re-authentication. The example below shows
//         // email and password credentials but there are multiple possible providers,
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

//     public void UpdateUserProfile(string name, string urlPhoto)
//     {
//         if (User != null)
//         {
//             // Reauthenticate(User);
//             UserProfile profile = new()
//             {
//                 // edit the profile here
//                 DisplayName = name,
//                 PhotoUrl = new System.Uri(urlPhoto),
//             };
//             User.UpdateUserProfileAsync(profile).ContinueWith(task =>
//             {
//                 if (task.IsCanceled)
//                 {
//                     Debug.LogError("UpdateUserProfileAsync was canceled.");
//                     return;
//                 }
//                 if (task.IsFaulted)
//                 {
//                     Debug.LogError("UpdateUserProfileAsync encountered an error: " + task.Exception);
//                     return;
//                 }

//                 Debug.Log("User profile updated successfully.");
//             });
//         }
//     }

//     public void UpdateEmail(string UpdateEmail)
//     {

//         User?.SendEmailVerificationBeforeUpdatingEmailAsync(UpdateEmail).ContinueWith(task =>
//             {
//                 if (task.IsCanceled)
//                 {
//                     Debug.LogError("UpdateEmailAsync was canceled.");
//                     return;
//                 }
//                 if (task.IsFaulted)
//                 {
//                     Debug.LogError("UpdateEmailAsync encountered an error: " + task.Exception);
//                     return;
//                 }

//                 Debug.Log("User email updated successfully.");
//             });
//     }

//     public void SendEmailVerification()
//     {
//         User?.SendEmailVerificationAsync().ContinueWith(task =>
//             {
//                 if (task.IsCanceled)
//                 {
//                     Debug.LogError("SendEmailVerificationAsync was canceled.");
//                     return;
//                 }
//                 if (task.IsFaulted)
//                 {
//                     Debug.LogError("SendEmailVerificationAsync encountered an error: " + task.Exception);
//                     return;
//                 }

//                 Debug.Log("Email sent successfully.");
//             });
//     }

//     public void UpdatePassword(string newPassword)
//     {
//         User?.UpdatePasswordAsync(newPassword).ContinueWith(task =>
//             {
//                 if (task.IsCanceled)
//                 {
//                     Debug.LogError("UpdatePasswordAsync was canceled.");
//                     return;
//                 }
//                 if (task.IsFaulted)
//                 {
//                     Debug.LogError("UpdatePasswordAsync encountered an error: " + task.Exception);
//                     return;
//                 }

//                 Debug.Log("Password updated successfully.");
//             });
//     }

//     public void SendPasswordResetEmail(string emailAddress)
//     {
//         if (User != null)
//         {
//             auth.SendPasswordResetEmailAsync(emailAddress).ContinueWith(task =>
//             {
//                 if (task.IsCanceled)
//                 {
//                     Debug.LogError("SendPasswordResetEmailAsync was canceled.");
//                     return;
//                 }
//                 if (task.IsFaulted)
//                 {
//                     Debug.LogError("SendPasswordResetEmailAsync encountered an error: " + task.Exception);
//                     return;
//                 }

//                 Debug.Log("Password reset email sent successfully.");
//             });
//         }
//     }

//     public void SaveDataButton()
//     {
//         StartCoroutine(UpdateUsernameAuth(usernameField.text));
//         StartCoroutine(UpdateUsernameDatabase(usernameField.text));

//         StartCoroutine(UpdateXp(int.Parse(xpField.text)));
//         StartCoroutine(UpdateKills(int.Parse(killsField.text)));
//         StartCoroutine(UpdateDeaths(int.Parse(deathsField.text)));
//     }
//     //Function for the scoreboard button
//     public void ScoreboardButton()
//     {
//         StartCoroutine(LoadScoreboardData());
//     }

//     private IEnumerator UpdateUsernameAuth(string _username)
//     {
//         //Create a user profile and set the username
//         UserProfile profile = new UserProfile { DisplayName = _username };

//         //Call the Firebase auth update user profile function passing the profile with the username
//         Task ProfileTask = User.UpdateUserProfileAsync(profile);
//         //Wait until the task completes
//         yield return new WaitUntil(predicate: () => ProfileTask.IsCompleted);

//         if (ProfileTask.Exception != null)
//         {
//             Debug.LogWarning(message: $"Failed to register task with {ProfileTask.Exception}");
//         }
//         else
//         {
//             //Auth username is now updated
//         }
//     }

//     private IEnumerator UpdateUsernameDatabase(string _username)
//     {
//         //Set the currently logged in user username in the database
//         Task DBTask = DBreference.Child("users").Child(User.UserId).Child("username").SetValueAsync(_username);

//         yield return new WaitUntil(predicate: () => DBTask.IsCompleted);

//         if (DBTask.Exception != null)
//         {
//             Debug.LogWarning(message: $"Failed to register task with {DBTask.Exception}");
//         }
//         else
//         {
//             //Database username is now updated
//         }
//     }

//     private IEnumerator UpdateXp(int _xp)
//     {
//         //Set the currently logged in user xp
//         Task DBTask = DBreference.Child("users").Child(User.UserId).Child("xp").SetValueAsync(_xp);

//         yield return new WaitUntil(predicate: () => DBTask.IsCompleted);

//         if (DBTask.Exception != null)
//         {
//             Debug.LogWarning(message: $"Failed to register task with {DBTask.Exception}");
//         }
//         else
//         {
//             //Xp is now updated
//         }
//     }

//     private IEnumerator UpdateKills(int _kills)
//     {
//         //Set the currently logged in user kills
//         Task DBTask = DBreference.Child("users").Child(User.UserId).Child("kills").SetValueAsync(_kills);

//         yield return new WaitUntil(predicate: () => DBTask.IsCompleted);

//         if (DBTask.Exception != null)
//         {
//             Debug.LogWarning(message: $"Failed to register task with {DBTask.Exception}");
//         }
//         else
//         {
//             //Kills are now updated
//         }
//     }

//     private IEnumerator UpdateDeaths(int _deaths)
//     {
//         //Set the currently logged in user deaths
//         Task DBTask = DBreference.Child("users").Child(User.UserId).Child("deaths").SetValueAsync(_deaths);

//         yield return new WaitUntil(predicate: () => DBTask.IsCompleted);

//         if (DBTask.Exception != null)
//         {
//             Debug.LogWarning(message: $"Failed to register task with {DBTask.Exception}");
//         }
//         else
//         {
//             //Deaths are now updated
//         }
//     }

//     private IEnumerator LoadUserData()
//     {
//         //Get the currently logged in user data
//         Task<DataSnapshot> DBTask = DBreference.Child("users").Child(User.UserId).GetValueAsync();

//         yield return new WaitUntil(predicate: () => DBTask.IsCompleted);

//         if (DBTask.Exception != null)
//         {
//             Debug.LogWarning(message: $"Failed to register task with {DBTask.Exception}");
//         }
//         else if (DBTask.Result.Value == null)
//         {
//             //No data exists yet
//             xpField.text = "0";
//             killsField.text = "0";
//             deathsField.text = "0";
//         }
//         else
//         {
//             //Data has been retrieved
//             DataSnapshot snapshot = DBTask.Result;

//             xpField.text = snapshot.Child("xp").Value.ToString();
//             killsField.text = snapshot.Child("kills").Value.ToString();
//             deathsField.text = snapshot.Child("deaths").Value.ToString();
//         }
//     }

//     private IEnumerator LoadScoreboardData()
//     {
//         if (isLogin)
//         {

//             //Get all the users data ordered by kills amount
//             Task<DataSnapshot> DBTask = DBreference.Child("users").OrderByChild("kills").GetValueAsync();

//             yield return new WaitUntil(predicate: () => DBTask.IsCompleted);

//             if (DBTask.Exception != null)
//             {
//                 Debug.LogWarning(message: $"Failed to register task with {DBTask.Exception}");
//             }
//             else
//             {
//                 // Data has been retrieved
//                 DataSnapshot snapshot = DBTask.Result;

//                 // Destroy any existing scoreboard elements
//                 foreach (Transform child in scoreboardContent.transform)
//                 {
//                     Destroy(child.gameObject);
//                 }

//                 // Get the top 3 users by kills
//                 var topUsers = snapshot.Children
//                     .OrderByDescending(childSnapshot => int.Parse(childSnapshot.Child("kills").Value.ToString()))
//                     .Take(3)
//                     .ToList();

//                 // Process the top 3 users
//                 for (int i = 0; i < topUsers.Count; i++)
//                 {
//                     DataSnapshot childSnapshot = topUsers[i];
//                     int rank = i + 1;
//                     string username = childSnapshot.Child("username").Value.ToString();
//                     int kills = int.Parse(childSnapshot.Child("kills").Value.ToString());
//                     int deaths = int.Parse(childSnapshot.Child("deaths").Value.ToString());
//                     int xp = int.Parse(childSnapshot.Child("xp").Value.ToString());

//                     Debug.Log(username + " - " + kills + " - " + deaths + " - " + xp + " \n");

//                     // Instantiate new scoreboard elements for top users
//                     GameObject scoreboardElement = Instantiate(scoreElement, scoreboardContent);
//                     scoreboardElement.GetComponent<ScoreElement>().NewScoreElementInTop(username, kills, xp, scoreElementImage[i]);
//                 }

//                 // Process the remaining users
//                 var remainingUsers = snapshot.Children
//                     .OrderByDescending(childSnapshot => int.Parse(childSnapshot.Child("kills").Value.ToString()))
//                     .Skip(3)
//                     .ToList();

//                 for (int i = 0; i < remainingUsers.Count; i++)
//                 {
//                     DataSnapshot childSnapshot = remainingUsers[i];
//                     int rank = i + 4; // Rank starts from 4 for the remaining users
//                     string username = childSnapshot.Child("username").Value.ToString();
//                     int kills = int.Parse(childSnapshot.Child("kills").Value.ToString());
//                     int deaths = int.Parse(childSnapshot.Child("deaths").Value.ToString());
//                     int xp = int.Parse(childSnapshot.Child("xp").Value.ToString());

//                     Debug.Log(username + " - " + kills + " - " + deaths + " - " + xp + " \n");

//                     // Instantiate new scoreboard elements for remaining users
//                     GameObject scoreboardElement = Instantiate(scoreElement, scoreboardContent);
//                     scoreboardElement.GetComponent<ScoreElement>().NewScoreElement(username, kills, xp, rank);
//                 }
//             }
//         }
//     }

//     public void SignOutButton()
//     {
//         auth.SignOut();
//         userDataUI.SetActive(false);
//         scoreboardUI.SetActive(false);
//         anonymousDataUI.SetActive(true);
//         anonymousScoreboardUI.SetActive(true);
//         Debug.Log("Signed out");
//         // ClearRegisterFeilds();
//         // ClearLoginFeilds();
//     }

// }
