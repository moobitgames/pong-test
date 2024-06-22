using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Photon.Pun;
using Photon.Realtime;
using Hashtable = ExitGames.Client.Photon.Hashtable;    


public class KGameController : MonoBehaviourPunCallbacks {

    public static KGameController instance;

    // Game state
        public int scoreOne = 0;
        public int scoreTwo = 0;
        public bool isGameOver = false;
        public bool isTurnToServe = false; // whether something happens if user presses space, initially true if master
        public bool isRoundInProgress = false; // whether ball is moving
        // public bool isBallInBounds = true; // set to false if player misses and ball is behind paddle
        public bool isHeadingTowardsMe = true; // initially true if master
        public bool isBEHeadingTowardsMe = true; // initially true if master
        private int pingCounter;
        private bool debuggingEndPanel = false;
        public bool isMasterClient = false;

    // Settings
        [SerializeField] int scoreToWin = 3;
        int originX = -2;
        int originY = 0;

    // UI components
        public Text textOne;
        public Text textTwo;
        [SerializeField] GameObject debugPanel;
        [SerializeField] GameObject gameOverPanel;
        [SerializeField] Text winnerText;
        [SerializeField] Text myName;
        [SerializeField] Text theirName;

    // Game world objects
        private static Player other;
        
        [SerializeField] Ball ball;
        [SerializeField] BallEntity ballEntity;
        [SerializeField] GameObject endZoneWallPanelOne;
        [SerializeField] GameObject endZoneWallPanelTwo;
        private GameObject otherPlayerWallPanel;
        private GameObject myWallPanel;

    private void Start(){
        // set player name
        myName.text=PhotonNetwork.NickName;

        if (PhotonNetwork.PlayerListOthers.Length>0){
            otherPlayerWallPanel = endZoneWallPanelOne;
            myWallPanel = endZoneWallPanelTwo;
        } else 
        {
            otherPlayerWallPanel = endZoneWallPanelTwo;
            myWallPanel = endZoneWallPanelOne;
        }
        GameReset();
    }

    // might not be needed
    public void SetTheirName(string nameIn){
        theirName.text=nameIn;
    }

    public override void OnEnable()
    {
        // TODO: why do we need instance?
        instance = this;
        base.OnEnable();
    }

    public override void OnPlayerEnteredRoom(Player newPlayer){
        other=newPlayer;
    }

    public override void OnJoinedRoom(){
        // TODO this code is not exexcuting,investigate why
        if (PhotonNetwork.PlayerListOthers.Length>0){
            other=PhotonNetwork.PlayerListOthers[0];
            otherPlayerWallPanel = endZoneWallPanelOne;
            myWallPanel = endZoneWallPanelTwo;
        } else 
        {
            otherPlayerWallPanel = endZoneWallPanelTwo;
            myWallPanel = endZoneWallPanelOne;
        }
    }

    public override void OnPlayerLeftRoom(Player otherPlayer){
        // reset game
        if(!isGameOver){
            GameReset();
        }
    }

    void GameReset(){
        // game start: 
        // 1. clear all ui panels
        // 2. clear player score, ball and paddle positions, lastKnownPositions
        // 3. set player name?
        // 4. initialize ball positions based on where ball object was placed?
        gameOverPanel.SetActive(false); //! move to text component?
        ball.SetPosition(originX, originY);
        ballEntity.SetPosition(originX, originY);
        scoreOne = 0;
        scoreTwo = 0;

        isGameOver = false;
        isTurnToServe = PhotonNetwork.IsMasterClient;
        isMasterClient = PhotonNetwork.IsMasterClient;
        isHeadingTowardsMe = PhotonNetwork.IsMasterClient;
        isRoundInProgress = false;
        ResetRound();
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
            KGameController.instance.debugPanel.GetComponent<MessagePanelController> ().ToggleMessagePanel(); // Show or hide the message panel
        }

        // debug panel logger
        if (KGameController.instance.pingCounter>=60){
            int localPing=pingCheck(PhotonNetwork.LocalPlayer);
            string otherPingString="";
            if(other!=null && other.CustomProperties!=null){
                int otherPing=pingCheck(other);
                otherPingString="\nPing2:"+ otherPing.ToString();
            }
            // KGameController.instance.debugPanel.GetComponent<MessagePanelController> ().ShowMessage("Ping:"+ localPing.ToString()+otherPingString);
            //TODO achen remove getComponent
            KGameController.instance.debugPanel.GetComponent<MessagePanelController> ().LogValue("Ping local", localPing.ToString());
            KGameController.instance.debugPanel.GetComponent<MessagePanelController> ().LogValue("Ping other", otherPingString);
            KGameController.instance.pingCounter=0;
        } else {
            KGameController.instance.pingCounter++;
        }

