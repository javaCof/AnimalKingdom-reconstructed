using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

/// <summary>
/// Inventory domain model responsible for managing item storage and lookup across the game.
/// Implemented as a singleton to provide global access across gameplay systems.
/// This class is independent from UI and presentation logic.
/// </summary>
public sealed class Inventory
{
    //Singleton instance for global inventory access
    public static readonly Inventory instance = new Inventory();

    //Item storage collections (runtime state)
    public List<ConsumableInfo> consumables = new List<ConsumableInfo>();
    public List<EquipmentInfo> equipments = new List<EquipmentInfo>();
    public List<PassiveSkillInfo> passiveSkills = new List<PassiveSkillInfo>();
    public List<ActiveSkillInfo> activeSkills = new List<ActiveSkillInfo>();

    //Private constructor to enforce singleton pattern
    private Inventory() { }

    //Initializes inventory state from saved data.
    //Converts InventoryInfo into runtime inventory items.
    public void Init(InventoryInfo info)
    {
        this.consumables.Clear();
        this.equipments.Clear();
        this.passiveSkills.Clear();
        this.activeSkills.Clear();

        foreach (var item in info.items)
        {
            switch (item.type)
            {
                case 0:
                    this.consumables.Add(GetData(item) as ConsumableInfo);
                    break;
                case 1:
                    this.equipments.Add(GetData(item) as EquipmentInfo);
                    break;
                case 2:
                    this.passiveSkills.Add(GetData(item) as PassiveSkillInfo);
                    break;
                case 3:
                    this.activeSkills.Add(GetData(item) as ActiveSkillInfo);
                    break;
            }
        }

        foreach (var equipment in this.equipments)
            equipment.UpdateSkills();
    }

    //Converts current inventory state into serializable InventoryInfo for save persistence
    public InventoryInfo GetInfo()
    {
        InventoryInfo info = new InventoryInfo();

        info.items = new List<InventoryItemInfo>();
        foreach (var item in this.consumables)
            info.items.Add(GetInfo(item));
        foreach (var item in this.equipments)
            info.items.Add(GetInfo(item));
        foreach (var item in this.passiveSkills)
            info.items.Add(GetInfo(item));
        foreach (var item in this.activeSkills)
            info.items.Add(GetInfo(item));

        return info;
    }

    //Creates a serializable inventory item data from a runtime item instance
    private InventoryItemInfo GetInfo(InventoryItemInfo data)
    {
        InventoryItemInfo info = new InventoryItemInfo();
        info.uid = data.uid;
        info.type = data.type;
        info.item_id = data.item_id;
        info.enforce = data.enforce;
        info.element_id = data.element_id;
        info.amount = data.amount;
        info.owner_uid = data.owner_uid;
        info.job_skill_uid = data.job_skill_uid;
        info.weapon_skill_uid = data.weapon_skill_uid;
        info.is_visible = data.is_visible;

        return info;
    }

    //Creates a runtime inventory item instance from saved item data
    private InventoryItemInfo GetData(InventoryItemInfo info)
    {
        switch (info.type)
        {
            case 0:
                return new ConsumableInfo(info);
            case 1:
                return new EquipmentInfo(info);
            case 2:
                return new PassiveSkillInfo(info);
            case 3:
                return new ActiveSkillInfo(info);
            default:
                return null;
        }
    }

    //Adds an equipment item to the inventory
    public void AddItem(EquipmentInfo item, bool isVisible = true)
    {
        item.uid = Guid.NewGuid().ToString();
        item.is_visible = isVisible;
        equipments.Add(item);

        DataManager.instance.SaveGame();
    }

    //Adds a passive skill item to the inventory
    public void AddItem(PassiveSkillInfo item, bool isVisible = true)
    {
        item.uid = Guid.NewGuid().ToString();
        item.is_visible = isVisible;
        passiveSkills.Add(item);

        DataManager.instance.SaveGame();
    }

    //Adds a active skill item to the inventory
    public void AddItem(ActiveSkillInfo item, bool isVisible = true)
    {
        item.uid = Guid.NewGuid().ToString();
        item.is_visible = isVisible;
        activeSkills.Add(item);

        DataManager.instance.SaveGame();
    }

    //Adds currency to the inventory
    public void AddCurrency(int id, int amount)
    {
        switch (id)
        {
            case 601:
                DataManager.instance.currencyGold += amount;
                break;
            case 602:
                DataManager.instance.currencyEnergy += amount;
                break;
            case 603:
                DataManager.instance.currencyCarrot += amount;
                break;
        }

        DataManager.instance.SaveGame();
    }

    //Adds a consumable item to the inventory
    public void AddConsumable(ConsumableInfo item, bool isVisible = true)
    {
        ConsumableInfo oldItem = consumables.Find(x => x.item_id == item.item_id);

        if (oldItem != null) oldItem.amount += item.amount;
        else
        {
            item.uid = Guid.NewGuid().ToString();
            item.is_visible = isVisible;
            consumables.Add(item);
        }

        if (item.item_id == 20001)
            DataManager.instance.itemExpPotion += item.amount;

        DataManager.instance.SaveGame();
    }

    //Consumes a specified amount of a consumable item
    public void UseConsumable(ConsumableInfo consumable, int count)
    {
        if (consumable.amount >= count)
        {
            consumable.amount -= count;

            if (consumable.item_id == 20001)
                DataManager.instance.itemExpPotion -= count;
        }
        DataManager.instance.SaveGame();
    }

    //Convenience method for using experience potions
    public void UseExpPotion(int count)
    {
        UseConsumable(FindConsumable(20001), count);
    }

    //Finds a consumable item by item ID
    public ConsumableInfo FindConsumable(int id)
    {
        return consumables.Find(x => x.item_id == id);
    }

    //Finds an equipment item by unique identifier
    public EquipmentInfo FindEquipment(string uid)
    {
        return equipments.Find(x => x.uid == uid);
    }

    //Finds a passive skill item by unique identifier
    public PassiveSkillInfo FindPassiveSkill(string uid)
    {
        return passiveSkills.Find(x => x.uid == uid);
    }

    //Finds a active skill item by unique identifier
    public ActiveSkillInfo FindActiveSkill(string uid)
    {
        return activeSkills.Find(x => x.uid == uid);
    }

    //Removes an inventory item based on its runtime type
    public void RemoveItem(InventoryItemInfo item)
    {
        if (item is EquipmentInfo) RemoveItem(item as EquipmentInfo);
        else if (item is PassiveSkillInfo) RemoveItem(item as PassiveSkillInfo);
        else if (item is ActiveSkillInfo) RemoveItem(item as ActiveSkillInfo);
    }

    //Removes an equipment item from the inventory
    public void RemoveItem(EquipmentInfo equipment)
    {
        this.equipments.Remove(equipment);
        DataManager.instance.SaveGame();
    }

    //Removes a passive skill item from the inventory
    public void RemoveItem(PassiveSkillInfo skill)
    {
        this.passiveSkills.Remove(skill);
        DataManager.instance.SaveGame();
    }

    //Removes a active skill item from the inventory
    public void RemoveItem(ActiveSkillInfo skill)
    {
        this.activeSkills.Remove(skill);
        DataManager.instance.SaveGame();
    }
}
