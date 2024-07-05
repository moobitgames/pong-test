using System;
using UnityEngine;
using Photon.Pun;

public class PaddleController : MonoBehaviour
{
    public string leftKey, rightKey;
    public float speed;
    private PhotonView myPV;

    // Use this for initialization
    private void Start(){
        Debug.Log("qwerty");
        myPV = GetComponent<PhotonView>();
        if(myPV.IsMine){
            Camera.main.transform.rotation = transform.rotation;
            myPV.RPC("RPC_SendName",RpcTarget.OthersBuffered,PhotonNetwork.NickName);
        }
    }

    [PunRPC]
    void RPC_SendName(string nameSent)
    {
        KGameController.instance.SetTheirName(nameSent);
    }

    // Update is called once per frame
    void Update()
    {
        if (myPV.IsMine){
            PaddleMovement();
        }
    }

    void PaddleMovement()
    {
        Vector3 old = transform.position;
        if(Input.GetKey(leftKey)){
            transform.Translate(Vector3.left * Time.deltaTime * speed, Space.Self);
        }
        if(Input.GetKey(rightKey)){
            transform.Translate(Vector3.right * Time.deltaTime * speed, Space.Self);
        }

        if(transform.position.x<-2.24||transform.position.x>2.24){
            transform.position=old;
        }
        // if (Input.GetKey(leftKey) && Math.Abs(transform.InverseTransformPoint(-2.24f,0,0).x)>.01)
        // {
        //     transform.Translate(Vector3.left * Time.deltaTime * speed, Space.Self);
        // }
        // if (Input.GetKey(rightKey) && Math.Abs(transform.InverseTransformPoint(2.24f,0,0).x)>.01)
        // {
        //     transform.Translate(Vector3.right * Time.deltaTime * speed, Space.Self);
        // }
    }
}   