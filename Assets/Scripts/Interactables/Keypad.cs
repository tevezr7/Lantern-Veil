using System.Collections;
using System.Collections.Generic;
using System.Xml.Linq;
using UnityEngine;

public class Keypad : Interactable
{
    [SerializeField]
    private GameObject door;
    private bool doorOpen = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    protected override void Interact()
    {
        doorOpen = !doorOpen;
        door.GetComponent<Animator>().SetBool("IsOpen", doorOpen);
    }
}
