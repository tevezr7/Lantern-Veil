using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMotor : MonoBehaviour
{
    private CharacterController controller;
    private PlayerCombat combat;
    private Vector3 playerVelocity;
    private bool isGrounded;

    [Header("Movement")]
    public float speed = 5f;
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

    [Header("Block & Attack")]
    public float blockSpeedMultiplier = 0.5f; // speed is reduced when blocking
    public float attackSpeedMultiplier = 0.8f; // speed is reduced when attacking

    private Vector3 dodgeDir;
    private float dodgeEndTime;
    private float lastDodgeTime = -999f;

    // states
    private bool crouching = false;
    private bool lerpCrouch = false;
    private bool sprinting = false;
    private bool dodging = false;
    

    // start 
    private float baseSpeed;

    // --- Exposed read-only helpers (optional) ---
    public bool IsGrounded => isGrounded;
    public bool IsSprinting => sprinting;
    public float HorizontalSpeed
    {
        get
        {
            if (controller == null) return 0f;
            Vector3 v = controller.velocity; v.y = 0f;
            return v.magnitude;
        }
    }

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        controller = GetComponent<CharacterController>();
        combat = GetComponent<PlayerCombat>();
        baseSpeed = speed;
    }

    void Update()
    {
        isGrounded = controller.isGrounded;

        if (lerpCrouch)
        {
            crouchTimer += Time.deltaTime;
            float p = crouchTimer / 1f;
            p *= p;

            if (crouching)
                controller.height = Mathf.Lerp(controller.height, crouchHeight, p);
            else
                controller.height = Mathf.Lerp(controller.height, standHeight, p);

            if (p > 1f)
            {
                lerpCrouch = false;
                crouchTimer = 0f;
            }
        }
        if (combat != null)
        {
            // Adjust speed based on combat state
            if (combat.IsBlocking)
                speed = baseSpeed * blockSpeedMultiplier;
            else if (combat.IsAttacking)
                speed = baseSpeed * attackSpeedMultiplier;
            else
                speed = baseSpeed;
        }
    }

    public void ProcessMove(Vector2 input)
    {
        if (dodging)
        {
            // move quickly during the dash window
            controller.Move(dodgeDir * 5f * dodgeDistance * Time.deltaTime);

            if (Time.time >= dodgeEndTime)
                dodging = false;
            return;
        }

        Vector3 moveDirection = new Vector3(input.x, 0f, input.y);
        float currentSpeed = sprinting ? sprintSpeed : speed;

        controller.Move(transform.TransformDirection(moveDirection) * currentSpeed * Time.deltaTime);

        // Gravity
        playerVelocity.y += gravity * Time.deltaTime;
        if (isGrounded && playerVelocity.y < 0f)
            playerVelocity.y = -2f;

        controller.Move(playerVelocity * Time.deltaTime);
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
        crouchTimer = 0f;
        lerpCrouch = true;
    }

    public void Sprint()
    {
        // Toggle sprint on press
        sprinting = !sprinting;
        // (Speed is applied in ProcessMove via currentSpeed)
    }

    // ✅ Called when sprint input is released
    public void StopSprinting()
    {
        sprinting = false;
        // (Speed falls back to 'speed' in ProcessMove)
    }

    public void Dodge(Vector2 input)
    {
        if (dodging) return;
        if (Time.time < lastDodgeTime + dodgeCooldown) return;

        // Direction: backwards if no input, otherwise input direction
        if (input.sqrMagnitude < 0.01f) 
            dodgeDir = -transform.forward; 
        else
            dodgeDir = transform.TransformDirection(new Vector3(input.x, 0f, input.y).normalized); // world space

        dodgeDir.y = 0f;

        dodging = true; 
        dodgeEndTime = Time.time + dodgeDuration; // set end time
        lastDodgeTime = Time.time; // start cooldown
    }

}
