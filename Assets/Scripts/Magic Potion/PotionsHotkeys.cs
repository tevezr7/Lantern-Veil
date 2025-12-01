using UnityEngine;
using UnityEngine.InputSystem;   // <<-- NEW

public class PotionHotkeys : MonoBehaviour
{
    [Header("Inventories")]
    [SerializeField] private PotionInventory healthPotions;       // optional
    [SerializeField] private MagicPotionInventory magicPotions;   // magic

    [Header("Keys (New Input System)")]
    [SerializeField] private Key healthPotionKey = Key.Digit1;    // "1" key
    [SerializeField] private Key magicPotionKey = Key.Digit2;    // "2" key

    void Update()
    {
        var kb = Keyboard.current;
        if (kb == null) return; // no keyboard (e.g., gamepad only)

        // Health potion (optional)
        if (healthPotions != null && kb[healthPotionKey].wasPressedThisFrame)
        {
            healthPotions.TryUsePotion();
        }

        // Magic potion
        if (magicPotions != null && kb[magicPotionKey].wasPressedThisFrame)
        {
            magicPotions.TryUseMagicPotion();
        }
    }
}
