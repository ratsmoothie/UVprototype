using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//credit to Dave / GameDevelopment on youtube for his movement tutorials helping me with these scripts

public class UVplayerMovement : MonoBehaviour
{
    [Header("Movement")]
    private float movementSpeed;
    public float walkingSpeed;
    public float sprintingSpeed;
    public float slidingSpeed;
    public float wallrunningSpeed;

    private float desiredMovementSpeed;
    private float lastDesiredMovementSpeed;

    public float groundDrag;
    

    [Header("Air Movement")]
    public float jumpForce;
    public float jumpCooldown;
    public float airMultiplier;

    [Header("Crouching")]
    public float crouchMovementSpeed;
    public float crouchHeightScale;
    private float normalPlayerHeight;

    [Header("Sliding")]
    public float maxSlideTime;
    public float slideForce;
    private float slideTime;
    public float accelerationIncreaseMult;
    public float slopeAccelIncreaseMult;
    //end movement header


    [Header("Boundary Checks")]
    [Header("Ground Check")]
    public float playerHeight;
    public LayerMask isGround;
    public LayerMask isSlope;
    //end ground check header

    [Header("Slope Movement")]
    public float maxAngle;
    private RaycastHit slopeRaycast;
    private bool jumpingOffSlope;

    [Header("Controls")]
    public KeyCode jumpButton = KeyCode.Space;
    public KeyCode sprintButton = KeyCode.LeftShift;
    public KeyCode crouchButton = KeyCode.LeftControl;
    public KeyCode slideButton = KeyCode.LeftControl;
    //end controls header

    [Header("Player Status")]
    [Header("Movement Status")]
    public bool grounded = false;
    private bool readyToJump = true;
    private bool canStand;
    private bool isSliding;
    private bool isCrouching;
    public bool isWallrunning;
    public playerMovementState currentMovementState;
    
    [Header("Combat Status")]

    [Header("Player Camera")]
    public Transform orientation;

    float horizontalInput;
    float verticalInput;

    Vector3 movementDirection;

    Rigidbody playerRigidBody;

    PlayerStats playerStats;

    public enum playerMovementState
    {
        walking,
        sprinting,
        crouching,
        wallrunning,
        sliding,
        airborne
    }

    // Start is called before the first frame update
    void Start()
    {
        playerRigidBody = GetComponent<Rigidbody>();
        playerRigidBody.freezeRotation = true;

        //saves normal height on game start
        normalPlayerHeight = transform.localScale.y;

        playerStats = GetComponentInChildren<PlayerStats>();
    }

