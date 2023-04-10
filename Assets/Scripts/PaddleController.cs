using UnityEngine;
using Photon.Pun;

public class PaddleController : MonoBehaviour
{
    public string leftKey, rightKey;
    public float speed;
    private PhotonView myPV;

    // Use this for initialization
    private void Start(){
        myPV = GetComponent<PhotonView>();
        if(myPV.IsMine){
            Camera.main.transform.rotation = transform.rotation;
        }
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

        if (Input.GetKey(leftKey) && transform.position.x > -2.24)
        {
            transform.Translate(Vector3.left * Time.deltaTime * speed, Space.Self);
        }
        if (Input.GetKey(rightKey) && transform.position.x < 2.24)
        {
            transform.Translate(Vector3.right * Time.deltaTime * speed, Space.Self);
        }
    }
}