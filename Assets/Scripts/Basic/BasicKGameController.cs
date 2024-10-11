using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Photon.Pun;
using Photon.Realtime;
using Hashtable = ExitGames.Client.Photon.Hashtable;    
using System.Collections.Generic; 

public class BasicKGameController : MonoBehaviourPunCallbacks {

    public static BasicKGameController instance;

    // Game state
        public Dictionary<string, int> _playerIdToScore = new Dictionary<string, int>();

        private static Player _otherPlayer;
        private static Player _localPlayer;
        public bool _isMasterClient = false; //**
        public bool _isGameOver = false; //**
        public bool _isTurnToServe = false; // whether something happens if user presses space, initially true if master //**
        public bool _isRoundInProgress = false; // whether ball is moving //**

        private int _pingCounter; // TODO: kz clean up

    // Settings
        [SerializeField] int _scoreToWin = 3;
        int _originX = -2;
        int _originY = 0;

    // UI components
        public Text _scoreTextOne;
        public Text _scoreTextTwo;
        [SerializeField] GameObject _debugPanel;
        [SerializeField] bool _debugEnableWallPanel = false;
        MessagePanelController _logPanel;
        [SerializeField] GameObject _gameOverPanel;
        [SerializeField] Text _winnerText;
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

        // Reset game
        ResetGame();
    }

    // Might not be needed
    public void SetTheirName(){
        _theirName.text=_otherPlayer.NickName;
    }

    public override void OnEnable()
    {
        // TODO: why do we need instance?
        instance = this;
        base.OnEnable();
    }

    public override void OnPlayerEnteredRoom(Player newPlayer){
        _otherPlayer = newPlayer;
        SetTheirName();
    }

    public override void OnJoinedRoom(){
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
        
        if (PhotonNetwork.PlayerListOthers.Length>0){
            other = PhotonNetwork.PlayerListOthers[0];
            _otherPlayerWallPanel = _endZoneWallPanelOne;
            _myWallPanel = _endZoneWallPanelTwo;
        }
    }

    void ResetGame(){
        // Game start: 
        // 1. Clear all ui panels
        // 2. Clear player score, ball and paddle positions, lastKnownPositions
        // 3. Set player name?
        // 4. Initialize ball positions based on where ball object was placed?
        _gameOverPanel.SetActive(false);
        _ballEntity.SetPosition(_originX, _originY);
        SetLocalPlayerScore(0);
        _isGameOver = false;
        _logPanel.LogValue("_isGameOver", _isGameOver.ToString());
        _isTurnToServe = PhotonNetwork.IsMasterClient;
        _logPanel.LogValue("_isTurnToServe", _isTurnToServe.ToString());
        _isMasterClient = PhotonNetwork.IsMasterClient;
        _logPanel.LogValue("_isMasterClient", _isMasterClient.ToString());

        _isRoundInProgress = false;
        _logPanel.LogValue("_isRoundInProgress", _isRoundInProgress.ToString());
        ResetRound();
    }

    int PingCheck(Player player){
        Hashtable properties = player.CustomProperties;
        int ping = PhotonNetwork.GetPing();
        if(properties.ContainsKey("Ping")){
            properties["Ping"]=ping;
        }else{
            properties.Add("Ping",ping);
        }
        //player.SetCustomProperties(properties);
        return ping;
    }

    // Update is called once per frame
    void Update () {
        if (Input.GetKeyDown(KeyCode.F2))
        {
            _logPanel.ToggleMessagePanel(); // Show or hide the message panel
        }

        // Debug panel logger
        if (this._pingCounter >= 60){
            int localPing = PingCheck(PhotonNetwork.LocalPlayer);
            string otherPingString="";
            if(other!= null && other.CustomProperties != null){
                int otherPing = PingCheck(other);
                otherPingString = "\nPing2:"+ otherPing.ToString();
            }
            // _logPanel.LogValue("Ping local", localPing.ToString());
            // _logPanel.LogValue("Ping other", otherPingString);
            this._pingCounter = 0;
        } else {
            this._pingCounter++;
        }

        // User presses space
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (!_isGameOver && !_isRoundInProgress && _isTurnToServe)
            {
                this.photonView.RPC("RPC_SpacePressed", other);
                StartRound();
            }
        }
        // User presses escape
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            GoToMainMenu();
        }

        // Upcoming: Handling game events as they happen 
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
        _logPanel.LogValue("_isRoundInProgress", _isRoundInProgress.ToString());
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

    public void HandleBallEnterEndZone(string rotValue)
    {
        switch(rotValue)
        {
            case "Rot0":
                if ((int) _localPlayer.CustomProperties["rot"] == 180)
                {
                    Debug.Log("goodbye");
                    GivePointToPlayer(_localPlayer);
                }else{
                    Debug.Log("Hello");
                    GivePointToPlayer(_otherPlayer);
                }
                break;
            case "Rot180":
                if ((int) _localPlayer.CustomProperties["rot"] ==  0)
                {
                    GivePointToPlayer(_localPlayer);
                }else{
                    GivePointToPlayer(_otherPlayer);
                }
                break;
        }
        Debug.Log(rotValue);
        ResetRound();    
    }

    public void GivePointToPlayer(Player scorePlayer)
    {
        int newScore = (int) scorePlayer.CustomProperties["score"] + 1;
        _logPanel.LogValue("score happened"+scorePlayer.NickName, newScore.ToString());
        SetPlayerScore(scorePlayer,newScore);
        // if(newScore >= _scoreToWin)
        // {
        //     DeclareWinner(instance._myName.text);
        // }
    }

    public void SetLocalPlayerScore(int score)
    {
        Hashtable props = _localPlayer.CustomProperties;
        props["score"] = score;
        _localPlayer.SetCustomProperties(props);
    }

    public void SetPlayerScore(Player scorePlayer, int score)
    {
        Hashtable props = scorePlayer.CustomProperties;
        props["score"] = score;
        scorePlayer.SetCustomProperties(props);
    }

    public override void OnPlayerPropertiesUpdate(Player target, Hashtable changedProps)  
    {  
        if(!changedProps.ContainsKey("score"))
        {
            return;
        }

        _logPanel.LogValue("local customprops", GetHashtableString(_localPlayer.CustomProperties));
        _logPanel.LogValue("other customprops", GetHashtableString(_otherPlayer.CustomProperties));
        int newScore=(int)changedProps["score"];
		if((int)target.CustomProperties["rot"] == (int)_localPlayer.CustomProperties["rot"])
        {
            this._scoreTextOne.text = newScore.ToString();
        }else
        {
            this._scoreTextTwo.text = newScore.ToString();
        }
        // TODO AC: only reset if score has changed
        // if (condition)
        // {
        //     ResetRound();
        // }
        if(newScore >= _scoreToWin){
            DeclareWinner(target.NickName);
        }
    }

    public void ResetRound()
    {
        ResetBall();
        // SetOtherPlayerWallPanel(true);
        // SetMyWallPanel(true);
    }

    public void ResetBall()
    {
        _ballEntity.SetPosition(_originX, _originY);
        _isRoundInProgress = false;
        _logPanel.LogValue("_isRoundInProgress", _isRoundInProgress.ToString());
    }

    public void GoToMainMenu()
    {
        PhotonNetwork.LeaveRoom();
        SceneManager.LoadScene(0);
    }

    void DeclareWinner(string playerName)
    {
        _isGameOver = true;
        _logPanel.LogValue("_isGameOver", _isGameOver.ToString());
        _winnerText.text = playerName + " Wins";
        _gameOverPanel.SetActive(true);
    }

    // Utility Functions:
    string GetHashtableString(Hashtable hashtable)
    {
        string valuesStr = "";
        foreach(string key in hashtable.Keys)
        {
            valuesStr += $"{key}: {hashtable[key]}\n";
        }
        Debug.Log(valuesStr);
        return valuesStr;
    }
   
}