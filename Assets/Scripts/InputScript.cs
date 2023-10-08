using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InputScript : MonoBehaviour
{
    private TMP_InputField inputField;
    [SerializeField] string type = "name";
    private SettingsController settingsController;
    string text;

    // Start is called before the first frame update
    void Start()
    {
        inputField = GetComponent<TMP_InputField>();
        settingsController = GameObject.FindGameObjectsWithTag("SettingsController")[0].GetComponent<SettingsController>();
        
        switch (type)
        {
            case "name":
                text = settingsController.GetPlayerName();
                break;
            case "room":
                text = settingsController.GetRoomName();
                break;
        }

        inputField.text = text != "" ? text : "";

    } 
}