using System;
using System.Collections;
using System.Numerics;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Quaternion = UnityEngine.Quaternion;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

public class playerController : MonoBehaviour
{
    [Header("UI")] 
    public GameObject panel;
    public TextMeshProUGUI speed;
    public Sprite seed_key;
    public Image key_display;
    
    [Header("Player Config")] 
    public float walkSpeed;
    public float jumpForce;
    public float speedMultiplier;

    [Header("Slope Config")] 
    public float downSlopeMultiplier;
    public float slideDownSlopeMultipier;
    public float upSlopeReduction;
    public float maxSlopeAngle;
    
    private Vector3 slopeDirection;
    private bool OnRightSlope;

    private float currentSpeed;

    private bool readyToJump;
    private bool isPaused;
    private bool isGrounded;
    private bool isSliding;
    private bool isOnSlope;
    private Vector2 currentDirection;

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

    [Header("Puzzle Inventory")] 
    public GameObject iceplant_seed;
    public iceseed seed_script;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        inputManager = InputManager.newInstance();

        currentSpeed = walkSpeed;
        slideTimer = maxSlideTimer;
        readyToSlide = true;

        iceplant_seed = null;
    }

    // Update is called once per frame
    void Update()
    {
        if(inputManager.pause())
            pauseGame(isPaused);

        if (isPaused)
            return;
        
        if (iceplant_seed != null)
        {
            Vector3 moundPos = iceplant_seed.GetComponent<iceseed>().mound_pos;
            Vector3 distanceToMound = transform.position - moundPos;
            if (distanceToMound.magnitude < 2f)
            {
                key_display.gameObject.SetActive(false);
                seed_script.PuzzleSolved();
            }
        }

        speed.text = "Speed: " + rb.velocity.magnitude;
        isGrounded = Physics2D.Raycast(transform.position, Vector2.down, 1, Ground);
        
        if (inputManager.jump() && isGrounded)
            readyToJump = true;

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
        isOnSlope = OnSlope();

        if (inputManager.moveLeft())
        {
            currentDirection = Vector2.left;
            
            slopeDirection = GetSlopeMoveDirection(currentDirection);
            
            if (slopeDirection.y < 0 && slopeDirection.x < 0)
            {
                OnRightSlope = true;
            }
            else
            {
                OnRightSlope = false;
            }

            if (OnRightSlope)
            {
                if (slopeDirection.y < 0 && isSliding)
                {

                    currentSpeed += slideDownSlopeMultipier;
                    
                    rb.AddForce(currentSpeed * speedMultiplier * (Vector2.left + Vector2.down), ForceMode2D.Force);
                }else if (slopeDirection.y < 0)
                {
                    currentSpeed += downSlopeMultiplier;
                    
                    rb.AddForce(currentSpeed * speedMultiplier * (Vector2.left + Vector2.down), ForceMode2D.Force);
                }
            }
            else if (isOnSlope)
            {
                if (slopeDirection.y > 0)
                {
                    rb.AddForce(currentSpeed * (Vector2.left + Vector2.up), ForceMode2D.Force);
                }
            }
            else
            {
                if (isSliding && slideTimer > 0)
                {

                    rb.AddForce(currentSpeed * speedMultiplier * Vector2.left, ForceMode2D.Impulse);
                    slideTimer -= Time.fixedDeltaTime;
                }
                else
                {
                    rb.AddForce(currentSpeed * speedMultiplier * Vector2.left, ForceMode2D.Force);
                } 
            }
        }else if (inputManager.moveRight())
        {
            currentDirection = Vector2.right;
            slopeDirection = GetSlopeMoveDirection(currentDirection);
            
            if (slopeDirection.x > 0 && slopeDirection.y > 0)
            {
                OnRightSlope = true;
            }
            else
            {
                OnRightSlope = false;
            }

            if (isOnSlope && OnRightSlope)
            {
                rb.AddForce(currentSpeed * speedMultiplier * (Vector2.right + Vector2.up), ForceMode2D.Force);
            }
            else if (isOnSlope)
            {
                if (slopeDirection.y < 0 && isSliding)
                {

                    currentSpeed += slideDownSlopeMultipier;

                    rb.AddForce(currentSpeed * speedMultiplier * (Vector2.right + Vector2.down), ForceMode2D.Force);
                }
                else if (slopeDirection.y < 0)
                {
                    currentSpeed += downSlopeMultiplier;

                    rb.AddForce(currentSpeed * (Vector2.right + Vector2.down), ForceMode2D.Force);
                }
                else if (slopeDirection.y > 0)
                {
                    rb.AddForce(currentSpeed * (Vector2.right + Vector2.up), ForceMode2D.Force);
                }
            }
            else
            {
                if (isSliding && slideTimer > 0)
                {

                    rb.AddForce(currentSpeed * speedMultiplier * Vector2.right, ForceMode2D.Impulse);
                    slideTimer -= Time.fixedDeltaTime;
                }
                else
                {
                    rb.AddForce(currentSpeed * speedMultiplier * Vector2.right, ForceMode2D.Force);
                }
            }
        }

        if (readyToJump)
        {
            rb.AddForce((Vector2.up + currentDirection) * currentSpeed, ForceMode2D.Impulse);
            readyToJump = false;
        }
        else
        {
            rb.AddForce(Vector3.down, ForceMode2D.Force);
        }

        // if our speed has drastically changed
            // reduce our speed slowly over time
                // EX.
                    // 11 -> 10.9 -> 10.8, instead of, 11 -> 9 or 11 -> 5
        if (Mathf.Abs(currentSpeed - walkSpeed) > 2f)
        {
            StopAllCoroutines();
            StartCoroutine(ReduceMovementSpeedOverTime());
        }
        
        // if (on a slope)
            // maintain speed based on the increase in our speed
        // else if(speed > currentspeed)
            // max our speed out to the current speed
            
        if (OnSlope())
        {
            if (rb.velocity.magnitude > currentSpeed)
                rb.velocity = rb.velocity.normalized * currentSpeed;
        }
        else if(rb.velocity.magnitude > currentSpeed)
        {
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
            {
                // Debug.Log(180 - Vector2.Angle(slopeHit.normal, Vector2.down) < maxSlopeAngle);
                return 180 - Vector2.Angle(slopeHit.normal, Vector2.down) < maxSlopeAngle;
            }
        }
        
        return false;
    }

    private Vector3 GetSlopeMoveDirection(Vector2 direction)
    {
        return Vector3.ProjectOnPlane(direction, slopeHit.normal).normalized;
    }

    private IEnumerator ReduceMovementSpeedOverTime()
    {
        while (currentSpeed > walkSpeed)
        {
            currentSpeed -= 0.01f;
            yield return null;
        }

        currentSpeed = walkSpeed;
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (col.CompareTag("iceplant_seed"))
        {
            iceplant_seed = col.gameObject;
            seed_script = iceplant_seed.GetComponent<iceseed>();

            key_display.sprite = seed_key;
            key_display.gameObject.SetActive(true);

            col.gameObject.SetActive(false);
        }
    }

    private void pauseGame(bool status)
    {
        if (status)
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
            
            isPaused = false;
            panel.SetActive(false);

            Time.timeScale = 1;
        }
        else
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.Confined;
            
            isPaused = true;
            panel.SetActive(true);

            Time.timeScale = 0;
        }
    }
}
