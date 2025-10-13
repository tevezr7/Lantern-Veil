using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerLook : MonoBehaviour
{
    public Camera cam;
    private float xRotation = 0f;

    public float xSensitivity = 30f;
    public float ySensitivity = 30f;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    
    public void ProcessLook(Vector2 input)
    {
        float mouseX = input.x;
        float mouseY = input.y;
        //calculate cam rotation for up and down
        xRotation -= mouseY * ySensitivity * Time.deltaTime;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);
        //apply rotation to cam
        cam.transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f); //rotate cam up and down 
        //rotate player left and right
        transform.Rotate(Vector3.up * mouseX * xSensitivity * Time.deltaTime);
    }
}
