using UnityEngine;

[System.Serializable]
public class ItemSlot
{
    public Item item;
    public int amount;

    // 添加属性
    public bool IsLocked { get; private set; }
    public bool IsHighlighted { get; private set; }

    public ItemSlot()
    {
        item = null;
        amount = 0;
        IsLocked = false;
        IsHighlighted = false;
    }

    public bool IsEmpty()
    {
        return item == null;
    }

    public bool CanAddItem(Item newItem, int quantity)
    {
        if (IsLocked) return false;
        if (IsEmpty()) return true;
        return item.Id == newItem.Id && amount + quantity <= item.MaxStackSize;
    }

    // 锁定/解锁槽位
    public void SetLocked(bool locked)
    {
        IsLocked = locked;
    }

    // 高亮/取消高亮槽位
    public void SetHighlight(bool highlight)
    {
        IsHighlighted = highlight;
    }

    // 清空槽位
    public void Clear()
    {
        if (IsLocked) return;
        item = null;
        amount = 0;
    }

    // 转移物品到另一个槽位
    public bool TransferTo(ItemSlot targetSlot, int transferAmount = -1)
    {
        if (IsEmpty() || IsLocked || targetSlot.IsLocked) return false;

        if (transferAmount == -1)
        {
            transferAmount = amount;
        }

        if (transferAmount > amount)
        {
            return false;
        }

        if (targetSlot.IsEmpty())
        {
            targetSlot.item = item.Clone();
            targetSlot.amount = transferAmount;
            amount -= transferAmount;
            if (amount <= 0)
            {
                Clear();
            }
            return true;
        }

        if (targetSlot.item.Id == item.Id)
        {
            int maxTransfer = targetSlot.item.MaxStackSize - targetSlot.amount;
            int actualTransfer = Mathf.Min(transferAmount, maxTransfer);
            if (actualTransfer <= 0) return false;

            targetSlot.amount += actualTransfer;
            amount -= actualTransfer;
            if (amount <= 0)
            {
                Clear();
            }
            return true;
        }

        return false;
    }

    // 分割堆叠
    public ItemSlot Split(int splitAmount)
    {
        if (IsEmpty() || IsLocked || splitAmount >= amount) return null;

        ItemSlot newSlot = new ItemSlot
        {
            item = item,
            amount = splitAmount
        };

        amount -= splitAmount;
        if (amount <= 0)
        {
            Clear();
        }

        return newSlot;
    }
} 