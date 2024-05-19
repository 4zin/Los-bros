using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    private Rigidbody2D Rb;
    [SerializeField] private float walkSpeed = 1;
    [SerializeField] private float jumpForce = 5;

    private bool canDash = true;
    private bool dashed;

    [SerializeField] GameObject dashEffect;
    [SerializeField] private float dashSpeed;
    [SerializeField] private float dashTime;
    [SerializeField] private float dashCooldown;

    private float coyoteTimeCounter = 0;
    [SerializeField]  private float coyoteTime;

    private int airJumpCounter =0;
    [SerializeField] private int maxAirJumps;

    private int jumpBufferCounter = 0;
    [SerializeField] private int jumpBufferFrames;

    private float xAxis;
    public Transform groundCheck; 
    public float groundCheckRadius = 0.2f;
    public float groundCheckRadiuss = 0.5f;

    private float gravity;

    [SerializeField] LayerMask Whatisground;

    public static PlayerMovement Instance;

    Animator anim;
    PlayerStateList pState;

    private void Awake()
    {
        if(Instance != null && Instance  != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    
    // Start is called before the first frame update
    void Start()
    {
        pState = GetComponent<PlayerStateList>();
        Rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        gravity = Rb.gravityScale;
    }

    // Update is called once per frame
    void Update()
    {
        GetInputs();
        UpdateJumpVariables();
        if(pState.Dashing) return;
        Flip();
        Move();
        Jump();
        StartDash();
    }

    void GetInputs()
    {
        xAxis = Input.GetAxisRaw("Horizontal");
    }

    void Flip()
    {
        if(xAxis < 0)
        {
            transform.localScale = new Vector2(-1, transform.localScale.y);
        }
        else if(xAxis > 0)
        {
             transform.localScale = new Vector2(1, transform.localScale.y);
        }
    }
    private void Move()
    {
        Rb.velocity = new Vector2(walkSpeed * xAxis, Rb.velocity.y  );
        anim.SetBool("Walking", Rb.velocity.x != 0 && Grounded());
    }

    void StartDash()
    {
        if(Input.GetButtonDown("Dash") && canDash) //&& canDash && !dashed)
        {
           
            StartCoroutine(Dash());
            dashed = true;
        }
        if(Grounded())
        {
            dashed = false;
        }

    }

IEnumerator Dash(){
    canDash = false;
    pState.Dashing=true;
    anim.SetTrigger("Dashing");
    Rb.gravityScale = 0;
    Rb.velocity = new Vector2(transform.localScale.x * dashSpeed, 0);
    if(Grounded()) Instantiate(dashEffect, transform);
    yield return new WaitForSeconds(dashTime);
    Rb.gravityScale = gravity;
    pState.Dashing = false;
    yield return new WaitForSeconds(dashCooldown);
    canDash = true;

}
     private void Jump()
    {
        if (Input.GetButtonUp("Jump") && Rb.velocity.y > 0)
        {
            Rb.velocity = new Vector2(Rb.velocity.x, 0);
            pState.Jumping = false;
        }
        if(!pState.Jumping)
        {
            if(jumpBufferCounter > 0 &&  coyoteTimeCounter > 0)
            {
                Rb.velocity = new Vector3(Rb.velocity.x, jumpForce);
                pState.Jumping = true;
            }
            else if(!Grounded() && airJumpCounter < maxAirJumps && Input.GetButtonDown("Jump"))
            {
                pState.Jumping = true;
                airJumpCounter++;
                Rb.velocity = new Vector3(Rb.velocity.x, jumpForce);
            }
          
        }
        anim.SetBool("Jumping", !Grounded());

       
    }

    public bool Grounded()
    {
        if(Physics2D.Raycast(groundCheck.position, Vector2.down, groundCheckRadius, Whatisground) || Physics2D.Raycast(groundCheck.position + new Vector3(groundCheckRadiuss, 0, 0), Vector2.down, groundCheckRadius, Whatisground)|| Physics2D.Raycast(groundCheck.position + new Vector3(-groundCheckRadiuss, 0, 0), Vector2.down, groundCheckRadius, Whatisground))
        {
            return true;
        }
        return false;

    }

    void UpdateJumpVariables()
    {
        if (Grounded()){
            pState.Jumping = false;
            coyoteTimeCounter = coyoteTime;
            airJumpCounter =0;
        }
        else
        {
            coyoteTimeCounter -= Time.deltaTime;
        }
        if(Input.GetButtonDown("Jump"))
        {
            jumpBufferCounter = jumpBufferFrames;
        }
        else
        {
            jumpBufferCounter--;
        }
    }
}
