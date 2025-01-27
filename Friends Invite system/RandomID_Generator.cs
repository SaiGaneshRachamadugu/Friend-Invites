using UnityEngine;
using System.Collections.Generic;
using System.Text;
using Firebase;
using Firebase.Extensions;
using Firebase.Firestore;
using TMPro;
using Photon.Pun;
using Photon.Realtime;
using Sirenix.OdinInspector;

public class RandomID_Generator : MonoBehaviourPunCallbacks
{
    public const string PlayerIDKey = "PlayerID";
    public static RandomID_Generator instance;

    private FirebaseFirestore firestore;
    private DocumentReference playerProfileRef;
    private string UID;

    void Start()
    {
        Debug.Log("Starting Firebase initialization...");

        // Initialize Firebase
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        {
            if (task.Result == DependencyStatus.Available)
            {
                Debug.Log("Firebase dependencies are available.");
                InitializeFirebase();
            }
            else
            {
                Debug.LogError("Could not resolve all Firebase dependencies: " + task.Result);
            }
        });
    }

    void InitializeFirebase()
    {
        Debug.Log("Initializing Firebase...");
        firestore = FirebaseFirestore.DefaultInstance;

        // Check and load or assign random ID
        LoadOrAssignRandomID();
    }

    public void LoadOrAssignRandomID()
    {
        string userId = AR_CloudData.instance.user_ID;

        if (string.IsNullOrEmpty(userId))
        {
            Debug.LogError("User ID is not set.");
            return;
        }

        // Set reference to user document in Firebase
        playerProfileRef = firestore.Collection(userId).Document(AR_CloudData.instance.Playerprofiel);

        // Check if an ID already exists in Firebase
        playerProfileRef.GetSnapshotAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted)
            {
                DocumentSnapshot snapshot = task.Result;
                if (snapshot.Exists && snapshot.ContainsField("UniqueID"))
                {
                    // ID exists, load it
                    string existingID = snapshot.GetValue<string>("UniqueID");
                    Debug.Log("Loaded existing ID from Firebase: " + existingID);

                    // Set the ID in PlayerPrefs and Photon
                    PlayerPrefs.SetString(PlayerIDKey, existingID);
                    PlayerPrefs.Save();
                    SetPhotonPlayerProperty(existingID);
                }
                else
                {

                    // ID does not exist, generate a new one
                    string newID = GenerateRandomID(8);
                    Debug.Log("Generated new Random ID: " + newID);

                    // Store the ID in PlayerPrefs and Firebase
                    PlayerPrefs.SetString(PlayerIDKey, newID);
                    PlayerPrefs.Save();
                    StoreRandomIDInFirestore(newID);
                    SetPhotonPlayerProperty(newID);
                }
            }
            else
            {
                Debug.LogError("Failed to load ID from Firebase: " + task.Exception);
            }
        });
    }

    void StoreRandomIDInFirestore(string randomID)
    {
        Dictionary<string, object> playerData = new Dictionary<string, object>
        {
            { "UniqueID", randomID }
        };

        playerProfileRef.SetAsync(playerData, SetOptions.MergeAll).ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted)
            {
                Debug.Log("Random ID stored successfully in Firestore.");
            }
            else
            {
                Debug.LogError("Failed to store random ID in Firestore: " + task.Exception);
            }
        });
    }

    string GenerateRandomID(int length)
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890";

        StringBuilder stringBuilder = new StringBuilder();
        System.Random random = new System.Random();

        for (int i = 0; i < length; i++)
        {
            stringBuilder.Append(chars[random.Next(chars.Length)]);
        }

        return stringBuilder.ToString();
    }

    void SetPhotonPlayerProperty(string randomID)
    {
        if (PhotonNetwork.LocalPlayer != null)
        {
            ExitGames.Client.Photon.Hashtable customProperties = new ExitGames.Client.Photon.Hashtable
            {
                { PlayerIDKey, randomID }
            };
            PhotonNetwork.LocalPlayer.SetCustomProperties(customProperties);
            Debug.Log("Random ID set in Photon player properties.");
        }
        else
        {
            Debug.LogError("Photon Network LocalPlayer is null.");
        }
    }

    [Button]
    public void PrintAllPlayerRandomIDs()
    {
        foreach (Player player in PhotonNetwork.PlayerList)
        {
            if (player.CustomProperties.ContainsKey(PlayerIDKey))
            {
                string randomID = player.CustomProperties[PlayerIDKey].ToString();
                Debug.Log($"Player {player.NickName} has random ID: {randomID}");
            }
        }
    }

    [Button]
    public void GetPlayerInfoByRandomID(string randomID)
    {
        // Construct the path to the player’s collection using the user's email (user_ID)
        // Assuming AR_CloudData.instance.user_ID is the user email and unique for each player
        string userCollectionPath = AR_CloudData.instance.user_ID; // Using user_ID to point to the collection

        // Reference to the player's collection
        DocumentReference playerDocRef = firestore.Collection(userCollectionPath).Document(AR_CloudData.instance.Playerprofiel);

        // Get the player's document data
        playerDocRef.GetSnapshotAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted)
            {
                DocumentSnapshot snapshot = task.Result;

                if (snapshot.Exists && snapshot.ContainsField("UniqueID"))
                {
                    string existingID = snapshot.GetValue<string>("UniqueID");

                    // If the UniqueID in the document matches the searched randomID, retrieve data
                    if (existingID == randomID)
                    {
                   
                        DocumentReference playerProgressRef = firestore
                            .Collection(userCollectionPath)  // User's collection
                            .Document("PlayerProgress");    // PlayerProgress document

                        playerProgressRef.GetSnapshotAsync().ContinueWithOnMainThread(progressTask =>
                        {
                            if (progressTask.IsCompleted)
                            {
                                DocumentSnapshot progressSnapshot = progressTask.Result;

                                if (progressSnapshot.Exists)
                                {
                                  
                                    string email = progressSnapshot.ContainsField("AR_ID") ? progressSnapshot.GetValue<string>("AR_ID") : "No email found";
                                    string displayName = progressSnapshot.ContainsField("NickName") ? progressSnapshot.GetValue<string>("NickName") : "No display name found";

                                    Debug.Log($"Player with Random ID {randomID} has Email: {email} and Display Name: {displayName}");
                                }
                                else
                                {
                                    Debug.LogError("No PlayerProgress document found.");
                                }
                            }
                            else
                            {
                                Debug.LogError("Failed to fetch PlayerProgress document: " + progressTask.Exception);
                            }
                        });
                    }
                    else
                    {
                        Debug.LogError($"No player found with Random ID {randomID}.");
                    }
                }
                else
                {
                    Debug.LogError("Failed to fetch player document or UniqueID field not found.");
                }
            }
            else
            {
                Debug.LogError("Failed to fetch player document: " + task.Exception);
            }
        });
    }

}
