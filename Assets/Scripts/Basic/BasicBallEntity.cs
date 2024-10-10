using UnityEngine;
using System.Collections;
using Photon.Pun;
using Hashtable = ExitGames.Client.Photon.Hashtable;    

public class BasicBallEntity : MonoBehaviourPunCallbacks {

    // Object state properties
    float _xVelocity = -1.2f/60f;
    float _yVelocity = -1.2f/60f;
    
    void Start()
    {
    }
    
    void FixedUpdate () {
        // Move ball only if round is in progress
        if(BasicKGameController.instance._isRoundInProgress && BasicKGameController.instance._isMasterClient)
        {
            SimpleMoveBall();
        }
    }

    // * DOC:
    // * BallEntity represents the ball object in an idealized environment with no lag
    // * and acts as the source of truth for where an idealized ball should be
    void SimpleMoveBall()
    {
        DisplaceBall(_xVelocity, _yVelocity);
    }

    void DisplaceBall(float x, float y) 
    {
        Vector3 displacement = new Vector3(x, y);
        transform.position = transform.position += displacement;
    }
    
    // * DOC:
    // * BallEntity should only bounce off of SideWallPanels and EndZoneWallPanels
    // * Until it gets an rpc saying other wise, it continues to move as if in
    // * a perfect match with no misses. Upon bouncing it corrects the position of
    // * the visible ball
    void OnTriggerEnter2D(Collider2D other)
    {
        if(other.tag == "Paddle")
        {
            _yVelocity = _yVelocity * -1f;
        }
        else if(other.tag == "EndZoneWallPanel")
        {
            _yVelocity = _yVelocity * -1f;
        }
        else if(other.tag == "SideWallPanel")
        {
            _xVelocity = _xVelocity * -1f;
        }
        else
        {
            BasicKGameController.instance.HandleBallEnterEndZone(other.tag);
        }
    }

    public void SetVelocity(float x, float y)
    {
        _xVelocity = x;
        _yVelocity = y;
    }

    public void SetPosition(float x, float y)
    {
        this.transform.position = new Vector3(x, y, -1); 
    }
}