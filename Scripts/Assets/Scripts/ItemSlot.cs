using UnityEngine;

[System.Serializable]
public class ItemSlot
{
    public Item item;
    public int amount;

    public ItemSlot()
    {
        item = null;
        amount = 0;
    }

    public bool IsEmpty()
    {
        return item == null;
    }

    public bool CanAddItem(Item newItem, int quantity)
    {
        if (IsEmpty()) return true;
        if (item.id == newItem.id && amount + quantity <= item.maxStackSize) return true;
        return false;
    }
} 