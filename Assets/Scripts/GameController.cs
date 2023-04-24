using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Photon.Pun;
using Photon.Realtime;

public class GameController : MonoBehaviourPunCallbacks {

    public static GameController instance;

    public bool inPlay = false;
    bool gameOver = false;

    public int scoreOne;
    public int scoreTwo;
    [SerializeField] int scoreToWin;

    public Text textOne;
    public Text textTwo;

    private Player other;

    [SerializeField] GameObject gameOverPanel;
    [SerializeField] Text winnerText;

    [SerializeField] Text myName;
    [SerializeField] Text theirName;

    private void Start(){
        myName.text=PhotonNetwork.NickName;
    }

    public void SetTheirName(string nameIn){
        theirName.text=nameIn;
    }

    public override void OnEnable()
    {
        instance = this;
        base.OnEnable();
    }

    public override void OnPlayerEnteredRoom(Player newPlayer){
        other=newPlayer;
        Debug.Log(other);
    }

    public override void OnJoinedRoom(){
        if(PhotonNetwork.PlayerListOthers.Length>0){
            other=PhotonNetwork.PlayerListOthers[0];
        }

    }

    // Update is called once per frame
    void Update () {
        if(other!=null && other.CustomProperties!=null){
            scoreOne=(int) PhotonNetwork.LocalPlayer.CustomProperties["Score"];
            scoreTwo=(int) other.CustomProperties["Score"];
            GameController.instance.textOne.text = scoreOne.ToString();
            GameController.instance.textTwo.text = scoreTwo.ToString();
        }

        if(inPlay == false && gameOver != true)
        {
            if(Input.GetKeyDown(KeyCode.Space))
            {
                inPlay = true;
            }
        }
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            PhotonNetwork.LeaveRoom();
            SceneManager.LoadScene(0);
        }
        Winner();
    }

    void Winner()
    {
        if(!gameOver)
        {
            if(scoreOne >= scoreToWin)
            {
                gameOver = true;
                winnerText.text = "Player 1 Wins";
                gameOverPanel.SetActive(true);
            }
            if(scoreTwo >= scoreToWin)
            {
                gameOver = true;
                winnerText.text = "Player 2 Wins";
                gameOverPanel.SetActive(true);
            }
        }
    }

    public void MainMenu()
    {
        PhotonNetwork.LeaveRoom();
        SceneManager.LoadScene(0);
    }
}