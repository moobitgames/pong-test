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
        private int ballOneLastKnownPosition;
        private int ballTwoLastKnownPosition;

        public bool isGameOver = false;
        public bool isTurnToServe = false; // initially true if master
        public bool isHeadingTowardsMe = false; // initially true if master
        public bool isBallInMotion = false;
        public bool isBallInBounds = true; // set to false if miss
        public bool isRoundInProgress = false;

    // Settings
        [SerializeField] int scoreToWin;

    // UI components
        public Text textOne;
        public Text textTwo;

    // Game world objects
        private static Player other;
        [SerializeField] GameObject gameOverPanel;
        [SerializeField] Text winnerText;
        [SerializeField] Text myName;
        [SerializeField] Text theirName;
        [SerializeField] Ball ball;
        [SerializeField] GameObject endZoneWallPanelOne;
        [SerializeField] GameObject endZoneWallPanelTwo;

    private void Start(){
        GameReset();

        // set player name
        myName.text=PhotonNetwork.NickName;
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
        if(PhotonNetwork.PlayerListOthers.Length>0){
            other=PhotonNetwork.PlayerListOthers[0];
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
        scoreOne = 0;
        scoreTwo = 0;

        isGameOver = false;
        isTurnToServe = PhotonNetwork.IsMasterClient;
        isHeadingTowardsMe = PhotonNetwork.IsMasterClient;
        isBallInBounds = true;
        isRoundInProgress = false;

        Debug.Log("Reseting Game");
    }

    // Update is called once per frame
    void Update () {
        if(Input.GetKeyDown(KeyCode.Space))
        {
            if (!isGameOver && !isRoundInProgress && isTurnToServe) {
                this.photonView.RPC("RPC_SpacePressed", other);
                StartRound();
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

    public void StartRound()
    {
        isRoundInProgress = true;

        //delay 500ms
        //set ball entity in motion
    }

    

    public void ToggleIsHeadingTowardsMe()
    {
        isHeadingTowardsMe = !isHeadingTowardsMe;
        if(isHeadingTowardsMe)
        {
            if (PhotonNetwork.IsMasterClient)
            {
                endZoneWallPanelOne.SetActive(false);
                endZoneWallPanelTwo.SetActive(true);
            } else
            {
                endZoneWallPanelOne.SetActive(true);
                endZoneWallPanelTwo.SetActive(false);
            }
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

    // TODO: Helper functions
    public void RPC_SyncBallPosition()
    {
    }

    public void ResetBall()
    {
        ball.SetPosition(0, 0);
        ball.SetVelocity(0, 0);
        isBallInBounds = true;
    }

    public void GoToMainMenu()
    {
        GameReset(); // DEL?
        PhotonNetwork.LeaveRoom();
        SceneManager.LoadScene(0);
    }

    void DeclareWinner(string playerName)
    {
        // 1. set game over state to true
        isGameOver = true;
        winnerText.text = playerName + " Wins";
        gameOverPanel.SetActive(true);
    }
}