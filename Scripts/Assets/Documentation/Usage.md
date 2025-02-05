# RPG物品系统使用指南

## 目录
1. [快速开始](#1-快速开始)
2. [基础功能](#2-基础功能)
3. [高级功能](#3-高级功能)
4. [编辑器使用](#4-编辑器使用)
5. [最佳实践](#5-最佳实践)
6. [常见问题](#6-常见问题)

## 1. 快速开始

### 1.1 系统要求
- Unity 2020.3 或更高版本
- .NET 4.x
- TextMesh Pro (UI显示)
- 推荐分辨率: 1920x1080

### 1.2 安装步骤

#### 1.2.1 导入资源包
1. 将资源包导入项目:
   - 在Unity编辑器中选择 Assets > Import Package > Custom Package
   - 选择 RPGInventorySystem.unitypackage
   - 确保所有文件都被选中并点击 Import

2. 检查文件结构:
```
Assets/
├── RPGInventory/
    ├── Scripts/
    │   ├── Core/           
    │   │   ├── Item.cs             # 物品基类
    │   │   ├── ItemSlot.cs         # 物品槽类
    │   │   ├── Inventory.cs        # 物品栏系统
    │   │   └── ItemDatabase.cs     # 物品数据库
    │   ├── UI/            
    │   │   ├── UIItemSlot.cs       # 物品槽UI
    │   │   ├── UIInventory.cs      # 物品栏UI
    │   │   ├── ItemTooltip.cs      # 提示框
    │   │   └── UIItemContextMenu.cs # 上下文菜单
    │   └── Editor/        
    │       └── ItemDatabaseEditor.cs # 数据库编辑器
    ├── Prefabs/           
    │   ├── UI/
    │   │   ├── InventorySlot.prefab # 物品槽预制体
    │   │   ├── InventoryPanel.prefab # 物品栏面板
    │   │   ├── Tooltip.prefab       # 提示框预制体
    │   │   └── ContextMenu.prefab   # 上下文菜单预制体
    └── Resources/         
        └── DefaultItems/   # 默认物品数据
```

#### 1.2.2 场景设置
1. **创建基础游戏对象**
   ```
   Hierarchy:
   ├── GameManager                    # 添加 GameManager.cs
   ├── InventorySystem               # 添加 Inventory.cs
   ├── ItemDatabase                 # 添加 ItemDatabase.cs
   └── UI
       ├── Canvas (UI)              # 设置为Screen Space - Overlay
       │   ├── InventoryPanel       # 添加 UIInventory.cs
       │   │   ├── GridLayout      # 配置网格布局
       │   │   └── Slots          # 物品槽容器
       │   ├── TooltipPanel        # 添加 ItemTooltip.cs
       │   └── ContextMenuPanel    # 添加 UIItemContextMenu.cs
       └── EventSystem             # 确保存在
   ```

2. **预制体设置**
   - 将 InventorySlot 预制体拖拽到 Slots 下作为示例
   - 确保所有UI元素都在Canvas内部

### 1.3 组件配置详解

#### 1.3.1 GameManager配置
1. 在GameManager游戏对象上:
   - 添加 GameManager.cs 脚本
   - 设置引用:
     ```
     Inventory: 拖拽 InventorySystem 对象
     UI Inventory: 拖拽 InventoryPanel 对象
     Item Database: 拖拽 ItemDatabase 对象
     ```
   - 配置参数:
     ```
     Inventory Size: 20
     Enable Autosave: √
     Autosave Interval: 300
     ```

#### 1.3.2 InventorySystem配置
1. 在InventorySystem游戏对象上:
   - 添加 Inventory.cs 脚本
   - 设置参数:
     ```
     Inventory Size: 20
     Slots: 自动生成
     ```

#### 1.3.3 ItemDatabase配置
1. 在ItemDatabase游戏对象上:
   - 添加 ItemDatabase.cs 脚本
   - 创建数据库文件:
     ```
     1. 在Assets/Resources/DefaultItems/目录下创建items_database.json
     2. 编写物品数据（参考下方示例）
     3. 将json文件拖拽到ItemDatabase组件的Database File字段
     ```

2. JSON数据格式说明:
   ```json
   {
       "items": [
           {
               "id": 1,              // 物品唯一ID
               "itemName": "物品名称",
               "description": "物品描述",
               "maxStackSize": 1,    // 最大堆叠数量
               "baseValue": 100,     // 基础价值
               "iconPath": "图标路径",  // 相对于Resources/Icons的路径
               "prefabPath": "预制体路径", // 相对于Resources/Prefabs的路径
               "type": 1,           // 物品类型(1:武器,2:护甲,3:消耗品,4:材料,5:任务物品)
               "rarity": 0,         // 稀有度(0:普通,1:优秀,2:稀有,3:史诗,4:传说)
               "level": 1           // 物品等级
           }
       ]
   }
   ```

3. 资源目录结构:
   ```
   Assets/
   └── Resources/
       ├── Icons/
       │   ├── weapons/
       │   │   └── iron_sword.png
       │   ├── armors/
       │   │   └── leather_armor.png
       │   └── consumables/
       │       └── health_potion.png
       ├── Prefabs/
       │   ├── weapons/
       │   │   └── iron_sword_prefab.prefab
       │   ├── armors/
       │   │   └── leather_armor_prefab.prefab
       │   └── consumables/
       │       └── health_potion_prefab.prefab
       └── DefaultItems/
           └── items_database.json
   ```

4. 运行时验证:
   - 在控制台中查看是否有"Item database initialized"消息
   - 检查是否显示正确的物品数量加载信息
   - 使用GameManager的AddTestItems()方法测试物品加载

#### 1.3.4 UI配置

1. **Canvas设置**
   - Render Mode: Screen Space - Overlay
   - UI Scale Mode: Scale With Screen Size
   - Reference Resolution: 1920 x 1080
   - Screen Match Mode: Match Width Or Height
   - Match: 0.5
   - Reference Pixels Per Unit: 100
   - Sort Order: 0

2. **InventoryPanel配置**
   - 添加 UIInventory.cs 脚本
   - 设置引用:
     ```
     Slot Prefab: 拖拽 InventorySlot.prefab
     Slots Parent: 拖拽 Slots 对象
     Inventory: 拖拽 InventorySystem 对象
     ```
   - 添加Grid Layout Group组件:
     ```
     Cell Size: 80 x 80
     Spacing: 5 x 5
     Start Corner: Upper Left
     Start Axis: Horizontal
     Child Alignment: Upper Left
     Constraint: Fixed Column Count
     Constraint Count: 5
     ```

3. **InventorySlot预制体配置**
   - 结构:
     ```
     InventorySlot (Prefab)
     ├── Background (Image)
     ├── ItemIcon (Image)
     ├── AmountText (TextMeshProUGUI)
     └── Highlight (Image)
     ```
   - 添加组件:
     - UIItemSlot.cs
     - Button (UI)
     - EventTrigger

4. **Tooltip配置**
   - 添加 ItemTooltip.cs 脚本
   - 设置引用:
     ```
     Item Name Text: 拖拽对应的TextMeshProUGUI组件
     Item Type Text: 拖拽对应的TextMeshProUGUI组件
     Rarity Text: 拖拽对应的TextMeshProUGUI组件
     Description Text: 拖拽对应的TextMeshProUGUI组件
     Stats Text: 拖拽对应的TextMeshProUGUI组件
     Background: 拖拽背景Image组件
     ```

5. **ContextMenu配置**
   - 添加 UIItemContextMenu.cs 脚本
   - 设置按钮引用和事件处理

### 1.4 基本UI设置

#### 1.4.1 物品栏UI
1. **创建物品格子预制体**
```
ItemSlot (Prefab)
├── Background
├── Icon
├── Amount Text
├── Highlight
└── Components
    ├── UIItemSlot
    ├── Button
    ├── EventTrigger
    └── Layout Element
```

2. **配置物品栏布局**
```csharp
// UIInventory Inspector设置
- Grid Layout Group:
  - Cell Size: 80x80
  - Spacing: 5x5
  - Constraint: Fixed Column Count
  - Columns: 5
```

#### 1.4.2 提示框设置
```csharp
// ItemTooltip.cs配置
[SerializeField] private float offsetX = 20f;
[SerializeField] private float offsetY = 20f;
[SerializeField] private float padding = 10f;
[SerializeField] private float minWidth = 200f;
[SerializeField] private float maxWidth = 400f;
```

### 1.5 基本功能测试

#### 1.5.1 添加测试物品
```csharp
void TestInventorySystem()
{
    // 创建测试物品
    Item sword = itemDatabase.GetItemById(1); // 假设ID 1是一把剑
    Item potion = itemDatabase.GetItemById(2); // 假设ID 2是药水
    
    // 添加物品到背包
    inventory.AddItem(sword);
    inventory.AddItem(potion, 5);
    
    // 测试物品堆叠
    inventory.AddItem(potion, 3); // 应该与已有的药水堆叠
    
    // 测试物品移除
    inventory.RemoveItem(0, 1); // 移除第一个格子的物品
}
```

#### 1.5.2 测试UI交互
```csharp
void TestUIInteraction()
{
    // 测试拖放功能
    inventory.SwapItems(0, 1);
    
    // 测试物品使用
    inventory.UseItem(0);
    
    // 测试提示框
    ItemTooltip.Show(inventory.GetItemAt(0), Input.mousePosition);
}
```

### 1.6 常见初始化问题

#### 1.6.1 检查清单
- [ ] 所有必要的脚本组件都已添加
- [ ] UI预制体引用正确设置
- [ ] 事件系统正确配置
- [ ] 物品数据库已正确加载
- [ ] 存档路径权限正确

#### 1.6.2 调试提示
```csharp
// 开启详细日志
public static class InventoryDebug
{
    public static bool VerboseLogging = true;
    
    public static void Log(string message)
    {
        if (VerboseLogging)
        {
            Debug.Log($"[Inventory] {message}");
        }
    }
}
```

### 1.7 下一步
- 配置物品数据库
- 设置自定义物品类型
- 实现物品行为
- 添加高级功能(强化、附魔等)

## 2. 基础功能

### 2.1 物品管理
```csharp
// 添加物品
inventory.AddItem(itemId, amount);

// 移除物品
inventory.RemoveItem(slotIndex, amount);

// 使用物品
inventory.UseItem(slotIndex);

// 检查物品
bool hasItem = inventory.HasItem(itemId, amount);
```

### 2.2 UI交互
```csharp
// 显示物品信息
ItemTooltip.Show(item, position);

// 处理拖放
public void OnItemDrop(int fromSlot, int toSlot)
{
    inventory.SwapItems(fromSlot, toSlot);
    UpdateUI();
}

// 显示上下文菜单
UIItemContextMenu.Show(item, position);
```

## 3. 高级功能

### 3.1 物品强化
```csharp
// 强化物品
public void EnhanceItem(Item item, List<Item> materials)
{
    if (enhanceSystem.CanEnhance(item, materials))
    {
        bool success = enhanceSystem.TryEnhance(item, materials);
        if (success)
        {
            // 处理强化成功
            UpdateItemUI(item);
            ShowEnhanceEffect();
        }
    }
}
```

### 3.2 附魔系统
```csharp
// 添加附魔
public void AddEnchantment(Item item, Enchantment enchant)
{
    if (enchantSystem.CanAddEnchantment(item, enchant))
    {
        enchantSystem.AddEnchantment(item, enchant);
        UpdateItemUI(item);
    }
}
```

### 3.3 耐久度系统
```csharp
// 更新耐久度
public void UpdateDurability(Item item, float wear)
{
    durabilitySystem.ApplyWear(item, wear);
    if (durabilitySystem.NeedsRepair(item))
    {
        ShowRepairPrompt(item);
    }
}
```

## 4. 编辑器使用

### 4.1 物品数据库编辑器
1. 打开物品编辑器 (Tools > RPG Inventory > Item Database Editor)
2. 创建新物品
3. 编辑物品属性
4. 保存更改

### 4.2 批量操作工具
```csharp
// 使用批量处理工具
public void BatchProcessItems()
{
    var processor = new ItemBatchProcessor();
    processor.ProcessItems(selectedItems, (item) => {
        item.Price *= 1.1f;  // 批量调整价格
        item.Level += 1;     // 批量调整等级
    });
}
```

## 5. 最佳实践

### 5.1 性能优化
```csharp
// 使用对象池
public class ItemFactory
{
    private ItemPool itemPool;
    
    public Item CreateItem(int itemId)
    {
        // 先从对象池获取
        Item item = itemPool.Get(itemId);
        if (item == null)
        {
            // 创建新实例
            item = new Item(itemId);
        }
        return item;
    }
}
```

### 5.2 数据管理
```csharp
// 定期保存
public void AutoSave()
{
    // 每5分钟保存一次
    InvokeRepeating("SaveInventory", 300f, 300f);
}

// 数据备份
public void BackupData()
{
    string backupPath = $"Backups/Inventory_{DateTime.Now:yyyyMMdd_HHmmss}.json";
    inventory.SaveInventory(backupPath);
}
```

## 6. 常见问题

### 6.1 问题排查
1. 物品不显示
   - 检查UI引用是否正确
   - 确认物品数据是否正确加载
   - 验证图标资源是否存在

2. 拖放不工作
   - 确认EventSystem组件存在
   - 检查RayCastTarget设置
   - 验证拖放脚本绑定

3. 存档加载失败
   - 检查存档格式
   - 验证文件权限
   - 确认数据完整性

### 6.2 调试技巧
```csharp
// 开启调试模式
public void EnableDebugMode()
{
    InventoryDebug.EnableLogging = true;
    InventoryDebug.ShowDebugUI = true;
}

// 输出调试信息
public void DebugItem(Item item)
{
    Debug.Log($"Item: {item.ItemName}");
    Debug.Log($"Stats: {JsonUtility.ToJson(item.Stats, true)}");
    Debug.Log($"State: {item.GetState()}");
}
```

### 6.3 错误处理
```csharp
try
{
    inventory.AddItem(item);
}
catch (InventoryException ex)
{
    Debug.LogError($"Failed to add item: {ex.Message}");
    ShowErrorMessage(ex.Message);
}
``` 