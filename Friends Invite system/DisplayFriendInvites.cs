using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Firebase.Firestore;
using System.Threading.Tasks;
using UnityEngine.UI;

public class DisplayFriendInvites : MonoBehaviour
{
    public AR_FriendsHandler friendsHandler;

    public GameObject Inbox_elements;
    public GameObject friends_elements;
    public GameObject Myfriends_holder;
    public GameObject MyInbox_holder;

    
    private AR_FirebaseManager firebaseManager;
    private bool checkOnce = true;
    private void Start()
    {
        firebaseManager = AR_FirebaseManager.instance;

        DisplayFriendsList();
        // DisplayInvitesList();
       // DisplayPlayerLevel();
       
    }


    public void switchToggle(GameObject highlightObj)
    {
        Myfriends_holder.transform.parent.parent.gameObject.SetActive(false);
        MyInbox_holder.transform.parent.parent.gameObject.SetActive(false);


        highlightObj.SetActive(true);
    }


    public void DisplayFriendsList()
    {
        DisplayFriendsListAsync();
    }
    private GameObject[] friendsObj;
    public async void DisplayFriendsListAsync()
    {
        Myfriends_holder.transform.parent.parent.gameObject.SetActive(true);


        if (firebaseManager.AR_FriendDataCollection != null)
        {

            Debug.Log("@ff myfriends="+ firebaseManager.AR_FriendDataCollection.MyFriendsList.Count);
            if (firebaseManager.AR_FriendDataCollection.MyFriendsList.Count == 0 )
            {
                return;
            }




            foreach (Transform child in Myfriends_holder.transform)
            {
                Destroy(child.gameObject);
            }



            //  Myfriends.gameObject.SetActive(true);
            Dictionary<string, string> friendsList = firebaseManager.AR_FriendDataCollection.MyFriendsList;
            foreach (KeyValuePair<string, string> friend in friendsList)
            {
                GameObject frndobj = Instantiate(friends_elements, Myfriends_holder.transform);
                frndobj.transform.GetChild(6).GetComponent<Button>().onClick.AddListener(OnPokeButton);
                frndobj.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = friend.Value;

                string levelNum = await GetPlayerLevel(friend.Key);
                frndobj.transform.GetChild(3).GetComponent<TextMeshProUGUI>().text = "Lvl: "+levelNum; 

                
                //friendsListText.text =  friend.Value + "\n";
            }
          
        }
        else
        {
            Debug.LogError("AR_FriendDataCollection is null.");
        }
    }

    public void OnPokeButton()
    {
        AndroidNotificationController.instance.ShowToastMessage_Android("Feature available soon");

        //Update logic for Poke functionality;

    }

    public void DisplayInvitesList()
    {
        MyInbox_holder.transform.parent.parent.gameObject.SetActive(true);

        if (firebaseManager.AR_FriendDataCollection != null)
        {

            if (firebaseManager.AR_FriendDataCollection.InviteList.Count==0 || checkOnce == false)
            {
                return;
            }
           
            Dictionary<string, string> invitesList = firebaseManager.AR_FriendDataCollection.InviteList;
           
            foreach (KeyValuePair<string, string> inviteDIC in invitesList)
            {
                string[] mval = inviteDIC.Value.Split('~'); ;
                GameObject Inviteobj = Instantiate(Inbox_elements, MyInbox_holder.transform);
                Inviteobj.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = mval[0];
               // Inviteobj.GetComponent<FriendsInBoxHandler>().f_mailID = inviteDIC.Value;// mval[2];



                Inviteobj.GetComponent<FriendsInBoxHandler>().acceptButton.onClick.RemoveAllListeners();

                Inviteobj.GetComponent<FriendsInBoxHandler>().acceptButton.onClick.AddListener(() => {

                    friendsHandler.AcceptFriendsFromStr(inviteDIC.Value);
                    Inviteobj.gameObject.SetActive(false);
                 //   Invoke(nameof(DisplayFriendsList), 1);
                });

            }

            checkOnce = false;




            

        }
        else
        {
            Debug.LogError("AR_FriendDataCollection is null.");
        }
    }

    DocumentReference Others_DocRef;
    private async Task<string> GetPlayerLevel(string DocID)
    {
        Others_DocRef = AR_CloudData.instance.database.Collection(DocID).
      Document(GameConstants.PlayerProgressID);

        DocumentSnapshot Dsnap = await Others_DocRef.GetSnapshotAsync();


        AR_PlayerProgress Others_DbProgressCollection = new AR_PlayerProgress();

        Others_DbProgressCollection = Dsnap.ConvertTo<AR_PlayerProgress>();


        return Others_DbProgressCollection.LevelNo.ToString();
    }


    //private void DisplayPlayerLevel()
    //{
    //    AR_PlayerStatsManager playerStatsManager = AR_PlayerStatsManager.instance;
    //    if (playerStatsManager != null)
    //    {
    //        int playerLevel = playerStatsManager.PlayerProgress.LevelNo;
    //        friends_elements.transform.GetChild(3).GetComponent<TextMeshProUGUI>().text = playerLevel.ToString();
    //        Debug.Log("@@PlayerLevel :" + playerLevel);
    //    }
    //    else
    //    {
    //        Debug.LogError("AR_PlayerStatsManager instance is null.");
    //    }
    //}

 


}
