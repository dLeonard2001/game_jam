using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;
using Quaternion = UnityEngine.Quaternion;
using Vector2 = UnityEngine.Vector2;

public class playerController : MonoBehaviour
{
    [Header("Player Config")] 
    public float walkSpeed;
    public float jumpForce;

    private float sprintSpeed;
    private float slideSpeed;
    private float currentSpeed;

    private bool readyToJump;
    private bool pauseGame;
    private bool isGrounded;
    private bool isSliding;

    [Header("Slide Config")] 
    public float slideTimer;
    public float maxSlideTimer;
    public float slideCooldown;

    private bool readyToSlide;

    [Header("Components")] 
    public Rigidbody2D rb;
    public LayerMask Ground;

    private RaycastHit2D slopeHit;

    private InputManager inputManager;
    
    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        
        inputManager = InputManager.newInstance();

        sprintSpeed = walkSpeed * 1.75f;
        slideSpeed = walkSpeed * 2.25f;

        currentSpeed = walkSpeed;

        slideTimer = maxSlideTimer;

        readyToSlide = true;
    }

    // Update is called once per frame
    void Update()
    {
        isGrounded = Physics2D.Raycast(transform.position, Vector2.down, 1, Ground);
        
        if (inputManager.jump() && isGrounded)
            readyToJump = true;
        if (inputManager.pause())
            pauseGame = true;

        if (inputManager.slide())
            isSliding = true;
        else
            isSliding = false;

        if (slideTimer < maxSlideTimer && isGrounded && !isSliding)
        {
            slideTimer = maxSlideTimer;
            slidePlayerAnimation(0);
        }
    }

    private void FixedUpdate()
    {
        if (inputManager.moveLeft())
        {
            transform.rotation = Quaternion.Euler(0f, -180f,0f);
            if (inputManager.sprint())
            {
                if (isSliding && slideTimer > 0)
                {
                    currentSpeed = slideSpeed;
                    rb.AddForce(Vector2.left * currentSpeed, ForceMode2D.Impulse);
                    slideTimer -= Time.fixedDeltaTime;
                    
                    slidePlayerAnimation(75f);
                }
                else
                {
                    currentSpeed = sprintSpeed;
                    rb.AddForce(Vector2.left * currentSpeed, ForceMode2D.Impulse);
                }
            }
            else
            {
                currentSpeed = walkSpeed;
                rb.AddForce(Vector2.left * currentSpeed, ForceMode2D.Impulse);
            }
        }else if (inputManager.moveRight())
        {
            transform.rotation = Quaternion.Euler(0f, 0f,0f);
            if (inputManager.sprint())
            {
                if (isSliding && slideTimer > 0)
                {
                    currentSpeed = slideSpeed;
                    rb.AddForce(Vector2.right * currentSpeed, ForceMode2D.Impulse);
                    slideTimer -= Time.fixedDeltaTime;
                    
                    slidePlayerAnimation(75f);
                }
                else
                {
                    currentSpeed = sprintSpeed;
                    rb.AddForce(Vector2.right * currentSpeed, ForceMode2D.Impulse);
                }
            }
            else
            {
                currentSpeed = walkSpeed;
                rb.AddForce(Vector2.right * currentSpeed, ForceMode2D.Impulse);
            }
        }

        if (readyToJump)
        {
            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
            readyToJump = false;
        }
        else
        {
            rb.AddForce(Vector2.down, ForceMode2D.Impulse);
        }

        if (rb.velocity.magnitude > currentSpeed)
        {
            if (!isGrounded)
                currentSpeed = jumpForce;
            Vector2 flatVel = new Vector2(rb.velocity.x, rb.velocity.y);
            Vector2 limitedVel = currentSpeed * flatVel.normalized;
            rb.velocity = new Vector2(limitedVel.x, rb.velocity.y);
        }
    }

    public void slidePlayerAnimation(float rotation)
    {
        transform.rotation = Quaternion.Euler(0f, transform.eulerAngles.y, rotation);
    }

    public bool OnSlope()
    {
        if (!isGrounded)
            return false;

        slopeHit = Physics2D.Raycast(transform.position, Vector2.down, 1, Ground);

        if (slopeHit)
        {
            if (slopeHit.normal != Vector2.up)
                return true;
        }
        
        return false;
    }
}
