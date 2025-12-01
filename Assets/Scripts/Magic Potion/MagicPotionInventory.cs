using UnityEngine;
using System.Collections;   // for IEnumerator

public class MagicPotionInventory : MonoBehaviour
{
    [Header("Data")]
    public int magicPotionCount;

    public event System.Action<int> OnMagicPotionChanged;

    [Header("Magic Restore")]
    [SerializeField] private PlayerMagic playerMagic;
    [SerializeField] private float restoreAmount = 40f;

    [Header("Audio (optional)")]
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioClip drinkSfx;

    void Awake()
    {
        if (!playerMagic)
        {
#if UNITY_2023_1_OR_NEWER
            playerMagic = Object.FindAnyObjectByType<PlayerMagic>();
#else
            playerMagic = FindObjectOfType<PlayerMagic>();
#endif
        }
    }

    void Start()
    {
        // Immediate refresh
        RefreshUI();

        // Same delayed trick as health, for when InventoryUIController appears slightly later
        StartCoroutine(DelayedRefreshUI());
    }

    private IEnumerator DelayedRefreshUI()
    {
        yield return null;
        RefreshUI();
    }

    // Called by Potion pickup when type == Magic
    public void AddMagicPotion()
    {
        magicPotionCount++;
        Debug.Log($"[MagicPotionInventory] AddMagicPotion -> {magicPotionCount}");
        RefreshUI();
    }

    public bool TryUseMagicPotion()
    {
        if (playerMagic == null) return false;

        // No potions
        if (magicPotionCount <= 0)
        {
            if (UIAudio.I != null) UIAudio.I.PlayPotionFail();
            return false;
        }

        // Already full magic
        if (playerMagic.magic >= playerMagic.maxMagic)
        {
            if (UIAudio.I != null) UIAudio.I.PlayPotionFail();
            return false;
        }

        // Success
        magicPotionCount--;
        playerMagic.RestoreMagic(restoreAmount);

        if (UIAudio.I != null)
            UIAudio.I.PlayPotionUse();

        if (sfxSource && drinkSfx)
            sfxSource.PlayOneShot(drinkSfx);

        Debug.Log($"[MagicPotionInventory] UseMagicPotion -> {magicPotionCount}");
        RefreshUI();
        return true;
    }

    public void UseMagicPotion()
    {
        TryUseMagicPotion();
    }

    private void RefreshUI()
    {
        if (InventoryUIController.Instance != null)
            InventoryUIController.Instance.RefreshMagicPotionDisplay(magicPotionCount);

        OnMagicPotionChanged?.Invoke(magicPotionCount);
    }
}
