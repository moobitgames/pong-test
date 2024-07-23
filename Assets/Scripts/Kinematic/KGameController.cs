using UnityEngine;
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
        public Text _textOne;
        public Text _textTwo;
        [SerializeField] GameObject _debugPanel;
        MessagePanelController _logPanel;
        [SerializeField] GameObject _gameOverPanel;
        [SerializeField] Text _winnerText;
        [SerializeField] Text _myName;
        [SerializeField] Text _theirName;        

    // Game world objects
        public static Player other;
        
        [SerializeField] Ball _ball;
        [SerializeField] BallEntity _ballEntity;
        [SerializeField] GameObject _endZoneWallPanelOne;
        [SerializeField] GameObject _endZoneWallPanelTwo;
        private GameObject _otherPlayerWallPanel;
        private GameObject _myWallPanel;

    private void Start(){
        // set player name
        _myName.text=PhotonNetwork.NickName;
        _logPanel = _debugPanel.GetComponent<MessagePanelController>();

        if (PhotonNetwork.PlayerListOthers.Length>0){
            _otherPlayerWallPanel = _endZoneWallPanelOne;
            _myWallPanel = _endZoneWallPanelTwo;
        } else 
        {
            _otherPlayerWallPanel = _endZoneWallPanelTwo;
            _myWallPanel = _endZoneWallPanelOne;
        }
        GameReset();
    }

    // might not be needed
    public void SetTheirName(string nameIn){
        _theirName.text=nameIn;
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
            _otherPlayerWallPanel = _endZoneWallPanelOne;
            _myWallPanel = _endZoneWallPanelTwo;
        } else 
        {
            _otherPlayerWallPanel = _endZoneWallPanelTwo;
            _myWallPanel = _endZoneWallPanelOne;
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

        _gameOverPanel.SetActive(false); //! move to text component?
        _ball.SetPosition(_originX, _originY);
        _ballEntity.SetPosition(_originX, _originY);
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
        _myWallPanel.SetActive(status);
        _logPanel.LogValue("My wall panel", status.ToString());
    }

    public void SetOtherPlayerWallPanel(bool status)
    {
        _otherPlayerWallPanel.SetActive(status);
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
        this._textOne.text = _scoreOne.ToString();
        if(_scoreOne >= _scoreToWin)
        {
            DeclareWinner(instance._myName.text);
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
        this._textTwo.text = _scoreTwo.ToString();
        if(_scoreTwo >= _scoreToWin)
        {
            DeclareWinner(instance._theirName.text);
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
        _ball.SetPosition(_originX, _originY);
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