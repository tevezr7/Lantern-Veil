using UnityEngine;

public class SimpleInventoryGrid : MonoBehaviour
{
    [Header("Slot Prefab (UI)")]
    [SerializeField] private GameObject slotPrefab;

    [Header("Test Settings")]
    [SerializeField] private int testSlotCount = 8;

    private void Start()
    {
        if (slotPrefab == null)
        {
            Debug.LogError("[SimpleInventoryGrid] No slotPrefab assigned!", this);
            return;
        }

        // Just spawn some dummy slots so we can see the grid working.
        for (int i = 0; i < testSlotCount; i++)
        {
            GameObject slot = Instantiate(slotPrefab, transform);
            slot.name = $"Slot_{i}";
        }
    }
}
