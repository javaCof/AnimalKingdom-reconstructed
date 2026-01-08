using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class InventoryMain : MonoBehaviour
{
    [HideInInspector]
    public bool IS_DEBUG = true;

    public struct SceneInfo
    {
        public int itemType;
        public InventoryItemInfo item;
        public bool showTabs;
        public bool showHiddenItems;
        public bool selectMode;

        public SceneInfo(int itemType, InventoryItemInfo item, bool showTabs, bool showHiddenItems, bool selectMode)
        {
            this.itemType = itemType;
            this.item = item;
            this.showTabs = showTabs;
            this.showHiddenItems = showHiddenItems;
            this.selectMode = selectMode;
        }
    }
    public static SceneInfo sceneInfo;

    public GameObject consumableIconPrefab;
    public GameObject equipmentIconPrefab;
    public GameObject skillIconPrefab;

    public Button btnBack;
    public UIInventoryInfo uIInventoryInfo;

    public GameObject equipmentGrid;
    public GameObject skillGrid;

    private ConsumableInfo expPotion;
    private List<EquipmentInfo> equipmentList;
    private List<ActiveSkillInfo> activeSkillList;

    private Dictionary<InventoryItemInfo, GameObject> dicItemIcons;
    private InventoryItemInfo curInventoryItem;

    public UITabs uITabs;
    private int curTabIdx;

    public Button btnSortType;
    public Button btnSortEnforce;
    public Button btnSortEquip;

    private void Start()
    {
        Inventory.instance.AddConsumable(new ConsumableInfo(20001, 100));

        if (IS_DEBUG)
        {
            Debug.Log("InventoryMain : Debug mode");

            FindObjectOfType<AppContents>().SetContents();
            DataManager.instance.LoadAllData();
            DataManager.instance.NewGame();

            this.btnBack.gameObject.SetActive(false);

            Init();
        }

        SoundManager.SetVolumeSFX(App.instance.fxVolume);
        SoundManager.SetVolumeMusic(App.instance.backgroundVolume);
    }

    public void Init()
    {
        this.btnBack.onClick.AddListener(() => {
            App.instance.LoadScene(App.eSceneType.LobbyScene);
        });

        Init(new SceneInfo(0, null, true, true, false));
    }

    public void Init(SceneInfo sceneInfo, App.eSceneType backScene)
    {
        this.btnBack.onClick.AddListener(() => {
            switch (backScene)
            {
                case App.eSceneType.LobbyScene:
                    App.instance.LoadScene(App.eSceneType.LobbyScene);
                    break;
                case App.eSceneType.CharacterScene:
                    App.instance.LoadCharacterScene(CharacterMain.sceneInfo, App.eSceneType.LobbyScene);
                    break;
            }
        });

        Init(sceneInfo);
    }

    public void Init(SceneInfo sceneInfo)
    {
        this.uIInventoryInfo.Init(sceneInfo.selectMode);
        this.uIInventoryInfo.onItemChange = () => UpdateIcon(false);
        this.uIInventoryInfo.onItemRemove = RemoveItem;

        this.uITabs.onTabChanged = (idx) => {
            this.curTabIdx = idx;
        };
        this.uITabs.Init();
        this.uITabs.gameObject.SetActive(!sceneInfo.selectMode);

        this.btnSortType.onClick.AddListener(() => {
            SortItems(this.curTabIdx, 0);
            UpdateGrid();
            this.uIInventoryInfo.ResetUI(null as InventoryItemInfo);
        });
        this.btnSortEnforce.onClick.AddListener(() => {
            SortItems(this.curTabIdx, 1);
            UpdateGrid();
            this.uIInventoryInfo.ResetUI(null as InventoryItemInfo);
        });
        this.btnSortEquip.onClick.AddListener(() => {
            SortItems(this.curTabIdx, 2);
            UpdateGrid();
            this.uIInventoryInfo.ResetUI(null as InventoryItemInfo);
        });

        this.expPotion = Inventory.instance.consumables.Find(x => x.item_id == 20001);
        this.equipmentList = (sceneInfo.itemType == 2) ? Inventory.instance.equipments.FindAll(x => x.IsWeapon()) :
            (sceneInfo.itemType == 3) ? Inventory.instance.equipments.FindAll(x => !x.IsWeapon()) :
            Inventory.instance.equipments.ToList();
        this.activeSkillList = Inventory.instance.activeSkills.ToList();

        SortItems(0, 0);
        SortItems(1, 0);

        this.dicItemIcons = new Dictionary<InventoryItemInfo, GameObject>();
        UpdateGrid();

        this.uITabs.ChangeTab((sceneInfo.itemType < 2) ? sceneInfo.itemType : 0);
        if (sceneInfo.item != null) SelectItem(sceneInfo.item);
    }

    private void UpdateIcon(bool isSelected)
    {
        if (this.curInventoryItem is ConsumableInfo)
            this.dicItemIcons[this.curInventoryItem].GetComponent<ConsumableIcon>().Init(this.curInventoryItem as ConsumableInfo, isSelected);
        else if (this.curInventoryItem is EquipmentInfo)
            this.dicItemIcons[this.curInventoryItem].GetComponent<EquipmentIcon>().Init(this.curInventoryItem as EquipmentInfo, isSelected, true);
        else if (this.curInventoryItem is ActiveSkillInfo)
            this.dicItemIcons[this.curInventoryItem].GetComponent<SkillIcon>().InitSkillstone(this.curInventoryItem as ActiveSkillInfo, isSelected, true);
    }

    private void UpdateGrid()
    {
        foreach (var icon in dicItemIcons.Values)
            Destroy(icon);
        this.dicItemIcons = new Dictionary<InventoryItemInfo, GameObject>();

        foreach (var equipment in this.equipmentList)
        {
            if (!sceneInfo.showHiddenItems && !equipment.is_visible) continue;

            GameObject iconGo = Instantiate(equipmentIconPrefab, this.equipmentGrid.transform);
            iconGo.GetComponent<EquipmentIcon>().Init(equipment, false, true);
            iconGo.GetComponent<Button>().onClick.AddListener(() => SelectItem(equipment));

            this.dicItemIcons.Add(equipment, iconGo);
        }

        if (this.expPotion.is_visible) {
            GameObject iconGo = Instantiate(consumableIconPrefab, this.skillGrid.transform);
            iconGo.GetComponent<ConsumableIcon>().Init(this.expPotion, false);
            iconGo.GetComponent<Button>().onClick.AddListener(() => SelectItem(this.expPotion));
            this.dicItemIcons.Add(this.expPotion, iconGo);
        }

        foreach (var skill in this.activeSkillList)
        {
            //if (!sceneInfo.showHiddenItems && !skill.is_visible) continue;
            if (!skill.is_visible) continue;

            GameObject iconGo = Instantiate(skillIconPrefab, this.skillGrid.transform);
            iconGo.GetComponent<SkillIcon>().InitSkillstone(skill, false, true);
            iconGo.GetComponent<Button>().onClick.AddListener(() => SelectItem(skill));

            this.dicItemIcons.Add(skill, iconGo);
        }
    }

    private void RemoveItem()
    {
        GameObject icon = this.dicItemIcons[this.curInventoryItem];
        this.dicItemIcons.Remove(this.curInventoryItem);
        Destroy(icon);

        if (this.curInventoryItem is EquipmentInfo) this.equipmentList.Remove(this.curInventoryItem as EquipmentInfo);
        else if (this.curInventoryItem is ActiveSkillInfo) this.activeSkillList.Remove(this.curInventoryItem as ActiveSkillInfo);

        this.curInventoryItem = null;
    }

    private void SelectItem(InventoryItemInfo inventoryItem)
    {
        UpdateIcon(false);

        this.curInventoryItem = inventoryItem;
        UpdateIcon(true);

        this.uIInventoryInfo.UpdateUI(inventoryItem);
    }

    private void SortItems(int tabIdx, int mode)
    {
        switch (tabIdx)
        {
            case 0:
                switch (mode)
                {
                    case 0:
                        this.equipmentList = this.equipmentList.OrderBy(x => !x.IsWeapon()).ThenBy(x => x.equipmentData.type_id).ThenBy(x => x.equipmentData.id).ThenByDescending(x => x.enforce).ToList();
                        break;
                    case 1:
                        this.equipmentList = this.equipmentList.OrderByDescending(x => x.enforce).ThenBy(x => !x.IsWeapon()).ThenBy(x => x.equipmentData.type_id).ThenBy(x => x.equipmentData.id).ToList();
                        break;
                    case 2:
                        this.equipmentList = this.equipmentList.OrderBy(x => x.owner_uid == null).ThenBy(x => !x.IsWeapon()).ThenByDescending(x => x.enforce).ToList();
                        break;
                }
                break;
            case 1:
                switch (mode)
                {
                    case 0:
                        this.activeSkillList = this.activeSkillList.OrderBy(x => x.skillData.skill_type).ThenBy(x => x.skillData.id).ThenByDescending(x => x.enforce).ToList();
                        break;
                    case 1:
                        this.activeSkillList = this.activeSkillList.OrderByDescending(x => x.enforce).ThenBy(x => x.skillData.id).ToList();
                        break;
                    case 2:
                        this.activeSkillList = this.activeSkillList.OrderBy(x => x.owner_uid == null).ThenByDescending(x => x.enforce).ThenBy(x => x.skillData.id).ToList();
                        break;
                }
                break;
        }
    }
}
