using UnityEngine;
using UnityEngine.UI; // If you're using Text for UI
using System.Collections.Generic;

public class MessagePanelController : MonoBehaviour
{
    [SerializeField] Text messageText; // Reference to the Text component where messages will be displayed
    public KeyCode activationKey = KeyCode.P; // The key to press to show the message panel
    private string errorLog;

    private Dictionary<string, string> logValues = new Dictionary<string, string>();

    void OnEnable()
    {
        Application.logMessageReceived += HandleLog;
    }

    void OnDisable()
    {
        Application.logMessageReceived -= HandleLog;
    }

    void HandleLog(string logString, string stackTrace, LogType type)
    {
        if (type == LogType.Exception || type == LogType.Error)
        {
            errorLog="\nError: " + logString + "\n" + stackTrace;
            ShowMessage(messageText.text + errorLog);
        }
    }


    void Update()
    {
        // Check if the activation key is pressed
        if (Input.GetKeyDown(activationKey))
        {
            ToggleMessagePanel(); // Show or hide the message panel
        }
    }

    // Show a message on the panel
    // TODO delete
    public void ShowMessage(string message)
    {
        if (messageText != null)
        {
            messageText.text = message+errorLog;
        }
        else
        {
            Debug.LogError("Message Text component is not assigned!");
        }
    }
    
    public void LogValue(string key, string value)
    {
        logValues[key] = value;
        DisplayAllValues();
    }
    
    public void DisplayAllValues()
    {
        string textBody = "";
        foreach(var entry in logValues)
        {
            textBody += $"{entry.Key}: {entry.Value} \n";
        }
        
        messageText.text = textBody;
    }

    // Hide the panel
    public void HideMessage()
    {
        gameObject.SetActive(false); // Hide the panel
    }

    // Toggle the message panel visibility
    public void ToggleMessagePanel()
    {
        gameObject.SetActive(!gameObject.activeSelf); // Toggle the panel visibility
    }
}