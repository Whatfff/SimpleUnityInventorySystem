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

### 1.2 安装步骤

#### 1.2.1 导入资源
1. 将以下文件夹导入到你的Unity项目中:
   ```
   Assets/
   ├── Scripts/
   │   ├── Core/           # 核心系统脚本
   │   ├── UI/            # UI相关脚本
   │   ├── Editor/        # 编辑器工具脚本
   │   └── Utils/         # 工具类脚本
   ├── Prefabs/           # UI预制体
   ├── Resources/         # 资源文件
   └── Documentation/     # 文档
   ```

2. 确保所有依赖包都已安装:
   - TextMesh Pro
   - Unity UI
   - JSON .NET (可选,用于高级序列化)

#### 1.2.2 场景设置
1. 创建一个新场景或打开现有场景
2. 创建必要的游戏对象:
```
Scene
├── GameManager           # 游戏管理器
├── InventorySystem       # 物品系统
└── UI
    ├── Canvas           # UI画布
    │   ├── Inventory    # 物品栏
    │   ├── Equipment    # 装备栏
    │   ├── Tooltip     # 提示框
    │   └── ContextMenu # 上下文菜单
    └── EventSystem      # UI事件系统
```

#### 1.2.3 组件配置
1. **GameManager配置**
```csharp
// GameManager.cs
public class GameManager : MonoBehaviour
{
    [Header("Systems")]
    public Inventory inventory;          // 物品栏系统
    public UIInventory uiInventory;      // 物品栏UI
    public ItemDatabase itemDatabase;    // 物品数据库
    
    [Header("Settings")]
    public int inventorySize = 20;       // 物品栏大小
    public bool enableAutosave = true;   // 启用自动保存
    public float autosaveInterval = 300f; // 自动保存间隔
    
    private void Start()
    {
        InitializeSystems();
        LoadSavedData();
        SetupEventHandlers();
    }
    
    private void InitializeSystems()
    {
        // 初始化物品数据库
        itemDatabase.Initialize();
        
        // 初始化物品栏
        inventory.Initialize(inventorySize);
        
        // 初始化UI
        uiInventory.Initialize(inventory);
        
        if (enableAutosave)
        {
            InvokeRepeating("AutoSave", autosaveInterval, autosaveInterval);
        }
    }
    
    private void LoadSavedData()
    {
        // 加载存档数据
        if (inventory.LoadInventory(itemDatabase))
        {
            Debug.Log("Successfully loaded inventory data");
        }
    }
    
    private void SetupEventHandlers()
    {
        // 注册事件处理
        inventory.OnItemAdded += HandleItemAdded;
        inventory.OnItemRemoved += HandleItemRemoved;
        inventory.OnItemUsed += HandleItemUsed;
    }
}
```

2. **Inventory组件配置**
```csharp
// 在Inspector中设置
- Inventory Size: 20
- UI References:
  - Slot Prefab
  - UI Inventory
  - Item Tooltip
  - Context Menu
```

3. **Canvas配置**
- Render Mode: Screen Space - Overlay
- UI Scale Mode: Scale With Screen Size
- Reference Resolution: 1920 x 1080
- Screen Match Mode: Match Width Or Height
- Match: 0.5

### 1.3 基本UI设置

#### 1.3.1 物品栏UI
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

#### 1.3.2 提示框设置
```csharp
// ItemTooltip.cs配置
[SerializeField] private float offsetX = 20f;
[SerializeField] private float offsetY = 20f;
[SerializeField] private float padding = 10f;
[SerializeField] private float minWidth = 200f;
[SerializeField] private float maxWidth = 400f;
```

### 1.4 基本功能测试

#### 1.4.1 添加测试物品
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

#### 1.4.2 测试UI交互
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

### 1.5 常见初始化问题

#### 1.5.1 检查清单
- [ ] 所有必要的脚本组件都已添加
- [ ] UI预制体引用正确设置
- [ ] 事件系统正确配置
- [ ] 物品数据库已正确加载
- [ ] 存档路径权限正确

#### 1.5.2 调试提示
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

### 1.6 下一步
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