using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.U2D;

public class UICharacterEquipment : MonoBehaviour
{
    public System.Action onCharacterChange;

    private Character character;
    private EquipmentInfo equipment;

    public GameObject contentsGo;

    public UITabs uITabs;

    public GameObject infoGo;
    public GameObject infoContentsGo;
    public Text nameText;
    public EquipmentIcon equipmentIcon;
    public Text enforceText;
    public Text hpText;
    public Text atkText;
    public Text defText;
    public Text spdText;
    public Text crcText;
    public Text crdText;

    public Transform infoPos;
    public Transform enforcePos;
    
    public GameObject menuGo;
    public Button btnEnforce;
    public Button btnChange;
    public Button btnUnequip;
    public Text btnChangeText;

    public GameObject enforceGoldInfoGo;
    public Text enforceGoldText;
    private int enforceGold;

    public GameObject confirmGo;
    public Button btnEnforceOK;
    public Button btnEnforceCancel;

    public void Init()
    {
        this.uITabs.onTabChanged = (idx) => {
            switch (idx)
            {
                case 0:
                    UpdateUI(character.equipWeapon);
                    break;
                case 1:
                    UpdateUI(character.equipArmor);
                    break;
            }
        };
        this.uITabs.Init();
        
        this.btnEnforce.onClick.AddListener(() => {
            ToEnforce();
        });
        this.btnChange.onClick.AddListener(() => {
            ChangeEquipment();
        });
        this.btnUnequip.onClick.AddListener(() => {
            UnequipEquipment();
        });
        this.btnEnforceOK.onClick.AddListener(() => {
            EnforceEquipment();
        });
        this.btnEnforceCancel.onClick.AddListener(() => {
            UpdateUI(equipment);
        });
    }

    public void UpdateUI()
    {
        this.contentsGo.SetActive(false);
    }

    public void UpdateUI(Character character, int subtabIdx = 0)
    {
        this.character = character;

        switch (subtabIdx)
        {
            case 0:
                UpdateUI(character.equipWeapon);
                break;
            case 1:
                UpdateUI(character.equipArmor);
                break;
        }
    }

    public void UpdateUI(EquipmentInfo equipment)
    {
        this.equipment = equipment;
        this.nameText.text = (equipment != null) ? equipment.equipmentData.name : "장비 없음";

        if (equipment != null)
        {
            this.equipmentIcon.Init(equipment, false, false);

            this.enforceText.text = string.Format("+{0}", equipment.enforce);
            this.hpText.text = string.Format("+{0}", equipment.HP);
            this.atkText.text = string.Format("+{0}", equipment.ATK);
            this.defText.text = string.Format("+{0}", equipment.DEF);
            this.spdText.text = string.Format("+{0}", equipment.SPD);
            this.crcText.text = string.Format("+{0:0.0}%", equipment.CRC * 100);
            this.crdText.text = string.Format("+{0:0.0}%", equipment.CRD * 100);

            this.hpText.color = Color.white;
            this.atkText.color = Color.white;
            this.defText.color = Color.white;
            this.spdText.color = Color.white;
            this.crcText.color = Color.white;
            this.crdText.color = Color.white;
        }

        this.uITabs.SelectTab((equipment != null && equipment.IsWeapon()) ? 0 : 1);
        this.infoGo.transform.localPosition = this.infoPos.localPosition;

        this.btnChangeText.text = (equipment != null) ? "교체" : "장착";
        SetButtonActive(this.btnEnforce, this.equipment != null && equipment.IsEnforceable());
        SetButtonActive(this.btnUnequip, this.equipment != null && !equipment.IsWeapon());

        this.infoContentsGo.SetActive(equipment != null);
        this.uITabs.gameObject.SetActive(true);
        this.menuGo.SetActive(true);
        this.confirmGo.SetActive(false);
        this.enforceGoldInfoGo.SetActive(false);
        this.contentsGo.SetActive(true);
    }

    private void ToEnforce()
    {
        this.enforceGold = (this.equipment.enforce + 1) * 1000;

        EquipmentInfo enforced = this.equipment.GetNextEnforceInfo();

        this.enforceText.text = string.Format("+{0}", enforced.enforce);
        this.hpText.text = string.Format("+{0}", enforced.HP);
        this.atkText.text = string.Format("+{0}", enforced.ATK);
        this.defText.text = string.Format("+{0}", enforced.DEF);
        this.spdText.text = string.Format("+{0}", enforced.SPD);
        this.crcText.text = string.Format("+{0:0.0}%", enforced.CRC * 100);
        this.crdText.text = string.Format("+{0:0.0}%", enforced.CRD * 100);

        this.hpText.color = (enforced.HP > equipment.HP) ? Color.red : Color.white;
        this.atkText.color = (enforced.ATK > equipment.ATK) ? Color.red : Color.white;
        this.defText.color = (enforced.DEF > equipment.DEF) ? Color.red : Color.white;
        this.spdText.color = (enforced.SPD > equipment.SPD) ? Color.red : Color.white;
        this.crcText.color = (enforced.CRC > equipment.CRC) ? Color.red : Color.white;
        this.crdText.color = (enforced.CRD > equipment.CRD) ? Color.red : Color.white;

        SetButtonActive(this.btnEnforceOK, DataManager.instance.currencyGold >= enforceGold);
        this.enforceGoldText.text = string.Format("{0}", enforceGold);
        this.enforceGoldText.color = (DataManager.instance.currencyGold >= enforceGold) ? Color.white : Color.red;

        this.infoGo.transform.localPosition = this.enforcePos.localPosition;
        this.uITabs.gameObject.SetActive(false);
        this.menuGo.SetActive(false);
        this.confirmGo.SetActive(true);
        this.enforceGoldInfoGo.SetActive(true);
    }

    private void EnforceEquipment()
    {
        this.equipment.Enforce();
        DataManager.instance.currencyGold -= this.enforceGold;
        UpdateUI(this.equipment);
        DataManager.instance.SaveGame();
    }

    private void ChangeEquipment()
    {
        CharacterMain.sceneInfo = new CharacterMain.SceneInfo(this.character, 2, (this.equipment != null && this.equipment.equipmentData.type_id != -1) ? 0 : 1);
        App.instance.LoadInventoryScene(new InventoryMain.SceneInfo((this.equipment != null && this.equipment.equipmentData.type_id != -1) ? 2 : 3, this.equipment, false, true, true), App.eSceneType.CharacterScene);
    }

    private void UnequipEquipment()
    {
        this.character.UnEquipArmor();
        UpdateUI(character.equipArmor);
        DataManager.instance.SaveGame();
    }

    private void SetButtonActive(Button button, bool condition)
    {
        button.enabled = condition;
        button.gameObject.GetComponent<Image>().color = (condition) ? Color.white : new Color(0.5f, 0.5f, 0.5f);
    }
}
