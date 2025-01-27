using UnityEngine;
using TMPro;
using Firebase;
using Firebase.Extensions;
using Firebase.Firestore;
using System.Collections.Generic;
using System.Text;
using Sirenix.OdinInspector;

public class DisplayRandomID : MonoBehaviour
{
    public TextMeshProUGUI UID_text;
    private FirebaseFirestore firestore;
    public TMP_InputField searchInputField;
    public TextMeshProUGUI mailText;
    public TextMeshProUGUI nameText;
    public GameObject resultData;

    public static DisplayRandomID instance;
    public AR_FriendsHandler friendsHandler;

    [Space(10)]
    [Header("friends list data")]
    [SerializeField] private GameObject[] _friendsGameobjects;
    [SerializeField] private GameObject _searchFriendElement;
    [SerializeField] private Transform _searchElementHolder;
    [SerializeField] private GameObject _invalidDetailsPopUp;

    void Start()
    {
        if (instance == null)
        {
            instance = this;
        }
        UID_text.text = "Loading Player UID...";

        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        {
            if (task.Result == DependencyStatus.Available)
            {
                firestore = FirebaseFirestore.DefaultInstance;
                LoadPlayerIDFromFirebase();
            }
            else
            {
                Debug.LogError("Could not resolve all Firebase dependencies: " + task.Result);
                UID_text.text = "Error loading Player UID";
            }
        });

