﻿using System;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

public class SettingsController : MonoBehaviour
{
	[SerializeField] GameObject settingMenu;
	[SerializeField] Dropdown regionSelect;
	[SerializeField] QuickStartLobbyController lobbyController;

	// Settings values
	static string playerName = "";
    static string roomName = "";
	static string regionString;
	static int regionValue= -1;
    static int musicVolume = 5;
    static SettingsController instance;

    void Awake()
    {
    	if (regionValue != -1)
    	{
    		this.regionSelect.value = SettingsController.regionValue;
    	}
    }

	private bool state=false;
	

	public void pullMenu(){
		if(!state){
			if(regionValue == -1)
			{
				regionString=PhotonNetwork.CloudRegion;
				int index = regionSelect.options.FindIndex((i) => { return i.text.Equals(PhotonNetwork.CloudRegion); });
				this.regionSelect.value = index;
			}
			settingMenu.SetActive(true);
			state=true;			
		}else{
			settingMenu.SetActive(false);
			state=false;
		}
		
	}

	public void changeRegion(){
		if(regionString!=regionSelect.captionText.text)
		{
			PhotonNetwork.Disconnect();
			PhotonNetwork.ConnectToRegion(regionSelect.captionText.text);
			SetRegionString(regionSelect.captionText.text,regionSelect.value);
		}
	}

	public string GetPlayerName(){
		return SettingsController.playerName;
	}

	public void SetPlayerName(string name){
		SettingsController.playerName = name;
		Debug.Log("set player name: " + SettingsController.playerName);
	}

	public string GetRoomName(){
		return SettingsController.roomName;
	}

	public void SetRoomName(string name){
		SettingsController.roomName = name;
		lobbyController.UpdateRoomNumber(roomName);
		Debug.Log("set room name: " + SettingsController.roomName);
	}

	public string GetRegionString(){
		return SettingsController.roomName;
	}

	public void SetRegionString(string name, int value){
		SettingsController.regionString = name;
		SettingsController.regionValue=value;
		Debug.Log("set region string: " + SettingsController.regionString);
	}
}