using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem;

public class InventoryScreenController : MonoBehaviour
{
    [Header("Links")]
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private GameObject root;

    [Header("Health Potions UI")]
    [SerializeField] private Image potionIcon;
    [SerializeField] private TMP_Text potionCountText;
    [SerializeField] private Sprite potionSprite;
    [SerializeField] private PotionInventory potionInventory;

    [Header("Magic Potions UI")]
    [SerializeField] private Image magicPotionIcon;              // NEW
    [SerializeField] private TMP_Text magicPotionCountText;      // NEW
    [SerializeField] private Sprite magicPotionSprite;           // NEW
    [SerializeField] private MagicPotionInventory magicPotionInventory; // NEW

    [Header("Behavior")]
    [SerializeField] private bool pauseOnOpen = true;
    [SerializeField] private KeyCode fallbackKey = KeyCode.B;
    [SerializeField] private float fade = 0.15f;

    [Header("Inventory Music")]
    [SerializeField] private AudioSource inventoryMusicSource;
    [SerializeField] private AudioClip inventoryMusicClip;
    [SerializeField][Range(0f, 1f)] private float inventoryMusicVolume = 0.5f;

    bool isOpen;

    void Awake()
    {
        if (!root) root = gameObject;
        if (!canvasGroup) canvasGroup = GetComponent<CanvasGroup>();

        // --- find inventories if not assigned ---
        if (!potionInventory)
        {
#if UNITY_2023_1_OR_NEWER
            potionInventory = Object.FindAnyObjectByType<PotionInventory>();
#else
            potionInventory = FindObjectOfType<PotionInventory>();
#endif
        }

        if (!magicPotionInventory)
        {
#if UNITY_2023_1_OR_NEWER
            magicPotionInventory = Object.FindAnyObjectByType<MagicPotionInventory>();
#else
            magicPotionInventory = FindObjectOfType<MagicPotionInventory>();
#endif
        }

        // assign sprites
        if (potionIcon && potionSprite) potionIcon.sprite = potionSprite;
        if (magicPotionIcon && magicPotionSprite) magicPotionIcon.sprite = magicPotionSprite;

        // start hidden
        root.SetActive(false);
        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;

        // subscribe to events
        if (potionInventory != null)
            potionInventory.OnPotionChanged += OnPotionChanged;

        if (magicPotionInventory != null)
            magicPotionInventory.OnMagicPotionChanged += OnMagicPotionChanged;
    }



    void OnDestroy()
    {
        if (potionInventory != null)
            potionInventory.OnPotionChanged -= OnPotionChanged;

        if (magicPotionInventory != null)
            magicPotionInventory.OnMagicPotionChanged -= OnMagicPotionChanged;
    }

    void Update()
    {
        // Handle ESC to close when open
        if (isOpen)
        {
            if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                Close();
                MarkEscConsumed();
                return;
            }

            if (Keyboard.current == null && Input.GetKeyDown(KeyCode.Escape))
            {
                Close();
                MarkEscConsumed();
                return;
            }
        }

        // Handle open/close (B key or fallback)
        if (Keyboard.current == null)
        {
            if (Input.GetKeyDown(fallbackKey)) Toggle();
        }
        else
        {
            if (Keyboard.current.bKey.wasPressedThisFrame) Toggle();
        }

        // Smooth fade
        float target = isOpen ? 1f : 0f;
        if (canvasGroup)
        {
            canvasGroup.alpha = Mathf.MoveTowards(
                canvasGroup.alpha,
                target,
                Time.unscaledDeltaTime / Mathf.Max(0.0001f, fade)
            );
        }
    }

    // -------------------------------------------------------------------------
    // ESC consumption
    // -------------------------------------------------------------------------

    static float _escConsumedUntil = -1f;
    static void MarkEscConsumed()
    {
        _escConsumedUntil = Time.unscaledTime + 0.05f;
    }
    public static bool EscRecentlyConsumed =>
        Time.unscaledTime <= _escConsumedUntil;

    // -------------------------------------------------------------------------
    // Potion UI (Health + Magic)
    // -------------------------------------------------------------------------

    void OnPotionChanged(int count)
    {
        if (potionCountText) potionCountText.text = "x" + count;
    }

    void OnMagicPotionChanged(int count)
    {
        if (magicPotionCountText) magicPotionCountText.text = "x" + count;
    }

    // -------------------------------------------------------------------------
    // Open / Close / Toggle
    // -------------------------------------------------------------------------

    public void Toggle()
    {
        if (isOpen) Close();
        else Open();
    }

    public void Open()
    {
        if (isOpen) return;
        isOpen = true;

        // 🔊 Inventory open UI SFX
        if (UIAudio.I != null)
            UIAudio.I.PlayInventoryOpen();

        // Refresh counts in the header text
        if (potionCountText && potionInventory)
            potionCountText.text = "x" + potionInventory.potion_counter;

       
        if (InventoryUIController.Instance != null)
        {
            if (potionInventory != null)
                InventoryUIController.Instance.RefreshHealthPotionDisplay(potionInventory.potion_counter);

            // If you have a magic inventory:
            var magicInv = FindObjectOfType<MagicPotionInventory>();
            if (magicInv != null)
                InventoryUIController.Instance.RefreshMagicPotionDisplay(magicInv.magicPotionCount);
        }

        root.SetActive(true);
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;

        if (pauseOnOpen) Time.timeScale = 0f;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        // 🎵 Start inventory music (your existing code)
        if (inventoryMusicSource != null && inventoryMusicClip != null)
        {
            if (inventoryMusicSource.clip != inventoryMusicClip)
                inventoryMusicSource.clip = inventoryMusicClip;

            inventoryMusicSource.volume = inventoryMusicVolume;
            inventoryMusicSource.loop = true;
            inventoryMusicSource.Play();
        }
    }

    public void Close()
    {
        if (!isOpen) return;

        // 🔊 Inventory close UI SFX
        if (UIAudio.I != null)
            UIAudio.I.PlayInventoryClose();

        isOpen = false;

        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;

        if (pauseOnOpen) Time.timeScale = 1f;
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        // 🎵 Stop inventory music
        if (inventoryMusicSource != null && inventoryMusicSource.isPlaying)
        {
            inventoryMusicSource.Stop();
        }

        StartCoroutine(DeactivateWhenInvisible());
    }

    System.Collections.IEnumerator DeactivateWhenInvisible()
    {
        while (canvasGroup && canvasGroup.alpha > 0.001f)
            yield return null;

        if (root) root.SetActive(false);
    }

    // -------------------------------------------------------------------------
    // Input System callback
    // -------------------------------------------------------------------------

    public void OnInventory(InputAction.CallbackContext ctx)
    {
        if (!ctx.started) return;
        Toggle();
    }
}
