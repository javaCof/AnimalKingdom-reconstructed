using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Character
{
    public static readonly int MAX_LEVEL = 80;

    public int Idx { get; private set; }
    public string Uid { get; private set; }
    public string Name { get; private set; }
    public int Level { get; private set; }
    public int Exp { get; private set; }
    public int RequiredExp { get; private set; }
    public int MaxHP { get; private set; }
    public float HP { get; set; }
    public int ATK { get; private set; }
    public int DEF { get; private set; }
    public int SPD { get; private set; }
    public float CRC { get; private set; }
    public float CRD { get; private set; }

    public TribeData tribeData;
    public JobData jobData;
    public EquipmentInfo equipWeapon;
    public EquipmentInfo equipArmor;
    public PassiveSkillInfo passiveSkill;
    public List<ActiveSkillInfo> activeSkills;

    public int listIdx;
    public GameObject model;
    public static int characterIdx = 0;

    /// <summary>테스트용 캐릭터 생성 코드</summary>
    public Character(string name, int level, int tribeId, int weaponId, eElementType element, int skillId)
    {
        string uid = System.Guid.NewGuid().ToString();
        EquipmentInfo weapon = new EquipmentInfo(weaponId, element);
        TribeData tribe = DataManager.instance.GetDicTribeDatas()[tribeId];
        JobData job = DataManager.instance.GetDicJobDatas()[weapon.equipmentData.type_id];
        PassiveSkillInfo passiveSkill = new PassiveSkillInfo(tribe.skill_id);
        List<ActiveSkillInfo> activeSkills = new List<ActiveSkillInfo>() {
            weapon.jobSkill,
            new ActiveSkillInfo(skillId),
            weapon.weaponSkill
        };

        weapon.owner_uid = uid;
        passiveSkill.owner_uid = uid;
        foreach (var skill in activeSkills)
            skill.owner_uid = uid;

        Inventory.instance.AddItem(weapon);
        Inventory.instance.AddItem(passiveSkill, false);
        Inventory.instance.AddItem(activeSkills[1]);

        Init(uid, name, level, 0, tribe, job, weapon, null, passiveSkill, activeSkills);
    }

    /// <summary>캐릭터 최초생성</summary>
    public Character(string name, int level, int exp, int tribeId, int jobId, EquipmentInfo eqWeapon, EquipmentInfo eqArmor, ActiveSkillInfo eqSkill)
    {
        string uid = System.Guid.NewGuid().ToString();
        TribeData tribe = DataManager.instance.GetDicTribeDatas()[tribeId];
        JobData job = DataManager.instance.GetDicJobDatas()[jobId];
        PassiveSkillInfo passiveSkill = new PassiveSkillInfo(tribe.skill_id);
        List<ActiveSkillInfo> activeSkills = new List<ActiveSkillInfo>() {
            eqWeapon.jobSkill,
            eqSkill,
            eqWeapon.weaponSkill
        };

        eqWeapon.owner_uid = uid;
        if (eqArmor != null) eqArmor.owner_uid = uid;
        passiveSkill.owner_uid = uid;
        foreach (var skill in activeSkills)
            skill.owner_uid = uid;

        Inventory.instance.AddItem(eqWeapon);
        Inventory.instance.AddItem(eqArmor);
        Inventory.instance.AddItem(passiveSkill, false);
        Inventory.instance.AddItem(activeSkills[1]);

        Init(uid, name, level, exp, tribe, job, eqWeapon, eqArmor, passiveSkill, activeSkills);
    }

    /// <summary>캐릭터정보 로드 (Characterinfo -> Character)</summary>
    public Character(Characterinfo info)
    {
        TribeData tribe = DataManager.instance.GetDicTribeDatas()[info.tribe_id];
        JobData job = DataManager.instance.GetDicJobDatas()[info.job_id];
        EquipmentInfo weapon = Inventory.instance.FindEquipment(info.equip_weapon_uid);
        EquipmentInfo armor = Inventory.instance.FindEquipment(info.equip_armor_uid);
        PassiveSkillInfo passiveSkill = Inventory.instance.FindPassiveSkill(info.tribe_skill_uid);
        List<ActiveSkillInfo> activeSkills = new List<ActiveSkillInfo>() {
            Inventory.instance.FindActiveSkill(weapon.job_skill_uid),
            Inventory.instance.FindActiveSkill(info.equip_skill_uid),
            Inventory.instance.FindActiveSkill(weapon.weapon_skill_uid)
        };

        Init(info.uid, info.name, info.level, info.experience, tribe, job, weapon, armor, passiveSkill, activeSkills);
    }

    /// <summary>캐릭터정보 저장 (Character -> Characterinfo)</summary>
    public Characterinfo GetInfo()
    {
        Characterinfo info = new Characterinfo();

        info.uid = this.Uid;
        info.name = this.Name;
        info.level = this.Level;
        info.experience = this.Exp;
        info.tribe_id = this.tribeData.id;
        info.job_id = this.jobData.id;
        info.equip_weapon_uid = this.equipWeapon.uid;
        info.equip_armor_uid = (this.equipArmor != null) ? this.equipArmor.uid : null;
        info.equip_skill_uid = this.activeSkills[1].uid;
        info.tribe_skill_uid = this.passiveSkill.uid;

        return info;
    }

    /// <summary>캐릭터 설정</summary>
    public void Init(string uid, string name, int level, int exp, TribeData tribe, JobData job, EquipmentInfo eqWeapon, EquipmentInfo eqArmor, PassiveSkillInfo passiveSkill, List<ActiveSkillInfo> activeSkills)
    {
        this.Idx = characterIdx++;
        this.Uid = uid;
        this.Name = name;
        this.Level = level;
        this.Exp = exp;
        this.tribeData = tribe;
        this.jobData = job;
        this.equipWeapon = eqWeapon;
        this.equipArmor = eqArmor;
        this.passiveSkill = passiveSkill;
        this.activeSkills = activeSkills;
        this.model = DataManager.instance.dicModels[tribe.model_name];

        UpdateStat();
        CheckSkills();
    }

    public void UpdateStat()
    {
        LevelData levelData = GetLevelData(this.Level);
        EquipmentInfo weapon = this.equipWeapon;
        EquipmentInfo armor = this.equipArmor;
        PassiveSkillInfo passive = this.passiveSkill;

        float hp = levelData.health + weapon.HP;
        float atk = levelData.attack + weapon.ATK;
        float def = levelData.defense + weapon.DEF;
        float spd = levelData.speed + weapon.SPD;
        float crc = levelData.critical_chance + weapon.CRC;
        float crd = levelData.critical_damage + weapon.CRD;

        if (armor != null)
        {
            hp += armor.HP;
            atk += armor.ATK;
            def += armor.DEF;
            spd += armor.SPD;
            crc += armor.CRC;
            crd += armor.CRD;
        }

        hp *= 1 + passive.HP;
        atk *= 1 + passive.ATK;
        def *= 1 + passive.DEF;
        spd *= 1 + passive.SPD;
        crc += passive.CRC;
        crd += passive.CRD;

        this.MaxHP = (int)hp;
        this.ATK = (int)atk;
        this.DEF = (int)def;
        this.SPD = (int)spd;
        this.CRC = crc;
        this.CRD = crd;

        this.HP = this.MaxHP;
        this.RequiredExp = levelData.experience_next;
    }

    public void UpdateSkills()
    {
        this.activeSkills[0].owner_uid = null;
        this.activeSkills[2].owner_uid = null;
        this.activeSkills[0] = this.equipWeapon.jobSkill;
        this.activeSkills[2] = this.equipWeapon.weaponSkill;
        this.activeSkills[0].owner_uid = this.Uid;
        this.activeSkills[2].owner_uid = this.Uid;
    }

    private void CheckSkills()
    {
        for (int i = 0; i < this.activeSkills.Count; i++)
            if (this.activeSkills[i].skillData.skill_type != i + 1)
                Debug.LogErrorFormat("SKILL ERROR, INCORRECT SKILL : {0}({1})", this.Name, i + 1);
    }

    public void Equip(EquipmentInfo equipment)
    {
        if (equipment.IsWeapon())
        {
            this.equipWeapon.owner_uid = null;
            this.equipWeapon = equipment;
            this.equipWeapon.owner_uid = this.Uid;
            this.jobData = DataManager.instance.GetDicJobDatas()[equipment.equipmentData.type_id];
        }
        else if (this.equipArmor != null)
        {
            this.equipArmor.owner_uid = null;
            this.equipArmor = equipment;
            this.equipArmor.owner_uid = this.Uid;
        }
        else
        {
            this.equipArmor = equipment;
            this.equipArmor.owner_uid = this.Uid;
        }

        UpdateStat();
        UpdateSkills();
    }

    public void UnEquipArmor()
    {
        if (this.equipArmor != null) this.equipArmor.owner_uid = null;
        this.equipArmor = null;

        UpdateStat();
    }

    public struct LevelExp
    {
        public int level;
        public int exp;
        public int expNext;

        public LevelExp(int level, int exp)
        {
            this.level = level;
            this.exp = exp;
            this.expNext = GetLevelData(level).experience_next;
        }
    }

    public static LevelData GetLevelData(int level)
    {
        return DataManager.instance.GetListLevelDatas()[level - 1];
    }

    public LevelExp GetLevelExp()
    {
        return new LevelExp(this.Level, this.Exp);
    }

    public int GetExpNext()
    {
        return GetLevelExp().expNext;
    }

    public static LevelExp LevelCalculation(LevelExp levelExp, int exp)
    {
        int toLevel = levelExp.level;
        int expSum = levelExp.exp + exp;

        int expNext = Character.GetLevelData(toLevel).experience_next;
        while (expSum >= expNext)
        {
            toLevel++;
            if (toLevel == MAX_LEVEL) return new LevelExp(toLevel, 0);

            expSum -= expNext;
            expNext = Character.GetLevelData(toLevel).experience_next;
        }

        return new LevelExp(toLevel, expSum);
    }

    public void AddExp(int exp)
    {
        if (this.Level == MAX_LEVEL) return;

        LevelExp toLevelExp = LevelCalculation(GetLevelExp(), exp);

        this.Exp = toLevelExp.exp;
        if (toLevelExp.level > this.Level)
        {
            this.Level = toLevelExp.level;
            OnLevelUp();
        }

        DataManager.instance.SaveGame();
    }

    private void OnLevelUp()
    {
        UpdateStat();
        //Firebase.Analytics.FirebaseAnalytics.LogEvent("level_up", "Level", this.Level);
    }

    public void UpdateModel(GameObject model)
    {
        var unitModel = model.GetComponent<UnitModel>();

        switch (this.jobData.id)
        {
            case 201:
                unitModel.WarriorWeapon.SetActive(true);
                unitModel.Shield.SetActive(true);
                unitModel.WizardWeapon.SetActive(false);
                break;
            case 202:
                unitModel.WarriorWeapon.SetActive(false);
                unitModel.Shield.SetActive(true);
                unitModel.WizardWeapon.SetActive(true);
                break;
        }
    }

    public static void ResetIdx(List<Character> characters)
    {
        var characterList = characters.OrderBy(x => x.Idx).ToList();
        for (int i = 0; i < characterList.Count; i++)
            characterList[i].Idx = i;
    }
}
