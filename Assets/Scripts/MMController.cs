using UnityEngine;
using UnityEngine.SceneManagement;
using Photon.Pun;

public class MMController : MonoBehaviour
{
    [SerializeField]
    private QuickStartLobbyController lobbyController; 
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
    private bool showIntroScreen = true;
    private int numPlayers;

    public void Play()
    {
        SceneManager.LoadScene(1);
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

    public void UsernameInput(string username){
        PhotonNetwork.NickName=username;
    }

    public void RoomNumberInput(string roomNumber){
        Debug.Log("Room number is: " + roomNumber);
        lobbyController.UpdateRoomNumber(roomNumber);
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
