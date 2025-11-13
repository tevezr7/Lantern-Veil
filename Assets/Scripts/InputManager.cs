using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    private PlayerInput playerInput;
    public PlayerInput.OnFootActions onFoot;

    private PlayerMotor motor;
    private PlayerLook look;
    private PlayerCombat combat;
    private PlayerHealth health;
    private FlameThrower flameThrower;
    public Animator anim;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip footstepWalkClip;
    [SerializeField] private AudioClip footstepSprintClip;
    [SerializeField] private AudioClip jumpClip;
    [SerializeField] private AudioClip landClip;
    [SerializeField] private AudioClip dashClip;


    [Header("Footstep Settings (Distance-Based)")]
    [Tooltip("Meters per step while walking.")]
    [SerializeField] private float strideLengthWalk = 0.9f;
    [Tooltip("Meters per step while sprinting.")]
    [SerializeField] private float strideLengthSprint = 1.2f;
    [Tooltip("Minimum input magnitude to consider moving (prevents tiny stick noise).")]
    [SerializeField] private float inputDeadzone = 0.05f;

    [Header("Fallback Timing (optional)")]
    [SerializeField] private float walkStepInterval = 0.5f;
    [SerializeField] private float sprintStepInterval = 0.35f;

    private bool isSprinting = false;
    private bool wasGrounded = true;

    private Vector2 movementInput;
    private Vector2 lookInput;

    // Distance tracking
    private Vector3 lastPlanarPos;
    private float distanceSinceStep = 0f;
    private float footstepTimer = 0f; // used only as a fallback

    // Optional debug
    [SerializeField] private bool debugFootsteps = false;

    void Awake()
    {
        playerInput = new PlayerInput();
        onFoot = playerInput.OnFoot;

        motor = GetComponent<PlayerMotor>();
        look = GetComponent<PlayerLook>();
        combat = GetComponent<PlayerCombat>();
        health = GetComponent<PlayerHealth>(); 
        flameThrower = GetComponentInChildren<FlameThrower>();

        if (motor == null) Debug.LogError("[InputManager] PlayerMotor not found.");
        if (look == null) Debug.LogError("[InputManager] PlayerLook not found.");

        // --- Input callbacks ---
        onFoot.Jump.performed += ctx =>
        {
            motor?.Jump();
            PlayOneShot(jumpClip);
        };

        onFoot.Crouch.performed += ctx => motor?.Crouch();

        onFoot.Sprint.performed += ctx =>
        {
            isSprinting = true;
            motor?.Sprint(); 
        };
        onFoot.Sprint.canceled += ctx =>
        {
            isSprinting = false;
            motor?.StopSprinting(); 
        };

        onFoot.Dodge.performed += ctx =>
        {
            var dir = onFoot.Movement.ReadValue<Vector2>(); // get current movement input
            motor?.Dodge(dir); // pass current movement input
            combat?.DodgeLogic();
            PlayOneShot(dashClip);
        };

        onFoot.Attack.performed += ctx =>
        {
            anim.SetTrigger("Attack");
        };

        onFoot.PowerAttack.performed += ctx =>
        {
            anim.SetTrigger("PowerAttack");
        };

        onFoot.Block.performed += ctx =>
        {
            combat?.Block(true);  // start blocking when button pressed
            anim.SetBool("isBlocking", true);
        };

        onFoot.Block.canceled += ctx =>
        {
            combat?.Block(false);  // stop blocking when button released
            anim.SetBool("isBlocking", false);
        };

        onFoot.Heal.performed += ctx =>
        {
            health?.DrinkPotion();
        };

        onFoot.Spell.performed += ctx =>
        {
            flameThrower?.StartFlame();  // on press | update with if statement later for checking which spell is selected
            anim.SetBool("isCasting", true);
        };
        onFoot.Spell.canceled += ctx =>
        {
            flameThrower?.StopFlame();   // on release
            anim.SetBool("isCasting", false);
        };
    }

    void OnEnable() => onFoot.Enable();
    void OnDisable() => onFoot.Disable();

    void Start()
    {
        lastPlanarPos = GetPlanarPosition(transform.position);
    }

    void Update()
    {
        movementInput = onFoot.Movement.ReadValue<Vector2>();
        lookInput = onFoot.Look.ReadValue<Vector2>();
        HandleLandingSound();
    }

    void FixedUpdate()
    {
        motor?.ProcessMove(movementInput);
    }

    void LateUpdate()
    {
        look?.ProcessLook(lookInput);
        HandleFootstepsDistanceBased();
    }

    // ------------------ AUDIO ------------------

    private void HandleLandingSound()
    {
        if (motor == null) return;

        bool groundedNow = motor.IsGrounded;
        if (!wasGrounded && groundedNow)
        {
            PlayOneShot(landClip);
            // Small grace so a step won't immediately fire on landing
            footstepTimer = 0.1f;
            distanceSinceStep = 0f;
            lastPlanarPos = GetPlanarPosition(transform.position);
        }
        wasGrounded = groundedNow;
    }

    private void HandleFootstepsDistanceBased()
    {
        if (motor == null) return;

        // Must be grounded and actually providing movement input
        if (!motor.IsGrounded) { ResetStrideOrigin(); return; }
        if (movementInput.magnitude < inputDeadzone) { ResetStrideOrigin(); return; }

        // Accumulate horizontal distance moved since last step
        Vector3 planarPos = GetPlanarPosition(transform.position);
        float frameDistance = Vector3.Distance(planarPos, lastPlanarPos);
        lastPlanarPos = planarPos;

        // If character didn't actually move (e.g., blocked), don't accumulate
        if (frameDistance <= Mathf.Epsilon) return;

        // Optional tiny grace to avoid steps right after landing
        if (footstepTimer > 0f) { footstepTimer -= Time.deltaTime; return; }

        distanceSinceStep += frameDistance;

        float stride = isSprinting ? strideLengthSprint : strideLengthWalk;
        if (distanceSinceStep >= stride)
        {
            var clip = isSprinting ? footstepSprintClip : footstepWalkClip;
            PlayOneShot(clip);
            distanceSinceStep = 0f;
        }

        if (debugFootsteps)
        {
            Debug.Log($"[Steps] grounded={motor.IsGrounded} input={movementInput.magnitude:F2} dist={distanceSinceStep:F2}/{stride:F2}");
        }
    }

    private void ResetStrideOrigin()
    {
        lastPlanarPos = GetPlanarPosition(transform.position);
        distanceSinceStep = 0f;
    }

    private static Vector3 GetPlanarPosition(Vector3 pos)
    {
        pos.y = 0f;
        return pos;
    }

    private void PlayOneShot(AudioClip clip)
    {
        if (audioSource != null && clip != null)
            audioSource.PlayOneShot(clip);
    }
}
