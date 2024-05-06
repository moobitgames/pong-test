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
        public bool isTurnToServe = false; // initially true if master
        public bool isRoundInProgress = false; // whether ball is moving
        public bool isBallInBounds = true; // set to false if player misses and ball is behind paddle
        public bool isHeadingTowardsMe = true; // initially true if master
        public bool isBEHeadingTowardsMe = true; // initially true if master
        // public bool isOtherPlayerWallPanelActive = true;
        private int pingCounter;

        private bool debuggingEndPanel = false;

    // Settings
        [SerializeField] int scoreToWin = 3;

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
        GameReset();

        // set player name
        myName.text=PhotonNetwork.NickName;

        if (PhotonNetwork.PlayerListOthers.Length>0){
            otherPlayerWallPanel = endZoneWallPanelOne;
            myWallPanel = endZoneWallPanelTwo;
            Debug.Log("abcB");
        } else 
        {
            otherPlayerWallPanel = endZoneWallPanelTwo;
            myWallPanel = endZoneWallPanelOne;
            Debug.Log("abcA");
        }
    }

    // might not be needed
    public void SetTheirName(string nameIn){
        theirName.text=nameIn;
    }

    public override void OnEnable()
    {
        // why we need instance?
        instance = this;
        base.OnEnable();
    }

    public override void OnPlayerEnteredRoom(Player newPlayer){
        other=newPlayer;
    }

    public override void OnJoinedRoom(){
        Debug.Log("abcA");
        if (PhotonNetwork.PlayerListOthers.Length>0){
            other=PhotonNetwork.PlayerListOthers[0];
            otherPlayerWallPanel = endZoneWallPanelOne;
            myWallPanel = endZoneWallPanelTwo;
            Debug.Log("abcB");
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
        ball.SetPosition(0,0);
        ballEntity.SetPosition(0,0);
        scoreOne = 0;
        scoreTwo = 0;

        isGameOver = false;
        isTurnToServe = PhotonNetwork.IsMasterClient;
        isHeadingTowardsMe = PhotonNetwork.IsMasterClient;
        isBallInBounds = true;
        isRoundInProgress = false;
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
        if(KGameController.instance.pingCounter>=60){
            int localPing=pingCheck(PhotonNetwork.LocalPlayer);
            string otherPingString="";
            if(other!=null && other.CustomProperties!=null){
                int otherPing=pingCheck(other);
                otherPingString="\nPing2:"+ otherPing.ToString();
            }
            KGameController.instance.debugPanel.GetComponent<MessagePanelController> ().ShowMessage("Ping:"+ localPing.ToString()+otherPingString);
            KGameController.instance.pingCounter=0;
        }else{
            KGameController.instance.pingCounter++;
        }

        if(Input.GetKeyDown(KeyCode.Space))
        {
            if (!isGameOver && !isRoundInProgress && isTurnToServe) {
                this.photonView.RPC("RPC_SpacePressed", other);
                StartRound();
                Debug.Log("qwe");
            }
        }
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            GoToMainMenu();
        }

        // TODO: Handling game events as they happen 
    }

    [PunRPC]
    private void RPC_SpacePressed(){
        StartRound();
    }

    [PunRPC]
    private void RPC_BallTraveledBehindPaddle(){
        otherPlayerWallPanel.SetActive(false);
    }

    public void StartRound()
    {
        isRoundInProgress = true;
        Debug.Log("abc2" + otherPlayerWallPanel);
        otherPlayerWallPanel.SetActive(true);
        
        myWallPanel.SetActive(false);
        Debug.Log("qq " + endZoneWallPanelOne.activeSelf);
        // endZoneWallPanelOne.SetActive(false);
    }

    public void NotifyOtherPlayerBallMissed()
    {
        this.photonView.RPC("BallTraveledBehindPaddle", other);
    }

    public void ToggleIsHeadingTowardsMe()
    {
        isHeadingTowardsMe = !isHeadingTowardsMe;
        if (!debuggingEndPanel) {
            return;
        }
    }

    public void ToggleIsBEHeadingTowardsMe()
    {
        isBEHeadingTowardsMe = !isBEHeadingTowardsMe;
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
        isRoundInProgress = false;
        ResetBall();
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
        isRoundInProgress = false;
        ResetBall();
    }

    public void ResetBall()
    {
        ball.SetPosition(0, 0);
        ballEntity.SetPosition(0 ,0);
        ball.SetVelocity(0, 0);
        ballEntity.SetVelocity(0 ,0);
        isBallInBounds = true;
        isRoundInProgress = false;
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