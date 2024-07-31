using UnityEngine;
using System.Collections;
using Photon.Pun;
using Hashtable = ExitGames.Client.Photon.Hashtable;    
using System.Collections.Generic;

public class Ball : MonoBehaviourPunCallbacks {

    // Object state properties
    [SerializeField] float _speedUp;
    
    float _boundDistance = 0.5f;
    bool _isShifting = true;
    float _xSpeed;
    float _ySpeed;
    // Reference to the object to follow
    [SerializeField] BallEntity target;

    void Start()
    {

       _xSpeed = target._startXSpeed;
       _ySpeed = target._startYSpeed;
    }
    
    void FixedUpdate() {
        // if game round is active, move ball
        if(KGameController.instance._isRoundInProgress)
        {
            SimpleMoveBall();
        }
        
    }

    void SimpleMoveBall()
    {
        // just bounced off enemy paddle and needs to catch up to other player position
        if (KGameController.instance._isHeadingTowardsMe)
        {
            if (_isShifting)
            {
                DisplaceBall(_xSpeed*1.5f, _ySpeed*1.5f);
                if (GetDistanceFromTarget() > _boundDistance)
                {
                    _isShifting = false;
                }
            }
            else
            {
                DisplaceBall(_xSpeed, _ySpeed);
            }
        }
        else
        {
            if (_isShifting)
            {
                DisplaceBall(_xSpeed*.8f, _ySpeed*.8f);
                if (GetDistanceFromTarget() < 0.045)
                {
                    _isShifting = false;
                }
            }
            else
            {
                DisplaceBall(_xSpeed, _ySpeed);
            }
        }
    }

    void DisplaceBall(float x, float y) 
    {
        Vector3 displacement = new Vector3(x, y);
        transform.position = transform.position += displacement;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if(other.tag == "Paddle")
        {
            ToggleIsHeadingTowardsMe();
            _isShifting = true;
            _ySpeed = _ySpeed * -1f;
        }
        else if(other.tag == "NotificationZone" && KGameController.instance._isHeadingTowardsMe)
        {
            KGameController.instance.NotifyOtherPlayerBallMissed();
            KGameController.instance.SetMyWallPanel(false);
        }
        else if(other.tag == "EndZoneWallPanel")
        {
            if (!KGameController.instance._isHeadingTowardsMe) //opposite player hits
            {
                ToggleIsHeadingTowardsMe();
                _isShifting = true;
                _ySpeed = _ySpeed * -1f;
            }
            return;
        }
        else if(other.tag == "SideWallPanel")
        {
            _xSpeed = _xSpeed * -1;
        }
        else if(other.tag == "EndTwo")
        {
            // player one scores
            KGameController.instance.GivePointToPlayerOne();
        }
        else if(other.tag == "EndOne")
        {
            // player two scores
            KGameController.instance.GivePointToPlayerTwo();
        } else
        {
            return;
        }
    }

    

    void OnCollisionEnter2D(Collision2D other)
    {
        // TODO: add back in going with collision rout
        // if(other.transform.tag =="Wall")
        // {
        //     _xSpeed = _xSpeed*-1;
        // }

        // if (other.transform.tag == "Paddle")
        // {
        //     if (KGameController.instance._isHeadingTowardsMe)
        //     {
        //         _ySpeed = _ySpeed * -1;
        //         RPC_NotifyPaddleBounce();
        //         KGameController.instance._isHeadingTowardsMe = false;
        //     } else {
        //         return;
        //     }
        // }
    }

    void ToggleIsHeadingTowardsMe()
    {
        KGameController.instance.ToggleIsHeadingTowardsMe();
    }

    public float GetDistanceFromTarget()
    {
        return Vector3.Distance(transform.position, target.transform.position);
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

    public void reset(){
        _xSpeed = target._startXSpeed;
        _ySpeed = target._startYSpeed;
    }
}