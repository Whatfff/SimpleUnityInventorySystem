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
            UpdateSlot(i);
        }
    }

    private void UpdateSlot(int index)
    {
        if (index >= 0 && index < uiSlots.Count)
        {
            ItemSlot slot = inventory.slots[index];
            UIItemSlot uiSlot = uiSlots[index];
            
            if (!slot.IsEmpty())
            {
                uiSlot.itemIcon.enabled = true;
                uiSlot.itemIcon.sprite = slot.item.Icon;
                
                if (slot.amount > 1)
                {
                    uiSlot.amountText.enabled = true;
                    uiSlot.amountText.text = slot.amount.ToString();
                }
                else
                {
                    uiSlot.amountText.enabled = false;
                }
            }
            else
            {
                uiSlot.itemIcon.enabled = false;
                uiSlot.amountText.enabled = false;
            }
        }
    }

    public void SwapItems(int fromIndex, int toIndex)
    {
        ItemSlot fromSlot = inventory.slots[fromIndex];
        ItemSlot toSlot = inventory.slots[toIndex];

        // 如果目标格子为空或者是相同物品且可以堆叠
        if (toSlot.IsEmpty() || (fromSlot.item.Id == toSlot.item.Id && toSlot.CanAddItem(fromSlot.item, fromSlot.amount)))
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