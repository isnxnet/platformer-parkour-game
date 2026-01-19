using System.Diagnostics;
using System.Runtime.Serialization;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;


public class Player : MonoBehaviour
{
    private Vector3 spawnPosition;

    //RigidBody
    private Rigidbody2D rb;
    private Animator animtr;

    //Key Input
    private float xInput;

    //where character facing
    private int facingDirection = 1;
    private bool facingRight = true;

    [Header("Movement")]
    [SerializeField] private float moveSpeed;
    [SerializeField] private float jumpForce;
    [Header("Movement")]
    [SerializeField] private float dashDuration;
    [SerializeField] private float dashTime;
    [SerializeField] private float dashSpeed;
    [SerializeField] private float dashCD;

    [Header("Collision info")]
    [SerializeField] private float groundCheckDistance;
    [SerializeField] private LayerMask whatIsGround;

    private bool isGrounded;
    [Header("Wall Collision")]
    [SerializeField] private float wallCheckWidth = 0.5f;  // Width of the box
    [SerializeField] private float wallCheckHeight = 1f;   // Height of the box
    [SerializeField] private LayerMask whatIsWall;
    // Offsets for the box origin
    [SerializeField] private float offsetX = 0.5f;
    [SerializeField] private float offsetY = 0f; 
    private bool isTouchingWall;
    [SerializeField] private float wallJumpCD = 0.2f;
    private float wallJumpTimer = 0f;

    [Header("GUI")]
    [SerializeField] private Image dashReadyIndicator;
    [SerializeField] private Image wallJumpReadyIndicator;

    

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //rb is Rigidbody2D for Player
        rb = GetComponent<Rigidbody2D>();
        animtr = GetComponentInChildren<Animator>();
        spawnPosition = transform.position;  // remember where you began
    }

    void Update()
    {
        CheckInput();
        Movement();
        CollisionChecks();
        AnimatorControllers();
        FlipController();
        dashTime -= Time.deltaTime;
        wallJumpTimer -= Time.deltaTime;
        if (Input.GetButtonDown("Sprint"))
        {
            if (dashTime < -1*dashCD)
            {
                dashTime = dashDuration; 
            }
        }
        dashReadyIndicator.enabled = (dashTime < -dashCD);
        wallJumpReadyIndicator.enabled = (wallJumpTimer <= 0f);
    }

    private void CheckInput()
    {
        if (Input.GetButtonDown("Jump"))
        {        
            if (isGrounded)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            }
            else if (isTouchingWall && wallJumpTimer <= 0f)
            {
                // Push the player away from the wall a little
                rb.linearVelocity = new Vector2(-facingDirection * moveSpeed * 0.75f, jumpForce);
                wallJumpTimer = wallJumpCD;
            }
        }
        //horizontal input
        xInput = Input.GetAxisRaw("Horizontal");
    }
    private void AnimatorControllers()
    {
        //check if it is moving/not
        bool isMoving = rb.linearVelocity.x != 0;
        animtr.SetFloat("yVelocity", rb.linearVelocity.y);
        animtr.SetBool("isMoving", isMoving);
        animtr.SetBool("isGrounded", isGrounded);
        animtr.SetBool("isDashing", dashTime > 0);
        animtr.SetBool("isTouchingWall", isTouchingWall);
    }
    private void Movement()
    {
        if (dashTime > 0)
        {
            rb.linearVelocity = new Vector2(facingDirection*dashSpeed, 0);
        }
        else
        {
            rb.linearVelocity = new Vector2(xInput*moveSpeed, rb.linearVelocity.y);
        }
        
    }
    private void CollisionChecks()
    {
        //ground collision check
        isGrounded = Physics2D.Raycast(transform.position, Vector2.down, groundCheckDistance, whatIsGround);
        //wall collision check
        Vector2 wallCheckCenter = new Vector2(transform.position.x + facingDirection * offsetX, transform.position.y + offsetY);
        // Define a box to check for walls
        Collider2D wallCollider = Physics2D.OverlapBox(wallCheckCenter, new Vector2(wallCheckWidth, wallCheckHeight), 0f, whatIsWall);
        isTouchingWall = wallCollider != null;  // True if there's a wall inside the box
    }
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red; 
        Gizmos.DrawLine(transform.position, transform.position + Vector3.down * groundCheckDistance);
        Gizmos.color = Color.blue; 
        Vector3 boxCenter = new Vector3(transform.position.x + facingDirection * offsetX, transform.position.y + offsetY, transform.position.z);
        Gizmos.DrawWireCube(boxCenter, new Vector3(wallCheckWidth, wallCheckHeight, 0f));
    }
    private void Flip()
    {
        facingDirection = facingDirection * -1;
        facingRight = !facingRight;
        transform.Rotate(0, 180, 0);
    }
    private void FlipController()
    {
        if(rb.linearVelocity.x > 0 && !facingRight)
        {
            Flip();
        }
        else if(rb.linearVelocity.x < 0 && facingRight)
        {
            Flip();
        }
    }

    private void Respawn()
    {
        rb.linearVelocity = Vector2.zero;
        transform.position = spawnPosition;  // Move to spawn point
    }

    // Detect trigger for kill zone or fall into void
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("KillZone"))
        {
            Respawn();
        }
        else if (other.CompareTag("Checkpoint"))
        {
            spawnPosition = other.transform.position;
        }
    }
}
