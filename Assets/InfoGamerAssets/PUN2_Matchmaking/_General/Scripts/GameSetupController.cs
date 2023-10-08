using Photon.Pun;
using System.IO;
using UnityEngine;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class GameSetupController : MonoBehaviour
{
    // This script will be added to any multiplayer scene
    void Start()
    {
        CreatePlayer(); //Create a networked player object for each player that loads into the multiplayer scenes.
    }

    private void CreatePlayer()
    {
        Debug.Log("Creating Player");
        Hashtable hash=new Hashtable();
        if(PhotonNetwork.IsMasterClient){
            PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", "Paddle"), Vector3.zero, Quaternion.identity);
            hash.Add("Rot",0);
        }else{
            GameObject paddle=null;
            if((int) PhotonNetwork.MasterClient.CustomProperties["Rot"]==0){
                paddle=PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", "Paddle"), Vector3.zero, Quaternion.Euler(new Vector3(0,0,180)));
                hash.Add("Rot",180);
            }else{
                paddle=PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", "Paddle"), Vector3.zero, Quaternion.identity);
                hash.Add("Rot",0);
            }
            paddle.tag="Paddle2";

        }
        hash.Add("Score",0);
        PhotonNetwork.LocalPlayer.SetCustomProperties(hash);
    }
}
