using UnityEngine;
using UnityEngine.SceneManagement;
using Photon.Pun;

public class MMController : MonoBehaviour
{
    // Controllers
    [SerializeField]
    private QuickStartLobbyController lobbyController;
    [SerializeField]
    private SettingsController settingsController;

    // Screen components
    [SerializeField]
    private GameObject introScreen; // Intro Title screen on boot
    [SerializeField]
    private GameObject selectModeScreen; // Screen for desktop client to select mode
    [SerializeField]
    private GameObject hostGameScreen; // Screen for desktop client to setup a hosted game
    [SerializeField]
    private GameObject joinGameScreen; // Screen for desktop client to join hosted game
    [SerializeField]
    private GameObject singleSetupScreen; // Screen for desktop client to setup single player game
    // UI elements
    // [SerializeField]
    // private GameObject showIntroScreen;
    
    // State variables
    private bool showIntroScreen = true;
    private int numPlayers;

    public void Play()
    {
        Debug.Log("asdfasdf");
        SceneManager.LoadScene(2);
    }

    // Update is called once per frame
    void Update()
    {
        if (showIntroScreen)
        {
            if (Input.anyKey)
            {   
                showIntroScreen = false;
                LoadSelectModeScreen();
            }
        }
    }

    public void UsernameInput(string playerName){
        PhotonNetwork.NickName=playerName;
        settingsController.SetPlayerName(playerName);
    }

    public void RoomNumberInput(string roomName){
        Debug.Log("Room number is: " + roomName);
        lobbyController.UpdateRoomNumber(roomName);
        settingsController.SetRoomName(roomName);
    }

    public void LoadSelectModeScreen()
    {
        introScreen.SetActive(false);
        hostGameScreen.SetActive(false);
        joinGameScreen.SetActive(false);
        singleSetupScreen.SetActive(false);
        selectModeScreen.SetActive(true);
    }

    public void LoadJoinGameScreen()
    {
        selectModeScreen.SetActive(false);
        joinGameScreen.SetActive(true);
    }

    public void LoadHostGameScreen()
    {
        selectModeScreen.SetActive(false);
        hostGameScreen.SetActive(true);
    }

    public void LoadSingleSetupScreen()
    {
        selectModeScreen.SetActive(false);
        singleSetupScreen.SetActive(true);
    }

    public void SetNumPlayers(int num)
    {
        this.numPlayers = num;
    }
}
