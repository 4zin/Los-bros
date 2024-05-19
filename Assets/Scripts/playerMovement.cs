using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class playerMovement : MonoBehaviour
{

    public float speed;
    public float jumpForce;
    private bool onTheFloor;
    private bool oneWayPlatform;
    private GameObject currentOneWayPlatform;
    private Rigidbody2D rb;
    [SerializeField] private BoxCollider2D playerCollider;


    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 2;
    }

    // Update is called once per frame
    void Update()
    {
        Move();
        Jump();
        HandleOneWayPlatform();
    }

    private void Move()
    {
        float moveInput = Input.GetAxis("Horizontal");
        transform.position += new Vector3(moveInput * speed * Time.deltaTime, 0, 0);
    }

    private void Jump()
    {
        if (Input.GetKeyDown(KeyCode.Space) && onTheFloor == true)
        {
            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
            onTheFloor = false;
        }
    }

    private void HandleOneWayPlatform()
    {
        if (Input.GetKeyDown(KeyCode.S) && oneWayPlatform)
        {
            StartCoroutine(DisableCollision());
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Floor") || collision.gameObject.CompareTag("oneWayPlatform"))
        {
            onTheFloor = true;

            if (collision.gameObject.CompareTag("oneWayPlatform"))
            {
                currentOneWayPlatform = collision.gameObject;
                oneWayPlatform = true;
            }
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("oneWayPlatform"))
        {
            currentOneWayPlatform = null;
            oneWayPlatform = false;
        }

        else if (collision.gameObject.CompareTag("Floor"))
        {
            onTheFloor = false;
        }
    }

    private IEnumerator DisableCollision()
    {
        BoxCollider2D platformCollider = currentOneWayPlatform.GetComponent<BoxCollider2D>();

        Physics2D.IgnoreCollision(playerCollider, platformCollider);
        yield return new WaitForSeconds(1f);
        Physics2D.IgnoreCollision(playerCollider, platformCollider, false);
    }

}
