using System;
using UnityEngine;
using Photon.Pun;

public class Paddle : MonoBehaviour
{
    public string leftKey, rightKey;
    public float speed;
    private float maxRange;
    private PhotonView myPV;

    // Use this for initialization
    private void Start(){
        myPV = GetComponent<PhotonView>();
        if(myPV.IsMine){
            Camera.main.transform.rotation = transform.rotation;
            myPV.RPC("RPC_SendName",RpcTarget.OthersBuffered,PhotonNetwork.NickName);
        }
    }

    [PunRPC]
    void RPC_SendName(string nameSent)
    {
        GameController.instance.SetTheirName(nameSent);
    }

    // Update is called once per frame, only move if player is the owner
    void Update()
    {
        if (myPV.IsMine){
            MovePaddle();
        }
    }

    void MovePaddle()
    {
        Vector3 old = transform.position;
        if(Input.GetKey(leftKey)){
            transform.Translate(Vector3.left * Time.deltaTime * speed, Space.Self);
        }
        if(Input.GetKey(rightKey)){
            transform.Translate(Vector3.right * Time.deltaTime * speed, Space.Self);
        }

        // TODO: refactor so it just returns automatically instead of moving then recorrecting
        if(transform.position.x< - 1* maxRange||transform.position.x > maxRange){
            transform.position=old;
        }
    }
}   