using UnityEngine;
using System.Collections;
using Photon.Pun;
using Hashtable = ExitGames.Client.Photon.Hashtable;    

public class BallController : MonoBehaviourPunCallbacks {

    Rigidbody2D myRb;
    bool setSpeed;
    [SerializeField] float speedUp;
    [SerializeField] float scale=1;
    float xSpeed;
    float ySpeed;

    void Start()
    {
        myRb = GetComponent<Rigidbody2D>();
    }
    
    void Update () {
        if(!PhotonNetwork.IsMasterClient){
            return;
        }
        if(GameController.instance.inPlay == true)
        {
            if(!setSpeed)
            {
                setSpeed = true;
                int rot=(int)PhotonNetwork.LocalPlayer.CustomProperties["Rot"];
                xSpeed = Random.Range(1f, 2f) * Random.Range(-1.0f,1.0f)*scale;
                if(GameController.instance.isTurn==(rot==0)){
                    ySpeed=-scale;
                }else{
                    ySpeed=scale;
                }
            }
            MoveBall();
        }
    }

    void MoveBall()
    {
        myRb.velocity = new Vector2(xSpeed, ySpeed);
    }

    [PunRPC]
    private void RPC_bounce(){
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
            RPC_bounce();
        }
        if(other.transform.tag=="Paddle2" && !PhotonNetwork.IsMasterClient){
            this.photonView.RPC("RPC_bounce",PhotonNetwork.MasterClient,true);
        }

    }
    
    void OnTriggerEnter2D(Collider2D other)
    {
        if((other.tag!="EndOne" && other.tag!="EndTwo")){return;}



        int rot=(int)PhotonNetwork.LocalPlayer.CustomProperties["Rot"];
        Hashtable hash=new Hashtable();
        hash.Add("Rot",rot);
        if((other.tag == "EndOne")==(rot==0))
        {
            if(PhotonNetwork.IsMasterClient){
                int score = (int)PhotonNetwork.LocalPlayer.CustomProperties["Score"];
                score++;
                hash.Add("Score",score);
                PhotonNetwork.SetPlayerCustomProperties(hash);
            }
            GameController.instance.isTurn=false;
        }
        else
        {
            //GameController.instance.scoreTwo++;
            if(PhotonNetwork.IsMasterClient){
                GameController.OtherPlayerScored();
            }
            GameController.instance.isTurn=true;

        }


        GameController.instance.inPlay = false;
        setSpeed = false;
        myRb.velocity = Vector2.zero;
        this.transform.position = new Vector3(0,0,-1); 
    }
}