using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class Inventory : MonoBehaviour
{
    // 添加事件系统
    public delegate void InventoryChangeHandler(int slotIndex);
    public event InventoryChangeHandler OnItemAdded;
    public event InventoryChangeHandler OnItemRemoved;
    public event InventoryChangeHandler OnItemUsed;

    public int inventorySize = 20;
    public ItemSlot[] slots;

    // 添加容量属性
    public int Capacity => inventorySize;
    public int UsedSlots => slots.Count(slot => !slot.IsEmpty());
    public bool IsFull => UsedSlots >= Capacity;

    private void Awake()
    {
        // 确保slots不为空
        if (slots == null || slots.Length != inventorySize)
        {
            slots = new ItemSlot[inventorySize];
            for (int i = 0; i < inventorySize; i++)
            {
                slots[i] = new ItemSlot();
            }
        }
    }

    public bool AddItem(Item item, int amount = 1)
    {
        if (item == null || amount <= 0) return false;

        // 先尝试堆叠到现有的物品上
        for (int i = 0; i < slots.Length; i++)
        {
            if (!slots[i].IsEmpty() && slots[i].item.Id == item.Id && slots[i].CanAddItem(item, amount))
            {
                slots[i].amount += amount;
                OnItemAdded?.Invoke(i);
                return true;
            }
        }

        // 如果无法堆叠，寻找空槽位
        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i].IsEmpty())
            {
                slots[i].item = item.Clone(); // 使用Clone方法避免引用问题
                slots[i].amount = amount;
                OnItemAdded?.Invoke(i);
                return true;
            }
        }

        return false; // 背包已满
    }

    public bool RemoveItem(int slotIndex, int amount = 1)
    {
        if (slotIndex < 0 || slotIndex >= slots.Length || amount <= 0) return false;
        if (slots[slotIndex].IsEmpty()) return false;
        if (slots[slotIndex].amount < amount) return false;

        slots[slotIndex].amount -= amount;
        if (slots[slotIndex].amount <= 0)
        {
            slots[slotIndex].item = null;
            slots[slotIndex].amount = 0;
        }
        OnItemRemoved?.Invoke(slotIndex);
        return true;
    }

    // 添加按物品ID移除方法
    public bool RemoveItemById(int itemId, int amount = 1)
    {
        int remainingAmount = amount;
        for (int i = 0; i < slots.Length && remainingAmount > 0; i++)
        {
            if (!slots[i].IsEmpty() && slots[i].item.Id == itemId)
            {
                int removeAmount = Mathf.Min(remainingAmount, slots[i].amount);
                RemoveItem(i, removeAmount);
                remainingAmount -= removeAmount;
            }
        }
        return remainingAmount == 0;
    }

    // 检查是否有足够空间
    public bool HasSpace(Item item, int amount = 1)
    {
        if (item == null || amount <= 0) return false;

        int remainingAmount = amount;
        
        // 检查现有堆叠
        for (int i = 0; i < slots.Length && remainingAmount > 0; i++)
        {
            if (!slots[i].IsEmpty() && slots[i].item.Id == item.Id)
            {
                remainingAmount -= (item.MaxStackSize - slots[i].amount);
            }
        }

        // 检查空槽位
        if (remainingAmount > 0)
        {
            int emptySlots = slots.Count(slot => slot.IsEmpty());
            remainingAmount -= emptySlots * item.MaxStackSize;
        }

        return remainingAmount <= 0;
    }

    public ItemSlot[] GetItemsByType(Item.ItemType type)
    {
        return slots.Where(slot => !slot.IsEmpty() && slot.item.Type == type).ToArray();
    }

    public ItemSlot[] GetItemsByRarity(Item.ItemRarity rarity)
    {
        return slots.Where(slot => !slot.IsEmpty() && slot.item.Rarity == rarity).ToArray();
    }

    // 存档功能
    [System.Serializable]
    private class InventoryData
    {
        public int[] itemIds;
        public int[] amounts;

        public InventoryData(ItemSlot[] slots)
        {
            itemIds = new int[slots.Length];
            amounts = new int[slots.Length];
            
            for (int i = 0; i < slots.Length; i++)
            {
                itemIds[i] = slots[i].IsEmpty() ? -1 : slots[i].item.Id;
                amounts[i] = slots[i].amount;
            }
        }
    }

    public void SaveInventory(string saveKey = "InventoryData")
    {
        InventoryData data = new InventoryData(slots);
        string json = JsonUtility.ToJson(data);
        PlayerPrefs.SetString(saveKey, json);
        PlayerPrefs.Save();
    }

    public bool LoadInventory(ItemDatabase itemDatabase, string saveKey = "InventoryData")
    {
        if (!PlayerPrefs.HasKey(saveKey)) return false;

        string json = PlayerPrefs.GetString(saveKey);
        InventoryData data = JsonUtility.FromJson<InventoryData>(json);

        for (int i = 0; i < slots.Length; i++)
        {
            if (data.itemIds[i] != -1)
            {
                Item item = itemDatabase.GetItemById(data.itemIds[i]);
                if (item != null)
                {
                    slots[i].item = item.Clone();
                    slots[i].amount = data.amounts[i];
                }
            }
            else
            {
                slots[i].item = null;
                slots[i].amount = 0;
            }
        }
        return true;
    }
} 