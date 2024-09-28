﻿using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Photon.Pun;
using Photon.Realtime;
using Hashtable = ExitGames.Client.Photon.Hashtable;    
using System.Collections.Generic; 

public class BasicKGameController : MonoBehaviourPunCallbacks {

    public static BasicKGameController instance;

    // Game state
        public int _scoreOne = 0; // remove
        public int _scoreTwo = 0; // remove
        public Dictionary<string, int> _playerIdToScore = new Dictionary<string, int>();
        //  PhotonNetwork.LocalPlayer.CustomProperties["Score"] make helper function

        private static Player _otherPlayer;
        private static Player _localPlayer;
        public bool _isMasterClient = false;

        public bool _isGameOver = false;
        public bool _isTurnToServe = false; // whether something happens if user presses space, initially true if master
        // rely on playerid 
        public bool _isRoundInProgress = false; // whether ball is moving

        private int _pingCounter; // TODO: kz clean up

    // Settings
        [SerializeField] int _scoreToWin = 3;
        int _originX = -2;
        int _originY = 0;

    // UI components
        public Text _textOne;
        public Text _textTwo;
        [SerializeField] GameObject _debugPanel;
        [SerializeField] bool _debugEnableWallPanel = false;
        MessagePanelController _logPanel;
        [SerializeField] GameObject _gameOverPanel;
        [SerializeField] Text _winnerText;
        [SerializeField] Text _myNanerText;
        [SerializeField] Text _myName;
        [SerializeField] Text _theirName;        

    // Game world objects
        private static Player other;
        
        [SerializeField] BasicBallEntity _ballEntity;
        [SerializeField] GameObject _endZoneWallPanelOne;
        [SerializeField] GameObject _endZoneWallPanelTwo;
        private GameObject _otherPlayerWallPanel;
        private GameObject _myWallPanel;

    private void Start(){
        // set local Photon Player
        _localPlayer=PhotonNetwork.LocalPlayer; 
        // set player name
        _myName.text=PhotonNetwork.NickName;
        // set player score
        SetLocalPlayerScore(0);

        // instantiate debugger pannel
        _logPanel = _debugPanel.GetComponent<MessagePanelController>();

        // deactivate/activate wall panels
        if (_debugEnableWallPanel)
        {
            _endZoneWallPanelOne.SetActive(true);
            _endZoneWallPanelTwo.SetActive(true);
        } else
        {
            _endZoneWallPanelOne.SetActive(false);
            _endZoneWallPanelTwo.SetActive(false);
        }

        // TODO: make player agnostic
        if (PhotonNetwork.PlayerListOthers.Length>0){
            _otherPlayerWallPanel = _endZoneWallPanelOne;
            _myWallPanel = _endZoneWallPanelTwo;
        } else 
        {
            _otherPlayerWallPanel = _endZoneWallPanelTwo;
            _myWallPanel = _endZoneWallPanelOne;
        }

        // reset game
        ResetGame();
    }

    // might not be needed
    public void SetTheirName(string nameIn){
        _theirName.text=_otherPlayer.NickName;
    }

    public override void OnEnable()
    {
        // TODO: why do we need instance?
        instance = this;
        base.OnEnable();
    }

    public override void OnPlayerEnteredRoom(Player newPlayer){
        _otherPlayer=newPlayer;
    }

    public override void OnJoinedRoom(){
        // TODO this code is not exexcuting,investigate why
        if (PhotonNetwork.PlayerListOthers.Length>0){
            other=PhotonNetwork.PlayerListOthers[0];
            _otherPlayerWallPanel = _endZoneWallPanelOne;
            _myWallPanel = _endZoneWallPanelTwo;
        } else 
        {
            _otherPlayerWallPanel = _endZoneWallPanelTwo;
            _myWallPanel = _endZoneWallPanelOne;
        }
        // TODO: what if old user rejoins room?
        SetLocalPlayerScore(0);
    }

    public override void OnPlayerLeftRoom(Player otherPlayer){
        // reset game
        if(!_isGameOver){
            ResetGame();
        }
    }

    void ResetGame(){
        // game start: 
        // 1. clear all ui panels
        // 2. clear player score, ball and paddle positions, lastKnownPositions
        // 3. set player name?
        // 4. initialize ball positions based on where ball object was placed?
        _gameOverPanel.SetActive(false); //! move to text component?
        _ballEntity.SetPosition(_originX, _originY);
        SetLocalPlayerScore(0);
        _isGameOver = false;
        _isTurnToServe = PhotonNetwork.IsMasterClient;
        _isMasterClient = PhotonNetwork.IsMasterClient;
        _isRoundInProgress = false;
        ResetRound();
    }

    private int PingCheck(Player player){
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
            int localPing=PingCheck(PhotonNetwork.LocalPlayer);
            string otherPingString="";
            if(other!=null && other.CustomProperties!=null){
                int otherPing=PingCheck(other);
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
        _myWallPanel.SetActive(status);
        _logPanel.LogValue("My wall panel", status.ToString());
    }

    public void SetOtherPlayerWallPanel(bool status)
    {
        _otherPlayerWallPanel.SetActive(status);
        _logPanel.LogValue("Other wall panel", status.ToString());
    }

    //TODO remove references to player 1 and 2
    public void GivePointToPlayerOne()
    {
        Debug.Log(_localPlayer.CustomProperties["score"] );
        int newScore = (int)_localPlayer.CustomProperties["score"] + 1;
        Debug.Log(newScore);
        SetLocalPlayerScore(newScore);
        //this._textOne.text =newScore.ToString();
        if(newScore >= _scoreToWin)
        {
            DeclareWinner(instance._myName.text);
        }
        ResetRound();
    }

    public void GivePointToPlayerTwo()
    {
        //TODO AC: figure out if we need casting to int
        int newScore = (int)_localPlayer.CustomProperties["score"] + 1;
        //TODO AC: make this agnostic but also playable with one person in scene
        SetLocalPlayerScore(newScore);
        this._textTwo.text = _scoreOne.ToString();
        if(newScore >= _scoreToWin)
        {
            // TODO AC : declare winner other person
        }
        ResetRound();
    }

    public void SetLocalPlayerScore(int score)
    {
        // TODO: move initialization to beginning
        Hashtable props= _localPlayer.CustomProperties;
        if(props.ContainsKey("score")){
            props["score"]=score;
        }else{
            props.Add("score", score);
        }
        _localPlayer.SetCustomProperties(props);
        //Debug.Log(_localPlayer.CustomProperties["score"] );
    }

    public override void OnPlayerPropertiesUpdate(Player target, Hashtable changedProps)  
    {  
        if(!changedProps.ContainsKey("score")){
            return;
        }
		if(target==_localPlayer){
            this._textOne.text=changedProps["score"].ToString();
        }else{
            this._textTwo.text=changedProps["score"].ToString();
        }
    }

    public void ResetRound()
    {
        ResetBall();
        // SetOtherPlayerWallPanel(true);
        // SetMyWallPanel(true);
    }

    //TODO remove references to player 1 and 2
    public void ResetBall()
    {
        _ballEntity.SetPosition(_originX, _originY);
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
        _winnerText.text = playerName + " Wins";
        _gameOverPanel.SetActive(true);
    }
}