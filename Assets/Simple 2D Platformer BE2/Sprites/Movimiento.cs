// using System.Collections;
// using System.Collections.Generic;
using UnityEngine;

public class Movimiento : MonoBehaviour
{

    public Transform GetTransform;
    public float Speed;
    public float jumpForce;
    public Rigidbody2D rbd;
    public bool OnTheFloor;

    // Start is called before the first frame update
    void Start()
    {
        rbd.gravityScale = 2;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey(KeyCode.D))
        {
            GetTransform.position += new Vector3(Speed, 0, 0) * Time.deltaTime;
        }

        if (Input.GetKey(KeyCode.A))
        {
            GetTransform.position -= new Vector3(Speed, 0, 0) * Time.deltaTime;
        }

        if (Input.GetKeyDown(KeyCode.Space) && OnTheFloor == true)
        {
            rbd.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
            OnTheFloor = false;
        }
    }

    public void OnCollisionEnter2D(Collision2D piso)
    {

        if (piso.gameObject.CompareTag("Floor"))
        {
            Debug.Log("Flow violento");
            OnTheFloor = true;
        }

    }

}
