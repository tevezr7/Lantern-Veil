using UnityEngine;
using UnityEngine.UI;

public class InventoryUIController : MonoBehaviour
{
    public static InventoryUIController Instance { get; private set; }

    [Header("Slots")]
    [SerializeField] private InventorySlotUI[] slots;   // assigned in Inspector or auto-found

    [Header("Drag Visual")]
    [SerializeField] private Canvas rootCanvas;         // main UI canvas (InventoryCanvas)
    [SerializeField] private Image dragIconPrefab;      // optional, can create at runtime

    [Header("Gameplay")]
    [SerializeField] private PotionInventory potionInventory;
    [SerializeField] private PlayerHealth playerHealth;

    private InventorySlotUI currentPotionSlot;          // where the stack currently lives
    private InventorySlotUI draggingFrom;               // slot we started dragging from
    private int currentPotionCount = 0;

    // runtime drag ghost
    private Image dragIconInstance;
    private RectTransform dragIconRT;
    private bool isDragging = false;

    private void Awake()
    {
        // basic singleton
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // find slots if not assigned
        if (slots == null || slots.Length == 0)
        {
            slots = GetComponentsInChildren<InventorySlotUI>(true);
        }

        if (slots != null && slots.Length > 0)
        {
            currentPotionSlot = slots[0];   // default first slot
        }

        if (!potionInventory)
        {
#if UNITY_2023_1_OR_NEWER
            potionInventory = Object.FindAnyObjectByType<PotionInventory>();
#else
    potionInventory = FindObjectOfType<PotionInventory>();
#endif
        }

        if (!playerHealth)
        {
#if UNITY_2023_1_OR_NEWER
            playerHealth = Object.FindAnyObjectByType<PlayerHealth>();
#else
    playerHealth = FindObjectOfType<PlayerHealth>();
#endif
        }


        // make sure we know the canvas
        if (!rootCanvas)
            rootCanvas = GetComponentInParent<Canvas>();

        CreateDragIcon();

        // initial display (no potions yet)
        RefreshPotionDisplay(0);
    }

    // -------------------------------------------------------------------------
    // Called by PotionInventory whenever potion count changes

    public void RefreshPotionDisplay(int totalPotions)
    {
        currentPotionCount = Mathf.Max(0, totalPotions);

        if (slots == null || slots.Length == 0)
            return;

        if (currentPotionSlot == null)
            currentPotionSlot = slots[0];

        foreach (var s in slots)
        {
            if (s == currentPotionSlot)
                s.SetCount(currentPotionCount);
            else
                s.SetCount(0);
        }
    }

    // -------------------------------------------------------------------------
    // Drag handling

    public void BeginDrag(InventorySlotUI from)
    {
        // Only allow drag if this slot holds our potion stack
        if (from == null || !from.HasItem) return;
        if (from != currentPotionSlot) return;

        draggingFrom = from;
        isDragging = true;

        // show drag ghost icon
        if (dragIconInstance != null)
        {
            dragIconInstance.enabled = true;
            dragIconInstance.color = new Color(1f, 1f, 1f, 0.9f);

            // use the slot's actual icon sprite
            var iconImg = from.IconImage;
            if (iconImg != null && iconImg.sprite != null)
            {
                dragIconInstance.sprite = iconImg.sprite;
                dragIconInstance.preserveAspect = true;
                dragIconInstance.SetNativeSize();

                // optionally shrink a bit so it’s not huge
                dragIconRT.sizeDelta *= 0.9f;
            }
        }

    }

    public void UpdateDrag(Vector2 screenPos)
    {
        if (!isDragging || dragIconRT == null) return;

        // convert from screen space to canvas space
        Vector2 localPos;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                rootCanvas.transform as RectTransform,
                screenPos,
                rootCanvas.worldCamera,
                out localPos))
        {
            dragIconRT.localPosition = localPos;
        }
    }

    public void EndDrag()
    {
        isDragging = false;
        draggingFrom = null;

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
            // dropped on same slot, nothing to do
            EndDrag();
            return;
        }

        // move the potion stack to the new slot
        currentPotionSlot = target;
        ApplyCurrentPotionStateToSlots();

        EndDrag();
    }

    private void ApplyCurrentPotionStateToSlots()
    {
        if (slots == null || slots.Length == 0) return;

        foreach (var s in slots)
        {
            if (s == currentPotionSlot)
                s.SetCount(currentPotionCount);
            else
                s.SetCount(0);
        }
    }

    // -------------------------------------------------------------------------
    // Drag ghost creation

    private void CreateDragIcon()
    {
        if (!rootCanvas) return;

        if (!dragIconInstance)
        {
            GameObject go = new GameObject("DragIcon", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            go.transform.SetParent(rootCanvas.transform, false);

            dragIconRT = go.GetComponent<RectTransform>();
            dragIconInstance = go.GetComponent<Image>();

            dragIconInstance.raycastTarget = false; // let raycasts pass through to slots
            dragIconInstance.enabled = false;       // hidden until dragging
        }
    }


    public void OnSlotClicked(InventorySlotUI slot)
    {
        if (slot == null || potionInventory == null)
            return;

        // Only allow using from the slot that currently holds the potion stack
        if (slot != currentPotionSlot)
            return;

        // Let PotionInventory handle all logic: HP check, count, sound, UI
        potionInventory.TryUsePotion();
    }



}
