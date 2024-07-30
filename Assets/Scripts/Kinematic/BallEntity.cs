using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Photon.Pun;
using Hashtable = ExitGames.Client.Photon.Hashtable;    

public class BallEntity : MonoBehaviourPunCallbacks {

    // Object state properties
    [SerializeField] public float _startxSpeed = -1.2f/60f;
    [SerializeField] public float _startySpeed = -1.2f/60f;
    [SerializeField] Ball _target; // Ball that this entity is tracking

    float _xSpeed;
    float _ySpeed;

    List<float> _bouncesX= new List<float>();
    List<float> _bouncesY= new List<float>();

    void Start(){
        _xSpeed=_startxSpeed;
        _ySpeed=_startySpeed;
    }
    
    void FixedUpdate () {
        // move ball only if round is in progress
        if(KGameController.instance._isRoundInProgress && KGameController.instance._isMasterClient)
        {
            SimpleMoveBall();
        }
    }

    // * DOC:
    // * BallEntity represents the ball object in an idealized environment with no lag
    // * and acts as the source of truth for where an idealized ball should be
    void SimpleMoveBall()
    {
        DisplaceBall(_xSpeed, _ySpeed);
    }

    void DisplaceBall(float x, float y) 
    {
        Vector3 displacement = new Vector3(x, y);
        transform.position = transform.position += displacement;
    }
    

    [PunRPC]
    void RPC_InformBounce(float x,float y){
        if(KGameController.instance._isMasterClient){
            return;
        }else{
            _bouncesX.Add(x);
            _bouncesY.Add(y);
            CourseCorrect();
        }

    }

    // * DOC:
    // * BallEntity should only bounce off of SideWallPanels and EndZoneWallPanels
    // * Until it gets an rpc saying other wise, it continues to move as if in
    // * a perfect match with no misses. Upon bouncing it corrects the position of
    // * the visible balls
    void OnTriggerEnter2D(Collider2D other)
    {
        if(!KGameController.instance._isMasterClient){
            return;
        }
        float x=transform.position.x;
        float y=transform.position.y;
        if(other.tag == "EndZoneWallPanel")
        {
            _ySpeed = _ySpeed * -1f;
            _bouncesX.Add(x);
            _bouncesY.Add(y);
            CourseCorrect();
            KGameController.instance.photonView.RPC("RPC_InformBounce",PhotonNetwork.PlayerListOthers[0], (x,y));

        }
        else if(other.tag == "SideWallPanel")
        {
            _xSpeed = _xSpeed * -1f;
            _bouncesX.Add(x);
            _bouncesY.Add(y);
            CourseCorrect();
            KGameController.instance.photonView.RPC("RPC_InformBounce",PhotonNetwork.PlayerListOthers[0], (x,y));

        }
        else
        {
            return;
        }
        Debug.Log(_bouncesX.Last().ToString()+_bouncesY.Last().ToString());
        return;
    }

    // * DOC:
    // * Calculate the position of where the visible Ball should be, had it bounced off
    // * the same spot as ball entity.
    // TODO: implement ray casting to account for when projected posistion is in opposite direction
    public void CourseCorrect()
    {
        Debug.Log("CourseCorrect");
        float distance = GetDistanceFromTarget();
        float degrees = 45f;
        float angle = (degrees * Mathf.PI) / 180f;
        float xMagnitude = distance * Mathf.Cos(angle) * Mathf.Sign(_xSpeed);
        float yMagnitude = distance * Mathf.Sin(angle) * Mathf.Sign(_ySpeed);
        float newX = this.transform.position.x + xMagnitude;
        float newY = this.transform.position.y + yMagnitude;

        bool inBounds =
            newX > -2.56 &&
            newX < 2.56 &&
            newY > -2.45 &&
            newY < 2.45;

        if (inBounds)
        {
            // set position
            _target.SetPosition(newX, newY);
            // set direction/speed
            //_target.SetVelocity(_xSpeed, _ySpeed);
        }
    }

    public float GetDistanceFromTarget()
    {
        return Vector3.Distance(transform.position, _target.transform.position);
    }

    public void SetVelocity(){
        _xSpeed=_startxSpeed;
        _ySpeed=_startySpeed;
    }

    public void SetVelocity(float x, float y)
    {
        _xSpeed = x;
        _ySpeed = y;
    }

    public void SetPosition(float x, float y)
    {
        this.transform.position = new Vector3(x, y, -1); 
    }
}