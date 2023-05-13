using UnityEngine;
using UnityEngine.SceneManagement;
using Photon.Pun;

public class MMController : MonoBehaviour
{
    [SerializeField]
    private QuickStartLobbyController lobbyController; 

    public void Play()
    {
        SceneManager.LoadScene(1);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {   
            Application.Quit();
        }
    }

    public void UsernameInput(string username){
        PhotonNetwork.NickName=username;
    }

    public void RoomNumberInput(string roomNumber){
        Debug.Log("Room number is: " + roomNumber);
        lobbyController.UpdateRoomNumber(roomNumber);
    }
}
