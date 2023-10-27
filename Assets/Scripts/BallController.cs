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
    bool clientGoal=false;
    bool masterGoal=false;
    Collider2D endGoal;

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
        masterGoal=false;
        clientGoal=false;

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
            masterGoal=false;
            clientGoal=false;
            this.photonView.RPC("RPC_bounce",PhotonNetwork.MasterClient,true);
        }

    }

    private void score(){
        int rot=(int)PhotonNetwork.LocalPlayer.CustomProperties["Rot"];
        Hashtable hash=new Hashtable();
        hash.Add("Rot",rot);
        if((endGoal.tag == "EndOne")==(rot==0))
        {
            if(PhotonNetwork.IsMasterClient){
                int score = (int)PhotonNetwork.LocalPlayer.CustomProperties["Score"];
                score++;
                hash.Add("Score",score);
                PhotonNetwork.SetPlayerCustomProperties(hash);
            }
            GameController.instance.setIsTurn(false);
        }
        else
        {
            //GameController.instance.scoreTwo++;
            if(PhotonNetwork.IsMasterClient){
                GameController.OtherPlayerScored();
            }
            GameController.instance.setIsTurn(true);
        }

        GameController.instance.setInPlay(false);
        setSpeed = false;
        myRb.velocity = Vector2.zero;
        this.transform.position = new Vector3(0,0,-1); 
        clientGoal=false;
        masterGoal=false;
    }


    [PunRPC]
    private void RPC_clientGoal(bool val){
        clientGoal=val;
        Debug.Log("clientGoal");
        if(clientGoal && masterGoal){
            score();
        }
    }
    
    void OnTriggerEnter2D(Collider2D other)
    {

        if((other.tag!="EndOne" && other.tag!="EndTwo")){return;}

        endGoal=other;

        if(!PhotonNetwork.IsMasterClient){
            this.photonView.RPC("RPC_clientGoal",PhotonNetwork.MasterClient,true);
        }

        if(PhotonNetwork.IsMasterClient && !masterGoal){
            masterGoal=true;
            Debug.Log("masterGoal");
        }

        if(clientGoal && masterGoal){
            score();
        }


    }
}