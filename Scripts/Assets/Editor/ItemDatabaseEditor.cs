using UnityEngine;
using UnityEditor;

public class ItemDatabaseEditor : EditorWindow
{
    private ItemDatabase database;
    private Vector2 scrollPosition;
    
    // 基础属性
    private string itemName = "";
    private string description = "";
    private Sprite icon;
    private Item.ItemType itemType;
    private Item.WeaponType weaponType;
    private Item.ItemRarity rarity;
    private float price;
    private int level = 1;
    private int maxStackSize = 1;
    private string tag = "";
    
    // 装备属性
    private float hp;
    private float mp;
    private float attack;
    private float defense;
    private float speed;
    private float critRate;
    private float dodgeRate;
    private float strength;
    private float intelligence;
    private float dexterity;
    private float vitality;
    private float luck;
    private float magicResist;
    private float armorPen;
    private float magicPen;
    private float lifeSteal;
    private float cooldownReduction;

    [MenuItem("Tools/Item Database Editor")]
    public static void ShowWindow()
    {
        GetWindow<ItemDatabaseEditor>("物品数据库编辑器");
    }

    private void OnGUI()
    {
        GUILayout.Label("物品数据库编辑器", EditorStyles.boldLabel);

        database = EditorGUILayout.ObjectField("数据库", database, typeof(ItemDatabase), false) as ItemDatabase;

        if (database == null)
        {
            EditorGUILayout.HelpBox("请先选择一个物品数据库!", MessageType.Warning);
            if (GUILayout.Button("创建新数据库"))
            {
                CreateNewDatabase();
            }
            return;
        }

        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

        EditorGUILayout.Space();
        GUILayout.Label("添加新物品", EditorStyles.boldLabel);

        // 基础属性
        itemName = EditorGUILayout.TextField("物品名称", itemName);
        description = EditorGUILayout.TextField("物品描述", description);
        icon = EditorGUILayout.ObjectField("物品图标", icon, typeof(Sprite), false) as Sprite;
        itemType = (Item.ItemType)EditorGUILayout.EnumPopup("物品类型", itemType);
        
        if (itemType == Item.ItemType.Weapon)
        {
            weaponType = (Item.WeaponType)EditorGUILayout.EnumPopup("武器类型", weaponType);
        }
        
        rarity = (Item.ItemRarity)EditorGUILayout.EnumPopup("稀有度", rarity);
        price = EditorGUILayout.FloatField("价格", price);
        level = EditorGUILayout.IntField("等级", level);
        maxStackSize = EditorGUILayout.IntField("最大堆叠数", maxStackSize);
        tag = EditorGUILayout.TextField("标签", tag);

        // 如果是装备类型，显示属性编辑器
        if (itemType == Item.ItemType.Weapon || itemType == Item.ItemType.Armor || 
            itemType == Item.ItemType.Helmet || itemType == Item.ItemType.Chestplate || 
            itemType == Item.ItemType.Leggings || itemType == Item.ItemType.Boots)
        {
            EditorGUILayout.Space();
            GUILayout.Label("装备属性", EditorStyles.boldLabel);
            
            hp = EditorGUILayout.FloatField("生命值", hp);
            mp = EditorGUILayout.FloatField("魔法值", mp);
            attack = EditorGUILayout.FloatField("攻击力", attack);
            defense = EditorGUILayout.FloatField("防御力", defense);
            speed = EditorGUILayout.FloatField("速度", speed);
            critRate = EditorGUILayout.Slider("暴击率", critRate, 0, 100);
            dodgeRate = EditorGUILayout.Slider("闪避率", dodgeRate, 0, 100);
            strength = EditorGUILayout.FloatField("力量", strength);
            intelligence = EditorGUILayout.FloatField("智力", intelligence);
            dexterity = EditorGUILayout.FloatField("敏捷", dexterity);
            vitality = EditorGUILayout.FloatField("体力", vitality);
            luck = EditorGUILayout.FloatField("幸运", luck);
            magicResist = EditorGUILayout.FloatField("魔法抗性", magicResist);
            armorPen = EditorGUILayout.FloatField("护甲穿透", armorPen);
            magicPen = EditorGUILayout.FloatField("魔法穿透", magicPen);
            lifeSteal = EditorGUILayout.Slider("生命偷取", lifeSteal, 0, 100);
            cooldownReduction = EditorGUILayout.Slider("冷却缩减", cooldownReduction, 0, 100);
        }

        EditorGUILayout.Space();

        if (GUILayout.Button("添加物品"))
        {
            AddItem();
        }

        EditorGUILayout.Space();
        
        // 显示现有物品列表
        GUILayout.Label("现有物品列表", EditorStyles.boldLabel);
        if (database.items != null)
        {
            foreach (var item in database.items)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField($"[{item.Id}] {item.ItemName} - {item.Type} - {item.Rarity}");
                if (GUILayout.Button("删除", GUILayout.Width(50)))
                {
                    database.items.Remove(item);
                    EditorUtility.SetDirty(database);
                    break;
                }
                EditorGUILayout.EndHorizontal();
            }
        }

