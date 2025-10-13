using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Interactable : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public string prompt;
    
    public void BaseInteract()
    {
        Interact();
    }
    protected virtual void Interact()
    {
        //This method is meant to be overwritten
        Debug.Log("Interacting with " + transform.name);
    }


}
