using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Photon.Pun;
using Photon.Realtime;
using Hashtable = ExitGames.Client.Photon.Hashtable;    


public class GameController : MonoBehaviourPunCallbacks {

    public static GameController instance;

    public bool inPlay = false;
    public bool isTurn = false;
    bool gameOver = false;

    public int scoreOne;
    public int scoreTwo;
    [SerializeField] int scoreToWin;

    public Text textOne;
    public Text textTwo;

    private static Player other;

    [SerializeField] GameObject gameOverPanel;
    [SerializeField] Text winnerText;

    [SerializeField] Text myName;
    [SerializeField] Text theirName;

    private void Start(){
        myName.text=PhotonNetwork.NickName;
        if(PhotonNetwork.IsMasterClient){
            isTurn=true;
        }
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
    }

    public override void OnJoinedRoom(){
        if(PhotonNetwork.PlayerListOthers.Length>0){
            other=PhotonNetwork.PlayerListOthers[0];
        }

    }

    public override void OnPlayerLeftRoom(Player otherPlayer){
        Hashtable hash=new Hashtable();
        hash.Add("Rot",(int)other.CustomProperties["Rot"]);
        hash.Add("Score",0);
        other.SetCustomProperties(hash);

    }

    public static void OtherPlayerScored(){
        Hashtable hash=new Hashtable();
        hash.Add("Rot",(int)other.CustomProperties["Rot"]);
        hash.Add("Score",((int)other.CustomProperties["Score"])+1);
        other.SetCustomProperties(hash);
    }

    [PunRPC]
    private void RPC_setInPlay(bool value){
        instance.inPlay=value;
    }

    // Update is called once per frame
    void Update () {
        scoreOne=(int) PhotonNetwork.LocalPlayer.CustomProperties["Score"];
        GameController.instance.textOne.text = scoreOne.ToString();

        if(other!=null && other.CustomProperties!=null){
            scoreTwo=(int) other.CustomProperties["Score"];
            GameController.instance.textTwo.text = scoreTwo.ToString();
        }

        if(GameController.instance.inPlay == false && gameOver != true && GameController.instance.isTurn)
        {
            if(Input.GetKeyDown(KeyCode.Space))
            {
                inPlay = true;
                if(!PhotonNetwork.IsMasterClient){
                    this.photonView.RPC("RPC_setInPlay",PhotonNetwork.MasterClient,true);
                }
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
                winnerText.text = instance.myName.text + " Wins";
                gameOverPanel.SetActive(true);
            }
            if(scoreTwo >= scoreToWin)
            {
                gameOver = true;
                winnerText.text = instance.theirName.text + " Wins";
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