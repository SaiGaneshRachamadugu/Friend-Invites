using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Threading.Tasks;
using TMPro;
using Firebase.Firestore;
using Sirenix.OdinInspector;
using System;

public class AR_FriendsHandler : MonoBehaviour
{


    // Start is called before the first frame update
    void Start()
    {
        
    }


    AR_FriendsDataCollection Others_DbFriendCollection = new AR_FriendsDataCollection();
    DocumentReference Others_DocRef;

    async Task GetOthersPlayerDBFriendCollection(string othersmailID)
    {

        Debug.Log("Invitec Reques GetOthersPlayerDBFriendCollection =" + othersmailID);// + "," + othersmailID);

        Others_DbFriendCollection = new AR_FriendsDataCollection();

        Others_DocRef = AR_CloudData.instance.database.Collection(othersmailID).
        Document(GameConstants.DB_FriendsDataCollection);


        DocumentSnapshot Dsnap = await Others_DocRef.GetSnapshotAsync();
        Debug.Log("Invitec Reques GetOthersPlayerDBFriendCollection DocumentSnapshot =" + Others_DocRef);// + "," + othersmailID);

        Others_DbFriendCollection = Dsnap.ConvertTo<AR_FriendsDataCollection>();



    }

    public TextMeshProUGUI inputFieldText; // Assign this in the Unity Inspector

    public void OnSearchButtonClicked()
    {
        if (inputFieldText == null)
        {
            Debug.LogError("Input Field Text is not assigned in the Inspector.");
            return;
        }

        string randomID = inputFieldText.text.Trim();

        if (string.IsNullOrEmpty(randomID))
        {
            Debug.LogError("Random ID is empty or null.");
            return;
        }

        Debug.Log("Searching for user with Random ID: " + randomID);
        InviteFriendsFromRandomID(randomID);
    }

    public void InviteFriendsFromRandomID(string randomID)
    {
        if (string.IsNullOrEmpty(randomID))
        {
            Debug.LogError("Random ID is null or empty in InviteFriendsFromRandomID.");
            return;
        }

        Debug.Log("InviteFriendsFromRandomID triggered for Random ID: " + randomID);
        StartCoroutine(SendFriendRequestCoroutine(randomID));
    }

    private IEnumerator SendFriendRequestCoroutine(string randomID)
    {
        // Adjust the arguments to pass default or necessary values for 'status' and 'index'.
        string status = "request"; // Define the status, e.g., 'request' or any other appropriate value
        int index = -1; // Default value if index is not needed

        Task inviteTask = InviteFriendsRequest(randomID, status, index);

        while (!inviteTask.IsCompleted)
        {
            yield return null;
        }

        if (inviteTask.IsFaulted)
        {
            Debug.LogError("Failed to send friend request: " + inviteTask.Exception);
        }
        else
        {
            Debug.Log("Friend request sent successfully for Random ID: " + randomID);
        }
    }






    /// <summary>
    /// -----------------------------
    /// </summary>
    /// <param name="aa"></param>
    public void InviteFriendsFromTxt(TextMeshProUGUI aa)
    {
        //AR_FirebaseManager.instance.InviteFriendsRequest(aa.text);
        int gameobJectIndex = aa.gameObject.transform.parent.GetSiblingIndex();
        InviteFriendsRequest(aa.text,"request", gameobJectIndex);



    }

    public void AcceptFriendsFromTxt(TextMeshProUGUI aa)
    {
        AcceptFriendsFun(aa.text);

    }
    public void AcceptFriendsFromStr(string aa)
    {
        Debug.Log("AcceptFriendsFromStr=" + aa);

        AcceptFriendsFun(aa,true);

    }

    public async Task AcceptFriendsFun(string playerName,bool ismailIDIncluded=false)
    {
        string othersmailID = "";
        string value_data = "";
        if (ismailIDIncluded)
        {
            string[] stringVal = playerName.Split('~');
            othersmailID = stringVal[2];

            value_data = stringVal[0];
        }
        else
        {
            othersmailID = RealPlayerData.realusersBio[playerName];
            value_data = playerName; // maiID,Name
        }
       


     
        Debug.Log("Add Friends before=" + AR_FirebaseManager.instance.AR_FriendDataCollection.MyFriendsList.Count);

        ////
        /// Add data to MyFriend List
        
        AR_FirebaseManager.instance.AR_FriendDataCollection.MyFriendsList.Add(othersmailID,value_data);


        Debug.Log("Add Friends  After=" + AR_FirebaseManager.instance.AR_FriendDataCollection.MyFriendsList.Count);


        ////
        /// Remove the invite data from our DB data - as we are accepting the invitation
        AR_FirebaseManager.instance.AR_FriendDataCollection.InviteList.Remove(othersmailID);


        Task task = AR_FirebaseManager.instance.FriendsData_DocRef.SetAsync(AR_FirebaseManager.instance.AR_FriendDataCollection);


     
        ///------->> send accept response to ORGINAL PLAYER

        await GetOthersPlayerDBFriendCollection(othersmailID);
        value_data = AR_CloudData.instance.displayName;// maiID,Name

        ////
        /// Add data to MyFriend List -- ORGINAL PLAYER
        Others_DbFriendCollection.MyFriendsList.Add(AR_CloudData.instance.user_ID,value_data);

        ///
        /// Refresh requestList as Accepted in ORGINAL PLAYER
        string mValue = AR_CloudData.instance.displayName + "~accepted";
        Others_DbFriendCollection.RequestList[AR_CloudData.instance.user_ID] = mValue;
        

        task = Others_DocRef.SetAsync(Others_DbFriendCollection);







    }

   



