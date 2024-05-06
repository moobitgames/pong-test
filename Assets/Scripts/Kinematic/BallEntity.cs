using UnityEngine;
using System.Collections;
using Photon.Pun;
using Hashtable = ExitGames.Client.Photon.Hashtable;    

public class BallEntity : MonoBehaviourPunCallbacks {

    // game object components
    Rigidbody2D rb;
    Collider2D collider;

    // object settings
    [SerializeField] float scale=1;

    // object state properties
    [SerializeField] float speedUp;
    
    float xSpeed = -1.9f/60f;
    float ySpeed = -1.9f/60f;
    float speedUpAmount = 1.4f;
    Vector2 velocityVector = new Vector2(-1,0);
    float positionX; // course correction only
    float positionY; // course correction only
    bool inMotion;
    Vector2 destination;
    public bool isBallEntityRestingAtOrigin = true;
    public float boundDistance = 0.5f;
    // ball that this entity is tracking
    [SerializeField] Ball ball;
    public bool isCatchingUp = false;
    [SerializeField] GameObject target;

    void Start()
    {
        // get game object components
        rb = GetComponent<Rigidbody2D>();
        collider = GetComponent<Collider2D>();
    }
    
    void Update () {
        // if game state is active and not equal to destination then move ball
        if(KGameController.instance.isRoundInProgress)
        {
            SimpleMoveBall();
        }
    }

    void SimpleMoveBall()
    {
        DisplaceBall(xSpeed, ySpeed);
    }

    public float GetDistanceFromTarget()
    {
        return Vector3.Distance(transform.position, target.transform.position);
    }

    void ToggleIsBEHeadingTowardsMe()
    {
        KGameController.instance.ToggleIsBEHeadingTowardsMe();
    }

    void DisplaceBall(float x, float y) 
    {
        Vector3 displacement = new Vector3(x, y);
        transform.position = transform.position += displacement;
    }

    [PunRPC] //DEL
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
    
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.tag == "Paddle")
        {
            Debug.Log("BEpaddle: " + transform.position.ToString("F3"));
            if (KGameController.instance.isBallInBounds)
            {
                ySpeed = ySpeed * -1f;
                
            }
        }
        else if(other.tag == "EndZoneWallPanel")
        {
            ySpeed = ySpeed * -1f;
        }
        else if(other.tag == "SideWallPanel")
        {
            xSpeed = xSpeed * -1;
            Debug.Log("BEsidewallpanel: " + transform.position.ToString("F3"));
        }
        else if(other.tag == "EndzoneBehindPaddle")
        {
            KGameController.instance.NotifyOtherPlayerBallMissed();
        } else
        {
            return;
        }
    }

    public void SetVelocity(float x, float y)
    {
        xSpeed = x;
        ySpeed = y;
    }

    public void SetPosition(float x, float y)
    {
        this.transform.position = new Vector3(x, y,-1); 
    }
}