using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class enemyAI : MonoBehaviour
{
    public GameObject player;
    public float speed;
    public float distanceBetween;
    public float chaseRangeX;
    public GameObject pointA;
    public GameObject pointB;
    private Rigidbody2D rb;
    private Transform currentPoint;
    private bool isChasing = false;

    // Start is called before the first frame update
    void Start()
    {
        rb =GetComponent<Rigidbody2D>();
        currentPoint = pointA.transform;
    }

    // Update is called once per frame
    void Update()
    {
        float distanceToPlayer = Vector2.Distance(transform.position, player.transform.position);

        if (distanceToPlayer < distanceBetween)
      
        {
            float distanceX = Mathf.Abs(player.transform.position.y - transform.position.y);
            if (distanceX < chaseRangeX )
            {
                isChasing = true;
                ChasePlayer();
            }
            else
            {
                isChasing = false;
               Patrol();
            }
        }
        else
        {
            isChasing = false;
           Patrol();
        }
    }

    private void ChasePlayer()
    {
        Vector2 direction = (player.transform.position - transform.position).normalized;
        direction.y = 0;
        rb.velocity = new Vector2(direction.x * speed, rb.velocity.y);
    }

    private void Patrol()
    {
        if (currentPoint == pointB.transform)
        {
            rb.velocity = new Vector2(speed, rb.velocity.y);
        }
        else
        {
            rb.velocity = new Vector2(-speed, rb.velocity.y);
        }

        if (Vector2.Distance(transform.position, currentPoint.position) < 0.5f)
        {
            if (currentPoint == pointB.transform)
            {
                currentPoint = pointA.transform;
            }
            else
            {
                currentPoint = pointB.transform;
            }
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(pointA.transform.position, 0.7f);
        Gizmos.DrawWireSphere(pointB.transform.position, 0.7f);
        Gizmos.DrawLine(pointA.transform.position, pointB.transform.position);
    }
}
