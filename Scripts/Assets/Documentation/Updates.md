# 物品系统文档

## 1. 系统架构
### 1.1 核心类
#### Item.cs - 物品基类
```csharp
public class Item
{
    // 基础属性
    private int _id;
    private string _itemName; 
    private string _description;
    private Sprite _icon;
    
    // 公共访问器
    public int Id => _id;
    public string ItemName => _itemName;
    public string Description => _description;
    public Sprite Icon => _icon;

    // 物品类型和稀有度枚举
    public enum ItemType { Weapon, Armor, Consumable, Material, Quest, Accessory }
    public enum ItemRarity { Common, Uncommon, Rare, Epic, Legendary }
}
```

#### ItemSlot.cs - 物品槽
```csharp
public class ItemSlot
{
    public Item item;
    public int amount;
    public bool IsLocked { get; private set; }
    public bool IsHighlighted { get; private set; }

    public bool IsEmpty() => item == null;
    public bool CanAddItem(Item newItem, int quantity);
    public bool TransferTo(ItemSlot targetSlot, int amount = -1);
}
```

#### Inventory.cs - 背包系统
```csharp
public class Inventory : MonoBehaviour
{
    public int inventorySize = 20;
    public ItemSlot[] slots;
    
    // 事件系统
    public delegate void InventoryChangeHandler(int slotIndex);
    public event InventoryChangeHandler OnItemAdded;
    public event InventoryChangeHandler OnItemRemoved;
    public event InventoryChangeHandler OnItemUsed;

    // 公共方法
    public bool AddItem(Item item, int amount = 1);
    public bool RemoveItem(int slotIndex, int amount = 1);
    public bool RemoveItemById(int itemId, int amount = 1);
}
```

### 1.2 数据管理
#### ItemDatabase.cs - 物品数据库
```csharp
[CreateAssetMenu(fileName = "ItemDatabase", menuName = "Inventory/Item Database")]
public class ItemDatabase : ScriptableObject
{
    public List<Item> items;
    
    public Item GetItemById(int id);
    public List<Item> GetItemsByType(Item.ItemType type);
    public List<Item> GetItemsByRarity(Item.ItemRarity rarity);
}
```

## 2. 功能系统
### 2.1 装备系统
- 装备属性(EquipmentStats)
- 耐久度系统
- 强化系统
- 附魔系统
- 套装系统

### 2.2 物品堆叠
- 最大堆叠数量限制
- 自动堆叠逻辑
- 堆叠分割功能

### 2.3 存档系统
```csharp
// 存档数据结构
[System.Serializable]
private class InventoryData
{
    public int[] itemIds;
    public int[] amounts;
}

// 存档和读档方法
public void SaveInventory(string saveKey = "InventoryData");
public bool LoadInventory(ItemDatabase itemDatabase, string saveKey = "InventoryData");
```

## 3. UI系统
### 3.1 主要组件
#### UIInventory.cs
- 物品槽UI生成和管理
- 物品拖拽功能
- 更新UI显示

#### UIItemSlot.cs
- 物品图标显示
- 数量显示
- 高亮和锁定状态
- 拖拽交互

#### UIItemContextMenu.cs
- 右键菜单
- 动态按钮生成
- 物品操作功能

### 3.2 交互功能
- 拖放操作
- 右键菜单
- 物品提示框
- 堆叠分割

## 4. 事件系统
### 4.1 背包事件
- OnItemAdded
- OnItemRemoved
- OnItemUsed

### 4.2 物品事件
- 使用事件
- 装备事件
- 强化事件
- 附魔事件

## 5. 安全性和优化
### 5.1 数据安全
- 属性封装
- 只读集合
- 深度克隆
- 数据验证

### 5.2 性能优化
- 对象池
- 延迟加载
- 缓存机制

## 6. 使用示例
### 6.1 添加物品
```csharp
// 创建物品
var sword = new Item(1, "铁剑", "一把普通的铁剑", swordIcon, Item.ItemType.Weapon, Item.ItemRarity.Common);

// 添加到背包
inventory.AddItem(sword, 1);
```

### 6.2 使用物品
```csharp
// 使用物品
public void UseItem(int slotIndex)
{
    if (slots[slotIndex].item != null)
    {
        slots[slotIndex].item.Use();
        RemoveItem(slotIndex, 1);
        OnItemUsed?.Invoke(slotIndex);
    }
}
```

## 7. 注意事项
- 物品ID的唯一性
- 存档数据的兼容性
- UI更新的性能优化
- 事件的注册和注销
