using UnityEngine;
using UnityEngine.ProBuilder.Shapes;

public class Potion : Interactable
{

    
    protected override void Interact()
    {
        var inventory = player.GetComponent<PotionInventory>();
        if (inventory == null) return;

        inventory.AddPotion();
        Destroy(gameObject);
    }
}
