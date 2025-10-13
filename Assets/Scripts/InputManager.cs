using UnityEngine;
using System.Collections;
using UnityEngine.InputSystem;
using System.Collections.Generic;


public class InputManager : MonoBehaviour
{
    private PlayerInput playerInput;
    public PlayerInput.OnFootActions onFoot;

    private PlayerMotor motor;
    private PlayerLook look;

    void Awake()
    {
        playerInput = new PlayerInput();
        onFoot = playerInput.OnFoot;

        look = GetComponent<PlayerLook>();
        motor = GetComponent<PlayerMotor>();

        onFoot.Jump.performed += ctx => motor.Jump(); //callback context to call the motor jump function performed, cancelled ,started all exist
    
        onFoot.Crouch.performed += ctx => motor.Crouch();
        onFoot.Sprint.performed += ctx => motor.Sprint();
        onFoot.Dodge.performed += ctx => motor.Dodge(onFoot.Movement.ReadValue<Vector2>());

    }

    void FixedUpdate()
    {
            motor.ProcessMove(onFoot.Movement.ReadValue<Vector2>());
    }

    void LateUpdate()
    {
            look.ProcessLook(onFoot.Look.ReadValue<Vector2>());
    }

    private void OnEnable()
    {
        onFoot.Enable();
    }

    private void OnDisable()
    {
        onFoot.Disable();
    }
}
