using UnityEngine;
using System.Collections;
using Photon.Pun;
using Hashtable = ExitGames.Client.Photon.Hashtable;    
using System.Collections.Generic;

public class Ball : MonoBehaviourPunCallbacks {

    // game object components
    Rigidbody2D rb;
    Collider2D collider;

    // object settings
    [SerializeField] float scale=1;

    // object state properties
    [SerializeField] float speedUp;
    float boundDistance = 0.5f;

    float xSpeed = 0;
    float ySpeed = 0;
    Vector2 velocityVector = new Vector2(-1,0);
    float positionX; // course correction only
    float positionY; // course correction only
    bool inMotion = true;
    Vector2 destination;


    bool isCatchingUp = false;
    bool isTrackingTarget = false;
    bool isBallRestingAtOrigin = false;

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
            if (isBallRestingAtOrigin)
            {
                if (KGameController.instance.isHeadingTowardsMe)
                {
                    isBallRestingAtOrigin = false;
                } else
                {
                    isBallRestingAtOrigin = true;
                }
            } else {
                SimpleMoveBall();
            }
        }
    }

    void MoveBall()
    {
        // if following ball entity
        if (isTrackingTarget)
        {
            if (target != null)
            {
                // Add the current target position to history
                UpdateTargetHistory();

                // Calculate the desired position behind the target
                Vector3 targetPosition = CalculateTargetPosition();

                // Move towards the desired position
                if (targetVelocityHistory.Count >= historySize)
                {
                    rb.velocity = targetVelocityHistory[0];
                }
            }
        } else // is not tracking ball entity
        {
            // is catching up
            if(isCatchingUp)
            {
                if (GetDistanceFromTarget() < 0.01)
                {
                    isCatchingUp = false;
                }
                rb.velocity = new Vector2(xSpeed*1.1f, ySpeed*1.1f);
            }
            else // surpassed
            {
                if (GetDistanceFromTarget() > boundDistance)
                {
                    rb.velocity = new Vector2(xSpeed, ySpeed);
                } else
                {
                    rb.velocity = new Vector2(xSpeed*1.1f, ySpeed*1.1f);
                }
            }
           // towards destination
            
        }
    }

    void SimpleMoveBall()
    {
        rb.velocity = new Vector2(xSpeed, ySpeed);
    }

    void UpdateTargetHistory()
    {
        // Add the current target position to history
        targetHistory.Add(target.transform.position);
        targetVelocityHistory.Add(targetRigidBody.velocity);

        // Remove oldest position if history exceeds maximum size
        if (targetHistory.Count > historySize)
        {
            targetHistory.RemoveAt(0);
        }
        // Remove oldest position if history exceeds maximum size
        if (targetVelocityHistory.Count > historySize)
        {
            targetVelocityHistory.RemoveAt(0);
        }
    }

    Vector3 CalculateTargetPosition()
    {
        // Calculate the desired time in the past based on distance and speed
        float desiredTimeInPast = distanceBehind / moveSpeed;

        // Interpolate between history points to find the target position at the desired time in the past
        Vector3 targetPosition = InterpolateTargetPosition(desiredTimeInPast);

        return targetPosition;
    }

    Vector3 InterpolateTargetPosition(float desiredTimeInPast)
    {
        // If there are not enough history points, return current target position
        if (targetHistory.Count < 2)
        {
            return target.transform.position;
        }

        // Find the index of the history point closest to the desired time in the past
        float closestTimeDifference = Mathf.Infinity;
        int closestIndex = -1;
        for (int i = 0; i < targetHistory.Count; i++)
        {
            float timeDifference = Mathf.Abs(i * historyInterval - desiredTimeInPast);
            if (timeDifference < closestTimeDifference)
            {
                closestTimeDifference = timeDifference;
                closestIndex = i;
            }
        }

        // Interpolate between the closest history points
        int nextIndex = Mathf.Min(closestIndex + 1, targetHistory.Count - 1);
        float t = desiredTimeInPast / (nextIndex * historyInterval);
        Vector3 interpolatedPosition = Vector3.Lerp(targetHistory[nextIndex], targetHistory[closestIndex], t);

        return interpolatedPosition;
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
        
        if(other.transform.tag =="Wall")
        {
            xSpeed = xSpeed*-1;
        }

        if (other.transform.tag == "Paddle")
        {
            if (KGameController.instance.isHeadingTowardsMe)
            {
                ySpeed = ySpeed * -1;
                ToggleIsTracking();
                RPC_NotifyPaddleBounce();
                KGameController.instance.isHeadingTowardsMe = false;
            } else {
                return;
            }
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

    void ToggleIsHeadingTowardsMe()
    {
        KGameController.instance.ToggleIsHeadingTowardsMe();
    }

    void ToggleIsTracking()
    {
        isTrackingTarget = false;
    }

    public float GetDistanceFromTarget()
    {
        return Vector3.Distance(transform.position, target.transform.position);
    }

    public void SetVelocity(float x, float y)
    {
        rb.velocity = new Vector2(x, y);
    }

    public void SetPosition(float x, float y)
    {
        this.transform.position = new Vector2(x, y); 
    }
}