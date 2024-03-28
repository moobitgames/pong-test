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
        public bool isBallInPlay = false;
        public bool isBallInBounds = true; // set to false if miss
        

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

    void GameReset(){
        // game start: 
        // 1. clear all ui panels
        // 2. clear player score, ball and paddle positions, lastKnownPositions
        // 3. set player name?
        // 4. initialize ball positions based on where ball object was placed?
        gameOverPanel.SetActive(false); 
        scoreOne = 0;
        scoreTwo = 0;
        if(PhotonNetwork.IsMasterClient)
        {
            isTurnToServe = true;
            isHeadingTowardsMe = true;
        } else {
            isTurnToServe = false;
            isHeadingTowardsMe = false;
        }

        other=null;
        isGameOver=false;
        isBallInPlay = true;
        gameOverPanel.SetActive(false);
        ball.SetPosition(0,0);
        // !!ballEntity.reset position

        Debug.Log("Reseting Game");
    }

    public override void OnPlayerLeftRoom(Player otherPlayer){
        // reset game
        if(!isGameOver){
            GameReset();
        }
    }

    [PunRPC]
    private void RPC_setInPlay(bool value){
        instance.isBallInPlay=value;
    }

    // Update is called once per frame
    void Update () {
        if(!isGameOver && KGameController.instance.isTurnToServe)
        {
            if(Input.GetKeyDown(KeyCode.Space))
            {
                // !!startRound()
                // isBallInPlay = true; del
                if(!PhotonNetwork.IsMasterClient){
                    this.photonView.RPC("RPC_setInPlay",PhotonNetwork.MasterClient,true);
                }
                // send message space has been pressed
                // this.photonView.RPC("RPC_SpacePressed",PhotonNetwork.MasterClient,true);
            }
        }
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            PhotonNetwork.LeaveRoom();
            SceneManager.LoadScene(0);
        }

        // TODO: Handling game events as they happen 
    }

    // Helper functions

    public void OnBeginSignalReceived()
    { 
    
    }

    public void SetBallInMotion()
    {

    }

    public void SetBallEntityInMotion()
    {

    }

    public void ToggleIsHeadingTowardsMe()
    {

    }

    public void MainMenu()
    {
        GameReset();
        PhotonNetwork.LeaveRoom();
        SceneManager.LoadScene(0);
    }

    void DeclareWinner(string playerName)
    {
        // 1. set game over state to true
        isGameOver = false;
        isBallInPlay = false;
        winnerText.text = playerName + " Wins";
        gameOverPanel.SetActive(true);
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
        } else {
            isTurnToServe = true;
        }
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
        } else
        {
            isTurnToServe = false;
        }
    }
}