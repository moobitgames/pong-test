﻿using System;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

public class SettingsController : MonoBehaviour
{
	[SerializeField] GameObject settingMenu;
	[SerializeField] Dropdown regionSelect;

	// Settings values
	[SerializeField] string playerName = "";
    [SerializeField] string roomName = "";
	[SerializeField] string regionString;
    [SerializeField] int musicVolume = 5;

    void Awake()
    {
        DontDestroyOnLoad(this.gameObject);
    }
	private bool state=false;
	

	public void pullMenu(){
		if(!state){
			settingMenu.SetActive(true);
			state=true;
		}else{
			settingMenu.SetActive(false);
			state=false;
		}
		Debug.Log("Hello");
		
	}

	public void changeRegion(){
		PhotonNetwork.Disconnect();
		PhotonNetwork.ConnectToRegion(regionSelect.captionText.text);
	}

	public string GetPlayerName(){
		return this.playerName;
	}

	public void SetPlayerName(string name){
		this.playerName = name;
		Debug.Log("player name: " + this.playerName);
	}

	public string GetRoomName(){
		return this.roomName;
	}

	public void SetRoomName(string name){
		this.roomName = name;
		Debug.Log("room name: " + this.roomName);
	}
}