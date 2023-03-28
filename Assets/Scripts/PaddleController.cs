using UnityEngine;

public class PaddleController : MonoBehaviour
{
    public string leftKey, rightKey;
    public float speed;

    // Use this for initialization

    // Update is called once per frame
    void Update()
    {
        PaddleMovement();
    }

    void PaddleMovement()
    {
        if (Input.GetKey(leftKey) && transform.position.x > -2.24)
        {
            transform.Translate(Vector3.left * Time.deltaTime * speed, Space.World);
        }
        if (Input.GetKey(rightKey) && transform.position.x < 2.24)
        {
            transform.Translate(Vector3.right * Time.deltaTime * speed, Space.World);
        }
    }
}