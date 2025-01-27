using Photon.Chat;
using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WarGround.Opsive;

public class RealPlayerCount : MonoBehaviour
{
    public List<GameObject> realPlayers = new List<GameObject>();
    public List<string> realusersMail = new List<string>();

    void Start()
    {
       
        StartCoroutine(CheckRealPlayersCountAfterDelay(40f));
    }

    IEnumerator CheckRealPlayersCountAfterDelay(float amount) 
    {
        yield return new WaitForSeconds(amount);

        Debug.Log("Checking for RealPlayers");
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        
        foreach (GameObject player in players)
        {
         AR_PunCharacterData characterData = player.GetComponent<AR_PunCharacterData>();
            AR_PlayerManager playerManager = player.GetComponent<AR_PlayerManager>();   
           if (characterData != null && playerManager != null)
           {

            realPlayers.Add(player);
            realusersMail.Add(characterData.PlayermailID);
                
                // string userID = AR_CloudData.instance.user_ID;
           }

        }

        Debug.Log("Number of real players after 40 seconds: " + realPlayers.Count);
        //Debug.Log("@@Mailid's of real players :" + string.Join(realusersMail));
    }

  
}