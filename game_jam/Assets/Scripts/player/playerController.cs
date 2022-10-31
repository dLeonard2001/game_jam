using System.Collections;
using System.Collections.Generic;
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
    public List<Image> key_display;
    private int count;
    
    [Header("Player Config")] 
    public float walkSpeed;
    public float jumpForce;
    public float speedMultiplier;
    public Animator playerAnimator;

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
    public CapsuleCollider2D boxCD;
    private CapsuleCollider2D originalBoxCD;

    private RaycastHit2D slopeHit;

    private InputManager inputManager;

    [Header("Puzzle Inventory")] 
    public GameObject iceplant_seed;
    public iceseed seed_script;

    [Header("Checkpoints")] 
    public GameObject lastKnownCheckpoint;
    private Vector3 checkpointPos;

    [Header("Final Puzzle")] 
    public GameObject SeedGameObject;
    public Sprite seed;
    public List<GameObject> first_area_possible_locations;
    public List<GameObject> second_area_possible_locations;
    public List<GameObject> third_area_possible_locations;
    public GameObject final_door;
    public GameObject hint_text;
    private Vector3 seed_spawn_location;
    private bool finalPuzzleIsActive;

    private bool levelFinished;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        inputManager = InputManager.newInstance();

        currentSpeed = walkSpeed;
        slideTimer = maxSlideTimer;
        readyToSlide = true;

        iceplant_seed = null;

        if (panel != null)
        {
            panel.gameObject.SetActive(false);
        }

        count = 0;

        boxCD = GetComponent<CapsuleCollider2D>();
        originalBoxCD = boxCD;
        playerAnimator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        if(inputManager.pause())
            pauseGame();

        if (isPaused)
            return;
        
        if (iceplant_seed != null)
        {
            Vector3 moundPos = iceplant_seed.GetComponent<iceseed>().mound_pos;
            Vector3 distanceToMound = transform.position - moundPos;
            if (distanceToMound.magnitude < 2f)
            {
                count--;
                Debug.Log(count);
                key_display[count].gameObject.SetActive(false);
                seed_script.PuzzleSolved();
                iceplant_seed = null;
            }
        }

        speed.text = "Speed: " + rb.velocity.magnitude;
        isGrounded = Physics2D.Raycast(transform.position, Vector2.down, 1, Ground);

        if (inputManager.jump() && isGrounded)
        {
            playerAnimator.SetTrigger("Jump");
            readyToJump = true;
        }

        if (inputManager.slide() && slideTimer > 0)
            isSliding = true;
        else
            isSliding = false;

        if (slideTimer < maxSlideTimer && isGrounded && !isSliding)
        {
            Debug.Log("done sliding");
            if(!inputManager.slide())
                slideTimer = maxSlideTimer;
            resetBoxCD();
            playerAnimator.SetTrigger("Stop Sliding");
        }
    }

    private void FixedUpdate()
    {
        isOnSlope = OnSlope();

        if (inputManager.moveLeft())
        {
            transform.rotation = Quaternion.Euler(0f, 180f, 0f);
            playerAnimator.SetTrigger("Run");
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
                    decreaseBoxCD();
                    playerAnimator.SetTrigger("Slide");

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
                    decreaseBoxCD();
                    playerAnimator.SetTrigger("Slide");

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
            transform.rotation = Quaternion.Euler(0f, 0, 0f);
            playerAnimator.SetTrigger("Run");
            
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
                    decreaseBoxCD();
                    playerAnimator.SetTrigger("Slide");

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
                    decreaseBoxCD();
                    playerAnimator.SetTrigger("Slide");

                    rb.AddForce(currentSpeed * speedMultiplier * Vector2.right, ForceMode2D.Impulse);
                    slideTimer -= Time.fixedDeltaTime;
                }
                else
                {
                    rb.AddForce(currentSpeed * speedMultiplier * Vector2.right, ForceMode2D.Force);
                }
            }
        }
        else
        {
            playerAnimator.SetTrigger("Stop Running");
            currentDirection = Vector2.zero;
        }

        if (readyToJump)
        {
            currentSpeed += jumpForce - currentSpeed;
            
            rb.AddForce((Vector2.up + currentDirection) * currentSpeed, ForceMode2D.Impulse);
            readyToJump = false;
            
            playerAnimator.SetTrigger("Land");
        }
        else
        {
            rb.AddForce(Vector2.down, ForceMode2D.Force);
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
        if (col.CompareTag("void"))
        {
            OnDeath();
        }
        else if (col.CompareTag("iceplant_seed"))
        {
            if (!finalPuzzleIsActive)
            {
                iceplant_seed = col.gameObject;
                seed_script = iceplant_seed.GetComponent<iceseed>();
            }
            
            key_display[count].sprite = seed_key;
            key_display[count].gameObject.SetActive(true);
            
            count++;
            col.gameObject.SetActive(false);
        }
        else if (col.CompareTag("checkpoint"))
        {
            lastKnownCheckpoint = col.gameObject;
            checkpointPos = lastKnownCheckpoint.transform.position;
        }
        else if (col.CompareTag("final_puzzle"))
        {
            if (count == key_display.Count && !levelFinished)
            {
                FinishLevel();
            }
            else if(!levelFinished)
            {
                SetUpFinalPuzzle(); 
            }
        }
    }

    public void pauseGame()
    {
        if (isPaused)
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;

            isPaused = false;
        }
        else
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.Confined;

            isPaused = true;
        }
    }
    
    public void SetUpFinalPuzzle()
    {
        StartCoroutine(increaseFontSize());
        
        Debug.Log("setting up final puzzle");
        finalPuzzleIsActive = true;
        
        int num = Random.Range(0, first_area_possible_locations.Count);
        seed_spawn_location = first_area_possible_locations[num].transform.position;
        SeedGameObject.transform.position = seed_spawn_location;
        Instantiate(SeedGameObject);
        
        num = Random.Range(0, second_area_possible_locations.Count);
        seed_spawn_location = second_area_possible_locations[num].transform.position;
        SeedGameObject.transform.position = seed_spawn_location;
        Instantiate(SeedGameObject);
        
        num = Random.Range(0, third_area_possible_locations.Count);
        seed_spawn_location = third_area_possible_locations[num].transform.position;
        SeedGameObject.transform.position = seed_spawn_location;
        Instantiate(SeedGameObject);
        
        SeedGameObject.SetActive(false);
    }

    private void FinishLevel()
    {
        for (int i = 0; i < key_display.Count; i++)
        {
            key_display[i].gameObject.SetActive(false);
        }
        levelFinished = true;
        StartCoroutine(openDoor(final_door));
        // final_door.transform.rotation = Quaternion.Euler(90f, 0f,0f);
    }
    
    private void OnDeath()
    {
        transform.position = checkpointPos;
    }

    private void decreaseBoxCD()
    {
        boxCD.offset = new Vector2(boxCD.offset.x, -4.401974f);
        boxCD.size = new Vector2(boxCD.size.x,4.958592f);
    }

    private void resetBoxCD()
    {
        boxCD.offset = new Vector2(boxCD.offset.x,0.5878139f);
        boxCD.size = new Vector2(boxCD.size.x, 15.49737f);
    }

    private IEnumerator increaseFontSize()
    {
        float font_size = 0;

        while (font_size < 12)
        {
            font_size += Time.fixedDeltaTime * 4f;
            hint_text.GetComponent<TextMeshPro>().fontSize = font_size;
            yield return null;
        }
        
    }

    private IEnumerator openDoor(GameObject obj)
    {
        float rot = 0;

        while (rot < 90f)
        {
            rot += Time.fixedDeltaTime * 4f;
            obj.transform.rotation = Quaternion.Euler(rot, 0f, 0f);
            yield return null;
        }
        
        StopAllCoroutines();
    }
}
