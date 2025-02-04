using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class Inventory : MonoBehaviour
{
    public int inventorySize = 20;
    public ItemSlot[] slots;

    private void Awake()
    {
        slots = new ItemSlot[inventorySize];
        for (int i = 0; i < inventorySize; i++)
        {
            slots[i] = new ItemSlot();
        }
    }

    public bool AddItem(Item item, int amount = 1)
    {
        // 先尝试堆叠到现有的物品上
        for (int i = 0; i < slots.Length; i++)
        {
            if (!slots[i].IsEmpty() && slots[i].item.id == item.id && slots[i].CanAddItem(item, amount))
            {
                slots[i].amount += amount;
                return true;
            }
        }

        // 如果无法堆叠，寻找空槽位
        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i].IsEmpty())
            {
                slots[i].item = item;
                slots[i].amount = amount;
                return true;
            }
        }

        return false; // 背包已满
    }

    public bool RemoveItem(int slotIndex, int amount = 1)
    {
        if (slotIndex < 0 || slotIndex >= slots.Length) return false;
        if (slots[slotIndex].IsEmpty()) return false;

        slots[slotIndex].amount -= amount;
        if (slots[slotIndex].amount <= 0)
        {
            slots[slotIndex].item = null;
            slots[slotIndex].amount = 0;
        }
        return true;
    }

    public ItemSlot[] GetItemsByType(Item.ItemType type)
    {
        return slots.Where(slot => !slot.IsEmpty() && slot.item.itemType == type).ToArray();
    }

    public ItemSlot[] GetItemsByRarity(Item.ItemRarity rarity)
    {
        return slots.Where(slot => !slot.IsEmpty() && slot.item.rarity == rarity).ToArray();
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
                itemIds[i] = slots[i].IsEmpty() ? -1 : slots[i].item.id;
                amounts[i] = slots[i].amount;
            }
        }
    }

    public void SaveInventory()
    {
        InventoryData data = new InventoryData(slots);
        string json = JsonUtility.ToJson(data);
        PlayerPrefs.SetString("InventoryData", json);
        PlayerPrefs.Save();
    }

    public void LoadInventory(ItemDatabase itemDatabase) // 需要一个物品数据库来根据ID获取物品
    {
        if (!PlayerPrefs.HasKey("InventoryData")) return;

        string json = PlayerPrefs.GetString("InventoryData");
        InventoryData data = JsonUtility.FromJson<InventoryData>(json);

        for (int i = 0; i < slots.Length; i++)
        {
            if (data.itemIds[i] != -1)
            {
                slots[i].item = itemDatabase.GetItemById(data.itemIds[i]);
                slots[i].amount = data.amounts[i];
            }
            else
            {
                slots[i].item = null;
                slots[i].amount = 0;
            }
        }
    }
} 