using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class InventorySlotUI : MonoBehaviour,
    IBeginDragHandler, IEndDragHandler, IDragHandler, IDropHandler, IPointerClickHandler
{
    [Header("UI References")]
    [SerializeField] private Image iconImage;               // Icon of the item
    [SerializeField] private TextMeshProUGUI countText;     // e.g. "x3"
    public Image IconImage => iconImage;

    [Header("Behaviour")]
    [SerializeField] private bool hideWhenZero = true;

    private int currentCount = 0;
    public bool HasItem => currentCount > 0;

    private void Awake()
    {
        SetCount(0);                // start empty-looking
    }

    // -------------------------------------------------------------------------
    // PUBLIC API
    // -------------------------------------------------------------------------

    public void SetCount(int count)
    {
        currentCount = Mathf.Max(0, count);
        bool hasItem = currentCount > 0;

        if (iconImage)
            iconImage.enabled = hasItem;

        if (countText)
        {
            if (!hasItem)
                countText.text = hideWhenZero ? "" : "x0";
            else
                countText.text = "x" + currentCount.ToString();
        }
    }

    public int GetCount() => currentCount;

    // -------------------------------------------------------------------------
    // DRAG HANDLERS
    // -------------------------------------------------------------------------

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (!HasItem) return;
        if (InventoryUIController.Instance == null) return;

        // Play drag start SFX
        UIAudio.I.PlayDragStart();

        InventoryUIController.Instance.BeginDrag(this);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (InventoryUIController.Instance == null) return;
        InventoryUIController.Instance.UpdateDrag(eventData.position); // moves ghost icon
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (InventoryUIController.Instance == null) return;

        InventoryUIController.Instance.EndDrag();

        // We assume it's a valid drop — controller handles the data
        UIAudio.I.PlayDropSuccess();
    }


    // -------------------------------------------------------------------------
    // DROP: Something was dropped ONTO this slot
    // -------------------------------------------------------------------------

    public void OnDrop(PointerEventData eventData)
    {
        if (InventoryUIController.Instance == null) return;
        InventoryUIController.Instance.DropOnto(this);
    }

    // -------------------------------------------------------------------------
    // CLICK
    // -------------------------------------------------------------------------

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left) return;

        if (InventoryUIController.Instance != null)
            InventoryUIController.Instance.OnSlotClicked(this);
    }
}
