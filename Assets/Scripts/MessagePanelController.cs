using UnityEngine;
using UnityEngine.UI; // If you're using Text for UI
using System.Collections.Generic;

public class MessagePanelController : MonoBehaviour
{
    [SerializeField] Text _messageText; // Reference to the Text component where messages will be displayed
    public KeyCode _activationKey = KeyCode.P; // The key to press to show the message panel
    private string _errorLog;

    private Dictionary<string, string> _logValues = new Dictionary<string, string>();

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
            ShowErrorMessage(_messageText.text + _errorLog);
        }
    }

    // Append an error message to the text on the panel
    public void ShowErrorMessage(string message)
    {
        if (_messageText != null)
        {
            _messageText.text = message + _errorLog;
        }
        else
        {
            Debug.LogError("Message Text component is not assigned!");
        }
    }

    void Update()
    {
        // Check if the activation key is pressed
        if (Input.GetKeyDown(_activationKey))
        {
            ToggleMessagePanel();
        }
    }

    
    public void LogValue(string key, string value)
    {
        _logValues[key] = value;
        DisplayAllValues();
    }
    
    // Displays all values contained in the log values dictionary
    public void DisplayAllValues()
    {
        string textBody = "";
        foreach(var entry in _logValues)
        {
            textBody += $"{entry.Key}: {entry.Value} \n";
        }
        
        _messageText.text = textBody;
    }

    // Toggle the message panel visibility
    public void ToggleMessagePanel()
    {
        gameObject.SetActive(!gameObject.activeSelf);
    }
}
