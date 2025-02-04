using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class Item
{
    // 基础属性
    public int id;
    public string itemName;
    public string description;
    public Sprite icon;
    public float price;
    public int level;
    public string tag;
    public int maxStackSize = 1;

    // 物品类型和稀有度
    public enum ItemType
    {
        Weapon,
        Armor,
        Consumable,
        Material,
        Quest,
        Accessory
    }

    public enum ItemRarity
    {
        Common,
        Uncommon,
        Rare,
        Epic,
        Legendary
    }

    public ItemType itemType;
    public ItemRarity rarity;

    // 装备属性
    [System.Serializable]
    public class EquipmentStats
    {
        public float hp;
        public float mp;
        public float attack;
        public float defense;
        public float speed;
        public float critRate;
        public float dodgeRate;
    }

    public EquipmentStats stats = new EquipmentStats();

    // 耐久度系统
    public float maxDurability = 100;
    public float currentDurability;
    public bool canBeBroken = true;

    // 强化系统
    public int enhanceLevel = 0;
    public const int MAX_ENHANCE_LEVEL = 20;
    public float enhanceSuccessRate = 100;
    public float enhanceBreakRate = 0;

    // 附魔系统
    [System.Serializable]
    public class Enchantment
    {
        public string name;
        public string description;
        public Dictionary<string, float> bonusStats = new Dictionary<string, float>();
        public int level;
        public int maxLevel = 5;

        public Enchantment(string name, string desc)
        {
            this.name = name;
            this.description = desc;
            this.level = 1;
        }
    }

    public List<Enchantment> enchantments = new List<Enchantment>();

    // 套装系统
    public string setName;
    public static Dictionary<string, HashSet<Item>> equipmentSets = new Dictionary<string, HashSet<Item>>();
    
    [System.Serializable]
    public class SetBonus
    {
        public int requiredPieces;
        public EquipmentStats bonusStats = new EquipmentStats();
        public string description;
    }

    public List<SetBonus> setBonuses = new List<SetBonus>();

    // 构造函数
    public Item(int id, string name, string desc, Sprite icon, ItemType type, ItemRarity rarity, int stackSize = 1)
    {
        this.id = id;
        this.itemName = name;
        this.description = desc;
        this.icon = icon;
        this.itemType = type;
        this.rarity = rarity;
        this.maxStackSize = stackSize;
        currentDurability = maxDurability;
        
        if (!string.IsNullOrEmpty(setName) && !equipmentSets.ContainsKey(setName))
        {
            equipmentSets[setName] = new HashSet<Item>();
        }
    }

    // 获取物品颜色（基于稀有度）
    public Color GetRarityColor()
    {
        switch (rarity)
        {
            case ItemRarity.Common:
                return Color.white;
            case ItemRarity.Uncommon:
                return Color.green;
            case ItemRarity.Rare:
                return Color.blue;
            case ItemRarity.Epic:
                return new Color(0.5f, 0, 0.5f); // 紫色
            case ItemRarity.Legendary:
                return Color.yellow;
            default:
                return Color.white;
        }
    }

    // 获取物品详细描述
    public virtual string GetDetailedDescription()
    {
        string details = description + "\n\n";
        
        if (itemType == ItemType.Weapon || itemType == ItemType.Armor)
        {
            details += $"等级要求: {level}\n";
            if (stats.hp != 0) details += $"生命值: +{stats.hp}\n";
            if (stats.mp != 0) details += $"魔法值: +{stats.mp}\n";
            if (stats.attack != 0) details += $"攻击力: +{stats.attack}\n";
            if (stats.defense != 0) details += $"防御力: +{stats.defense}\n";
            if (stats.speed != 0) details += $"速度: +{stats.speed}\n";
            if (stats.critRate != 0) details += $"暴击率: +{stats.critRate}%\n";
            if (stats.dodgeRate != 0) details += $"闪避率: +{stats.dodgeRate}%\n";
        }

        details += $"\n价格: {price} 金币";
        if (!string.IsNullOrEmpty(tag))
        {
            details += $"\n标签: {tag}";
        }

        return details;
    }

    public virtual void Use()
    {
        Debug.Log($"使用物品: {itemName}");
        // 在这里实现具体的使用逻辑
    }

    // 强化系统方法
    public bool Enhance()
    {
        if (enhanceLevel >= MAX_ENHANCE_LEVEL) return false;

        float random = Random.Range(0f, 100f);
        if (random <= enhanceSuccessRate)
        {
            enhanceLevel++;
            UpdateStatsForEnhancement();
            return true;
        }
        else if (random <= enhanceSuccessRate + enhanceBreakRate && canBeBroken)
        {
            BreakItem();
            return false;
        }
        return false;
    }

    private void UpdateStatsForEnhancement()
    {
        float bonus = enhanceLevel * 0.1f; // 每级增加10%基础属性
        stats.attack *= (1 + bonus);
        stats.defense *= (1 + bonus);
        stats.hp *= (1 + bonus);
        // ... 其他属性的增强
    }

    // 耐久度系统方法
    public void UpdateDurability(float amount)
    {
        currentDurability = Mathf.Clamp(currentDurability + amount, 0, maxDurability);
        if (currentDurability <= 0 && canBeBroken)
        {
            BreakItem();
        }
    }

    public void RepairItem(float amount)
    {
        currentDurability = Mathf.Min(currentDurability + amount, maxDurability);
    }

    private void BreakItem()
    {
        // 物品破损的处理逻辑
        Debug.Log($"物品 {itemName} 已损坏！");
    }

    // 附魔系统方法
    public bool AddEnchantment(Enchantment enchantment)
    {
        if (enchantments.Count >= 3) return false; // 最多3个附魔
        
        var existing = enchantments.Find(e => e.name == enchantment.name);
        if (existing != null)
        {
            if (existing.level >= existing.maxLevel) return false;
            existing.level++;
        }
        else
        {
            enchantments.Add(enchantment);
        }
        return true;
    }

    // 套装效果计算
    public EquipmentStats GetSetBonuses(List<Item> equippedItems)
    {
        if (string.IsNullOrEmpty(setName)) return null;

        int equippedSetPieces = equippedItems.Count(item => item.setName == this.setName);
        EquipmentStats totalBonus = new EquipmentStats();

        foreach (var setBonus in setBonuses)
        {
            if (equippedSetPieces >= setBonus.requiredPieces)
            {
                // 累加套装加成
                totalBonus.hp += setBonus.bonusStats.hp;
                totalBonus.mp += setBonus.bonusStats.mp;
                totalBonus.attack += setBonus.bonusStats.attack;
                // ... 其他属性的套装加成
            }
        }

        return totalBonus;
    }

    // 重写获取详细描述方法
    public override string GetDetailedDescription()
    {
        string details = base.GetDetailedDescription() + "\n";

        // 添加强化等级
        if (enhanceLevel > 0)
        {
            details += $"\n强化等级: +{enhanceLevel}";
        }

        // 添加耐久度
        if (maxDurability > 0)
        {
            details += $"\n耐久度: {currentDurability}/{maxDurability}";
        }

        // 添加附魔信息
        if (enchantments.Count > 0)
        {
            details += "\n\n附魔:";
            foreach (var enchant in enchantments)
            {
                details += $"\n{enchant.name} Lv.{enchant.level}";
                details += $"\n  {enchant.description}";
            }
        }

        // 添加套装信息
        if (!string.IsNullOrEmpty(setName))
        {
            details += $"\n\n套装: {setName}";
            foreach (var bonus in setBonuses)
            {
                details += $"\n{bonus.requiredPieces}件套装效果:";
                details += $"\n  {bonus.description}";
            }
        }

        return details;
    }
} 