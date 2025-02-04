using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ItemTooltip : MonoBehaviour
{
    public TextMeshProUGUI itemNameText;
    public TextMeshProUGUI itemTypeText;
    public TextMeshProUGUI rarityText;
    public TextMeshProUGUI descriptionText;
    public TextMeshProUGUI statsText;
    public Image background;
    
    private static ItemTooltip instance;
    
    private void Awake()
    {
        instance = this;
        gameObject.SetActive(false);
    }

    public static void Show(Item item, Vector2 position)
    {
        instance.itemNameText.text = item.ItemName;
        instance.itemTypeText.text = $"[{item.Type}] Level {item.Level}";
        instance.rarityText.text = item.Rarity.ToString();
        instance.itemNameText.color = item.GetRarityColor();
        instance.descriptionText.text = item.GetDetailedDescription();
        
        instance.gameObject.SetActive(true);
        instance.transform.position = position;
    }

    public static void Hide()
    {
        instance.gameObject.SetActive(false);
    }
} 