    /// <summary>
    /// INVITE
    ///
    /// A sending invitation to B.... Accessing B profile and sending A data intoFriends InviteList
    /// 
    /// </summary>
    /// <param name="playerName"></param>
    /// <returns></returns>
    public async Task InviteFriendsRequest(string playerName,string status,int index)
    {
        string othersmailID = RealPlayerData.realusersBio[playerName];
        Debug.Log("InviteFriendsRequest init=" + playerName);// + "," + othersmailID);
        string value_data = AR_CloudData.instance.displayName + "~"+ status+"~"+ AR_CloudData.instance.user_ID;

        await GetOthersPlayerDBFriendCollection(othersmailID);


        Debug.Log("InviteFriends got Dsnap of others before =" + Others_DbFriendCollection.InviteList.Count);
        bool alreadyRequested = false;
        foreach (var FrindInvitationDic in Others_DbFriendCollection.InviteList.Keys)
        {
            if (FrindInvitationDic  == AR_CloudData.instance.user_ID)
            {
                alreadyRequested = true;
            }
        }

        if (alreadyRequested ==false)
        {
            Others_DbFriendCollection.InviteList.Add(AR_CloudData.instance.user_ID, value_data);

            Debug.Log("InviteFriends got Dsnap of others After=" + Others_DbFriendCollection.InviteList.Count);

           // Task task = Others_DocRef.SetAsync(Others_DbFriendCollection);

            SendFriendsRequest(playerName);
           // SendFriendsRequestbyID(playerName, value_data);

        }

          Others_DbFriendCollection.TempInviteList.Add(AR_CloudData.instance.user_ID, value_data);

      
        Task task = Others_DocRef.SetAsync(Others_DbFriendCollection);

        Debug.Log("InviteFriends DONE");

       
    }
    /// <summary>
    /// 
    /// REQUEST LIST
    /// 
    /// </summary>
    /// <param name="playerName"></param>
    /// <returns></returns>
    [Button("SendFriendsRequest")]
    public async Task SendFriendsRequest(string playerName="hello")
    {

        string othersmailID =  RealPlayerData.realusersBio[playerName];
        Debug.Log("request SendFriendsRequest init=" + playerName+" , "+ othersmailID);


        string value_data = playerName + "~weInvited";
        Debug.Log("request SendFriendsRequest before=" + AR_FirebaseManager.instance.AR_FriendDataCollection.RequestList.Count);

        AR_FirebaseManager.instance.AR_FriendDataCollection.RequestList.Add(othersmailID, value_data);


        Debug.Log("request SendFriendsRequest After=" + AR_FirebaseManager.instance.AR_FriendDataCollection.RequestList.Count);
        Debug.Log("request SendFriendsRequest After=" + AR_FirebaseManager.instance.FriendsData_DocRef);

        Task task = AR_FirebaseManager.instance.FriendsData_DocRef.SetAsync(AR_FirebaseManager.instance.AR_FriendDataCollection);

        Debug.Log("Request Added DONE");

    }

    [Button("SendFriendsRequest")]
    public async Task SendFriendsRequestbyID(string othersmailID, string playerName)
    {
        if (string.IsNullOrEmpty(othersmailID) || string.IsNullOrEmpty(playerName))
        {
            Debug.LogError("Invalid user details. Friend request cannot be sent.");
            return;
        }

        Debug.Log($"SendFriendsRequest init: PlayerName={othersmailID}, OthersMailID={playerName}");

       
        string value_data = AR_CloudData.instance.displayName + "~weInvited~" + AR_CloudData.instance.user_ID;

       
        if (!AR_FirebaseManager.instance.AR_FriendDataCollection.RequestList.ContainsKey(othersmailID))
        {
            AR_FirebaseManager.instance.AR_FriendDataCollection.RequestList[othersmailID] = value_data;

            try
            {
                await AR_FirebaseManager.instance.FriendsData_DocRef.SetAsync(AR_FirebaseManager.instance.AR_FriendDataCollection);
                Debug.Log("RequestList updated successfully in your collection.");
            }
            catch (Exception e)
            {
                Debug.LogError("Error updating RequestList in your collection: " + e.Message);
            }
        }
        else
        {
            Debug.LogWarning("The user is already in your RequestList.");
        }

      
        await GetOthersPlayerDBFriendCollection(playerName); 

        if (Others_DbFriendCollection != null && !Others_DbFriendCollection.InviteList.ContainsKey(AR_CloudData.instance.user_ID))
        {
            Others_DbFriendCollection.InviteList[AR_CloudData.instance.user_ID] = value_data;

            try
            {
                await Others_DocRef.SetAsync(Others_DbFriendCollection);
                Debug.Log("InviteList updated successfully in the target user's collection.");
            }
            catch (Exception e)
            {
                Debug.LogError("Error updating InviteList in the target user's collection: " + e.Message);
            }
        }
        else
        {
            Debug.LogWarning("The user is already in the target's InviteList or target collection is null.");
        }
    }


    public void OnSendFriendRequestButtonClick(string othersmailID, string playerName)
    {
        Debug.Log("@FR_friendRequest Sent Successfully :" + othersmailID + " ," + playerName);
        if (!string.IsNullOrEmpty(othersmailID) && !string.IsNullOrEmpty(playerName))
        {
            _ = SendFriendsRequestbyID(othersmailID, playerName);
        }
        else
        {
            Debug.LogError("Invalid user details. Cannot send friend request.");
        }
    }




}
