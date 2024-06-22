using UnityEngine;
using System.Collections;
using Photon.Pun;
using Hashtable = ExitGames.Client.Photon.Hashtable;    

public class BallEntity : MonoBehaviourPunCallbacks {

    // Object state properties
    float xSpeed = -1.2f/60f;
    float ySpeed = -1.2f/60f;
    [SerializeField] Ball target; // Ball that this entity is tracking
    // private bool isStartingFromRest = true;
    
    void Start()
    {
    }
    
    void Update () {
        // move ball only if round is in progress
        if(KGameController.instance.isRoundInProgress && KGameController.instance.isMasterClient)
        {
            SimpleMoveBall();
        }
    }

    // * DOC:
    // * BallEntity represents the ball object in an idealized environment with no lag
    // * and acts as the source of truth for where an idealized ball should be
    void SimpleMoveBall()
    {
        DisplaceBall(xSpeed, ySpeed);
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
        if(other.tag == "EndZoneWallPanel")
        {
            ySpeed = ySpeed * -1f;
            CourseCorrect();
        }
        else if(other.tag == "SideWallPanel")
        {
            xSpeed = xSpeed * -1f;
            CourseCorrect();
        }
        else
        {
            return;
        }
    }

    // * DOC:
    // * Calculate the position of where the visible Ball should be, had it bounced off
    // * the same spot as ball entity.
    // TODO: implement ray casting to account for when projected posistion is in opposite direction
    public void CourseCorrect()
    {
        float distance = GetDistanceFromTarget();
        float degrees = 45f;
        float angle = (degrees * Mathf.PI) / 180f;
        float xMagnitude = distance * Mathf.Cos(angle) * Mathf.Sign(xSpeed);
        float yMagnitude = distance * Mathf.Sin(angle) * Mathf.Sign(ySpeed);
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
            target.SetPosition(newX, newY);
            // set direction/speed
            target.SetVelocity(xSpeed, ySpeed);
        }
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