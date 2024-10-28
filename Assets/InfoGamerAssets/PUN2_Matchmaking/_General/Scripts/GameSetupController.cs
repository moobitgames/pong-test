using Photon.Pun;
using Photon.Realtime;
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
        // if(PhotonNetwork.IsMasterClient){
        //     PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", "Paddle"), Vector3.zero, Quaternion.identity);
        //     hash.Add("rot",0);
        // }else{
        //     GameObject paddle=null;
        //     if((int) PhotonNetwork.MasterClient.CustomProperties["rot"]==0){
        //         paddle=PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", "Paddle"), Vector3.zero, Quaternion.Euler(new Vector3(0,0,180)));
        //         hash.Add("rot",180);
        //     }else{
        //         paddle=PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", "Paddle"), Vector3.zero, Quaternion.identity);
        //         hash.Add("rot",0);
        //     }
        //     paddle.tag="Paddle";

        // }
        // PhotonNetwork.LocalPlayer.SetCustomProperties(hash);

        // Instantiate the player object and local custom prop hash
        GameObject paddle;
        Hashtable hash = new Hashtable();

        // Get the room size based on the maximum players allowed in the room
        int roomSize = PhotonNetwork.CurrentRoom.MaxPlayers;

        // Check if the room has the "rotPositions" property
        if (PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue("rotPositions", out object rotPositionsObj))
        {
            // Cast the property value to a Player array
            Player[] rotPositions = (Player[]) rotPositionsObj;

            // Find the first null position in the array and set it to the player object
            for (int i = 0; i < rotPositions.Length; i++)
            {
                if (rotPositions[i] == null)
                {
                    int rotDegrees = 0 + i * (360/roomSize);
                    hash.Add("rot", rotDegrees);
                    paddle = PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", "Paddle"), Vector3.zero, Quaternion.Euler(new Vector3(0,0,rotDegrees)));
                    paddle.tag = "Paddle";
                    rotPositions[i] = PhotonNetwork.LocalPlayer;
                    break;
                }
            }

            // Update the "rotPositions" property in the room
            PhotonNetwork.CurrentRoom.SetCustomProperties(new Hashtable() { { "rotPositions", rotPositions } });
        }
        else
        {
            // Initialize the "rotPositions" array with size roomSize and set all positions to null
            Player[] rotPositions = new Player[roomSize];
            for (int i = 0; i < rotPositions.Length; i++)
            {
                rotPositions[i] = null;
            }

            hash.Add("rot", 0);
            paddle = PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", "Paddle"), Vector3.zero, Quaternion.identity);
            paddle.tag = "Paddle";

            // Set the first position to the newly created player
            rotPositions[0] = PhotonNetwork.LocalPlayer;

            // Add the new "rotPositions" array to the room properties
            Hashtable rotHash = new Hashtable();
            rotHash.Add("rotPositions", rotPositions);
            PhotonNetwork.CurrentRoom.SetCustomProperties(rotHash);

            // Todo: (10/28) AC figure out why current room custom props aren't working
        }

        PhotonNetwork.LocalPlayer.SetCustomProperties(hash);

    }
}
