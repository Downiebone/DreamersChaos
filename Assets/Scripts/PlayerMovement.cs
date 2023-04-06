using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    float playerHeight = 2f;

    [SerializeField] Transform orientation;
    [SerializeField] private WallRun wallrun;

    [Header("Movement")]
    [SerializeField] private float maxVelocity = 30f;
    [SerializeField] float moveSpeed = 6f;
    [SerializeField] float airMultiplier = 0.4f;
    float movementMultiplier = 10f;

    [Header("Sprinting")]
    [SerializeField] float walkSpeed = 4f;
    [SerializeField] float sprintSpeed = 6f;
    [SerializeField] float acceleration = 10f;

    [Header("Jumping")]
    public float jumpForce = 5f;
    [SerializeField] private int doubleJumps = 1;
    private int doubleJumps_used = 0;

    [Header("Keybinds")]
    [SerializeField] KeyCode jumpKey = KeyCode.Space;
    [SerializeField] KeyCode sprintKey = KeyCode.LeftShift;

    [Header("Drag")]
    [SerializeField] float groundDrag = 6f;
    [SerializeField] float airDrag = 2f;

    float horizontalMovement;
    float verticalMovement;

    [Header("Ground Detection")]
    [SerializeField] Transform groundCheck;
    [SerializeField] LayerMask groundMask;
    [SerializeField] float groundDistance = 0.2f;
    public bool isGrounded { get; private set; }

    Vector3 moveDirection;
    Vector3 slopeMoveDirection;

    Rigidbody rb;

    RaycastHit slopeHit;



    [SerializeField] GameObject stepRayUpper;
    [SerializeField] GameObject stepRayLower;
    [SerializeField] float stepHeight = 0.3f;
    [SerializeField] float stepSmooth = 2f;
    [SerializeField] private float stepRayLength = 0.2f;


    private bool isInitedMovement = false;

    public void initPlayerMovement()
    {
        isInitedMovement = true;
        wallrun.InitWallRun();
        rb.useGravity = true;
    }
    public void initPlayerGame()
    {
        //allow player to use spells

        transform.position = GameObject.FindGameObjectWithTag("SpawnPoints").transform.GetChild(Random.Range(0, GameObject.FindGameObjectWithTag("SpawnPoints").transform.childCount)).transform.position;

        GameObject.FindGameObjectWithTag("SpawnRoom").transform.parent.gameObject.SetActive(false);
        hotBarScript.Instance.initializeUseOfHotbar();
    }

    private bool OnSlope()
    {
        if (Physics.Raycast(transform.position, Vector3.down, out slopeHit, playerHeight / 2 + 0.7f))
        {
            if (slopeHit.normal != Vector3.up)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        return false;
    }

    private void Start()
    {
        
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        stepRayUpper.transform.localPosition = new Vector3(0, stepHeight, 0);
    }

    private void Update()
    {
        if (isInitedMovement == false)
            return;

        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);

        MyInput();
        ControlDrag();
        ControlSpeed();

        if (Input.GetKeyDown(jumpKey))
        {
            if (isGrounded)
            {
                Jump();
            }
            else if(doubleJumps_used < doubleJumps && !wallrun.wallRunning)
            {
                doubleJumps_used++;
                Jump();
            }
            
        }
        if (isGrounded)
        {
            doubleJumps_used = 0;
        }

        



        slopeMoveDirection = Vector3.ProjectOnPlane(moveDirection, slopeHit.normal);
    }

    void MyInput()
    {   
        horizontalMovement = Input.GetAxisRaw("Horizontal");
        verticalMovement = Input.GetAxisRaw("Vertical");

        moveDirection = orientation.forward * verticalMovement + orientation.right * horizontalMovement;
    }

    void Jump()
    {

        rb.velocity = new Vector3(rb.velocity.x, rb.velocity.y/3, rb.velocity.z);

        rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
     
    }

    void ControlSpeed()
    {
        if (Input.GetKey(sprintKey) && isGrounded)
        {
            moveSpeed = Mathf.Lerp(moveSpeed, sprintSpeed, acceleration * Time.deltaTime);
        }
        else
        {
            moveSpeed = Mathf.Lerp(moveSpeed, walkSpeed, acceleration * Time.deltaTime);
        }
    }

    void ControlDrag()
    {
        if (isGrounded)
        {
            rb.drag = groundDrag;
        }
        else
        {
            rb.drag = airDrag;
        }


        rb.velocity = new Vector3(Mathf.Clamp(rb.velocity.x, -maxVelocity, maxVelocity), Mathf.Clamp(rb.velocity.y, -maxVelocity * 1.5f, maxVelocity), Mathf.Clamp(rb.velocity.z, -maxVelocity, maxVelocity));


    }

    private void FixedUpdate()
    {
        MovePlayer();
        stepMove();


        //extra fall speed
        if (rb.velocity.y >= -Mathf.Epsilon)
            return;

        rb.AddForce(Vector3.down*4f, ForceMode.Acceleration); //TODO temp velocity down
    }

    void stepMove()
    {
        if (!isGrounded)
            return;

        Debug.DrawRay(stepRayLower.transform.position, moveDirection * stepRayLength, Color.red, 0.1f);
        Debug.DrawRay(stepRayUpper.transform.position, moveDirection * (stepRayLength + 0.1f), Color.red, 0.1f);

        RaycastHit hitLower;
        if (Physics.Raycast(stepRayLower.transform.position, moveDirection.normalized, out hitLower, stepRayLength, groundMask))
        {
            RaycastHit hitUpper;
            if (!Physics.Raycast(stepRayUpper.transform.position, moveDirection.normalized, out hitUpper, stepRayLength + 0.1f, groundMask))
            {
                if (Physics.Raycast(stepRayUpper.transform.position + (moveDirection.normalized * (stepRayLength + 0.1f)), Vector3.down, out hitUpper, 1 + stepHeight, groundMask))
                {
                    if (hitUpper.normal != Vector3.up)
                        return;
                    Debug.DrawRay(stepRayUpper.transform.position + (moveDirection.normalized * (stepRayLength + 0.1f)), Vector3.down * (1 + stepHeight), Color.red, 0.1f);

                    transform.position = new Vector3(
                        transform.position.x,
                        hitUpper.point.y + 1.01f,
                        transform.position.z
                        );
                    //rb.position -= new Vector3(0f, -stepSmooth * Time.deltaTime, 0f);
                }
            }
        }
    }

    void MovePlayer()
    {

        if (isGrounded && !OnSlope())
        {
            rb.AddForce(moveDirection.normalized * moveSpeed * movementMultiplier, ForceMode.Acceleration);
        }
        else if (isGrounded && OnSlope())
        {
            rb.AddForce(slopeMoveDirection.normalized * moveSpeed * movementMultiplier, ForceMode.Acceleration);
        }
        else if (!isGrounded)
        {
            rb.AddForce(moveDirection.normalized * moveSpeed * movementMultiplier * airMultiplier, ForceMode.Acceleration);
        }
    }
}