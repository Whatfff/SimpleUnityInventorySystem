using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class UIItemContextMenu : MonoBehaviour
{
    public GameObject menuPanel;
    public GameObject buttonPrefab;
    public Transform buttonContainer;

    private UIItemSlot currentSlot;
    private List<GameObject> currentButtons = new List<GameObject>();

    private void Awake()
    {
        menuPanel.SetActive(false);
    }

    public void Show(UIItemSlot slot, Vector2 position)
    {
        if (slot.ItemSlot.IsEmpty()) return;

        currentSlot = slot;
        ClearButtons();
        CreateButtons(slot);

        menuPanel.SetActive(true);
        transform.position = position;
    }

    private void CreateButtons(UIItemSlot slot)
    {
        // 使用按钮
        AddButton("使用", () => 
        {
            slot.ItemSlot.item.Use();
            slot.UIInventory.OnItemUsed(slot.slotIndex);
            Hide();
        });

        // 丢弃按钮
        AddButton("丢弃", () => 
        {
            slot.UIInventory.inventory.RemoveItem(slot.slotIndex);
            slot.UIInventory.UpdateUI();
            Hide();
        });

        // 如果是装备，添加装备相关选项
        if (slot.ItemSlot.item.Type == Item.ItemType.Weapon || 
            slot.ItemSlot.item.Type == Item.ItemType.Armor)
        {
            AddButton("强化", () => 
            {
                // 实现强化逻辑
                Hide();
            });

            AddButton("附魔", () => 
            {
                // 实现附魔逻辑
                Hide();
            });
        }
    }

    private void AddButton(string text, UnityEngine.Events.UnityAction action)
    {
        GameObject buttonObj = Instantiate(buttonPrefab, buttonContainer);
        Button button = buttonObj.GetComponent<Button>();
        button.GetComponentInChildren<TMPro.TextMeshProUGUI>().text = text;
        button.onClick.AddListener(action);
        currentButtons.Add(buttonObj);
    }

    private void ClearButtons()
    {
        foreach (var button in currentButtons)
        {
            Destroy(button);
        }
        currentButtons.Clear();
    }

    public void Hide()
    {
        menuPanel.SetActive(false);
        currentSlot = null;
    }

    private void Update()
    {
        // 点击其他地方关闭菜单
        if (Input.GetMouseButtonDown(0) && menuPanel.activeSelf)
        {
            Hide();
        }
    }
} 