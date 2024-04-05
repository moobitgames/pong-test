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
    
    float xSpeed = 0;
    float ySpeed = 0;
    Vector2 velocityVector = new Vector2(-1,0);
    float positionX; // course correction only
    float positionY; // course correction only
    bool inMotion;
    Vector2 destination;
    public bool isBallEntityRestingAtOrigin = true;
    public float boundDistance = 0.5f;
    // ball that this entity is tracking
    [SerializeField] Ball ball;

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
            MoveBall();
        }
    }

    void MoveBall()
    {
        if (isBallEntityRestingAtOrigin)
        {
            if (GetDistanceFromBall() >= boundDistance) {
                isBallEntityRestingAtOrigin = false;
            }
        } else {
            rb.velocity = new Vector2(xSpeed, ySpeed);
        }
        
    }

    [PunRPC] //DEL
    private void RPC_PaddleBounce(){
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

    void OnCollisionEnter2D(Collision2D other)
    {
        
        if(other.transform.tag =="Wall" && PhotonNetwork.IsMasterClient)
        {
            xSpeed = xSpeed*-1;
        }
    }
    
    void OnTriggerEnter2D(Collider2D other)
    {
        // move to other end zone?
        if(other.tag == "EndZoneWallPanel")
        {
            ySpeed = ySpeed * -1;
            ToggleIsTracking();
        }
    }

    public void SetVelocity(float x, float y)
    {
        rb.velocity = new Vector2(x, y);
    }

    public void SetPosition(float x, float y)
    {
        this.transform.position = new Vector3(x, y,-1); 
    }

    public float GetDistanceFromBall()
    {
        return Vector3.Distance(transform.position, ball.transform.position);
    }

    public void ToggleIsTracking()
    {
        
    }
}