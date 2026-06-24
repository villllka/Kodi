using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{

    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float runMultiplier = 1.8f;
    [SerializeField] private float jumpForce = 10f;
    [Header("Ground Check")]
    [SerializeField] private Transform groundCheckPoint;
    [SerializeField] private float groundCheckRadius = 0.2f;
    [SerializeField] private LayerMask groundLayer;

    [Header("Animation")]
    [SerializeField] private Animator animator;
    [SerializeField] private SpriteRenderer spriteRenderer;
    
private bool isFacingRight = true;
    private Rigidbody2D rb;
    private float horizontalInput;
    private bool isGrounded;
    private bool isRunning;

    //public bool isGrounded{ get; private set; }

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        if(animator == null)
        {
            animator = GetComponent<Animator>();
        }
        if(spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }
        if(groundCheckPoint  == null){
            Debug.LogError("Не назначен Ground Check Point в инспекторе!", this);
        }
        if(spriteRenderer != null)
        {
            spriteRenderer.sortingOrder = 10;
        }
    }

    void Update()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        isRunning = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);

        isGrounded = Physics2D.OverlapCircle(groundCheckPoint.position, groundCheckRadius, groundLayer);

        if(isGrounded && (Input.GetButtonDown("Jump") || Input.GetKeyDown(KeyCode.W))){
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
        }

        UpdateAnimator();
        FlipSprite();
    }

    private void FixedUpdate() {
        float currentSpeed = horizontalInput * moveSpeed;
        if(isRunning && horizontalInput != 0){
            currentSpeed *= runMultiplier;
            }

        rb.velocity = new Vector2(currentSpeed, rb.velocity.y);
    }

    private void OnDrawGizmosSelected() {
        if(groundCheckPoint != null){
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(groundCheckPoint.position, groundCheckRadius);
        }
    }

    private void UpdateAnimator()
    {
        if (animator != null)
        {
            float speed = Mathf.Abs(horizontalInput);
            if(isRunning && speed > 0.1f)
            {
                speed *= 1.5f;
            }

            animator.SetFloat("Speed", speed);
            animator.SetBool("IsGrounded", isGrounded);

            bool isJumping = !isGrounded && rb.velocity.y > 0.1f;
            bool isFalling = !isGrounded && rb.velocity.y > -0.1f;

            animator.SetBool("IsJumping", isJumping);
            animator.SetFloat("VerticalVelocity", rb.velocity.y);
        }
    }

    private void FlipSprite()
    {
        if(spriteRenderer == null) return;
        if(horizontalInput > 0 && !isFacingRight)
        {
            Flip();
        } else if(horizontalInput < 0 && isFacingRight)
        {
            Flip();
        }
    }

    private void Flip()
    {
        isFacingRight = !isFacingRight;
        spriteRenderer.flipX = !spriteRenderer.flipX;
    }

    private void OnDisable()
    {
        horizontalInput = 0f;

        if (rb != null)
        {
            rb.velocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }

        if (animator != null)
        {
            animator.SetFloat("Speed", 0f);
            animator.SetBool("IsJumping", false);
            animator.SetFloat("VerticalVelocity", 0f);
        }
    }
}