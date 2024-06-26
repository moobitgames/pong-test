using UnityEngine;
using System.Collections;
using Photon.Pun;
using Hashtable = ExitGames.Client.Photon.Hashtable;    
using System.Collections.Generic;

public class Ball : MonoBehaviourPunCallbacks {

    // Object state properties
    [SerializeField] float speedUp;
    
    float boundDistance = 0.5f;
    float xSpeed = -1f/60f;
    float ySpeed = -1f/60f;
    bool isShifting = true;

    // Reference to the object to follow
    [SerializeField] GameObject target;

    void Start()
    {
    }
    
    void Update() {
        // if game round is active, move ball
        if(KGameController.instance.isRoundInProgress)
        {
            SimpleMoveBall();
        }
    }

    void SimpleMoveBall()
    {
        // just bounced off enemy paddle and needs to catch up to other player position
        if (KGameController.instance.isHeadingTowardsMe)
        {
            if (isShifting)
            {
                DisplaceBall(xSpeed*1.5f, ySpeed*1.5f);
                if (GetDistanceFromTarget() > boundDistance)
                {
                    isShifting = false;
                }
            }
            else
            {
                DisplaceBall(xSpeed, ySpeed);
            }
        }
        else
        {
            if (isShifting)
            {
                DisplaceBall(xSpeed*.8f, ySpeed*.8f);
                if (GetDistanceFromTarget() < 0.045)
                {
                    isShifting = false;
                }
            }
            else
            {
                DisplaceBall(xSpeed, ySpeed);
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
            Debug.Log("paddle: " + transform.position.ToString("F3"));
            ToggleIsHeadingTowardsMe();
            isShifting = true;
            ySpeed = ySpeed * -1f;
            Debug.Log("paddle speed: " + ySpeed);
        }
        else if(other.tag == "NotificationZone" && KGameController.instance.isHeadingTowardsMe)
        {
            KGameController.instance.NotifyOtherPlayerBallMissed();
            KGameController.instance.SetMyWallPanel(false);
        }
        else if(other.tag == "EndZoneWallPanel")
        {
            if (!KGameController.instance.isHeadingTowardsMe) //opposite player hits
            {
                ToggleIsHeadingTowardsMe();
                isShifting = true;
                ySpeed = ySpeed * -1f;
            }
            return;
        }
        else if(other.tag == "SideWallPanel")
        {
            Debug.Log("sidewallpanel: " + transform.position.ToString("F3"));
            xSpeed = xSpeed * -1;
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
        //     xSpeed = xSpeed*-1;
        // }

        // if (other.transform.tag == "Paddle")
        // {
        //     if (KGameController.instance.isHeadingTowardsMe)
        //     {
        //         ySpeed = ySpeed * -1;
        //         RPC_NotifyPaddleBounce();
        //         KGameController.instance.isHeadingTowardsMe = false;
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
        xSpeed = x;
        ySpeed = y;
    }

    public void SetPosition(float x, float y)
    {
        this.transform.position = new Vector3(x, y, -1); 
    }
}