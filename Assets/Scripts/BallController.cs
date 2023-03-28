using UnityEngine;
using System.Collections;

public class BallController : MonoBehaviour {

    Rigidbody2D myRb;
    bool setSpeed;
    [SerializeField] float speedUp;
    float xSpeed;
    float ySpeed;

    void Start()
    {
        myRb = GetComponent<Rigidbody2D>();
    }
    
    void Update () {
        
            if(!setSpeed)
            {
                setSpeed = true;
                
                xSpeed = Random.Range(1f, 2f) * Random.Range(0, 2) * 2 - 1;
                ySpeed = Random.Range(1f, 2f) * Random.Range(0, 2) * 2 - 1;
            }
            MoveBall();
        
    }

    void MoveBall()
    {
        myRb.velocity = new Vector2(xSpeed, ySpeed);
    }

    void OnCollisionEnter2D(Collision2D other)
    {
        
        if(other.transform.tag =="Wall")
        {
            xSpeed = xSpeed*-1;
        }
        
        if (other.transform.tag == "Paddle" )
        {
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

    }
    
    void OnTriggerEnter2D(Collider2D other)
    {
        if(other.tag == "EndOne")
        {
            GameController.instance.scoreOne++;
            GameController.instance.textOne.text = GameController.instance.scoreOne.ToString();
            GameController.instance.inPlay = false;
            setSpeed = false;
            myRb.velocity = Vector2.zero;
            this.transform.position = Vector2.zero; 
        }
        else if(other.tag == "EndTwo")
        {
            GameController.instance.scoreTwo++;
            GameController.instance.textTwo.text = GameController.instance.scoreTwo.ToString();
            GameController.instance.inPlay = false;
            setSpeed = false;
            myRb.velocity = Vector2.zero;
            this.transform.position = Vector2.zero; 
        }
    }
}