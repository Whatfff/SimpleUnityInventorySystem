using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "ItemDatabase", menuName = "Inventory/Item Database")]
public class ItemDatabase : ScriptableObject
{
    public List<Item> items = new List<Item>();

    public Item GetItemById(int id)
    {
        return items.Find(item => item.Id == id);
    }

    public List<Item> GetItemsByType(Item.ItemType type)
    {
        return items.FindAll(item => item.Type == type);
    }

    public List<Item> GetItemsByRarity(Item.ItemRarity rarity)
    {
        return items.FindAll(item => item.Rarity == rarity);
    }
} 