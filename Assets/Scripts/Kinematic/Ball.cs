using UnityEngine;
using System.Collections;
using Photon.Pun;
using Hashtable = ExitGames.Client.Photon.Hashtable;    
using System.Collections.Generic;

public class Ball : MonoBehaviourPunCallbacks {

    // game object components
    Rigidbody2D rb; //del
    Collider2D collider; //del

    // object state properties
    [SerializeField] float speedUp;
    
    float boundDistance = 0.5f;
    float xSpeed = -0.4f/60f;
    float ySpeed = -0.4f/60f;
    bool isShifting = true;

    //test
    // Reference to the object to follow
    [SerializeField] GameObject target;
    Rigidbody2D targetRigidBody;

    // Number of history points to store
    public int historySize = 100;
    
    // Time interval between history points
    public float historyInterval = 0.1f;

    // Distance behind the target
    public float distanceBehind;

    // Speed of movement
    public float moveSpeed;

    // List to store history of target positions
    private List<Vector3> targetHistory = new List<Vector3>();
    // List to store history of target velocities
    private List<Vector3> targetVelocityHistory = new List<Vector3>();

    void Start()
    {
        // get game object components
        rb = GetComponent<Rigidbody2D>();
        collider = GetComponent<Collider2D>();
        targetRigidBody = target.GetComponent<Rigidbody2D>();
    }
    
    void Update() {
        // if game round is active
        if(KGameController.instance.isRoundInProgress)
        {
            SimpleMoveBall();
        }
    }

    void SimpleMoveBall()
    {
        // DisplaceBall(xSpeed, ySpeed);
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
        }
        else if(other.tag == "EndZoneNotificationZone" && KGameController.instance.isHeadingTowardsMe)
        {
            KGameController.instance.NotifyOtherPlayerBallMissed();
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

    [PunRPC]
    private void RPC_PaddleBounce(){
        ySpeed = ySpeed * -1.1f;

        if(ySpeed > 0)
        {
            ySpeed += speedUp;
        }
        else
        {
            ySpeed -= speedUp;
        }
        if (xSpeed > 0)
        {
            xSpeed += speedUp;
        }
        else
        {
            xSpeed -= speedUp;
        }
    }

    [PunRPC]
    private void RPC_NotifyPaddleBounce(){
         ySpeed = ySpeed * -1;

            if(ySpeed > 0)
            {
                ySpeed += speedUp;
            }
            else
            {
                ySpeed -= speedUp;
            }
            if (xSpeed > 0)
            {
                xSpeed += speedUp;
            }
            else
            {
                xSpeed -= speedUp;
            }
    }

    void OnCollisionEnter2D(Collision2D other)
    {
        // if(other.transform.tag =="Wall")
        // {
        //     xSpeed = xSpeed*-1;
        //     Debug.Log("ava");
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