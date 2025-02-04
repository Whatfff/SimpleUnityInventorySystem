using System.Linq;
using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class Item
{
    // 基础属性
    private int _id;
    private string _itemName;
    private string _description;
    private Sprite _icon;
    private float _price;
    private int _level;
    private string _tag;
    private int _maxStackSize = 1;
    
    public int Id => _id;
    public string ItemName => _itemName;
    public string Description => _description;
    public Sprite Icon => _icon;
    public float Price => _price;
    public int Level => _level;
    public string Tag => _tag;
    public int MaxStackSize => _maxStackSize;

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

    private ItemType _itemType;
    private ItemRarity _rarity;
    public ItemType Type => _itemType;
    public ItemRarity Rarity => _rarity;

    // 装备属性
    [System.Serializable]
    public class EquipmentStats
    {
        private float _hp;
        private float _mp;
        private float _attack;
        private float _defense;
        private float _speed;
        private float _critRate;
        private float _dodgeRate;
        
        public float HP { get => _hp; set => _hp = value; }
        public float MP { get => _mp; set => _mp = value; }
        public float Attack { get => _attack; set => _attack = value; }
        public float Defense { get => _defense; set => _defense = value; }
        public float Speed { get => _speed; set => _speed = value; }
        public float CritRate { get => _critRate; set => _critRate = value; }
        public float DodgeRate { get => _dodgeRate; set => _dodgeRate = value; }
    }

    public EquipmentStats stats = new EquipmentStats();

    // 耐久度系统
    private float _maxDurability = 100;
    private float _currentDurability;
    private bool _canBeBroken = true;
    
    public float MaxDurability => _maxDurability;
    public float CurrentDurability => _currentDurability;
    public bool CanBeBroken => _canBeBroken;

    // 强化系统
    private int _enhanceLevel = 0;
    public int EnhanceLevel => _enhanceLevel;
    public const int MAX_ENHANCE_LEVEL = 20;
    private float _enhanceSuccessRate = 100;
    private float _enhanceBreakRate = 0;
    public float EnhanceSuccessRate => _enhanceSuccessRate;
    public float EnhanceBreakRate => _enhanceBreakRate;

    // 附魔系统
    [System.Serializable]
    public class Enchantment
    {
        private string _name;
        private string _description;
        private Dictionary<string, float> _bonusStats = new Dictionary<string, float>();
        private int _level;
        private int _maxLevel = 5;
        
        public string Name { get => _name; set => _name = value; }
        public string Description { get => _description; set => _description = value; }
        public Dictionary<string, float> BonusStats { get => _bonusStats; set => _bonusStats = value; }
        public int Level { get => _level; set => _level = value; }
        public int MaxLevel { get => _maxLevel; set => _maxLevel = value; }

        public Enchantment(string name, string desc)
        {
            this._name = name;
            this._description = desc;
            this._level = 1;
        }
    }

    public List<Enchantment> enchantments = new List<Enchantment>();

    // 套装系统
    private string _setName;
    public string SetName => _setName;
    private static Dictionary<string, HashSet<Item>> equipmentSets = new Dictionary<string, HashSet<Item>>();
    public static IReadOnlyDictionary<string, HashSet<Item>> EquipmentSets => equipmentSets;
    
    [System.Serializable]
    public class SetBonus
    {
        private int _requiredPieces;
        private EquipmentStats _bonusStats = new EquipmentStats();
        private string _description;
        
        public int RequiredPieces => _requiredPieces;
        public EquipmentStats BonusStats => _bonusStats;
        public string Description => _description;
        
        public SetBonus(int pieces, string desc)
        {
            _requiredPieces = pieces;
            _description = desc;
        }
    }

    private List<SetBonus> _setBonuses = new List<SetBonus>();
    public IReadOnlyList<SetBonus> SetBonuses => _setBonuses;

    // 构造函数
    public Item(int id, string name, string desc, Sprite icon, ItemType type, ItemRarity rarity, int stackSize = 1)
    {
        this._id = id;
        this._itemName = name;
        this._description = desc;
        this._icon = icon;
        this._itemType = type;
        this._rarity = rarity;
        this._maxStackSize = stackSize;
        _currentDurability = _maxDurability;
        
        if (!string.IsNullOrEmpty(SetName))
        {
            if (!equipmentSets.ContainsKey(SetName))
            {
                equipmentSets[SetName] = new HashSet<Item>();
            }
            equipmentSets[SetName].Add(this);
        }
    }

    // 获取物品颜色（基于稀有度）
    public Color GetRarityColor()
    {
        switch (_rarity)
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

    public virtual void Use()
    {
        Debug.Log($"使用物品: {_itemName}");
        // 在这里实现具体的使用逻辑
    }

    // 强化系统方法
    public bool Enhance()
    {
        if (_enhanceLevel >= MAX_ENHANCE_LEVEL) return false;

        float random = Random.Range(0f, 100f);
        if (random <= _enhanceSuccessRate)
        {
            _enhanceLevel++;
            UpdateStatsForEnhancement();
            return true;
        }
        else if (random <= _enhanceSuccessRate + _enhanceBreakRate && _canBeBroken)
        {
            BreakItem();
            return false;
        }
        return false;
    }

    private void UpdateStatsForEnhancement()
    {
        float bonus = _enhanceLevel * 0.1f; // 每级增加10%基础属性
        stats.Attack *= (1 + bonus);
        stats.Defense *= (1 + bonus);
        stats.HP *= (1 + bonus);
        // ... 其他属性的增强
    }

    // 耐久度系统方法
    public void UpdateDurability(float amount)
    {
        _currentDurability = Mathf.Clamp(_currentDurability + amount, 0, _maxDurability);
        if (_currentDurability <= 0 && _canBeBroken)
        {
            BreakItem();
        }
    }

    public void RepairItem(float amount)
    {
        _currentDurability = Mathf.Min(_currentDurability + amount, _maxDurability);
    }

    private void BreakItem()
    {
        // 物品破损的处理逻辑
        Debug.Log($"物品 {_itemName} 已损坏！");
    }

    // 附魔系统方法
    public bool AddEnchantment(Enchantment enchantment)
    {
        if (enchantments.Count >= 3) return false; // 最多3个附魔
        
        var existing = enchantments.Find(e => e.Name == enchantment.Name);
        if (existing != null)
        {
            if (existing.Level >= existing.MaxLevel) return false;
            existing.Level++;
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
        if (string.IsNullOrEmpty(SetName)) return null;

        int equippedSetPieces = equippedItems?.Count(item => 
            item != null && item.SetName == this.SetName) ?? 0;

        EquipmentStats totalBonus = new EquipmentStats();
        if (equippedSetPieces == 0) return totalBonus;

        foreach (var setBonus in _setBonuses)
        {
            if (equippedSetPieces >= setBonus.RequiredPieces)
            {
                // 累加套装加成
                totalBonus.HP += setBonus.BonusStats.HP;
                totalBonus.MP += setBonus.BonusStats.MP;
                totalBonus.Attack += setBonus.BonusStats.Attack;
                totalBonus.Defense += setBonus.BonusStats.Defense;
                totalBonus.Speed += setBonus.BonusStats.Speed;
                totalBonus.CritRate += setBonus.BonusStats.CritRate;
                totalBonus.DodgeRate += setBonus.BonusStats.DodgeRate;
            }
        }

        return totalBonus;
    }

    // 重写获取详细描述方法
    public virtual string GetDetailedDescription()
    {
        string details = _description + "\n\n";
        
        // 添加基本属性
        if (_itemType == ItemType.Weapon || _itemType == ItemType.Armor)
        {
            details += $"等级要求: {_level}\n";
            if (stats.HP != 0) details += $"生命值: +{stats.HP}\n";
            if (stats.MP != 0) details += $"魔法值: +{stats.MP}\n";
            if (stats.Attack != 0) details += $"攻击力: +{stats.Attack}\n";
            if (stats.Defense != 0) details += $"防御力: +{stats.Defense}\n";
            if (stats.Speed != 0) details += $"速度: +{stats.Speed}\n";
            if (stats.CritRate != 0) details += $"暴击率: +{stats.CritRate}%\n";
            if (stats.DodgeRate != 0) details += $"闪避率: +{stats.DodgeRate}%\n";
        }

        // 添加强化等级
        if (_enhanceLevel > 0)
        {
            details += $"\n强化等级: +{_enhanceLevel}";
        }

        // 添加耐久度
        if (_maxDurability > 0)
        {
            details += $"\n耐久度: {_currentDurability}/{_maxDurability}";
        }

        // 添加附魔信息
        if (enchantments.Count > 0)
        {
            details += "\n\n附魔:";
            foreach (var enchant in enchantments)
            {
                details += $"\n{enchant.Name} Lv.{enchant.Level}";
                details += $"\n  {enchant.Description}";
            }
        }

        // 添加套装信息
        if (!string.IsNullOrEmpty(SetName))
        {
            details += $"\n\n套装: {SetName}";
            foreach (var bonus in _setBonuses)
            {
                details += $"\n{bonus.RequiredPieces}件套装效果:";
                details += $"\n  {bonus.Description}";
            }
        }

        // 添加价格和标签信息
        details += $"\n\n价格: {_price} 金币";
        if (!string.IsNullOrEmpty(_tag))
        {
            details += $"\n标签: {_tag}";
        }

        return details;
    }

    // 添加析构或Dispose方法来清理套装引用
    ~Item()
    {
        if (!string.IsNullOrEmpty(SetName) && equipmentSets.ContainsKey(SetName))
        {
            equipmentSets[SetName].Remove(this);
        }
    }

    // 添加Clone方法以支持物品复制
    public Item Clone()
    {
        var clone = new Item(_id, _itemName, _description, _icon, _itemType, _rarity, _maxStackSize);
        clone.stats = new EquipmentStats
        {
            HP = this.stats.HP,
            MP = this.stats.MP,
            Attack = this.stats.Attack,
            Defense = this.stats.Defense,
            Speed = this.stats.Speed,
            CritRate = this.stats.CritRate,
            DodgeRate = this.stats.DodgeRate
        };
        clone._currentDurability = this._currentDurability;
        clone._enhanceLevel = this._enhanceLevel;
        clone.enchantments = this.enchantments.Select(e => new Enchantment(e.Name, e.Description)
        {
            Level = e.Level,
            MaxLevel = e.MaxLevel,
            BonusStats = new Dictionary<string, float>(e.BonusStats)
        }).ToList();
        return clone;
    }

    // 添加属性验证
    private void ValidateStats()
    {
        stats.HP = Mathf.Max(0, stats.HP);
        stats.MP = Mathf.Max(0, stats.MP);
        stats.Attack = Mathf.Max(0, stats.Attack);
        stats.Defense = Mathf.Max(0, stats.Defense);
        stats.Speed = Mathf.Max(0, stats.Speed);
        stats.CritRate = Mathf.Clamp(stats.CritRate, 0, 100);
        stats.DodgeRate = Mathf.Clamp(stats.DodgeRate, 0, 100);
    }

    // 添加套装相关的方法
    public void AddSetBonus(int requiredPieces, string description)
    {
        var bonus = new SetBonus(requiredPieces, description);
        _setBonuses.Add(bonus);
    }

    public void SetSetName(string name)
    {
        if (_setName == name) return;
        
        // 从旧套装中移除
        if (!string.IsNullOrEmpty(_setName) && equipmentSets.ContainsKey(_setName))
        {
            equipmentSets[_setName].Remove(this);
        }
        
        _setName = name;
        
        // 添加到新套装
        if (!string.IsNullOrEmpty(_setName))
        {
            if (!equipmentSets.ContainsKey(_setName))
            {
                equipmentSets[_setName] = new HashSet<Item>();
            }
            equipmentSets[_setName].Add(this);
        }
    }
} 