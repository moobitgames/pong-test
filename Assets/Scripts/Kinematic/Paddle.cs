using System;
using UnityEngine;
using Photon.Pun;

public class Paddle : MonoBehaviour
{
    public string _leftKey, _rightKey;
    public float _speed;
    private float _maxRange;
    private PhotonView _myPV;

    // Use this for initialization
    private void Start(){
        _myPV = GetComponent<PhotonView>();
        if(_myPV.IsMine){
            Camera.main.transform.rotation = transform.rotation;
            _myPV.RPC("RPC_SendName",RpcTarget.OthersBuffered,PhotonNetwork.NickName);
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
        if (_myPV.IsMine){
            MovePaddle();
        }
    }

    void MovePaddle()
    {
        Vector3 old = transform.position;
        if(Input.GetKey(_leftKey)){
            transform.Translate(Vector3.left * Time.deltaTime * _speed, Space.Self);
        }
        if(Input.GetKey(_rightKey)){
            transform.Translate(Vector3.right * Time.deltaTime * _speed, Space.Self);
        }

        // TODO: refactor so it just returns automatically instead of moving then recorrecting
        if(transform.position.x< - 1* _maxRange||transform.position.x > _maxRange){
            transform.position=old;
        }
    }
}   