        // Initialize the FriendRequestHandler
        //FR   friendRequestHandler = FindObjectOfType<FriendRequestHandler>();
    }

    void LoadPlayerIDFromFirebase()
    {
        string userId = AR_CloudData.instance.user_ID;
        if (string.IsNullOrEmpty(userId))
        {
            Debug.LogError("User ID is not set.");
            UID_text.text = "Error: User ID not set";
            return;
        }

        DocumentReference playerProfileRef = firestore.Collection(userId).Document(AR_CloudData.instance.Playerprofiel);
        Debug.Log("@cc Attempting to access the UniqueID field from the Path" + playerProfileRef.Path);

        playerProfileRef.GetSnapshotAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted)
            {
                DocumentSnapshot snapshot = task.Result;
                if (snapshot.Exists && snapshot.ContainsField("UniqueID"))
                {
                    string randomID = snapshot.GetValue<string>("UniqueID");
                    UID_text.text = "Player UID: " + randomID;
                    Debug.Log("Loaded Unique ID from Firebase: " + randomID);

                    // Ensure the ID is also stored in GlobalData
                    StoreRandomIDInGlobalData(randomID);
                }
                else
                {
                    Debug.LogError("No UniqueID found in the specified Firebase document.");
                    UID_text.text = "Player UID not found in Firebase";

                    string newID = GenerateRandomID(8);
                    Debug.Log("Generated new Unique ID: " + newID);

                    // Store the new UniqueID in Firestore
                    StoreRandomIDInFirestore(newID, playerProfileRef);

                    // Add the new ID to GlobalData
                    StoreRandomIDInGlobalData(newID);
                }
            }
            else
            {
                Debug.LogError("Failed to load Player UID from Firebase: " + task.Exception);
                UID_text.text = "Error loading Player UID";
            }
        });
    }

    void StoreRandomIDInFirestore(string randomID, DocumentReference playerProfileRef)
    {
        Dictionary<string, object> playerData = new Dictionary<string, object>
        {
            { "UniqueID", randomID }
        };

        playerProfileRef.SetAsync(playerData, SetOptions.MergeAll).ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted)
            {
                UID_text.text = "UID: " + randomID;
                Debug.Log("Random ID stored successfully in Firestore.");

                // Also store the UniqueID in the 0.1GlobalData collection
                StoreRandomIDInGlobalData(randomID);
            }
            else
            {
                Debug.LogError("Failed to store random ID in Firestore: " + task.Exception);
            }
        });
    }

    void StoreRandomIDInGlobalData(string randomID)
    {
        string userId = AR_CloudData.instance.user_ID;
        DocumentReference playerProgressRef = firestore
            .Collection(userId)
            .Document(GameConstants.PlayerProgressID);

        playerProgressRef.GetSnapshotAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted)
            {
                DocumentSnapshot snapshot = task.Result;
                if (snapshot.Exists)
                {
                    string arID = snapshot.ContainsField("AR_ID") ? snapshot.GetValue<string>("AR_ID") : "Unknown";
                    string nickName = snapshot.ContainsField("NickName") ? snapshot.GetValue<string>("NickName") : "Unknown";

                    Dictionary<string, object> globalData = new Dictionary<string, object>
                    {
                        { "UniqueID", randomID },
                        { "AR_ID", arID },
                        { "NickName", nickName }
                    };

                    CollectionReference globalDataRef = firestore.Collection("0.1GlobalData");
                    DocumentReference globalDataDocRef = globalDataRef.Document(randomID);

                    globalDataDocRef.SetAsync(globalData).ContinueWithOnMainThread(globalDataTask =>
                    {
                        if (globalDataTask.IsCompleted)
                        {
                            Debug.Log($"Random ID {randomID} successfully stored in 0.1GlobalData collection.");
                        }
                        else
                        {
                            Debug.LogError($"Failed to store Random ID {randomID} in 0.1GlobalData collection: {globalDataTask.Exception}");
                        }
                    });
                }
                else
                {
                    Debug.LogError("PlayerProgress document does not exist or is missing fields.");
                }
            }
            else
            {
                Debug.LogError("Failed to fetch PlayerProgress data: " + task.Exception);
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

    [Button]
    public void GetPlayerInfo(string randomID)
    {
        Debug.Log("Random ID :" + randomID);
        if (string.IsNullOrEmpty(randomID))
        {
            _invalidDetailsPopUp.SetActive(true);
            Debug.LogError("Random ID is empty or null.");
            return;
        }

        DocumentReference globalDataDocRef = firestore.Collection("0.1GlobalData").Document(randomID);
        globalDataDocRef.GetSnapshotAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted)
            {
                DocumentSnapshot snapshot = task.Result;
                if (snapshot.Exists)
                {
                    resultData.SetActive(true);
                    string arID = snapshot.ContainsField("AR_ID") ? snapshot.GetValue<string>("AR_ID") : "Unknown";
                    string nickName = snapshot.ContainsField("NickName") ? snapshot.GetValue<string>("NickName") : "Unknown";

                    Debug.Log($"Fetched data for RandomID {randomID}: AR_ID = {arID}, NickName = {nickName}");
                    /*mailText.text = "Email:" + arID;
                    nameText.text = "Name:" + nickName;*/
                    GameObject searchElement = Instantiate(_searchFriendElement, _searchElementHolder);
                    SearchFriendsElement element = searchElement.GetComponent<SearchFriendsElement>();
                    element.PlayerName.text = "Name:" + nickName;
                    OnUserFound(arID, nickName);
                }
                else
                {
                    Debug.Log($"@F No document found for RandomID: {randomID}");
                    _invalidDetailsPopUp.SetActive(true);
                }
            }
            else
            {
                Debug.LogError($"Failed to fetch data for RandomID: {randomID}. Exception: {task.Exception}");
            }
        });
    }

    public void OnSearchwithID(GameObject itemToActive)
    {
        foreach (var item in _friendsGameobjects)
        {
            item.SetActive(false);
        }
        itemToActive.SetActive(true);
        string inputid = searchInputField.text.ToString();
        GetPlayerInfo(inputid);
    }

    public async void OnUserFound(string playerName, string playerEmail)
    {
        if (string.IsNullOrEmpty(playerName) || string.IsNullOrEmpty(playerEmail))
        {
            Debug.LogError("Player details are incomplete. Cannot send friend request.");
            return;
        }

        Debug.Log($"User found: {playerName} ({playerEmail}). you can send friend request...");
        // Call SendFriendRequest with the player name, email, and receiver's user name
        string receiverUserName = playerName;  // Pass player name as receiverUserName (or another value if required)
        await friendsHandler.SendFriendsRequestbyID(playerEmail, playerName);

    }

}
