using UnityEngine;

public class PotionInventory : MonoBehaviour
{
    public int potion_counter;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void AddPotion()
    {
        potion_counter++;

        var ui = GetComponentInChildren<PlayerUI>();
        if (ui != null)
            ui.UpdatePotionCount();
    }

    public void UsePotion()
    {
        if (potion_counter > 0)
        {
            potion_counter--;
            var ui = GetComponentInChildren<PlayerUI>();
            if (ui != null)
                ui.UpdatePotionCount();
        }
    }
}
