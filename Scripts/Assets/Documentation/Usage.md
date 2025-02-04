# 物品系统使用指南

## 1. 初始设置
### 1.1 创建物品数据库
1. 在Project窗口右键 -> Create -> Inventory -> Item Database
2. 命名为"ItemDatabase"
3. 在Inspector中添加物品:
```csharp
// 示例物品数据
id: 1
itemName: "铁剑"
description: "一把普通的铁剑"
icon: [拖入武器图标]
type: Weapon
rarity: Common
maxStackSize: 1
```

### 1.2 设置背包系统
1. 创建一个空GameObject，命名为"InventorySystem"
2. 添加Inventory组件:
```csharp
// Inspector设置
Inventory Size: 20  // 设置背包大小
```

### 1.3 设置UI
1. 创建Canvas
2. 添加背包面板
3. 添加UIInventory组件并设置:
```csharp
Slot Prefab: [拖入物品槽预制体]
Slots Parent: [拖入物品槽的父物体]
Inventory: [拖入Inventory组件]
```

## 2. 基本使用
### 2.1 添加物品
```csharp
// 获取物品数据库引用
[SerializeField] private ItemDatabase itemDatabase;

// 通过ID添加物品
public void AddItemById(int itemId, int amount = 1)
{
    Item item = itemDatabase.GetItemById(itemId);
    if (item != null)
    {
        inventory.AddItem(item, amount);
    }
}

// 示例:添加3个药水
AddItemById(1001, 3);
```

### 2.2 移除物品
```csharp
// 通过槽位索引移除
inventory.RemoveItem(slotIndex, amount);

// 通过物品ID移除
inventory.RemoveItemById(itemId, amount);
```

### 2.3 查找物品
```csharp
// 按类型查找
var weapons = inventory.GetItemsByType(Item.ItemType.Weapon);

// 按稀有度查找
var rareItems = inventory.GetItemsByRarity(Item.ItemRarity.Rare);
```

## 3. 高级功能
### 3.1 监听事件
```csharp
void Start()
{
    // 注册事件
    inventory.OnItemAdded += HandleItemAdded;
    inventory.OnItemRemoved += HandleItemRemoved;
    inventory.OnItemUsed += HandleItemUsed;
}

void HandleItemAdded(int slotIndex)
{
    Debug.Log($"物品已添加到槽位 {slotIndex}");
}
```

### 3.2 自定义物品行为
```csharp
// 继承Item类创建特殊物品
public class HealthPotion : Item
{
    public override void Use()
    {
        // 实现使用效果
        PlayerStats.Instance.RestoreHealth(50);
        base.Use();
    }
}
```

### 3.3 存档和读档
```csharp
// 保存背包数据
inventory.SaveInventory("PlayerInventory");

// 加载背包数据
inventory.LoadInventory(itemDatabase, "PlayerInventory");
```

## 4. 常见问题解决
### 4.1 物品不显示
- 检查ItemDatabase是否正确设置
- 确认物品图标已赋值
- 检查UI层级和Canvas设置

### 4.2 拖拽不工作
- 确保EventSystem存在
- 检查Canvas的RenderMode设置
- 验证UIItemSlot组件配置

### 4.3 性能优化建议
- 使用对象池管理频繁创建的UI元素
- 避免在Update中频繁更新UI
- 合理使用事件系统而不是轮询

## 5. 示例场景
在 Assets/Scenes/InventoryDemo 中提供了完整的示例场景，展示了:
- 基础物品操作
- 拖拽交互
- 右键菜单
- 物品提示框
- 存档演示
