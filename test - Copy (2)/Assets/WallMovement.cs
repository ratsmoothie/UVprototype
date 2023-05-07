using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Dave / GameDevelopment's tutorials on youtube were referenced
public class WallMovement : MonoBehaviour
{
    [Header("Wallrunning")]
    public LayerMask isWall;
    public LayerMask isGround;

    public float wallrunningForce;
    public float maxWallrunningTime;
    private float wallrunningTimer;

    [Header("Wall Jumping")]

    public float wallJumpUpwardsForce;
    public float wallJumpSidewaysForce;

    [Header("Wallrunning Exit")]
    private bool exitingWall;
    private float exitWallTimer;
    public float exitWallTime;

    [Header("Gravity while WR")]
    public bool isThereGravity;
    public float antiGravForce;

    [Header("Player Input")]
    private float horizontalInput;
    private float verticalInput;
    public KeyCode jumpButton = KeyCode.Space;
    //get mouse x input here for wallrunning up and down

    [Header("Collision Detection")]
    public float distanceToWall;
    public float minWallrunningHeight;
    private RaycastHit leftWallRaycast;
    private RaycastHit rightWallRaycast;
    private bool isWallLeft;
    private bool isWallRight;

    [Header("References")]
    public Transform orientation;
    public new PlayerCam camera;
    private UVplayerMovement playerMovement;
    private Rigidbody playerRigidBody;

    private void Start()
    {
        playerMovement = GetComponent<UVplayerMovement>();
        playerRigidBody = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        wallAvailable();
        playerStateHandler();
    }

    private void FixedUpdate()
    {
        if (playerMovement.isWallrunning)
        {
            wallrunning();
        }
    }

    //sideways raycasts to check for walls
    private void wallAvailable()
    {
        isWallLeft = Physics.Raycast(transform.position, -orientation.right, out leftWallRaycast, distanceToWall, isWall);
        isWallRight = Physics.Raycast(transform.position, orientation.right, out rightWallRaycast, distanceToWall, isWall);
    }

    //returns true if downward raycast hits nothing
    private bool isPlayerOffTheGround()
    {
        return !Physics.Raycast(transform.position, Vector3.down, minWallrunningHeight, isGround);
    }

    private void playerStateHandler()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

        //wallrunning state
        //if there is a wall close enough, the player is hitting w or s, and the player is not on the ground, begin wallrunning.
        if ((isWallLeft || isWallRight) && (verticalInput > 0) && isPlayerOffTheGround() && !exitingWall)
        {
            if (!playerMovement.isWallrunning)
            {
                beginWallrunning();
            }

            if(exitWallTimer > 0)
            {
                exitWallTimer -= Time.deltaTime;
            }

            if(exitWallTimer <= 0 && playerMovement.isWallrunning)
            {
                exitingWall = true;
                exitWallTime = exitWallTimer;
            }

            //do wall jump if space is pressed
            if (Input.GetKeyDown(jumpButton))
            {
                doWallJump();
            }
        }

        //ending wallrun state
        else if(exitingWall)
        {
            if (playerMovement.isWallrunning)
            {
                stopWallrunning();
            }

            //counts timer down
            while (exitWallTimer > 0)
            {
                exitWallTimer -= Time.deltaTime;
            }

            if(exitWallTimer <= 0)
            {
                exitingWall = false;
            }
        }

        //back to normal
        else
        {
            if (playerMovement.isWallrunning)
            {
                stopWallrunning();
            }
        }
    }

    private void beginWallrunning()
    {
        playerMovement.isWallrunning = true;

        exitWallTimer = exitWallTime;

        playerRigidBody.velocity = new Vector3(playerRigidBody.velocity.x, 0f, playerRigidBody.velocity.z);

        //camera effects
        camera.changeFOV(110f);
        if (isWallLeft) camera.cameraTilt(-5f);
        if (isWallRight) camera.cameraTilt(5f);
    }

    private void stopWallrunning()
    {
        playerMovement.isWallrunning = false;
        playerRigidBody.useGravity = true;

        //end camera effects
        camera.changeFOV(90f);
        camera.cameraTilt(0f);
    }

    private void wallrunning()
    {
        //turn gravity off while wallrunning so we dont fall off
        playerRigidBody.useGravity = isThereGravity;
  
        //get mouse input for movement up and down the wall

        //use the normal of the wall to calculate which direction is forward
        Vector3 getWallNormal = isWallLeft ? leftWallRaycast.normal : rightWallRaycast.normal;

        Vector3 getWallForward = Vector3.Cross(getWallNormal, transform.up);

        //flips which way is forward relative to the player's orientation
        if ((orientation.forward - getWallForward).magnitude > (orientation.forward - -getWallForward).magnitude)
        {
            getWallForward = -getWallForward;
        }

        //add force
        //add force in the direction the player is moving
        playerRigidBody.AddForce(getWallForward * wallrunningForce, ForceMode.Force);

        /*
        //add force pushing the player into the wall
        if (!(isWallLeft && horizontalInput > 0) && !(isWallRight && horizontalInput < 0))
        {
            playerRigidBody.AddForce(-getWallNormal * 60, ForceMode.Force);
        }*/

        //leaves gravity on but pushes against it a bit
        if(isThereGravity)
        {
            playerRigidBody.AddForce(transform.up * antiGravForce, ForceMode.Force);
        }
    }

    private void doWallJump()
    {
        exitingWall = true;
        exitWallTimer = maxWallrunningTime;
        //use the normal of the wall to calculate which direction is forward
        Vector3 getWallNormal = isWallLeft ? leftWallRaycast.normal : rightWallRaycast.normal;

        Vector3 wallJumpingForce = transform.up * wallJumpUpwardsForce + getWallNormal * wallJumpSidewaysForce;

        //reset y velocity before adding the force to reduce wonk
        playerRigidBody.velocity = new Vector3(playerRigidBody.velocity.x, 0f, playerRigidBody.velocity.z);
        playerRigidBody.AddForce(wallJumpingForce, ForceMode.Impulse);
    }
}