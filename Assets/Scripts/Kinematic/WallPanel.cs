using System;
using UnityEngine;
using Photon.Pun;

public class WallPanel : MonoBehaviour
{
    public string leftKey, rightKey;
    public float speed;
    private float maxRange;
    private PhotonView myPV;

    // Use this for initialization
    private void Start(){
        
    }

    public void SetWallActive()
    {
        return;
    }

    public void SetWallInactive()
    {
        return;
   } 
}