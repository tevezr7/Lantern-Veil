using UnityEngine;
using UnityEngine.UI;

public class InventoryUIController : MonoBehaviour
{
    public static InventoryUIController Instance { get; private set; }

    [Header("Slots")]
    [SerializeField] private InventorySlotUI[] slots;          // All slots in the grid
    [SerializeField] private InventorySlotUI healthPotionSlot; // Starting slot for HEALTH potions
    [SerializeField] private InventorySlotUI magicPotionSlot;  // Starting slot for MAGIC potions

    [Header("Icons")]
    [SerializeField] private Sprite healthPotionSprite;        // icon for health potions
    [SerializeField] private Sprite magicPotionSprite;         // icon for magic potions

    [Header("Drag Visual")]
    [SerializeField] private Canvas rootCanvas;                // Main UI canvas (InventoryCanvas)

    [Header("Inventories")]
    [SerializeField] private PotionInventory healthPotionInventory;
    [SerializeField] private MagicPotionInventory magicPotionInventory;

    // --- Internal state for the two stacks ---
    private InventorySlotUI currentHealthSlot;
    private InventorySlotUI currentMagicSlot;
    private int healthCount = 0;
    private int magicCount = 0;

    // Drag state
    private enum DragType { None, Health, Magic }
    private DragType draggingType = DragType.None;
    private InventorySlotUI draggingFrom;
    private Image dragIconInstance;
    private RectTransform dragIconRT;
    private bool isDragging = false;

    private void Awake()
    {
        // Singleton
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // Auto-find slots
        if (slots == null || slots.Length == 0)
            slots = GetComponentsInChildren<InventorySlotUI>(true);

        // Default starting slots
        if (healthPotionSlot == null && slots != null && slots.Length > 0)
            healthPotionSlot = slots[0];

        if (magicPotionSlot == null && slots != null && slots.Length > 1)
            magicPotionSlot = slots[1];

        currentHealthSlot = healthPotionSlot;
        currentMagicSlot = magicPotionSlot;

        // Canvas reference
        if (!rootCanvas)
            rootCanvas = GetComponentInParent<Canvas>();

        CreateDragIcon();

        // Initial display
        ApplyCountsToSlots();
    }

    // -------------------------------------------------------------------------
    // Called by PotionInventory (Health)
    // -------------------------------------------------------------------------

    public void RefreshHealthPotionDisplay(int count)
    {
        healthCount = Mathf.Max(0, count);
        ApplyCountsToSlots();
    }

    // Kept for backwards compatibility if something still calls this:
    public void RefreshPotionDisplay(int count)
    {
        RefreshHealthPotionDisplay(count);
    }

    // -------------------------------------------------------------------------
    // Called by MagicPotionInventory (Magic)
    // -------------------------------------------------------------------------

    public void RefreshMagicPotionDisplay(int count)
    {
        magicCount = Mathf.Max(0, count);
        ApplyCountsToSlots();
    }

    // -------------------------------------------------------------------------
    // Assign counts + correct icon sprites to each slot
    // -------------------------------------------------------------------------

    private void ApplyCountsToSlots()
    {
        if (slots == null || slots.Length == 0) return;

        foreach (var s in slots)
        {
            bool hasHealthHere = (s == currentHealthSlot && healthCount > 0);
            bool hasMagicHere = (s == currentMagicSlot && magicCount > 0);

            int displayCount = 0;
            if (hasHealthHere) displayCount += healthCount;
            if (hasMagicHere) displayCount += magicCount;

            // Update count (this will show/hide the icon based on >0)
            s.SetCount(displayCount);

            // Now set the correct icon sprite
            var iconImg = s.IconImage;
            if (iconImg != null)
            {
                if (hasHealthHere && !hasMagicHere)
                {
                    iconImg.sprite = healthPotionSprite;
                }
                else if (!hasHealthHere && hasMagicHere)
                {
                    iconImg.sprite = magicPotionSprite;
                }
                else if (!hasHealthHere && !hasMagicHere)
                {
                    // No stack in this slot: optional – keep whatever sprite was last, or clear it:
                    // iconImg.sprite = null;
                }
                // (We already prevent overlapping stacks, so hasHealthHere && hasMagicHere shouldn't happen.)
            }
        }
    }

