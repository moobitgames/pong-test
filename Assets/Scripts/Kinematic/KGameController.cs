﻿using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Photon.Pun;
using Photon.Realtime;
using Hashtable = ExitGames.Client.Photon.Hashtable;    


public class KGameController : MonoBehaviourPunCallbacks {

    public static KGameController instance;

    // Game state
        public int _scoreOne = 0;
        public int _scoreTwo = 0;
        public bool _isGameOver = false;
        public bool _isTurnToServe = false; // whether something happens if user presses space, initially true if master
        public bool _isRoundInProgress = false; // whether ball is moving
        public bool _isHeadingTowardsMe = true; // initially true if master
        public bool _isBEHeadingTowardsMe = true; // initially true if master
        private int _pingCounter;
        private bool _debuggingEndPanel = false;
        public bool _isMasterClient = false;

    // Settings
        [SerializeField] int _scoreToWin = 3;
        int _originX = -2;
        int _originY = 0;

    // UI components
        public Text textOne;
        public Text textTwo;
        [SerializeField] GameObject debugPanel;
        MessagePanelController _logPanel;
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
        _logPanel = debugPanel.GetComponent<MessagePanelController>();

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
        if(!_isGameOver){
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
        ball.SetPosition(_originX, _originY);
        ballEntity.SetPosition(_originX, _originY);
        _scoreOne = 0;
        _scoreTwo = 0;

        _isGameOver = false;
        _isTurnToServe = PhotonNetwork.IsMasterClient;
        _isMasterClient = PhotonNetwork.IsMasterClient;
        _isHeadingTowardsMe = PhotonNetwork.IsMasterClient;
        _isRoundInProgress = false;
        ResetRound();
    }

    private int pingCheck(Player player){
        Hashtable properties = player.CustomProperties;
        int ping = PhotonNetwork.GetPing();
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
            _logPanel.ToggleMessagePanel(); // Show or hide the message panel
        }

        // debug panel logger
        if (this._pingCounter>=60){
            int localPing=pingCheck(PhotonNetwork.LocalPlayer);
            string otherPingString="";
            if(other!=null && other.CustomProperties!=null){
                int otherPing=pingCheck(other);
                otherPingString="\nPing2:"+ otherPing.ToString();
            }
            _logPanel.LogValue("Ping local", localPing.ToString());
            _logPanel.LogValue("Ping other", otherPingString);
            this._pingCounter=0;
        } else {
            this._pingCounter++;
        }

        // user presses space
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (!_isGameOver && !_isRoundInProgress && _isTurnToServe)
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
        _isRoundInProgress = true;
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
        _logPanel.LogValue("My wall panel", status.ToString());
    }

    public void SetOtherPlayerWallPanel(bool status)
    {
        otherPlayerWallPanel.SetActive(status);
        _logPanel.LogValue("Other wall panel", status.ToString());
    }

    public void ToggleIsHeadingTowardsMe()
    {
        _isHeadingTowardsMe = !_isHeadingTowardsMe;
        if (!_debuggingEndPanel) {
            return;
        }
    }
    
    //TODO remove references to player 1 and 2
    public void GivePointToPlayerOne()
    {
        _scoreOne++;
        this.textOne.text = _scoreOne.ToString();
        if(_scoreOne >= _scoreToWin)
        {
            DeclareWinner(instance.myName.text);
        }
        // flip _isTurnToServe if necessary
        if (_isMasterClient)
        {
            _isTurnToServe = false;
            _isHeadingTowardsMe = false;
        } else {
            _isTurnToServe = true;
            _isHeadingTowardsMe = true;
        }
        ResetRound();
    }

    public void GivePointToPlayerTwo()
    {
        _scoreTwo++;
        this.textTwo.text = _scoreTwo.ToString();
        if(_scoreTwo >= _scoreToWin)
        {
            DeclareWinner(instance.theirName.text);
        }
        // flip _isTurnToServe if necessary
        if (PhotonNetwork.IsMasterClient)
        {
            _isTurnToServe = true;
            _isHeadingTowardsMe = true;
        } else
        {
            _isTurnToServe = false;
            _isHeadingTowardsMe = false;
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
        ball.SetPosition(_originX, _originY);
        ballEntity.SetPosition(_originX, _originY);
        _isRoundInProgress = false;
    }

    public void GoToMainMenu()
    {
        PhotonNetwork.LeaveRoom();
        SceneManager.LoadScene(0);
    }

    void DeclareWinner(string playerName)
    {
        _isGameOver = true;
        winnerText.text = playerName + " Wins";
        gameOverPanel.SetActive(true);
    }
}