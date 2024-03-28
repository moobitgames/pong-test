using UnityEngine;
using System.Collections;
using Photon.Pun;
using Hashtable = ExitGames.Client.Photon.Hashtable;    

public class Ball : MonoBehaviourPunCallbacks {

    // game object components
    Rigidbody2D rb;
    Collider2D collider;

    // object settings
    [SerializeField] float scale=1;

    // object state properties
    [SerializeField] float speedUp;
    
    float xSpeed = 1;
    float ySpeed = 1;
    Vector2 velocityVector = new Vector2(-1,0);
    float positionX; // course correction only
    float positionY; // course correction only
    bool inMotion;
    Vector2 destination;

    void Start()
    {
        // get game object components
        rb = GetComponent<Rigidbody2D>();
        collider = GetComponent<Collider2D>();
    }
    
    void Update () {
        // if game state is active and not equal to destination then move ball
        if(KGameController.instance.isBallInPlay)
        {
            MoveBall();
        }
    }

    void MoveBall()
    {
        Debug.Log("asdf111 "+this.transform.position);
        rb.velocity = new Vector2(xSpeed, ySpeed);
    }

    [PunRPC]
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
        
        if(other.transform.tag =="Wall" && PhotonNetwork.IsMasterClient)
        {
            xSpeed = xSpeed*-1;
        }

        if (other.transform.tag == "Paddle" && PhotonNetwork.IsMasterClient)
        {
            ySpeed = ySpeed*-1;
            RPC_NotifyPaddleBounce();
            RPC_PaddleBounce();
        }
        if(other.transform.tag=="Paddle2" && !PhotonNetwork.IsMasterClient){
            this.photonView.RPC("RPC_PaddleBounce", PhotonNetwork.MasterClient,true);
        }

    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // move to other end zone?
        if((other.tag != "EndOne" && other.tag != "EndTwo"))
        {
            return;
        }

        if(other.tag == "EndTwo")
        {
            // player one scores
            KGameController.instance.GivePointToPlayerOne();
        }
        if(other.tag == "EndOne")
        {
            // player two scores
            KGameController.instance.GivePointToPlayerTwo();
        }

        // post score stuff
        // TODO: move logic to game controller
        KGameController.instance.isBallInPlay = false;

        // ToggleBallInMotion(); //?
        SetVelocity(0, 0);
        // reset ball to origin
        SetPosition(0, 0); 
    }

    public void SetVelocity(float x, float y)
    {
        rb.velocity = new Vector2(x, y);
    }

    public void SetPosition(float x, float y)
    {
        this.transform.position = new Vector3(x, y, -1); 
    }
    
    public Vector2 GetProjectedImpactPoint()
    {
        //ray casting
        return new Vector2(0, 0);
    }
}