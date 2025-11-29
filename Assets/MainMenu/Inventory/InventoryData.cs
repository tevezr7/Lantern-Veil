using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct ItemStack
{
    public ItemSO item;
    public int count;

    public bool IsEmpty => item == null || count <= 0;
}

public class InventoryData : MonoBehaviour
{
    [Header("Layout")]
    public int capacity = 20;                     // number of slots in the grid

    [Header("State (runtime)")]
    public List<ItemStack> slots;                 // size == capacity at runtime

    public event Action OnChanged;                // UI listens to this

    void Awake()
    {
        if (slots == null || slots.Count != capacity)
        {
            slots = new List<ItemStack>(capacity);
            for (int i = 0; i < capacity; i++) slots.Add(new ItemStack());
        }
    }

    public bool Add(ItemSO item, int amount = 1)
    {
        if (item == null || amount <= 0) return false;

        // 1) fill existing stacks
        for (int i = 0; i < slots.Count && amount > 0; i++)
        {
            if (slots[i].item == item && slots[i].count < item.maxStack)
            {
                int canAdd = Mathf.Min(item.maxStack - slots[i].count, amount);
                var s = slots[i];
                s.count += canAdd;
                slots[i] = s;
                amount -= canAdd;
            }
        }
        // 2) use empty slots
        for (int i = 0; i < slots.Count && amount > 0; i++)
        {
            if (slots[i].IsEmpty)
            {
                int put = Mathf.Min(item.maxStack, amount);
                slots[i] = new ItemStack { item = item, count = put };
                amount -= put;
            }
        }

        bool changed = amount == 0;
        if (changed) OnChanged?.Invoke();
        return changed;
    }

    public bool Remove(ItemSO item, int amount = 1)
    {
        if (item == null || amount <= 0) return false;
        int remaining = amount;

        // remove from stacks left-to-right
        for (int i = 0; i < slots.Count && remaining > 0; i++)
        {
            if (slots[i].item == item && slots[i].count > 0)
            {
                int take = Mathf.Min(slots[i].count, remaining);
                var s = slots[i];
                s.count -= take;
                if (s.count <= 0) s = new ItemStack();
                slots[i] = s;
                remaining -= take;
            }
        }
        bool changed = remaining == 0;
        if (changed) OnChanged?.Invoke();
        return changed;
    }

    public void SwapSlots(int a, int b)
    {
        if (a == b) return;
        if ((uint)a >= (uint)slots.Count || (uint)b >= (uint)slots.Count) return;
        (slots[a], slots[b]) = (slots[b], slots[a]);
        OnChanged?.Invoke();
    }

    public void MoveOrMerge(int from, int to)
    {
        if ((uint)from >= (uint)slots.Count || (uint)to >= (uint)slots.Count) return;
        if (from == to) return;

        var A = slots[from];
        var B = slots[to];

        if (A.IsEmpty)
            return;

        // Merge if same item and room
        if (!B.IsEmpty && B.item == A.item && B.count < B.item.maxStack)
        {
            int move = Mathf.Min(A.count, B.item.maxStack - B.count);
            B.count += move;
            A.count -= move;
            if (A.count <= 0) A = new ItemStack();
            slots[from] = A;
            slots[to] = B;
        }
        else
        {
            // otherwise swap
            slots[from] = B;
            slots[to] = A;
        }

        OnChanged?.Invoke();
    }
}
