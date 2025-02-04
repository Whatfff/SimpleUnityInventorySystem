using UnityEngine;
using System.Collections.Generic;

public class UIInventory : MonoBehaviour
{
    public GameObject slotPrefab;
    public Transform slotsParent;
    public Inventory inventory;

    private List<UIItemSlot> uiSlots = new List<UIItemSlot>();

    private void Start()
    {
        CreateSlots();
        UpdateUI();
    }

    private void CreateSlots()
    {
        for (int i = 0; i < inventory.inventorySize; i++)
        {
            GameObject slotGO = Instantiate(slotPrefab, slotsParent);
            UIItemSlot uiSlot = slotGO.GetComponent<UIItemSlot>();
            uiSlot.slotIndex = i;
            uiSlots.Add(uiSlot);
        }
    }

    public void UpdateUI()
    {
        for (int i = 0; i < inventory.slots.Length; i++)
        {
            uiSlots[i].UpdateUI(inventory.slots[i]);
        }
    }

    public void SwapItems(int fromIndex, int toIndex)
    {
        ItemSlot fromSlot = inventory.slots[fromIndex];
        ItemSlot toSlot = inventory.slots[toIndex];

        // 如果目标格子为空或者是相同物品且可以堆叠
        if (toSlot.IsEmpty() || (fromSlot.item.id == toSlot.item.id && toSlot.CanAddItem(fromSlot.item, fromSlot.amount)))
        {
            Item tempItem = fromSlot.item;
            int tempAmount = fromSlot.amount;

            fromSlot.item = toSlot.item;
            fromSlot.amount = toSlot.amount;

            toSlot.item = tempItem;
            toSlot.amount = tempAmount;

            UpdateUI();
        }
    }

    public void OnItemUsed(int slotIndex)
    {
        inventory.RemoveItem(slotIndex, 1);
        UpdateUI();
    }
} 