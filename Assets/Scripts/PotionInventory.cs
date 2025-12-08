using UnityEngine;
using System.Collections;   // for IEnumerator

public class PotionInventory : MonoBehaviour
{
    [Header("Data")]
    public int potion_counter;

    public event System.Action<int> OnPotionChanged;

    [Header("Healing")]
    [SerializeField] private PlayerHealth playerHealth;   // assign in Inspector
    [SerializeField] private float healAmount = 25f;      // tweak to your liking

    [Header("UI Links")]
    [SerializeField] private PlayerUI playerUI;           // HUD potion text (optional)

    [Header("Audio (world drink sound, optional)")]
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioClip drinkSfx;

    void Awake()
    {
        if (!playerUI)
            playerUI = GetComponentInChildren<PlayerUI>(true);

        if (!playerHealth)
        {
#if UNITY_2023_1_OR_NEWER
            playerHealth = Object.FindAnyObjectByType<PlayerHealth>();
#else
            playerHealth = FindObjectOfType<PlayerHealth>();
#endif
        }
    }

    void Start()
    {
        // First immediate refresh (for when everything is already set up)
        RefreshUI();

        // Then do a delayed refresh so that if the InventoryUIController
        // was created AFTER us (like when coming from the main menu),
        // it still gets the correct potion count.
        StartCoroutine(DelayedRefreshUI());
    }

    private IEnumerator DelayedRefreshUI()
    {
        // Wait one frame so all other Awake/Start calls (like InventoryUIController)
        // have a chance to run and set their static Instance.
        yield return null;

        RefreshUI();
    }

    // Called when you pick up a potion in the world
    public void AddPotion()
    {
        potion_counter++;
        RefreshUI();
    }

    /// <summary>
    /// The one true way to use a potion (keyboard, inventory click, etc.).
    /// Plays success/fail SFX and only heals if we have potions AND are below max HP.
    /// </summary>
    public void TryUsePotion()
    {
        if (playerHealth == null) return;

        // No potions left?
        if (potion_counter <= 0)
        {
            // UI "fail" sound (no potion / can't use)
            if (UIAudio.I != null)
                UIAudio.I.PlayPotionFail();
            return;
        }

        // Already full HP?
        if (playerHealth.health >= playerHealth.maxHealth)
        {
            // UI "fail" sound (already at full health)
            if (UIAudio.I != null)
                UIAudio.I.PlayPotionFail();
            return;
        }

        // ----- Successful use -----
        potion_counter--;

        // Heal player
        playerHealth.RestoreHealth(healAmount);

        // UI "potion used" sound
        if (UIAudio.I != null)
            UIAudio.I.PlayPotionUse();

        // Optional world-space drink sound (on the player, etc.)
        if (sfxSource && drinkSfx)
            sfxSource.PlayOneShot(drinkSfx);

        // Track stats
        if (GameSessionStats.Instance != null)
            GameSessionStats.Instance.OnHealthPotionUsed();

        // Update HUD + inventory grid
        RefreshUI();
    }

    // If you still need it elsewhere, have UsePotion just call TryUsePotion
    public void UsePotion()
    {
        TryUsePotion();
    }

    private void RefreshUI()
    {
        // HUD text (top-left, etc.)
        if (playerUI != null)
            playerUI.UpdatePotionCount();

        // Inventory grid – explicitly HEALTH potions now
        if (InventoryUIController.Instance != null)
            InventoryUIController.Instance.RefreshHealthPotionDisplay(potion_counter);
        // (InventoryUIController also still supports RefreshPotionDisplay if anything calls that)

        OnPotionChanged?.Invoke(potion_counter);
    }
}
