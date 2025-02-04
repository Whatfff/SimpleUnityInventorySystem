using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class UIItemSlot : MonoBehaviour, IPointerClickHandler, IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler
{
    public Image itemIcon;
    public TextMeshProUGUI amountText;
    public Image background;
    public Image highlightImage;
    public Image lockIcon;
    public int slotIndex;

    [SerializeField] private ItemSlot itemSlot;
    [SerializeField] private UIInventory uiInventory;
    
    // 添加公共访问器
    public ItemSlot ItemSlot => itemSlot;
    public UIInventory UIInventory => uiInventory;

    private static UIItemSlot draggedSlot;
    private static GameObject draggedIcon;
    private static Canvas canvas;

    private UIItemContextMenu contextMenu;

    private void Start()
    {
        canvas = GetComponentInParent<Canvas>();
        contextMenu = GameObject.FindFirstObjectByType<UIItemContextMenu>();
        itemIcon.enabled = false;
        amountText.enabled = false;
        if (highlightImage != null) highlightImage.enabled = false;
        if (lockIcon != null) lockIcon.enabled = false;
    }

    public void UpdateUI(ItemSlot slot)
    {
        itemSlot = slot;
        
        if (lockIcon != null)
        {
            lockIcon.enabled = slot.IsLocked;
        }

        if (highlightImage != null)
        {
            highlightImage.enabled = slot.IsHighlighted;
        }

        if (slot.IsEmpty())
        {
            itemIcon.enabled = false;
            amountText.enabled = false;
        }
        else
        {
            itemIcon.enabled = true;
            itemIcon.sprite = slot.item.Icon;
            
            if (background != null)
            {
                background.color = slot.item.GetRarityColor();
            }
            
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
        if (itemSlot.IsEmpty()) return;

        if (eventData.button == PointerEventData.InputButton.Left)
        {
            itemSlot.item.Use();
            uiInventory.OnItemUsed(slotIndex);
        }
        else if (eventData.button == PointerEventData.InputButton.Right)
        {
            ShowContextMenu(eventData.position);
        }
    }

    private void ShowContextMenu(Vector2 position)
    {
        if (contextMenu != null)
        {
            contextMenu.Show(this, position);
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (itemSlot.IsEmpty() || itemSlot.IsLocked) return;

        draggedSlot = this;
        draggedIcon = new GameObject("Dragged Icon");
        draggedIcon.transform.SetParent(canvas.transform);
        
        Image draggedImage = draggedIcon.AddComponent<Image>();
        draggedImage.sprite = itemIcon.sprite;
        draggedImage.raycastTarget = false;
        draggedImage.color = new Color(1, 1, 1, 0.8f);
        
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