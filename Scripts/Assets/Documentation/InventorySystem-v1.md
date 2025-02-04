# Unity高级背包系统完整指南

## 目录
1. [系统架构概述](#1-系统架构概述)
2. [基础物品系统](#2-基础物品系统)
3. [装备属性系统](#3-装备属性系统)
4. [强化系统](#4-强化系统)
5. [耐久度系统](#5-耐久度系统)
6. [附魔系统](#6-附魔系统)
7. [套装系统](#7-套装系统)
8. [UI系统](#8-ui系统)
9. [实战示例](#9-实战示例)

## 1. 系统架构概述

### 1.1 系统结构图
本系统采用模块化设计，主要包含以下组件：
- 基础物品管理（物品属性、类型、稀有度）
- 高级特性（强化、耐久度、附魔、套装）
- UI系统（物品槽、拖拽系统、提示框）

### 1.2 系统特点
- **模块化设计**：每个子系统都是独立的，可以根据需求选择性使用
- **高扩展性**：预留了充足的扩展接口，方便添加新功能
- **完整文档**：提供详细的API文档和使用示例
- **易于使用**：提供直观的接口和完整的事件系统

## 2. 基础物品系统

### 2.1 物品基类实现
物品基类(Item)是整个系统的核心，定义了物品的基本属性和行为：

```csharp
public class Item
{
    // 基础属性
    public int id;             // 物品唯一标识符
    public string itemName;    // 物品名称
    public string description; // 物品描述
    public Sprite icon;        // 物品图标
    public float price;        // 物品价格
    public int level;          // 物品等级要求
    public string tag;         // 物品标签（用于分类和筛选）
    public int maxStackSize;   // 最大堆叠数量

    // 物品类型和稀有度
    public enum ItemType
    {
        Weapon,     // 武器
        Armor,      // 护甲
        Consumable, // 消耗品
        Material,   // 材料
        Quest,      // 任务物品
        Accessory   // 饰品
    }

    public enum ItemRarity
    {
        Common,    // 普通（白色）
        Uncommon,  // 优秀（绿色）
        Rare,      // 稀有（蓝色）
        Epic,      // 史诗（紫色）
        Legendary  // 传说（金色）
    }

    public ItemType itemType;
    public ItemRarity rarity;

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
    }
}
```

### 2.2 物品槽实现
物品槽(ItemSlot)用于管理背包中的每个格子：

```csharp
[System.Serializable]
public class ItemSlot
{
    public Item item;
    public int amount;

    public ItemSlot()
    {
        item = null;
        amount = 0;
    }

    public bool IsEmpty()
    {
        return item == null;
    }

    public bool CanAddItem(Item newItem, int quantity)
    {
        if (IsEmpty()) return true;
        if (item.id == newItem.id && amount + quantity <= item.maxStackSize) return true;
        return false;
    }
}
```

### 2.3 背包系统实现
背包系统(Inventory)管理所有物品槽：

```csharp
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
}
```

## 3. 装备属性系统

### 3.1 属性类型详解
装备属性系统定义了装备的各种属性和效果：

```csharp
[System.Serializable]
public class EquipmentStats
{
    // 基础属性
    public float hp;        // 生命值
    public float mp;        // 魔法值
    public float attack;    // 攻击力
    public float defense;   // 防御力
    public float speed;     // 速度
    
    // 特殊属性
    public float critRate;  // 暴击率
    public float critDamage; // 暴击伤害
    public float dodgeRate; // 闪避率
    
    // 属性合并方法
    public EquipmentStats Add(EquipmentStats other)
    {
        EquipmentStats result = new EquipmentStats();
        result.hp = this.hp + other.hp;
        result.mp = this.mp + other.mp;
        result.attack = this.attack + other.attack;
        result.defense = this.defense + other.defense;
        result.speed = this.speed + other.speed;
        result.critRate = this.critRate + other.critRate;
        result.critDamage = this.critDamage + other.critDamage;
        result.dodgeRate = this.dodgeRate + other.dodgeRate;
        return result;
    }
}
```

### 3.2 属性计算系统

```csharp
public class StatsCalculator
{
    // 计算最终属性
    public static EquipmentStats CalculateFinalStats(
        EquipmentStats baseStats,      // 基础属性
        EquipmentStats additiveStats,  // 附加属性
        float multiplier)              // 百分比加成
    {
        // 先进行属性叠加
        EquipmentStats combined = baseStats.Add(additiveStats);
        
        // 再进行百分比加成
        combined.hp *= (1 + multiplier);
        combined.mp *= (1 + multiplier);
        combined.attack *= (1 + multiplier);
        combined.defense *= (1 + multiplier);
        combined.speed *= (1 + multiplier);
        
        return combined;
    }

    // 计算暴击伤害
    public static float CalculateCriticalDamage(float baseDamage, float critMultiplier)
    {
        return baseDamage * (1 + critMultiplier);
    }

    // 计算实际伤害
    public static float CalculateActualDamage(float attack, float defense)
    {
        return Mathf.Max(0, attack - defense * 0.5f);
    }
}
```

### 3.3 属性修饰器系统
用于实现各种属性加成效果：

```csharp
// 属性修饰器基类
public abstract class StatModifier
{
    public float Value { get; protected set; }
    public StatModifierType Type { get; protected set; }
    public int Order { get; protected set; }
    public object Source { get; protected set; }

    protected StatModifier(float value, StatModifierType type, int order, object source)
    {
        Value = value;
        Type = type;
        Order = order;
        Source = source;
    }
}

// 修饰器类型
public enum StatModifierType
{
    Flat = 100,           // 固定值加成
    PercentAdd = 200,     // 百分比加法
    PercentMult = 300     // 百分比乘法
}

// 属性修饰器管理器
public class StatModifierManager
{
    private List<StatModifier> modifiers = new List<StatModifier>();
    private bool isDirty = true;
    private float lastValue;
    private float baseValue;

    // 添加修饰器
    public void AddModifier(StatModifier mod)
    {
        isDirty = true;
        modifiers.Add(mod);
        modifiers.Sort((a, b) => a.Order.CompareTo(b.Order));
    }

    // 计算最终值
    public float GetValue()
    {
        if (!isDirty) return lastValue;

        lastValue = baseValue;
        float sumPercentAdd = 0;

        for (int i = 0; i < modifiers.Count; i++)
        {
            StatModifier mod = modifiers[i];
            if (mod.Type == StatModifierType.Flat)
            {
                lastValue += mod.Value;
            }
            else if (mod.Type == StatModifierType.PercentAdd)
            {
                sumPercentAdd += mod.Value;
            }
            else if (mod.Type == StatModifierType.PercentMult)
            {
                lastValue *= (1 + mod.Value);
            }
        }

        // 应用累积的百分比加成
        lastValue *= (1 + sumPercentAdd);
        isDirty = false;
        return lastValue;
    }
}
```

## 4. 强化系统

### 4.1 强化系统核心实现
强化系统用于提升装备的基础属性：

```csharp
public class EnhanceSystem
{
    // 强化配置
    [System.Serializable]
    public class EnhanceConfig
    {
        public int level;              // 强化等级
        public float successRate;      // 成功率
        public float breakRate;        // 破损率
        public float statMultiplier;   // 属性提升倍率
        public int materialCount;      // 所需材料数量
        public int goldCost;           // 所需金币
    }

    // 强化结果
    public class EnhanceResult
    {
        public bool success;           // 是否成功
        public bool broken;            // 是否破损
        public string message;         // 结果信息
        public EquipmentStats oldStats;  // 原始属性
        public EquipmentStats newStats;  // 新属性
    }

    private const int MAX_ENHANCE_LEVEL = 20;
    private List<EnhanceConfig> enhanceConfigs;

    // 初始化配置
    public void Initialize()
    {
        enhanceConfigs = new List<EnhanceConfig>();
        // 添加各等级的强化配置
        for (int i = 0; i <= MAX_ENHANCE_LEVEL; i++)
        {
            enhanceConfigs.Add(new EnhanceConfig
            {
                level = i,
                successRate = 100 - i * 5,     // 每级降低5%成功率
                breakRate = i > 15 ? (i - 15) * 5 : 0,  // 15级后有破损率
                statMultiplier = 1 + (i * 0.1f),  // 每级提升10%属性
                materialCount = i + 1,
                goldCost = 1000 * (int)Mathf.Pow(2, i)
            });
        }
    }

    // 执行强化
    public EnhanceResult Enhance(Item item, List<Item> materials, int gold)
    {
        EnhanceResult result = new EnhanceResult();
        
        // 检查前置条件
        if (!CanEnhance(item, materials, gold, out string errorMessage))
        {
            result.success = false;
            result.message = errorMessage;
            return result;
        }

        // 获取当前等级的配置
        EnhanceConfig config = enhanceConfigs[item.enhanceLevel];
        
        // 保存原始属性
        result.oldStats = item.stats;

        // 计算强化结果
        float random = Random.Range(0f, 100f);
        if (random <= config.successRate)
        {
            // 强化成功
            item.enhanceLevel++;
            UpdateItemStats(item, config.statMultiplier);
            result.success = true;
            result.message = $"强化成功！{item.itemName}已升级到+{item.enhanceLevel}";
        }
        else if (random <= config.successRate + config.breakRate)
        {
            // 强化失败且物品破损
            BreakItem(item);
            result.success = false;
            result.broken = true;
            result.message = $"强化失败！{item.itemName}已损坏！";
        }
        else
        {
            // 强化失败但物品安全
            result.success = false;
            result.message = $"强化失败！但{item.itemName}安全！";
        }

        // 保存新属性
        result.newStats = item.stats;

        // 消耗材料和金币
        ConsumeMaterials(materials);
        ConsumeGold(gold);

        return result;
    }

    // 更新物品属性
    private void UpdateItemStats(Item item, float multiplier)
    {
        item.stats.attack *= multiplier;
        item.stats.defense *= multiplier;
        item.stats.hp *= multiplier;
        item.stats.mp *= multiplier;
        item.stats.speed *= multiplier;
    }

    // 物品破损处理
    private void BreakItem(Item item)
    {
        item.broken = true;
        item.currentDurability = 0;
        // 可以添加额外的破损效果
    }
}
```

### 4.2 强化UI实现

```csharp
public class EnhanceUI : MonoBehaviour
{
    [SerializeField] private Image targetItemSlot;
    [SerializeField] private Image[] materialSlots;
    [SerializeField] private Text successRateText;
    [SerializeField] private Text costText;
    [SerializeField] private Button enhanceButton;
    [SerializeField] private Animator enhanceAnimator;
    [SerializeField] private ParticleSystem successEffect;
    [SerializeField] private ParticleSystem failEffect;

    private EnhanceSystem enhanceSystem;
    private Item targetItem;
    private List<Item> materials = new List<Item>();

    // 更新UI显示
    public void UpdateUI()
    {
        if (targetItem != null)
        {
            var config = enhanceSystem.GetEnhanceConfig(targetItem.enhanceLevel);
            
            // 更新成功率显示
            successRateText.text = $"成功率: {config.successRate}%\n" +
                                 $"破损率: {config.breakRate}%";

            // 更新消耗显示
            costText.text = $"需要金币: {config.goldCost}\n" +
                          $"需要材料: {config.materialCount}个";

            // 更新按钮状态
            enhanceButton.interactable = CanEnhance();
        }
    }

    // 执行强化
    public void OnEnhanceButtonClick()
    {
        if (!CanEnhance()) return;

        StartCoroutine(EnhanceCoroutine());
    }

    private IEnumerator EnhanceCoroutine()
    {
        // 播放强化动画
        enhanceAnimator.SetTrigger("StartEnhance");
        yield return new WaitForSeconds(1.5f);

        // 执行强化
        EnhanceResult result = enhanceSystem.Enhance(targetItem, materials, currentGold);

        // 显示结果
        ShowEnhanceResult(result);

        // 播放特效
        PlayEnhanceEffect(result.success);

        // 更新UI
        UpdateUI();
    }
}
```

## 5. 耐久度系统

### 5.1 耐久度系统核心实现
耐久度系统用于管理装备的使用寿命和状态：

```csharp
public class DurabilitySystem
{
    // 耐久度状态枚举
    public enum DurabilityState
    {
        Perfect,     // 完好 (100% ~ 80%)
        Good,        // 良好 (79% ~ 50%)
        Damaged,     // 受损 (49% ~ 20%)
        Critical,    // 危险 (19% ~ 1%)
        Broken       // 损坏 (0%)
    }

    // 耐久度属性
    public float maxDurability = 100;
    public float currentDurability;
    public bool canBeBroken = true;

    // 属性衰减配置
    private static readonly Dictionary<DurabilityState, float> statMultipliers = new Dictionary<DurabilityState, float>
    {
        { DurabilityState.Perfect, 1.0f },
        { DurabilityState.Good, 0.9f },
        { DurabilityState.Damaged, 0.7f },
        { DurabilityState.Critical, 0.4f },
        { DurabilityState.Broken, 0.0f }
    };

    // 获取当前耐久度状态
    public DurabilityState GetDurabilityState()
    {
        float percentage = currentDurability / maxDurability;
        if (percentage <= 0) return DurabilityState.Broken;
        if (percentage < 0.2f) return DurabilityState.Critical;
        if (percentage < 0.5f) return DurabilityState.Damaged;
        if (percentage < 0.8f) return DurabilityState.Good;
        return DurabilityState.Perfect;
    }

    // 更新耐久度
    public void UpdateDurability(float amount)
    {
        DurabilityState oldState = GetDurabilityState();
        currentDurability = Mathf.Clamp(currentDurability + amount, 0, maxDurability);
        DurabilityState newState = GetDurabilityState();

        if (oldState != newState)
        {
            OnDurabilityStateChanged?.Invoke(oldState, newState);
            UpdateItemStats();
        }
    }

    // 计算战斗消耗
    public void ApplyCombatDurabilityLoss(CombatAction action)
    {
        float loss = 0;
        switch (action)
        {
            case CombatAction.Attack:
                loss = -1f;
                break;
            case CombatAction.Defend:
                loss = -0.5f;
                break;
            case CombatAction.SpecialAttack:
                loss = -2f;
                break;
        }
        UpdateDurability(loss);
    }

    // 修理系统
    public bool RepairItem(float amount, int goldCost)
    {
        if (currentDurability >= maxDurability) return false;
        if (!PlayerGold.Instance.HasEnough(goldCost)) return false;

        PlayerGold.Instance.Consume(goldCost);
        UpdateDurability(amount);
        return true;
    }

    // 更新物品属性
    private void UpdateItemStats()
    {
        DurabilityState state = GetDurabilityState();
        float multiplier = statMultipliers[state];
        
        // 更新物品的所有属性
        item.stats.attack *= multiplier;
        item.stats.defense *= multiplier;
        item.stats.hp *= multiplier;
        item.stats.mp *= multiplier;
        item.stats.speed *= multiplier;
    }
}
```

### 5.2 耐久度UI实现

```csharp
public class DurabilityUI : MonoBehaviour
{
    [SerializeField] private Image durabilityBar;
    [SerializeField] private Text durabilityText;
    [SerializeField] private Image itemIcon;
    [SerializeField] private GameObject warningIcon;
    [SerializeField] private GameObject brokenIcon;

    // 更新UI显示
    public void UpdateDurabilityUI(Item item)
    {
        if (item == null || item.maxDurability <= 0)
        {
            gameObject.SetActive(false);
            return;
        }

        gameObject.SetActive(true);
        float percentage = item.currentDurability / item.maxDurability;
        
        // 更新进度条
        durabilityBar.fillAmount = percentage;
        durabilityBar.color = GetDurabilityColor(percentage);

        // 更新文本
        durabilityText.text = $"{Mathf.Round(item.currentDurability)}/{item.maxDurability}";

        // 更新警告图标
        warningIcon.SetActive(percentage < 0.2f && percentage > 0);
        brokenIcon.SetActive(percentage <= 0);

        // 更新物品图标状态
        UpdateItemIconState(percentage);
    }

    // 获取耐久度颜色
    private Color GetDurabilityColor(float percentage)
    {
        if (percentage > 0.8f) return Color.green;
        if (percentage > 0.5f) return Color.yellow;
        if (percentage > 0.2f) return new Color(1f, 0.5f, 0f); // 橙色
        return Color.red;
    }

    // 更新物品图标状态
    private void UpdateItemIconState(float percentage)
    {
        Color iconColor = itemIcon.color;
        iconColor.a = percentage <= 0 ? 0.5f : 1f;
        itemIcon.color = iconColor;
    }
}
```

## 6. 附魔系统

### 6.1 附魔系统核心实现
附魔系统用于为装备添加特殊效果：

```csharp
public class EnchantmentSystem
{
    // 附魔基础类
    [System.Serializable]
    public class Enchantment
    {
        public string name;                // 附魔名称
        public string description;         // 附魔描述
        public int level;                  // 当前等级
        public int maxLevel = 5;           // 最大等级
        public EnchantmentType type;       // 附魔类型
        public Dictionary<string, float> bonusStats; // 属性加成

        public Enchantment(string name, string desc)
        {
            this.name = name;
            this.description = desc;
            this.level = 1;
            this.bonusStats = new Dictionary<string, float>();
        }

        // 升级附魔
        public bool Upgrade()
        {
            if (level >= maxLevel) return false;
            level++;
            UpdateBonusStats();
            return true;
        }
    }

    // 附魔类型
    public enum EnchantmentType
    {
        ElementalDamage,    // 元素伤害
        StatBoost,          // 属性提升
        SpecialEffect       // 特殊效果
    }

    // 元素类型
    public enum ElementType
    {
        Fire,       // 火焰
        Ice,        // 冰霜
        Lightning,  // 闪电
        Poison      // 毒素
    }

    // 元素附魔实现
    public class ElementalEnchantment : Enchantment
    {
        public ElementType elementType;

        public ElementalEnchantment(string name, string desc, ElementType type) 
            : base(name, desc)
        {
            this.elementType = type;
            this.type = EnchantmentType.ElementalDamage;
            InitializeElementalBonus();
        }

        private void InitializeElementalBonus()
        {
            switch (elementType)
            {
                case ElementType.Fire:
                    bonusStats.Add("fireDamage", 10f);
                    break;
                case ElementType.Ice:
                    bonusStats.Add("iceDamage", 8f);
                    bonusStats.Add("slowEffect", 5f);
                    break;
                case ElementType.Lightning:
                    bonusStats.Add("lightningDamage", 12f);
                    bonusStats.Add("attackSpeed", 5f);
                    break;
                case ElementType.Poison:
                    bonusStats.Add("poisonDamage", 6f);
                    bonusStats.Add("dotDuration", 3f);
                    break;
            }
        }
    }

    // 附魔管理器
    public class EnchantmentManager
    {
        private List<Enchantment> enchantments = new List<Enchantment>();
        private const int MAX_ENCHANTMENTS = 3;

        // 添加附魔
        public bool AddEnchantment(Enchantment enchantment)
        {
            if (enchantments.Count >= MAX_ENCHANTMENTS)
                return false;

            var existing = enchantments.Find(e => e.name == enchantment.name);
            if (existing != null)
            {
                return existing.Upgrade();
            }

            enchantments.Add(enchantment);
            return true;
        }

        // 计算附魔效果
        public Dictionary<string, float> CalculateEnchantmentEffects()
        {
            Dictionary<string, float> totalEffects = new Dictionary<string, float>();

            foreach (var enchant in enchantments)
            {
                foreach (var bonus in enchant.bonusStats)
                {
                    if (totalEffects.ContainsKey(bonus.Key))
                        totalEffects[bonus.Key] += bonus.Value * enchant.level;
                    else
                        totalEffects[bonus.Key] = bonus.Value * enchant.level;
                }
            }

            return totalEffects;
        }
    }
}
```

### 6.2 附魔UI实现

```csharp
public class EnchantmentUI : MonoBehaviour
{
    [SerializeField] private GameObject enchantmentSlotPrefab;
    [SerializeField] private Transform enchantmentContainer;
    [SerializeField] private Button addEnchantmentButton;
    [SerializeField] private GameObject enchantmentPanel;

    private List<EnchantmentSlotUI> enchantmentSlots = new List<EnchantmentSlotUI>();
    private Item currentItem;

    // 初始化UI
    public void Initialize(Item item)
    {
        currentItem = item;
        UpdateEnchantmentSlots();
        UpdateAddButton();
    }

    // 更新附魔槽位
    private void UpdateEnchantmentSlots()
    {
        // 清理现有槽位
        foreach (var slot in enchantmentSlots)
        {
            Destroy(slot.gameObject);
        }
        enchantmentSlots.Clear();

        // 创建新槽位
        foreach (var enchantment in currentItem.enchantments)
        {
            CreateEnchantmentSlot(enchantment);
        }
    }

    // 创建附魔槽位
    private void CreateEnchantmentSlot(Enchantment enchantment)
    {
        GameObject slotObj = Instantiate(enchantmentSlotPrefab, enchantmentContainer);
        EnchantmentSlotUI slotUI = slotObj.GetComponent<EnchantmentSlotUI>();
        slotUI.Initialize(enchantment);
        enchantmentSlots.Add(slotUI);
    }

    // 更新添加按钮状态
    private void UpdateAddButton()
    {
        addEnchantmentButton.interactable = 
            currentItem != null && 
            currentItem.enchantments.Count < EnchantmentManager.MAX_ENCHANTMENTS;
    }
}
```

## 7. 套装系统

### 7.1 套装系统核心实现
套装系统用于管理装备的套装效果：

```csharp
public class SetSystem
{
    // 套装数据结构
    [System.Serializable]
    public class SetData
    {
        public string setName;           // 套装名称
        public string description;       // 套装描述
        public List<SetBonus> bonuses;   // 套装效果列表
        public List<int> itemIds;        // 套装包含的物品ID
    }

    // 套装效果数据结构
    [System.Serializable]
    public class SetBonus
    {
        public int requiredPieces;       // 激活所需件数
        public string description;       // 效果描述
        public EquipmentStats bonusStats; // 属性加成
        public SpecialEffect specialEffect; // 特殊效果
    }

    // 套装管理器
    public class SetManager
    {
        private Dictionary<string, SetData> setDatabase = new Dictionary<string, SetData>();
        private Dictionary<string, HashSet<Item>> equippedSets = new Dictionary<string, HashSet<Item>>();

        // 初始化套装数据
        public void Initialize(List<SetData> setDataList)
        {
            foreach (var setData in setDataList)
            {
                setDatabase[setData.setName] = setData;
                equippedSets[setData.setName] = new HashSet<Item>();
            }
        }

        // 装备物品时检查套装
        public void OnItemEquipped(Item item)
        {
            if (string.IsNullOrEmpty(item.setName)) return;

            if (!equippedSets.ContainsKey(item.setName))
            {
                equippedSets[item.setName] = new HashSet<Item>();
            }
            equippedSets[item.setName].Add(item);

            // 检查并激活套装效果
            CheckSetBonuses(item.setName);
        }

        // 卸下物品时检查套装
        public void OnItemUnequipped(Item item)
        {
            if (string.IsNullOrEmpty(item.setName)) return;

            if (equippedSets.ContainsKey(item.setName))
            {
                equippedSets[item.setName].Remove(item);
                // 重新检查套装效果
                CheckSetBonuses(item.setName);
            }
        }

        // 检查套装效果
        private void CheckSetBonuses(string setName)
        {
            if (!setDatabase.ContainsKey(setName)) return;

            SetData setData = setDatabase[setName];
            int equippedCount = equippedSets[setName].Count;

            foreach (var bonus in setData.bonuses)
            {
                if (equippedCount >= bonus.requiredPieces)
                {
                    // 激活套装效果
                    ApplySetBonus(bonus);
                }
                else
                {
                    // 移除套装效果
                    RemoveSetBonus(bonus);
                }
            }
        }

        // 应用套装效果
        private void ApplySetBonus(SetBonus bonus)
        {
            // 应用属性加成
            if (bonus.bonusStats != null)
            {
                PlayerStats.Instance.AddBonusStats(bonus.bonusStats);
            }

            // 应用特殊效果
            if (bonus.specialEffect != null)
            {
                bonus.specialEffect.Apply();
            }

            // 触发套装效果激活事件
            OnSetBonusActivated?.Invoke(bonus);
        }
    }

    // 特殊效果系统
    public abstract class SpecialEffect
    {
        public abstract void Apply();
        public abstract void Remove();
    }
}
```

### 7.2 套装UI实现

```csharp
public class SetUI : MonoBehaviour
{
    [SerializeField] private GameObject setBonusEntryPrefab;
    [SerializeField] private Transform setBonusContainer;
    [SerializeField] private Image setIcon;
    [SerializeField] private Text setNameText;
    [SerializeField] private Text setPiecesText;

    private SetManager setManager;
    private Dictionary<string, SetBonusEntry> bonusEntries = new Dictionary<string, SetBonusEntry>();

    // 初始化UI
    public void Initialize(SetManager manager)
    {
        setManager = manager;
        setManager.OnSetBonusActivated += UpdateSetBonusUI;
        setManager.OnSetBonusDeactivated += UpdateSetBonusUI;
    }

    // 更新套装UI
    public void UpdateSetUI(string setName, int equippedCount)
    {
        SetData setData = setManager.GetSetData(setName);
        if (setData == null) return;

        // 更新基本信息
        setNameText.text = setData.setName;
        setPiecesText.text = $"已装备: {equippedCount}/{setData.itemIds.Count}件";

        // 更新套装效果显示
        foreach (var bonus in setData.bonuses)
        {
            UpdateBonusEntry(bonus, equippedCount);
        }
    }

    // 更新套装效果条目
    private void UpdateBonusEntry(SetBonus bonus, int equippedCount)
    {
        string bonusKey = $"{bonus.requiredPieces}件套装效果";
        SetBonusEntry entry;

        if (!bonusEntries.ContainsKey(bonusKey))
        {
            // 创建新的效果条目
            GameObject entryObj = Instantiate(setBonusEntryPrefab, setBonusContainer);
            entry = entryObj.GetComponent<SetBonusEntry>();
            bonusEntries[bonusKey] = entry;
        }
        else
        {
            entry = bonusEntries[bonusKey];
        }

        // 更新效果显示
        bool isActive = equippedCount >= bonus.requiredPieces;
        entry.UpdateDisplay(bonus, isActive);
    }
}
```

## 8. UI系统

### 8.1 背包UI核心实现
背包UI系统负责显示和管理物品界面：

```csharp
public class UIInventory : MonoBehaviour
{
    // UI组件引用
    [SerializeField] private GameObject slotPrefab;
    [SerializeField] private Transform slotsParent;
    [SerializeField] private ItemTooltip tooltip;
    [SerializeField] private GameObject draggedItemPrefab;

    // 核心引用
    private Inventory inventory;
    private List<UIItemSlot> uiSlots = new List<UIItemSlot>();
    private UIItemSlot draggedSlot;
    private GameObject draggedItem;

    // 初始化方法
    public void Initialize(Inventory inv)
    {
        inventory = inv;
        CreateSlots();
        RegisterEvents();
        UpdateUI();
    }

    // 创建物品槽
    private void CreateSlots()
    {
        // 清理现有槽位
        foreach (Transform child in slotsParent)
        {
            Destroy(child.gameObject);
        }
        uiSlots.Clear();

        // 创建新槽位
        for (int i = 0; i < inventory.inventorySize; i++)
        {
            GameObject slotObj = Instantiate(slotPrefab, slotsParent);
            UIItemSlot slot = slotObj.GetComponent<UIItemSlot>();
            slot.Initialize(i, this);
            uiSlots.Add(slot);
        }
    }

    // 注册事件
    private void RegisterEvents()
    {
        inventory.OnItemAdded += OnItemAdded;
        inventory.OnItemRemoved += OnItemRemoved;
        inventory.OnItemUsed += OnItemUsed;
    }

    // 更新UI显示
    public void UpdateUI()
    {
        for (int i = 0; i < inventory.slots.Length; i++)
        {
            if (i < uiSlots.Count)
            {
                uiSlots[i].UpdateUI(inventory.slots[i]);
            }
        }
    }
}
```

### 8.2 物品槽UI实现

```csharp
public class UIItemSlot : MonoBehaviour, IPointerClickHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [SerializeField] private Image itemIcon;
    [SerializeField] private Image background;
    [SerializeField] private Image qualityFrame;
    [SerializeField] private Text amountText;
    [SerializeField] private Image durabilityBar;

    private int slotIndex;
    private UIInventory uiInventory;
    private ItemSlot itemSlot;
    private bool isDragging;

    // 初始化
    public void Initialize(int index, UIInventory ui)
    {
        slotIndex = index;
        uiInventory = ui;
        ResetSlot();
    }

    // 更新UI显示
    public void UpdateUI(ItemSlot slot)
    {
        itemSlot = slot;
        
        if (slot.IsEmpty())
        {
            ResetSlot();
            return;
        }

        // 更新物品图标
        itemIcon.gameObject.SetActive(true);
        itemIcon.sprite = slot.item.icon;

        // 更新品质边框
        qualityFrame.gameObject.SetActive(true);
        qualityFrame.color = GetQualityColor(slot.item.rarity);

        // 更新数量显示
        if (slot.amount > 1)
        {
            amountText.gameObject.SetActive(true);
            amountText.text = slot.amount.ToString();
        }
        else
        {
            amountText.gameObject.SetActive(false);
        }

        // 更新耐久度条
        if (slot.item.maxDurability > 0)
        {
            durabilityBar.gameObject.SetActive(true);
            float durabilityPercent = slot.item.currentDurability / slot.item.maxDurability;
            durabilityBar.fillAmount = durabilityPercent;
            durabilityBar.color = GetDurabilityColor(durabilityPercent);
        }
        else
        {
            durabilityBar.gameObject.SetActive(false);
        }
    }

    // 处理拖拽事件
    public void OnBeginDrag(PointerEventData eventData)
    {
        if (!itemSlot.IsEmpty())
        {
            isDragging = true;
            uiInventory.OnBeginDrag(this);
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (isDragging)
        {
            uiInventory.OnDrag(eventData.position);
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (isDragging)
        {
            isDragging = false;
            uiInventory.OnEndDrag(eventData);
        }
    }
}
```

### 8.3 物品提示框系统实现
物品提示框用于显示物品的详细信息：

```csharp
public class ItemTooltip : MonoBehaviour
{
    // UI组件引用
    [SerializeField] private Text itemNameText;
    [SerializeField] private Text itemTypeText;
    [SerializeField] private Text rarityText;
    [SerializeField] private Text descriptionText;
    [SerializeField] private Text statsText;
    [SerializeField] private Text setBonusText;
    [SerializeField] private Text enchantmentText;
    [SerializeField] private Image background;
    [SerializeField] private LayoutGroup contentLayout;

    private static ItemTooltip instance;
    private RectTransform rectTransform;

    private void Awake()
    {
        instance = this;
        rectTransform = GetComponent<RectTransform>();
        gameObject.SetActive(false);
    }

    // 显示提示框
    public static void Show(Item item, Vector2 position)
    {
        instance.gameObject.SetActive(true);
        instance.UpdateContent(item);
        instance.UpdatePosition(position);
    }

    // 隐藏提示框
    public static void Hide()
    {
        instance.gameObject.SetActive(false);
    }

    // 更新提示框内容
    private void UpdateContent(Item item)
    {
        // 更新基本信息
        itemNameText.text = item.itemName;
        itemNameText.color = item.GetRarityColor();
        itemTypeText.text = $"[{item.itemType}] 等级 {item.level}";
        rarityText.text = item.rarity.ToString();
        descriptionText.text = item.description;

        // 更新属性信息
        StringBuilder stats = new StringBuilder();
        if (item.stats.hp != 0) stats.AppendLine($"生命值: +{item.stats.hp}");
        if (item.stats.mp != 0) stats.AppendLine($"魔法值: +{item.stats.mp}");
        if (item.stats.attack != 0) stats.AppendLine($"攻击力: +{item.stats.attack}");
        if (item.stats.defense != 0) stats.AppendLine($"防御力: +{item.stats.defense}");
        if (item.stats.speed != 0) stats.AppendLine($"速度: +{item.stats.speed}");
        if (item.stats.critRate != 0) stats.AppendLine($"暴击率: +{item.stats.critRate}%");
        if (item.stats.dodgeRate != 0) stats.AppendLine($"闪避率: +{item.stats.dodgeRate}%");
        statsText.text = stats.ToString();

        // 更新套装信息
        if (!string.IsNullOrEmpty(item.setName))
        {
            StringBuilder setInfo = new StringBuilder();
            setInfo.AppendLine($"\n{item.setName}套装");
            foreach (var bonus in item.setBonuses)
            {
                setInfo.AppendLine($"{bonus.requiredPieces}件套装效果:");
                setInfo.AppendLine($"  {bonus.description}");
            }
            setBonusText.text = setInfo.ToString();
            setBonusText.gameObject.SetActive(true);
        }
        else
        {
            setBonusText.gameObject.SetActive(false);
        }

        // 更新附魔信息
        if (item.enchantments.Count > 0)
        {
            StringBuilder enchantInfo = new StringBuilder();
            enchantInfo.AppendLine("\n附魔效果:");
            foreach (var enchant in item.enchantments)
            {
                enchantInfo.AppendLine($"{enchant.name} Lv.{enchant.level}");
                enchantInfo.AppendLine($"  {enchant.description}");
            }
            enchantmentText.text = enchantInfo.ToString();
            enchantmentText.gameObject.SetActive(true);
        }
        else
        {
            enchantmentText.gameObject.SetActive(false);
        }

        // 刷新布局
        LayoutRebuilder.ForceRebuildLayoutImmediate(contentLayout.GetComponent<RectTransform>());
    }

    // 更新提示框位置
    private void UpdatePosition(Vector2 position)
    {
        // 获取屏幕尺寸
        Vector2 screenSize = new Vector2(Screen.width, Screen.height);
        
        // 计算提示框尺寸
        Vector2 tooltipSize = rectTransform.sizeDelta;
        
        // 调整位置，确保提示框不会超出屏幕
        float xPos = position.x + tooltipSize.x > screenSize.x ? 
            position.x - tooltipSize.x : position.x;
        float yPos = position.y + tooltipSize.y > screenSize.y ? 
            position.y - tooltipSize.y : position.y;

        rectTransform.position = new Vector2(xPos, yPos);
    }
}
```

## 9. 实战示例

### 9.1 完整游戏示例
下面是一个完整的游戏示例，展示如何集成所有系统：

```csharp
public class GameManager : MonoBehaviour
{
    // 系统引用
    public Inventory playerInventory;
    public UIInventory uiInventory;
    public ItemDatabase itemDatabase;
    public SetManager setManager;
    public EnchantmentManager enchantManager;

    // 预制体引用
    public GameObject itemDropPrefab;
    public GameObject lootPopupPrefab;

    void Start()
    {
        // 初始化所有系统
        InitializeSystems();
        
        // 添加测试物品
        AddTestItems();
        
        // 注册事件
        RegisterEvents();
    }

    private void InitializeSystems()
    {
        // 初始化背包系统
        playerInventory = new Inventory();
        uiInventory.Initialize(playerInventory);

        // 初始化套装系统
        setManager.Initialize(LoadSetData());

        // 初始化附魔系统
        enchantManager.Initialize();
    }

    private void AddTestItems()
    {
        // 创建一个完整的装备示例
        Item sword = CreateLegendaryWeapon();
        playerInventory.AddItem(sword);

        // 添加套装其他部件
        Item armor = CreateDragonArmor();
        playerInventory.AddItem(armor);

        // 添加一些消耗品
        Item healthPotion = itemDatabase.GetItemById(2001);
        playerInventory.AddItem(healthPotion, 10);
    }

    // 创建传说武器示例
    private Item CreateLegendaryWeapon()
    {
        // 基础物品创建
        Item sword = new Item(
            id: 1001,
            name: "炎龙之剑",
            desc: "传说中由远古火龙的骨骼打造而成的神器",
            icon: Resources.Load<Sprite>("Icons/DragonSword"),
            type: Item.ItemType.Weapon,
            rarity: Item.ItemRarity.Legendary
        );

        // 设置基础属性
        sword.level = 50;
        sword.price = 100000;
        sword.tag = "神器,单手剑,火属性";

        // 设置装备属性
        sword.stats.attack = 200;
        sword.stats.critRate = 25;
        sword.stats.speed = 1.2f;

        // 设置耐久度
        sword.maxDurability = 200;
        sword.currentDurability = 200;

        // 添加套装信息
        sword.setName = "炎龙套装";
        sword.setBonuses.Add(new Item.SetBonus 
        {
            requiredPieces = 2,
            description = "攻击力提升20%",
            bonusStats = new Item.EquipmentStats { attack = 20 }
        });

        // 添加附魔
        sword.AddEnchantment(new Item.Enchantment("烈焰", "攻击附带火焰伤害")
        {
            bonusStats = new Dictionary<string, float> { { "fireDamage", 50f } }
        });

        return sword;
    }
}
```

### 9.2 实战练习

#### 练习1: 实现物品过滤和排序
```csharp
// 1. 为背包添加过滤功能
public class InventoryFilter
{
    public List<Item> FilterItems(List<Item> items, FilterOptions options)
    {
        // LINQ的使用示例
        return items.Where(item => 
            (options.rarity == Item.ItemRarity.Common || item.rarity == options.rarity) &&
            (options.minLevel == 0 || item.level >= options.minLevel) &&
            (string.IsNullOrEmpty(options.tag) || item.tag.Contains(options.tag))
        ).ToList();
    }

    // 排序功能
    public void SortItems(List<Item> items, SortType sortType)
    {
        switch (sortType)
        {
            case SortType.ByName:
                items.Sort((a, b) => a.itemName.CompareTo(b.itemName));
                break;
            case SortType.ByRarity:
                items.Sort((a, b) => b.rarity.CompareTo(a.rarity));
                break;
            case SortType.ByLevel:
                items.Sort((a, b) => b.level.CompareTo(a.level));
                break;
        }
    }
}
```

#### 练习2: 实现物品合成系统
```csharp
public class CraftingSystem
{
    // 合成配方
    public class Recipe
    {
        public List<ItemRequirement> requirements;
        public Item result;
        public int resultAmount;
    }

    // 检查是否可以合成
    public bool CanCraft(Recipe recipe, Inventory inventory)
    {
        foreach (var req in recipe.requirements)
        {
            int count = inventory.GetItemCount(req.itemId);
            if (count < req.amount) return false;
        }
        return true;
    }

    // 执行合成
    public bool Craft(Recipe recipe, Inventory inventory)
    {
        if (!CanCraft(recipe, inventory)) return false;

        // 扣除材料
        foreach (var req in recipe.requirements)
        {
            inventory.RemoveItemById(req.itemId, req.amount);
        }

        // 添加成品
        inventory.AddItem(recipe.result, recipe.resultAmount);
        return true;
    }
}
```

#### 练习3: 实现存档系统
```csharp
public class SaveSystem
{
    // 物品数据序列化
    [System.Serializable]
    public class ItemData
    {
        public int id;
        public int amount;
        public float durability;
        public List<EnchantmentData> enchantments;
    }

    // 保存背包数据
    public void SaveInventory(Inventory inventory, string savePath)
    {
        List<ItemData> itemDataList = new List<ItemData>();
        
        foreach (var slot in inventory.slots)
        {
            if (!slot.IsEmpty())
            {
                itemDataList.Add(new ItemData
                {
                    id = slot.item.id,
                    amount = slot.amount,
                    durability = slot.item.currentDurability,
                    enchantments = SerializeEnchantments(slot.item.enchantments)
                });
            }
        }

        string json = JsonUtility.ToJson(new { items = itemDataList });
        File.WriteAllText(savePath, json);
    }

    // 加载背包数据
    public void LoadInventory(Inventory inventory, string savePath)
    {
        if (!File.Exists(savePath)) return;

        string json = File.ReadAllText(savePath);
        var data = JsonUtility.FromJson<InventoryData>(json);

        inventory.Clear();
        foreach (var itemData in data.items)
        {
            Item item = ItemDatabase.Instance.GetItemById(itemData.id);
            item.currentDurability = itemData.durability;
            item.enchantments = DeserializeEnchantments(itemData.enchantments);
            inventory.AddItem(item, itemData.amount);
        }
    }
}
```

## 10. C#知识要点

### 10.1 泛型和委托
```csharp
// 泛型接口示例
public interface IInventorySystem<T> where T : Item
{
    bool AddItem(T item);
    bool RemoveItem(T item);
    List<T> GetItems();
}

// 委托和事件
public delegate void ItemEventHandler(Item item);
public event ItemEventHandler OnItemAdded;
public event ItemEventHandler OnItemRemoved;
```

### 10.2 LINQ的使用
```csharp
// 查找物品
public Item FindItem(string name)
{
    return inventory.slots
        .Where(slot => !slot.IsEmpty())
        .Select(slot => slot.item)
        .FirstOrDefault(item => item.itemName == name);
}

// 统计物品
public Dictionary<ItemType, int> GetItemTypeCount()
{
    return inventory.slots
        .Where(slot => !slot.IsEmpty())
        .GroupBy(slot => slot.item.itemType)
        .ToDictionary(g => g.Key, g => g.Count());
}
```

### 10.3 异步操作
```csharp
// 异步加载物品数据
public async Task<List<Item>> LoadItemDataAsync()
{
    string json = await File.ReadAllTextAsync("ItemData.json");
    return JsonUtility.FromJson<List<Item>>(json);
}

// 带进度的异步操作
public async Task InitializeWithProgress(IProgress<float> progress)
{
    int total = 100;
    for (int i = 0; i < total; i++)
    {
        await Task.Delay(10); // 模拟耗时操作
        progress.Report((float)i / total);
    }
}
```

### 10.4 性能优化技巧
```csharp
// 对象池
public class ItemPool
{
    private Stack<GameObject> pool = new Stack<GameObject>();
    private GameObject prefab;

    public GameObject Get()
    {
        if (pool.Count > 0)
            return pool.Pop();
        return GameObject.Instantiate(prefab);
    }

    public void Return(GameObject obj)
    {
        obj.SetActive(false);
        pool.Push(obj);
    }
}

// 缓存优化
public class ItemCache
{
    private Dictionary<int, Item> itemCache = new Dictionary<int, Item>();

    public Item GetItem(int id)
    {
        if (!itemCache.ContainsKey(id))
            itemCache[id] = LoadItem(id);
        return itemCache[id];
    }
}