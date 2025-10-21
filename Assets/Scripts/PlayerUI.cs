using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PlayerUI : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI promptText;
    [SerializeField]
    private TextMeshProUGUI potionCounter;

    [SerializeField]
    private PotionInventory potionInventory;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        UpdatePotionCount();
    }
    
    void Update()
    {
         UpdatePotionCount();
    }

    // Update is called once per frame
    public void UpdateText(string promptMessage)
    {
        promptText.text = promptMessage;
    }

    public void UpdatePotionCount()
    {
        if (potionCounter != null && potionInventory != null)
        {
            potionCounter.text = "Potions: " + potionInventory.potion_counter.ToString();
        }

        return;
    }
}
