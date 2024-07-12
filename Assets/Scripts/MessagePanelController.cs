using UnityEngine;
using UnityEngine.UI; // If you're using Text for UI
using System.Collections.Generic;

public class MessagePanelController : MonoBehaviour
{
    [SerializeField] Text _messageText; // Reference to the Text component where messages will be displayed
    public KeyCode _activationKey = KeyCode.P; // The key to press to show the message panel
    private string _errorLog;

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
            _errorLog="\nError: " + logString + "\n" + stackTrace;
            ShowMessage(_messageText.text + _errorLog);
        }
    }


    void Update()
    {
        // Check if the activation key is pressed
        if (Input.GetKeyDown(_activationKey))
        {
            ToggleMessagePanel(); // Show or hide the message panel
        }
    }

    // Show a message on the panel
    // TODO delete
    public void ShowMessage(string message)
    {
        if (_messageText != null)
        {
            _messageText.text = message+ _errorLog;
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
        
        _messageText.text = textBody;
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
