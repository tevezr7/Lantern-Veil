using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Interactable : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public bool useEvents;
    [SerializeField]
    public string prompt;
    
    protected Transform player;

    protected virtual void Awake()
    {
        player = GameObject.FindWithTag("Player")?.transform;
    }

    public void BaseInteract()
    {
        if (useEvents)
        {
            GetComponent<InteractionEvent>().onInteract.Invoke(); // Invoke the event if useEvents is true
        }
        Interact();
    }
    protected virtual void Interact()
    {
        //This method is meant to be overwritten
        Debug.Log("Interacting with " + transform.name);
    }


}
