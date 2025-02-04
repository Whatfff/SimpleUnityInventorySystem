using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class UIItemSlot : MonoBehaviour, IPointerClickHandler, IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler
{
    public Image itemIcon;
    public TextMeshProUGUI amountText;
    public int slotIndex;

    private ItemSlot itemSlot;
    private UIInventory uiInventory;
    private static UIItemSlot draggedSlot;
    private static GameObject draggedIcon;
    private static Canvas canvas;

    private void Start()
    {
        canvas = GetComponentInParent<Canvas>();
        uiInventory = GetComponentInParent<UIInventory>();
        itemIcon.enabled = false;
        amountText.enabled = false;
    }

    public void UpdateUI(ItemSlot slot)
    {
        itemSlot = slot;
        if (slot.IsEmpty())
        {
            itemIcon.enabled = false;
            amountText.enabled = false;
        }
        else
        {
            itemIcon.enabled = true;
            itemIcon.sprite = slot.item.icon;
            
            if (slot.amount > 1)
            {
                amountText.enabled = true;
                amountText.text = slot.amount.ToString();
            }
            else
            {
                amountText.enabled = false;
            }
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Right && !itemSlot.IsEmpty())
        {
            itemSlot.item.Use();
            uiInventory.OnItemUsed(slotIndex);
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (itemSlot.IsEmpty()) return;

        draggedSlot = this;
        draggedIcon = new GameObject("Dragged Icon");
        draggedIcon.transform.SetParent(canvas.transform);
        
        Image draggedImage = draggedIcon.AddComponent<Image>();
        draggedImage.sprite = itemIcon.sprite;
        draggedImage.raycastTarget = false;
        
        itemIcon.enabled = false;
        amountText.enabled = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (draggedIcon != null)
        {
            draggedIcon.transform.position = Input.mousePosition;
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (draggedIcon != null)
        {
            Destroy(draggedIcon);
            UpdateUI(itemSlot);
            draggedSlot = null;
        }
    }

    public void OnDrop(PointerEventData eventData)
    {
        if (draggedSlot != null && draggedSlot != this)
        {
            uiInventory.SwapItems(draggedSlot.slotIndex, slotIndex);
        }
    }
} 