﻿using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

public class QuickStartLobbyController : MonoBehaviourPunCallbacks
{
    [SerializeField]
    private GameObject quickLoadingButton; //Displays loading while connecting to Photon servers.
    [SerializeField]
    private GameObject quickStartButton; //button used for creating and joining a game.
    [SerializeField]
    private GameObject quickCancelButton; //button used to stop searing for a game to join.

    [SerializeField]
    private GameObject quickLoadingButtonHost; //Displays loading while connecting to Photon servers.
    [SerializeField]
    private GameObject quickStartButtonHost; //button used for creating and joining a game.
    [SerializeField]
    private GameObject quickCancelButtonHost; //button used to stop searing for a game to join.
    
    [SerializeField]
    private int roomSize; //Manual set the number of player in the room at one time.
    private string roomNumber = "a";

    public override void OnConnectedToMaster() //Callback function for when the first connection is established successfully.
    {
        PhotonNetwork.AutomaticallySyncScene = true; //Makes it so whatever scene the master client has loaded is the scene all other clients will load
        quickStartButton.SetActive(true);
        quickLoadingButton.SetActive(false);
        quickStartButtonHost.SetActive(true);
        quickLoadingButtonHost.SetActive(false);
    }

    public void QuickStart() //Paired to the Quick Start button
    {
        Debug.Log("Starting with room number: " + roomNumber);
        quickStartButton.SetActive(false);
        quickCancelButton.SetActive(true);
        quickStartButtonHost.SetActive(false);
        quickCancelButtonHost.SetActive(true);
        RoomOptions roomOps = new RoomOptions() { IsVisible = true, IsOpen = true, MaxPlayers = (byte)roomSize };
        TypedLobby type  = new TypedLobby("lobby", LobbyType.Default);
        // PhotonNetwork.JoinRandomRoom(); //First tries to join an existieng room
        PhotonNetwork.JoinOrCreateRoom(this.roomNumber, roomOps, type); //First tries to join an existing room
        Debug.Log("Quick start");
    }

    public override void OnJoinRandomFailed(short returnCode, string message) //Callback function for if we fail to join a rooom
    {
        Debug.Log("Failed to join a room");
        CreateRoom();
    }

    void CreateRoom() //trying to create our own room
    {
        Debug.Log("Creating room now");
        int randomRoomNumber = Random.Range(0, 10000); //creating a random name for the room
        RoomOptions roomOps = new RoomOptions() { IsVisible = true, IsOpen = true, MaxPlayers = (byte)roomSize };
        PhotonNetwork.CreateRoom(this.roomNumber, roomOps); //attempting to create a new room
    }

    public override void OnCreateRoomFailed(short returnCode, string message) //callback function for if we fail to create a room. Most likely fail because room name was taken.
    {
        Debug.Log("Failed to create room... trying again");
        CreateRoom(); //Retrying to create a new room with a different name.
    }

    public void QuickCancel() //Paired to the cancel button. Used to stop looking for a room to join.
    {
        quickCancelButton.SetActive(false);
        quickStartButton.SetActive(true);
        quickCancelButtonHost.SetActive(false);
        quickStartButtonHost.SetActive(true);
        PhotonNetwork.LeaveRoom();
    }

    public void UpdateRoomNumber(string num)
    {
        Debug.Log("UpdateRoomNumber: " + this.roomNumber);
        this.roomNumber = num;
    }
}
