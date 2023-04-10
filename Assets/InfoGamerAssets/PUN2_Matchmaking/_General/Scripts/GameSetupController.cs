using Photon.Pun;
using System.IO;
using UnityEngine;

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
        if(PhotonNetwork.IsMasterClient){
            PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", "Paddle"), Vector3.zero, Quaternion.identity);
        }else{
            PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", "Paddle"), Vector3.zero, Quaternion.Euler(new Vector3(0,0,180)));
        }
    }
}
