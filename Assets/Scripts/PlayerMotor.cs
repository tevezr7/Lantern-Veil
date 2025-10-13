using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class PlayerMotor : MonoBehaviour
{
    private CharacterController controller;
    private Vector3 playerVelocity;
    private bool isGrounded;

    [Header("Movement")]
    public float speed = 5f; //f means interpret as float
    public float sprintSpeed = 8f;
    public float gravity = -9.8f;
    public float jumpHeight = 3f;


    [Header("Crouch")]
    public float standHeight = 2f;
    public float crouchHeight = 1.2f;
    public float crouchTimer = 0f;

    [Header("Dodge")]
    public float dodgeDistance = 0.1f;
    public float dodgeDuration = 0.1f;
    public float dodgeCooldown = 0.4f;
    
    private Vector3 dodgeDir;
    private float dodgeEndTime;
    private float lastDodgeTime = -999f;


    //states
    private bool crouching = false;
    private bool lerpCrouch = false;
    private bool sprinting = false;
    private bool dodging = false;

    //start 
    private float baseSpeed;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        baseSpeed = speed;
    }

    void Update()
    {
        isGrounded = controller.isGrounded;
        if (lerpCrouch)
        {
            crouchTimer += Time.deltaTime; //time between current and previous frame
            float p = crouchTimer / 1;
            p *= p;
            if (crouching)
                controller.height = Mathf.Lerp(controller.height, crouchHeight, p);
            else
                controller.height = Mathf.Lerp(controller.height, standHeight, p);

            if (p > 1)
            { 
                lerpCrouch = false;
                crouchTimer = 0f;
            }
        }
    }
    public void ProcessMove(Vector2 input)
    {
        if (dodging)
        {
            // move at 3x speed during the dash window
            controller.Move(dodgeDir * 5 * dodgeDistance * Time.deltaTime);

            // end dash when the timer expires
            if (Time.time >= dodgeEndTime)
                dodging = false;
        }
        else
        {
            Vector3 moveDirection = Vector3.zero;
            moveDirection.x = input.x;
            moveDirection.z = input.y;
            controller.Move(transform.TransformDirection(moveDirection) * speed * Time.deltaTime);
            playerVelocity.y += gravity * Time.deltaTime;
            if (isGrounded && playerVelocity.y < 0)
                playerVelocity.y = -2f;
            controller.Move(playerVelocity * Time.deltaTime);
            Debug.Log(playerVelocity.y);
        }
    }
    public void Jump()
    {
        if (isGrounded)
        {
            playerVelocity.y = Mathf.Sqrt(jumpHeight * -3.0f * gravity);
                
        }
    }

    public void Crouch()
    {
        crouching = !crouching; 
        crouchTimer = 0;
        lerpCrouch = true;
    }

    public void Sprint()
    {
        sprinting = !sprinting; //just a toggle for now
        if (sprinting)
        {
            speed = baseSpeed+4;
        }
        else
            speed = baseSpeed;
    }

    public void Dodge(Vector2 input)
    {
        if (dodging) return; // already dashing
        if (Time.time < lastDodgeTime + dodgeCooldown) return; // cooldown

        // Direction: backwards if no input, otherwise input direction
        if (input.sqrMagnitude < 0.01f)
            dodgeDir = -transform.forward;
        else
            dodgeDir = transform.TransformDirection(new Vector3(input.x, 0f, input.y).normalized);

        dodgeDir.y = 0f; // keep it horizontal

        dodging = true;
        dodgeEndTime = Time.time + dodgeDuration; // ~0.1s window
        lastDodgeTime = Time.time;                // start cooldown
    }

}

