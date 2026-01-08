using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public sealed class Inventory
{
    public static readonly Inventory instance = new Inventory();

    public List<ConsumableInfo> consumables = new List<ConsumableInfo>();
    public List<EquipmentInfo> equipments = new List<EquipmentInfo>();
    public List<PassiveSkillInfo> passiveSkills = new List<PassiveSkillInfo>();
    public List<ActiveSkillInfo> activeSkills = new List<ActiveSkillInfo>();

    private Inventory() { }

    /// <summary>인벤토리 로드 (InventoryInfo -> Inventory)</summary>
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

    /// <summary>인벤토리 저장 (Inventory -> InventoryInfo)</summary>
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

    public void AddItem(EquipmentInfo item, bool isVisible = true)
    {
        item.uid = Guid.NewGuid().ToString();
        item.is_visible = isVisible;
        equipments.Add(item);
    }

    public void AddItem(PassiveSkillInfo item, bool isVisible = true)
    {
        item.uid = Guid.NewGuid().ToString();
        item.is_visible = isVisible;
        passiveSkills.Add(item);
    }

    public void AddItem(ActiveSkillInfo item, bool isVisible = true)
    {
        item.uid = Guid.NewGuid().ToString();
        item.is_visible = isVisible;
        activeSkills.Add(item);
    }

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
    }

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
    }

    public void UseConsumable(ConsumableInfo consumable, int count)
    {
        if (consumable.amount >= count)
        {
            consumable.amount -= count;

            if (consumable.item_id == 20001)
                DataManager.instance.itemExpPotion -= count;
        }
    }

    public void UseExpPotion(int count)
    {
        UseConsumable(FindConsumable(20001), count);
    }

    public ConsumableInfo FindConsumable(int id)
    {
        return consumables.Find(x => x.item_id == id);
    }

    public EquipmentInfo FindEquipment(string uid)
    {
        return equipments.Find(x => x.uid == uid);
    }

    public PassiveSkillInfo FindPassiveSkill(string uid)
    {
        return passiveSkills.Find(x => x.uid == uid);
    }

    public ActiveSkillInfo FindActiveSkill(string uid)
    {
        return activeSkills.Find(x => x.uid == uid);
    }

    public void RemoveItem(InventoryItemInfo item)
    {
        if (item is EquipmentInfo) RemoveItem(item as EquipmentInfo);
        else if (item is PassiveSkillInfo) RemoveItem(item as PassiveSkillInfo);
        else if (item is ActiveSkillInfo) RemoveItem(item as ActiveSkillInfo);
    }

    public void RemoveItem(EquipmentInfo equipment)
    {
        this.equipments.Remove(equipment);
    }

    public void RemoveItem(PassiveSkillInfo skill)
    {
        this.passiveSkills.Remove(skill);
    }

    public void RemoveItem(ActiveSkillInfo skill)
    {
        this.activeSkills.Remove(skill);
    }
}