        EditorGUILayout.EndScrollView();
    }

    private void CreateNewDatabase()
    {
        string path = EditorUtility.SaveFilePanelInProject(
            "创建物品数据库",
            "ItemDatabase",
            "asset",
            "请选择保存位置"
        );
        
        if (!string.IsNullOrEmpty(path))
        {
            ItemDatabase newDatabase = ScriptableObject.CreateInstance<ItemDatabase>();
            AssetDatabase.CreateAsset(newDatabase, path);
            AssetDatabase.SaveAssets();
            database = newDatabase;
        }
    }

    private void AddItem()
    {
        if (string.IsNullOrEmpty(itemName))
        {
            EditorUtility.DisplayDialog("错误", "物品名称不能为空!", "确定");
            return;
        }

        if (icon == null)
        {
            EditorUtility.DisplayDialog("错误", "请选择物品图标!", "确定");
            return;
        }

        // 生成新的物品ID
        int newId = 1;
        if (database.items.Count > 0)
        {
            newId = database.items[database.items.Count - 1].Id + 1;
        }

        // 创建新物品
        Item newItem = new Item(newId, itemName, description, icon, itemType, rarity, maxStackSize);
        
        // 设置装备属性
        if (itemType == Item.ItemType.Weapon || itemType == Item.ItemType.Armor || 
            itemType == Item.ItemType.Helmet || itemType == Item.ItemType.Chestplate || 
            itemType == Item.ItemType.Leggings || itemType == Item.ItemType.Boots)
        {
            newItem.stats.HP = hp;
            newItem.stats.MP = mp;
            newItem.stats.Attack = attack;
            newItem.stats.Defense = defense;
            newItem.stats.Speed = speed;
            newItem.stats.CritRate = critRate;
            newItem.stats.DodgeRate = dodgeRate;
            newItem.stats.Strength = strength;
            newItem.stats.Intelligence = intelligence;
            newItem.stats.Dexterity = dexterity;
            newItem.stats.Vitality = vitality;
            newItem.stats.Luck = luck;
            newItem.stats.MagicResist = magicResist;
            newItem.stats.ArmorPen = armorPen;
            newItem.stats.MagicPen = magicPen;
            newItem.stats.LifeSteal = lifeSteal;
            newItem.stats.CooldownReduction = cooldownReduction;
        }

        // 将物品添加到数据库
        database.items.Add(newItem);

        // 标记数据库已修改
        EditorUtility.SetDirty(database);

        // 清空输入字段
        ClearFields();

        EditorUtility.DisplayDialog("成功", "物品已添加到数据库!", "确定");
    }

    private void ClearFields()
    {
        itemName = "";
        description = "";
        icon = null;
        price = 0;
        level = 1;
        maxStackSize = 1;
        tag = "";
        
        hp = 0;
        mp = 0;
        attack = 0;
        defense = 0;
        speed = 0;
        critRate = 0;
        dodgeRate = 0;
        strength = 0;
        intelligence = 0;
        dexterity = 0;
        vitality = 0;
        luck = 0;
        magicResist = 0;
        armorPen = 0;
        magicPen = 0;
        lifeSteal = 0;
        cooldownReduction = 0;
    }
} 