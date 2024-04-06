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

    private int pingCounter;

    [SerializeField] GameObject gameOverPanel;
    [SerializeField] Text winnerText;

    [SerializeField] Text myName;
    [SerializeField] Text theirName;
    [SerializeField] GameObject debugPanel;

    private void Start(){
        myName.text=PhotonNetwork.NickName;
        if(PhotonNetwork.IsMasterClient){
            isTurn=true;
        }
        gameOverPanel.SetActive(false);
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

    void gameReset(){
        scoreOne=0;
        scoreTwo=0;
        if(PhotonNetwork.IsMasterClient){
            isTurn=true;
        }
        Hashtable hash=new Hashtable();
        hash.Add("Rot",(int)PhotonNetwork.LocalPlayer.CustomProperties["Rot"]);
        hash.Add("Score",0);
        PhotonNetwork.LocalPlayer.SetCustomProperties(hash);
        other=null;
        gameOver=false;
        gameOverPanel.SetActive(false);
        Debug.Log("Reseting Game");

    }

    public override void OnPlayerLeftRoom(Player otherPlayer){
        if(!gameOver){
            gameReset();
        }
    }
    [PunRPC] 
    private void RPC_setIsTurn(bool value){
        instance.isTurn=value;
        Debug.Log("setIsTurn_RPC"+value);
    }

    public void setIsTurn(bool value){
        instance.isTurn=value;
        this.photonView.RPC("RPC_setIsTurn",other,!value);
        Debug.Log("setIsTurn");
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

    public void setInPlay(bool value){
        instance.inPlay=value;
        this.photonView.RPC("RPC_setInPlay",other,value);
    }

    private int pingCheck(Player player){
        Hashtable properties= player.CustomProperties;
        int ping=PhotonNetwork.GetPing();
        if(properties.ContainsKey("Ping")){
            properties["Ping"]=ping;
        }else{
            properties.Add("Ping",ping);
        }
        player.SetCustomProperties(properties);
        return ping;
    }

    // Update is called once per frame
    void Update () {

        if (Input.GetKeyDown(KeyCode.F2))
        {
            GameController.instance.debugPanel.GetComponent<MessagePanelController> ().ToggleMessagePanel(); // Show or hide the message panel
        }

        if(GameController.instance.pingCounter>=60){
            int localPing=pingCheck(PhotonNetwork.LocalPlayer);
            string otherPingString="";
            if(other!=null && other.CustomProperties!=null){
                int otherPing=pingCheck(other);
                otherPingString="\nPing2:"+ otherPing.ToString();
            }
            GameController.instance.debugPanel.GetComponent<MessagePanelController> ().ShowMessage("Ping:"+ localPing.ToString()+otherPingString);
            GameController.instance.pingCounter=0;
        }else{
            GameController.instance.pingCounter++;
        }

        if(other!=null && other.CustomProperties!=null){
            scoreOne=(int) PhotonNetwork.LocalPlayer.CustomProperties["Score"];
            GameController.instance.textOne.text = scoreOne.ToString();
            scoreTwo=(int) other.CustomProperties["Score"];
            GameController.instance.textTwo.text = scoreTwo.ToString();
        }

        if(GameController.instance.inPlay == false && gameOver != true && GameController.instance.isTurn)
        {
            if(Input.GetKeyDown(KeyCode.Space))
            {
                instance.setInPlay(true);
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
        gameReset();
        PhotonNetwork.LeaveRoom();
        SceneManager.LoadScene(0);
    }
}