    // Update is called once per frame
    void Update()
    {
        //check if player is on the ground
        grounded = (Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + 0.2f, isSlope) || Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + 0.2f, isGround));



        // COME BACK TO THIS!!!!!

        //check if the player has room to stand up
        //canStand = Physics.Raycast(transform.position, Vector3.up, playerHeight * 0.25f, isGround);

        playerInput();
        SpeedLimit();
        playerStateHandler();

        //less drag in the air
        if (grounded)
        {
            playerRigidBody.drag = groundDrag;
        }
        else
        {
            playerRigidBody.drag = 2;
        }

        //sliding
        if(Input.GetKeyDown(slideButton) && (horizontalInput != 0 || verticalInput != 0))
        {
            startSlide();
        }

        if(Input.GetKeyUp(slideButton) && isSliding)
        {
            endSlide();
        }
    }//end update

    private void FixedUpdate()
    {
        playerPhysics();

        if (isSliding)
        {
            Sliding();
        }
    }

    private void playerInput()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

        //jumping and jump reset
        if(Input.GetKey(jumpButton) && readyToJump == true && grounded == true)
        {
            readyToJump = false;
            Jump();

            //allows for continuous jumping
            Invoke(nameof(JumpReset), jumpCooldown);
        }

        //transforms our player to be crouch height, keeping x and y scaling the same. Adds force to the player to push it to the ground.
        if(Input.GetKeyDown(crouchButton))
        {
            transform.localScale = new Vector3(transform.localScale.x, crouchHeightScale, transform.localScale.z);
            playerRigidBody.AddForce(Vector3.down * 5f, ForceMode.Impulse);
        }

        //uncrouch when releasing ctrl IF the player has room

        //COME BACK TO THE CAN STAND PART!!!!!

        if (Input.GetKeyUp(crouchButton) /*&& canStand*/)
        {
            transform.localScale = new Vector3(transform.localScale.x, normalPlayerHeight, transform.localScale.z);
        }

        /*
        // SLIDEMAN
        if(Input.GetKeyDown(crouchButton) && playerRigidBody.velocity.magnitude > 0)
        {
            isSliding = true;

            //crouching resize
            transform.localScale = new Vector3(transform.localScale.x, crouchHeightScale, transform.localScale.z);
            
            //force forward
            playerRigidBody.AddForce(Vector3.forward * 7f, ForceMode.Impulse);
            //reduce drag
            playerRigidBody.drag = 1;
            //higher speed limit
            movementSpeed = slidingSpeed;

            if(playerRigidBody.velocity.magnitude == 0f)
            {
                isSliding = false;
            }
        }*/
       
    }

    /*
    //script from Dave / GameDevelopment at https://www.youtube.com/watch?v=SsckrYYxcuM&t=448s
    private IEnumerator SmoothlyLerpMoveSpeed()
    {
        //smoothly lerp movementSpeed to desired value
        float time = 0;
        float difference = Mathf.Abs(desiredMovementSpeed - movementSpeed);
        float startValue = movementSpeed;

        while (time < difference)
        {
            movementSpeed = Mathf.Lerp(startValue, desiredMovementSpeed, time / difference);

            if(onSlope())
                {
                float slopeAngle = Vector3.Angle(Vector3.up, slopeRaycast.normal);
                float slopeAngleIncrease = 1 + (slopeAngle / 90f);

                time += Time.deltaTime * accelerationIncreaseMult * slopeAccelIncreaseMult * slopeAngleIncrease;
                }
            else
            {
                time += Time.deltaTime * accelerationIncreaseMult;
            }
            
            yield return null;
        }

        movementSpeed = desiredMovementSpeed;
    }
    */
    //above script from Dave / GameDevelopment at https://www.youtube.com/watch?v=SsckrYYxcuM&t=448s
    //tweaked by me bc movement speed was being slowed strangely
    private IEnumerator SmoothlyLerpMoveSpeed()
    {
        //smoothly lerp movementSpeed to desired value
        float time = 0;
        float difference = Mathf.Abs(desiredMovementSpeed - movementSpeed);
        float startValue = movementSpeed;
        float accelerationMultiplier = 1f;

        if (onSlope())
        {
            float slopeAngle = Vector3.Angle(Vector3.up, slopeRaycast.normal);
            float slopeAngleIncrease = 1f + (slopeAngle / 90f);
            accelerationMultiplier *= slopeAccelIncreaseMult * slopeAngleIncrease;
        }

        while (time < difference)
        {
            movementSpeed = Mathf.Lerp(startValue, desiredMovementSpeed, Mathf.Clamp01(time / difference));

            time += Time.deltaTime * accelerationIncreaseMult * accelerationMultiplier;

            yield return null;
        }

        movementSpeed = desiredMovementSpeed;
    }


    private void playerPhysics()
    {
        //maths out our movement direction
        movementDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;
        
        //ground movement
        if(grounded == true)
        { 
        playerRigidBody.AddForce(movementDirection.normalized * movementSpeed * 10f, ForceMode.Force);
        }

        if (onSlope() && !jumpingOffSlope)
        {
            playerRigidBody.AddForce(slopeMovementVector(movementDirection) * movementSpeed * 20f, ForceMode.Force);

            //if player is moving upwards on a slope adds some downward force to prevent bouncing
            if(playerRigidBody.velocity.y > 0)
            {
                playerRigidBody.AddForce(Vector3.down * 80f, ForceMode.Force);
            }
        }

        //air control
        else if(grounded == false)
        {
            playerRigidBody.AddForce(movementDirection.normalized * movementSpeed * 10f * airMultiplier, ForceMode.Force);
        }

        if(!isWallrunning)
        {
            playerRigidBody.useGravity = !onSlope();
        }
        

    }

    //speed would surpass set movement speed for some reason, maybe acceleration or something? This function locks it down
    private void SpeedLimit()
    {
        //slope speed limit
        if (onSlope() && !jumpingOffSlope)
        {
            if(playerRigidBody.velocity.magnitude > movementSpeed)
            { 
                playerRigidBody.velocity = playerRigidBody.velocity.normalized * movementSpeed;
            }
        }

        else
        {
            Vector3 flatVelocity = new Vector3(playerRigidBody.velocity.x, 0f, playerRigidBody.velocity.z);

            //if player is travelling faster than the allowed movement speed, the desired movement speed is calculated and applied to the player
            if (flatVelocity.magnitude > movementSpeed)
            {
                Vector3 limitedVelocity = flatVelocity.normalized * movementSpeed;
                playerRigidBody.velocity = new Vector3(limitedVelocity.x, playerRigidBody.velocity.y, limitedVelocity.z);
            }
        }
    }

    private void Jump()
    {
        jumpingOffSlope = true;

        playerRigidBody.velocity = new Vector3(playerRigidBody.velocity.x, 0f, playerRigidBody.velocity.z);

        playerRigidBody.AddForce(transform.up * jumpForce, ForceMode.Impulse);
    }

    private void JumpReset()
    {
        readyToJump = true;

        jumpingOffSlope = false;
    }

    private void playerStateHandler()
    {
        if(isWallrunning)
        {
            currentMovementState = playerMovementState.wallrunning;
            desiredMovementSpeed = wallrunningSpeed;
        }
        //walk state
        if (grounded /* && !Input.GetKey(crouchButton) && !Input.GetKey(sprintButton) && !isSliding*/)
        {
            currentMovementState = playerMovementState.walking;
            desiredMovementSpeed = walkingSpeed;
        }

        //sprint state
        if (Input.GetKey(sprintButton) && grounded)
        {
            currentMovementState = playerMovementState.sprinting;
            desiredMovementSpeed = sprintingSpeed;
        }

        //crouch state
        if (Input.GetKeyDown(crouchButton) && grounded)
        {
            currentMovementState = playerMovementState.crouching;
            desiredMovementSpeed = crouchMovementSpeed;
        }

        //sliding state
        if (isSliding)
        {
            currentMovementState = playerMovementState.sliding;

            //if the player is on a slope we'll slide at an increased speed
            if(onSlope() && playerRigidBody.velocity.y < 0.1f)
            {
                desiredMovementSpeed = slidingSpeed;
            }
            else
            {
                desiredMovementSpeed = sprintingSpeed;
            }
        }
        //airborne state
        if (!grounded)
        {
            currentMovementState = playerMovementState.airborne;
        }

        //if the player's movement speed changes by a lot it will happen over time.
        //Little changes such as transitioning from walking to sprinting will happen instantly.
        //once again credit to Dave / GameDevelopment on youtube
        else if (Mathf.Abs(desiredMovementSpeed - lastDesiredMovementSpeed) > 4f && movementSpeed != 0)
        {
            StopAllCoroutines();
            StartCoroutine(SmoothlyLerpMoveSpeed());
        }
        else
        {
            movementSpeed = desiredMovementSpeed;
        }

        lastDesiredMovementSpeed = desiredMovementSpeed;
    }

    public bool onSlope()
    {
        //raycast slope check
        // - out slopeRaycast shoots a raycast out with length playerHeight * 0.5f + 0.25f and stores the the object hit.
        if (Physics.Raycast(transform.position, Vector3.down, out slopeRaycast, playerHeight * 0.5f + 0.25f))
        {
            float angle = Vector3.Angle(Vector3.up, slopeRaycast.normal);
            return angle < maxAngle && angle != 0;
        }

        else 
        { 
        return false;
        }
    }

    public Vector3 slopeMovementVector(Vector3 direction)
    {
        return Vector3.ProjectOnPlane(direction, slopeRaycast.normal).normalized;
    }

    private void startSlide()
    {
        isSliding = true;

        //scales player down and adds force down so player doesnt float
        transform.localScale = new Vector3(transform.localScale.x, crouchHeightScale, transform.localScale.z);
        playerRigidBody.AddForce(Vector3.down * 5f, ForceMode.Impulse);

        slideTime = maxSlideTime;
    }

    private void Sliding()
    {
        // Check if the player is trying to jump while sliding
        if (Input.GetButtonDown("Jump"))
        {
            // End the slide and initiate a jump
            endSlide();
            Jump();
            return;
        }

        //not on slope or moving up
        if (!onSlope() || playerRigidBody.velocity.y > -0.1f)
        {
            playerRigidBody.AddForce(movementDirection.normalized * slideForce, ForceMode.Force);

            slideTime -= Time.deltaTime;
        }

        else
        {
            playerRigidBody.AddForce(slopeMovementVector(movementDirection) * slideForce, ForceMode.Force);
        }
        

        if (slideTime <= 0)
        {
            endSlide();
        }
    }

    private void endSlide()
    {
        isSliding = false;

        transform.localScale = new Vector3(transform.localScale.x, normalPlayerHeight, transform.localScale.z);

    }
}//end UVplayerMovement.cs