    // -------------------------------------------------------------------------
    // Drag handling (for both stacks)
    // -------------------------------------------------------------------------

    public void BeginDrag(InventorySlotUI from)
    {
        if (from == null || !from.HasItem) return;

        // Decide what we’re dragging
        if (from == currentHealthSlot && healthCount > 0)
            draggingType = DragType.Health;
        else if (from == currentMagicSlot && magicCount > 0)
            draggingType = DragType.Magic;
        else
            return; // slot doesn’t belong to any active stack

        draggingFrom = from;
        isDragging = true;

        // Show drag ghost icon with correct sprite based on WHAT we're dragging
        if (dragIconInstance != null)
        {
            dragIconInstance.enabled = true;
            dragIconInstance.color = new Color(1f, 1f, 1f, 0.9f);

            Sprite spriteToUse = null;
            switch (draggingType)
            {
                case DragType.Health:
                    spriteToUse = healthPotionSprite;
                    break;
                case DragType.Magic:
                    spriteToUse = magicPotionSprite;
                    break;
            }

            if (spriteToUse != null)
            {
                dragIconInstance.sprite = spriteToUse;
                dragIconInstance.preserveAspect = true;
                dragIconInstance.SetNativeSize();
                dragIconRT.sizeDelta *= 0.9f;
            }
        }
    }

    public void UpdateDrag(Vector2 screenPos)
    {
        if (!isDragging || dragIconRT == null) return;

        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                rootCanvas.transform as RectTransform,
                screenPos,
                rootCanvas.worldCamera,
                out var localPos))
        {
            dragIconRT.localPosition = localPos;
        }
    }

    public void EndDrag()
    {
        isDragging = false;
        draggingFrom = null;
        draggingType = DragType.None;

        if (dragIconInstance != null)
            dragIconInstance.enabled = false;
    }

    public void DropOnto(InventorySlotUI target)
    {
        if (!isDragging || draggingFrom == null || target == null)
        {
            EndDrag();
            return;
        }

        if (target == draggingFrom)
        {
            EndDrag();
            return;
        }

        // Optional: prevent overlapping stacks
        if (draggingType == DragType.Health && target == currentMagicSlot && magicCount > 0)
        {
            EndDrag();
            return;
        }
        if (draggingType == DragType.Magic && target == currentHealthSlot && healthCount > 0)
        {
            EndDrag();
            return;
        }

        // Move whichever stack we’re dragging
        switch (draggingType)
        {
            case DragType.Health:
                currentHealthSlot = target;
                break;
            case DragType.Magic:
                currentMagicSlot = target;
                break;
        }

        ApplyCountsToSlots();
        EndDrag();
    }

    // -------------------------------------------------------------------------
    // Drag ghost creation
    // -------------------------------------------------------------------------

    private void CreateDragIcon()
    {
        if (!rootCanvas) return;

        if (!dragIconInstance)
        {
            GameObject go = new GameObject("DragIcon", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            go.transform.SetParent(rootCanvas.transform, false);

            dragIconRT = go.GetComponent<RectTransform>();
            dragIconInstance = go.GetComponent<Image>();

            dragIconInstance.raycastTarget = false;
            dragIconInstance.enabled = false;
        }
    }

    // -------------------------------------------------------------------------
    // Slot click handling -> actually use the potions
    // -------------------------------------------------------------------------

    public void OnSlotClicked(InventorySlotUI slot)
    {
        if (slot == currentHealthSlot)
        {
            if (healthPotionInventory != null)
                healthPotionInventory.TryUsePotion();
        }
        else if (slot == currentMagicSlot)
        {
            if (magicPotionInventory != null)
                magicPotionInventory.TryUseMagicPotion();
        }
    }
}
