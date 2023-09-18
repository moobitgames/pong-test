using System;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

public class SettingsController : MonoBehaviour
{
	[SerializeField] GameObject settingMenu;
	[SerializeField] Dropdown regionSelect;

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

}