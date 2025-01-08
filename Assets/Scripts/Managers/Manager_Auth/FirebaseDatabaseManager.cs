// using Firebase;
// using Firebase.Database;
// using Firebase.Extensions;
// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;

// public class FirebaseDatabaseManager : MonoBehaviour
// {
//     private DatabaseReference reference;
//     private void Awake(){
//         FirebaseApp app = FirebaseApp.DefaultInstance;
//         reference = FirebaseDatabase.DefaultInstance.RootReference;
//     }

//     void Start(){
//         WriteDatabase("123", "Hello World!");
//         ReadDatabase("123");
//     }

//     public void WriteDatabase(string id, string message){
//         reference.Child("Users").Child(id).SetValueAsync(message).ContinueWithOnMainThread(task => 
//         {
//             if(task.IsCompleted){
//                 Debug.Log("Data written successfully!");
//                 // Update UI or do something with the data
//             }
//             else{
//                 Debug.LogError("Error writing data: " + task.Exception.Message);
//             }

//         });

//     }

//     public void ReadDatabase(string id){
//         reference.Child("Users").Child(id).GetValueAsync().ContinueWithOnMainThread(task =>
//         {
//             if(task.IsCompleted && task.Result.Exists){
//                 DataSnapshot snapshot = task.Result;
//                 string data = task.Result.Value.ToString();
//                 Debug.Log("Data read successfully: " + data);
//                 // Update UI or do something with the data
//             }
//             else{
//                 Debug.LogError("Data does not exist!");
//             }
//         });
//     }
// }
