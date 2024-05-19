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

    private float xAxis, yAxis;
    public Transform groundCheck; 
    public float groundCheckRadius = 0.2f;
    public float groundCheckRadiuss = 0.5f;

    private float gravity;

    [SerializeField] LayerMask Whatisground;

    public static PlayerMovement Instance;

    Animator anim;
    [HideInInspector] public PlayerStateList pState;


    [Header("Attack Settings")]
    bool attack;
    float timeBetweenAttack, timeSinceAttack;
    [SerializeField] Transform SideAttackTransform, UpAttackTransform, DownAttackTransform;
    [SerializeField] Vector2 SideAttackArea, UpAttackArea, DownAttackArea;
    [SerializeField] LayerMask attackableLayer;
    [SerializeField] float damage;
    [Space(5)]
    
    [Header("Health Settings")]
    public int health;
    public int maxHealth;
    [Space(5)]

    [Header("Recoil")]
    [SerializeField] int recoilXSteps = 5;
    [SerializeField] int recoilYSteps = 5;
    [SerializeField] float recoilXSpeed = 100;
    [SerializeField] float  recoilYSpeed = 100;
    int stepsXRecoiled, stepsYRecoiled;

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

        health = maxHealth;
    }

    
    // Start is called before the first frame update
    void Start()
    {
        pState = GetComponent<PlayerStateList>();
        Rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        gravity = Rb.gravityScale;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(SideAttackTransform.position, SideAttackArea);
        Gizmos.DrawWireCube(UpAttackTransform.position, UpAttackArea);
        Gizmos.DrawWireCube(DownAttackTransform.position, DownAttackArea);
    }

    // Update is called once per frame
    void  FixedUpdate()
    {
        GetInputs();
        UpdateJumpVariables();
        if(pState.Dashing) return;
        Flip();
        Move();
        Jump();
        StartDash();
        Attack();
        Recoil();
    }

    void GetInputs()
    {
        xAxis = Input.GetAxisRaw("Horizontal");
        yAxis = Input.GetAxisRaw("Vertical");
        attack = Input.GetMouseButtonDown(0);
    }

    void Flip()
    {
        if(xAxis < 0)
        {
            transform.localScale = new Vector2(-1, transform.localScale.y);
            pState.lookingRight = false;
        }
        else if(xAxis > 0)
        {
             transform.localScale = new Vector2(1, transform.localScale.y);
            pState.lookingRight = true;
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

    void Attack()
    {
        timeSinceAttack += Time.deltaTime;
        if (attack && timeSinceAttack > timeBetweenAttack)
        {
            timeSinceAttack = 0;
            anim.SetTrigger("Attacking");

            if (yAxis == 0 || yAxis < 0 && Grounded())
            {
                Hit(SideAttackTransform, SideAttackArea, ref pState.recoilingX, recoilXSpeed);
            }
            else if (yAxis > 0)
            {
                Hit(UpAttackTransform, UpAttackArea, ref pState.recoilingY, recoilYSpeed);
            }
            else if (yAxis < 0 && !Grounded())
            {
                Hit(DownAttackTransform, DownAttackArea, ref pState.recoilingY, recoilYSpeed);
            }
        }
    }

    private void Hit(Transform _attackTransform, Vector2 _attackArea, ref bool _recoilDir, float _recoilStrenght)
    {
        Collider2D[] objectsToHit = Physics2D.OverlapBoxAll(_attackTransform.position, _attackArea, 0, attackableLayer);

        bool hitEnemy = false;

        // if (objectsToHit.Length > 0)
        // {
        //     _recoilDir = true;
        //     stepsXRecoiled = 0;
        //     stepsYRecoiled = 0;
        // }
        for (int i = 0; i < objectsToHit.Length; i++)
        {
            if (objectsToHit[i].GetComponent<Enemy>() != null)
            {
                objectsToHit[i].GetComponent<Enemy>().EnemyHit(damage, (transform.position - objectsToHit[i].transform.position).normalized, _recoilStrenght);
                hitEnemy = true;
            }
        }

        if (hitEnemy)
        {
            _recoilDir = true;
            stepsXRecoiled = 0;
            stepsYRecoiled = 0;
        }
    }

    void Recoil()
    {
        if (pState.recoilingX)
        {
            if (pState.lookingRight)
            {
                Rb.velocity = new Vector2(-recoilXSpeed, 0);
            }
            else
            {
                Rb.velocity = new Vector2(recoilXSpeed, 0);
            }
        }

        if (pState.recoilingY)
        {
            Rb.gravityScale = 0;
            if (yAxis < 0)
            {
                Rb.velocity = new Vector2(Rb.velocity.x, recoilYSpeed);
            }
            else
            {
                Rb.velocity = new Vector2(Rb.velocity.x, -recoilYSpeed);
            }
            airJumpCounter = 0;
        }
        else
        {
            Rb.gravityScale = gravity;
        }

        if (pState.recoilingX && stepsXRecoiled < recoilXSteps)
        {
            stepsXRecoiled ++;
        }
        else
        {
            StopRecoilX();
        }

        if (pState.recoilingY && stepsYRecoiled < recoilYSteps)
        {
            stepsYRecoiled ++;
        }
        else
        {
            StopRecoilY();
        }

        if (Grounded())
        {
            StopRecoilY();
        }
    }

    void StopRecoilX()
    {
        stepsXRecoiled = 0;
        pState.recoilingX = false;
    }

    void StopRecoilY()
    {
        stepsYRecoiled = 0;
        pState.recoilingY = false;
    }

    public void TakeDamage(float _damage)
    {
        Debug.Log("Player took damage: " + _damage);
        health -= Mathf.RoundToInt(_damage);
        StartCoroutine(StopTakingDamage());
    }

    IEnumerator StopTakingDamage()
    {
        pState.invincible = true;
        anim.SetTrigger("TakeDamage");
        ClampHealth();
        yield return new WaitForSeconds(1f);
        pState.invincible = false;
    }

    void ClampHealth()
    {
        health = Mathf.Clamp(health, 0, maxHealth);
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
