// using System;
// using System.Collections.Generic;
// using Firebase;
// using Firebase.Database;
// using Firebase.Extensions;
// using UnityEngine;

// public class FirebaseManager : MonoBehaviour
// {
//     private DatabaseReference dbReference;

//     void Start()
//     {
//         // Initialize Firebase Database
//         FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task => {
//             if (task.Result == DependencyStatus.Available)
//             {
//                 dbReference = FirebaseDatabase.DefaultInstance.RootReference;
//                 Debug.Log("Firebase initialized successfully.");
//             }
//             else
//             {
//                 Debug.LogError($"Could not resolve Firebase dependencies: {task.Result}");
//             }
//         });
//     }

//     // Get user info
//     private void GetUserInfo(string userId)
//     {
//         Debug.Log("get user info");
//         dbReference.Child("users").Child(userId).Child("info").GetValueAsync().ContinueWithOnMainThread(task => {
//             if (task.IsCompleted)
//             {
//                 DataSnapshot snapshot = task.Result;
//                 if (snapshot.Exists)
//                 {
//                     Debug.Log($"User Info: {snapshot.GetRawJsonValue()}");
//                 }
//                 else
//                 {
//                     Debug.Log("User info not found.");
//                 }
//             }
//             else
//             {
//                 Debug.LogError("Failed to retrieve user info.");
//             }
//         });
//     }

//     // Get user stats
//     private void GetUserStats(string userId)
//     {
//         Debug.Log("get user stats");
//         dbReference.Child("users").Child(userId).Child("stats").GetValueAsync().ContinueWithOnMainThread(task => {
//             if (task.IsCompleted)
//             {
//                 DataSnapshot snapshot = task.Result;
//                 if (snapshot.Exists)
//                 {
//                     Debug.Log($"User Stats: {snapshot.GetRawJsonValue()}");
//                 }
//                 else
//                 {
//                     Debug.Log("User stats not found.");
//                 }
//             }
//             else
//             {
//                 Debug.LogError("Failed to retrieve user stats.");
//             }
//         });
//     }

//     // Get purchased items
//     private void GetPurchasedItems(string userId)
//     {
//         Debug.Log("get user purchase items");
//         dbReference.Child("users").Child(userId).Child("purchasedItems").GetValueAsync().ContinueWithOnMainThread(task => {
//             if (task.IsCompleted)
//             {
//                 DataSnapshot snapshot = task.Result;
//                 if (snapshot.Exists)
//                 {
//                     Debug.Log($"Purchased Items: {snapshot.GetRawJsonValue()}");
//                 }
//                 else
//                 {
//                     Debug.Log("No purchased items found.");
//                 }
//             }
//             else
//             {
//                 Debug.LogError("Failed to retrieve purchased items.");
//             }
//         });
//     }

//     // Update user info
//     private void UpdateUserInfo(string userId, string name, string email, string profilePicture)
//     {
//         Debug.Log("update user info");  
//         var userInfo = new Dictionary<string, object>
//         {
//             { "name", name },
//             { "email", email },
//             { "profilePicture", profilePicture },
//             { "createdAt", DateTime.UtcNow.ToString("o") }
//         };

//         dbReference.Child("users").Child(userId).Child("info").UpdateChildrenAsync(userInfo).ContinueWithOnMainThread(task => {
//             if (task.IsCompleted)
//             {
//                 Debug.Log("User info updated successfully.");
//             }
//             else
//             {
//                 Debug.LogError("Failed to update user info.");
//             }
//         });
//     }

//     // Update user stats
//     private void UpdateUserStats(string userId, int level, int points)
//     {
//         var userStats = new Dictionary<string, object>
//         {
//             { "level", level },
//             { "points", points }
//         };

//         dbReference.Child("users").Child(userId).Child("stats").UpdateChildrenAsync(userStats).ContinueWithOnMainThread(task => {
//             if (task.IsCompleted)
//             {
//                 Debug.Log("User stats updated successfully.");
//             }
//             else
//             {
//                 Debug.LogError("Failed to update user stats.");
//             }
//         });
//     }

//     // Add purchased item
//     private void AddPurchasedItem(string userId, string itemId, string name, string description)
//     {
//         var itemInfo = new Dictionary<string, object>
//         {
//             { "name", name },
//             { "description", description },
//             { "purchasedAt", DateTime.UtcNow.ToString("o") }
//         };

//         dbReference.Child("users").Child(userId).Child("purchasedItems").Child(itemId).SetValueAsync(itemInfo).ContinueWithOnMainThread(task => {
//             if (task.IsCompleted)
//             {
//                 Debug.Log("Purchased item added successfully.");
//             }
//             else
//             {
//                 Debug.LogError("Failed to add purchased item.");
//             }
//         });
//     }

//     public void TestDatabase(){
//         UpdateUserInfo("userId1", "Nguyen Van A", "user1@example.com", "url-to-profile-picture");
//         // UpdateUserStats("userId1", 6, 1500);
//         // AddPurchasedItem("userId1", "itemId1", "Sword of Destiny", "A legendary sword with immense power");
//     }

//     public void TestGetDatabase(){
//         GetUserInfo("userId1");
//         GetUserStats("userId1");
//         GetPurchasedItems("userId1");
//     }
// }
