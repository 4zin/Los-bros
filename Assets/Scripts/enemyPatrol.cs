using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class enemyPatrol2 : MonoBehaviour
{

    public GameObject pointA;
    public GameObject pointB;
    public float speed;
    private Rigidbody2D rb;
    private Transform currentPoint;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        currentPoint = pointA.transform;
    }

    // Update is called once per frame
    void Update()
    {
        Vector2 point = currentPoint.position - transform.position;
        if (currentPoint == pointB.transform)
        {
            rb.velocity = new Vector2(speed, 0);
        }
        else
        {
            rb.velocity = new Vector2(-speed, 0);
        }

        if (Vector2.Distance(transform.position, currentPoint.position) < 0.5f && currentPoint == pointB.transform)
        {
            currentPoint = pointA.transform;
        }

        if (Vector2.Distance(transform.position, currentPoint.position) < 0.5f && currentPoint == pointA.transform)
        {
            currentPoint = pointB.transform;
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(pointA.transform.position, 0.7f);
        Gizmos.DrawWireSphere(pointB.transform.position, 0.7f);
        Gizmos.DrawLine(pointA.transform.position, pointB.transform.position);
    }
}
