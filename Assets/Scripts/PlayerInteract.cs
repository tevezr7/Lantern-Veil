using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInteract : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    private Camera cam;
    [SerializeField]
    private float distance = 3f; // Interaction distance
    [SerializeField]
    private LayerMask mask; // Layer for interactable objects
    private PlayerUI playerUI;
    private InputManager inputManager;
    void Start()
    {
        cam = GetComponent<PlayerLook>().cam;
        playerUI = GetComponent<PlayerUI>();
        inputManager = GetComponent<InputManager>();
    }

    // Update is called once per frame
    void Update()
    {
        playerUI.UpdateText(string.Empty); // Clear the prompt text each frame
        Ray ray = new Ray(cam.transform.position, cam.transform.forward); // Create a ray from the camera's position forward
        Debug.DrawRay(ray.origin, ray.direction * distance); // Visualize the ray in the editor
        RaycastHit hitInfo; // Variable to store hit information
        if (Physics.Raycast(ray, out hitInfo, distance, mask)) // Perform the raycast
        {
            if(hitInfo.collider.GetComponent<Interactable>() != null) // Check if the hit object has an Interactable component
            {
               Interactable interactable = hitInfo.collider.GetComponent<Interactable>();
               playerUI.UpdateText(interactable.prompt);
               if(inputManager.onFoot.Interact.triggered)
               {
                    interactable.BaseInteract();
               }
            }
        }
    }
}
