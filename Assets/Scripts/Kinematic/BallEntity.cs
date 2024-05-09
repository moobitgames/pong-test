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
    
    float xSpeed = -0.4f/60f;
    float ySpeed = -0.4f/60f;
    public float boundDistance = 0.5f;
    [SerializeField] Ball target; // ball that this entity is tracking

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
    
    void OnTriggerEnter2D(Collider2D other)
    {
        if(other.tag == "EndZoneWallPanel")
        {
            ySpeed = ySpeed * -1f;
            CourseCorrect();
        }
        // else if (other.tag == "Paddle")
        // {
        //     Debug.Log("BEpaddle: " + transform.position.ToString("F3"));
        //     if (KGameController.instance.isBallInBounds)
        //     {
        //         ySpeed = ySpeed * -1f;
                
        //     }
        // }
        else if(other.tag == "SideWallPanel")
        {
            xSpeed = xSpeed * -1f;
            CourseCorrect();
            Debug.Log("BEsidewallpanel: " + transform.position.ToString("F3"));
        }
        else if(other.tag == "EndzoneBehindPaddle")
        {
            // KGameController.instance.NotifyOtherPlayerBallMissed();
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

    public void CourseCorrect()
    {
        float distance = GetDistanceFromTarget();
        float degrees = 45f;
        float angle = (degrees * Mathf.PI) / 180f;
        float xMagnitude = distance * Mathf.Cos(angle) * Mathf.Sign(xSpeed);
        float yMagnitude = distance * Mathf.Sin(angle) * Mathf.Sign(ySpeed);
        float newX = this.transform.position.x + xMagnitude;
        float newY = this.transform.position.y + yMagnitude;
        Debug.Log("111: " + target.transform.position.ToString("F3"));
        target.SetPosition(newX, newY);
        Debug.Log("111: " + target.transform.position.ToString("F3"));

    }
}