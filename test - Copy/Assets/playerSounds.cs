using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class playerSounds : MonoBehaviour
{
    [Header("Player Input")]
    float horizontalInput;
    float verticalInput;

    [Header("Footsteps")]
    public AudioSource walkingSound;

    [Header("Collision Detection")]
    public float distanceToWall;
    private RaycastHit leftWallRaycast;
    private RaycastHit rightWallRaycast;
    private bool isWallLeft;
    private bool isWallRight;

    [Header("Player Status")]
    public bool grounded;
    public LayerMask isGround;
    public LayerMask isWall;
    public float playerHeight;
    private float isWallrunning;

    [Header("References")]
    public Transform orientation;
    private UVplayerMovement playerMovement;
    private WallMovement wallMovement;

    //sideways raycasts to check for walls
    private void wallAvailable()
    {
        isWallLeft = Physics.Raycast(transform.position, -orientation.right, out leftWallRaycast, distanceToWall, isWall);
        isWallRight = Physics.Raycast(transform.position, orientation.right, out rightWallRaycast, distanceToWall, isWall);
    }

    void Start()
    {
        playerMovement = GetComponent<UVplayerMovement>();
    }

    void Update()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

        //check if player is on the ground
        grounded = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + 0.2f, isGround);

        wallAvailable();

        //if the player is on the ground and pressing a movement key footsteps will play
        if (grounded && ((Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.D))))
        {
            walkingSound.enabled = true;

            //pitch and volume up for sprinting
            if(Input.GetKey(KeyCode.LeftShift))
            {
                walkingSound.volume = 0.65f;
                walkingSound.pitch = 1.0f;               
            }

            //crouch walking
            if (Input.GetKey(KeyCode.LeftControl))
            {
                walkingSound.volume = 0.4f;
                walkingSound.pitch = 0.65f;
            }

            //back to normal
            else
            {               
                walkingSound.volume = 0.55f;
                walkingSound.pitch = 0.83f;
            }
        }
        else if(!grounded && (isWallLeft || isWallRight))
        {
            walkingSound.enabled = true;

            walkingSound.volume = 0.61f;
            walkingSound.pitch = 0.9f;
        }
        //no input
        else
        {
            walkingSound.enabled = false;   
        }

    }
}