        // user presses space
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (!isGameOver && !isRoundInProgress && isTurnToServe)
            {
                this.photonView.RPC("RPC_SpacePressed", other);
                StartRound();
            }
        }
        // user presses escape
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            GoToMainMenu();
        }

        // TODO: Handling game events as they happen 
    }

    // * DOC:
    // * This function is called to notify other players that a player has pressed space
    [PunRPC]
    private void RPC_SpacePressed(){
        StartRound();
    }

    public void StartRound()
    {
        // int direction = isHeadingTowardsMe ? -1 : 1;
        // ball.SetVelocity(direction*1.2f/60f, direction*1.2f/60f);
        // ballEntity.SetVelocity(direction*1.2f/60f, direction*1.2f/60f);
        isRoundInProgress = true;
    }

    // * DOC:
    // * This function is called whenever a ball passes into the space behind a receiving
    // * player's paddle but before hitting the end zone score area. This is to give enough
    // * time to opposing players to disable their wall panels to allow their ball entity to
    // * pass through
    public void NotifyOtherPlayerBallMissed()
    {
        this.photonView.RPC("RPC_BallTraveledBehindOtherPaddle", other);
    }

    [PunRPC]
    private void RPC_BallTraveledBehindOtherPaddle(){
        SetOtherPlayerWallPanel(false);
    }

    public void SetMyWallPanel(bool status)
    {
        myWallPanel.SetActive(status);
        KGameController.instance.debugPanel.GetComponent<MessagePanelController> ().LogValue("My wall panel", status.ToString());
    }

    public void SetOtherPlayerWallPanel(bool status)
    {
        otherPlayerWallPanel.SetActive(status);
        KGameController.instance.debugPanel.GetComponent<MessagePanelController> ().LogValue("Other wall panel", status.ToString());
    }

    public void ToggleIsHeadingTowardsMe()
    {
        isHeadingTowardsMe = !isHeadingTowardsMe;
        if (!debuggingEndPanel) {
            return;
        }
    }
    
    //TODO remove references to player 1 and 2
    public void GivePointToPlayerOne()
    {
        scoreOne++;
        KGameController.instance.textOne.text = scoreOne.ToString();
        if(scoreOne >= scoreToWin)
        {
            DeclareWinner(instance.myName.text);
        }
        // flip isTurnToServe if necessary
        if (PhotonNetwork.IsMasterClient)
        {
            isTurnToServe = false;
            isHeadingTowardsMe = false;
        } else {
            isTurnToServe = true;
            isHeadingTowardsMe = true;
        }
        ResetRound();
    }

    public void GivePointToPlayerTwo()
    {
        scoreTwo++;
        KGameController.instance.textTwo.text = scoreTwo.ToString();
        if(scoreTwo >= scoreToWin)
        {
            DeclareWinner(instance.theirName.text);
        }
        // flip isTurnToServe if necessary
        if (PhotonNetwork.IsMasterClient)
        {
            isTurnToServe = true;
            isHeadingTowardsMe = true;
        } else
        {
            isTurnToServe = false;
            isHeadingTowardsMe = false;
        }
        ResetRound();
    }

    public void ResetRound()
    {
        ResetBall();
        SetOtherPlayerWallPanel(true);
        SetMyWallPanel(true);
    }

    //TODO remove references to player 1 and 2
    public void ResetBall()
    {
        ball.SetPosition(originX, originY);
        ballEntity.SetPosition(originX, originY);
        isRoundInProgress = false;
        // ball.SetVelocity(0, 0);
        // ballEntity.SetVelocity(0, 0);
    }

    public void GoToMainMenu()
    {
        PhotonNetwork.LeaveRoom();
        SceneManager.LoadScene(0);
    }

    void DeclareWinner(string playerName)
    {
        isGameOver = true;
        winnerText.text = playerName + " Wins";
        gameOverPanel.SetActive(true);
    }
}