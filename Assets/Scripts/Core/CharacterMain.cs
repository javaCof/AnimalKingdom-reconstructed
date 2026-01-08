using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class CharacterMain : MonoBehaviour
{
    [HideInInspector]
    public bool IS_DEBUG = true;

    public struct SceneInfo
    {
        public Character character;
        public int tabIdx;
        public int subTabIdx;

        public SceneInfo(Character character, int tabIdx, int subTabIdx)
        {
            this.character = character;
            this.tabIdx = tabIdx;
            this.subTabIdx = subTabIdx;
        }
    }
    public static SceneInfo sceneInfo;

    public GameObject characterIconPrefab;

    public Button btnBack;
    public UICharacterInfo uICharacterInfo;
    public UICharacterLevel uICharacterLevel;
    public UICharacterEquipment uICharacterEquipment;
    public UICharacterSkill uICharacterSkill;

    public Transform listGrid;
    public Transform modelPos;
    public Button btnSetRep;
    public Image repIcon;

    private List<Character> characterList;
    private Dictionary<string, GameObject> dicCharacterIcons;
    private Character curCharacter;
    private GameObject curModel;

    public UITabs uITabs;
    private int curTabIdx;

    private void Start()
    {
        if (IS_DEBUG)
        {
            Debug.Log("CharacterMain : Debug mode");

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
        /*Back 버튼 설정*/
        this.btnBack.onClick.AddListener(() => {
            App.instance.LoadScene(App.eSceneType.LobbyScene);
        });

        Init(new SceneInfo(null, 0, -1));
    }

    public void Init(SceneInfo sceneInfo, App.eSceneType backScene)
    {
        /*Back 버튼 설정*/
        this.btnBack.onClick.AddListener(() => {
            switch (backScene)
            {
                case App.eSceneType.LobbyScene:
                    App.instance.LoadScene(App.eSceneType.LobbyScene);
                    break;
                case App.eSceneType.InventoryScene:
                    App.instance.LoadInventoryScene(InventoryMain.sceneInfo, App.eSceneType.LobbyScene);
                    break;
            }
        });

        Init(sceneInfo);
    }

    private void Init(SceneInfo sceneInfo)
    {
        /*모델 설정*/
        this.btnSetRep.onClick.AddListener(SetRep);

        /*뷰 UI 설정*/
        this.uICharacterLevel.onCharacterChange = UpdateIcon;
        this.uICharacterEquipment.onCharacterChange = UpdateIcon;

        this.uICharacterInfo.Init();
        this.uICharacterLevel.Init();
        this.uICharacterEquipment.Init();
        this.uICharacterSkill.Init();

        this.uICharacterInfo.onCharacterRemove = RemoveCharacter;

        /*탭 설정*/
        this.curTabIdx = sceneInfo.tabIdx;
        this.uITabs.onTabChanged = UpdateUI;
        this.uITabs.Init();

        /*캐릭터 아이콘 설정*/
        this.dicCharacterIcons = new Dictionary<string, GameObject>();
        ResetListUI();

        /*시작 UI 설정*/
        SelectCharcter((sceneInfo.character != null) ? sceneInfo.character : DataManager.instance.ownedCharacters.Find(x => x.Uid == LobbyMain.repCharacterUid));
        this.uITabs.ChangeTab(sceneInfo.tabIdx);
        if (sceneInfo.subTabIdx != -1) ShowTabContent(sceneInfo.tabIdx, sceneInfo.subTabIdx);
    }

    private void SelectCharcter(Character character)
    {
        if (this.curCharacter != null) this.dicCharacterIcons[this.curCharacter.Uid].GetComponent<CharacterIcon>().Init(this.curCharacter, false, true);
        if (this.curModel != null) Destroy(this.curModel);
        
        this.curCharacter = character;
        this.dicCharacterIcons[this.curCharacter.Uid].GetComponent<CharacterIcon>().Init(this.curCharacter, true, true);
        this.curModel = Instantiate(character.model, this.modelPos);
        character.UpdateModel(this.curModel);

        this.uITabs.ChangeTab(0);
    }

    private void SetRep()
    {
        if (this.curCharacter != null)
        {
            LobbyMain.repCharacterUid = this.curCharacter.Uid;
            UpdateUI(this.curTabIdx);
        }
    }

    private void ShowTabContent(int tabIdx, int subTabIdx)
    {
        switch (tabIdx)
        {
            case 2:
                this.uICharacterEquipment.UpdateUI(this.curCharacter, subTabIdx);
                break;
            case 3:
                this.uICharacterSkill.UpdateUI(this.curCharacter, subTabIdx);
                break;
        }
    }

    private void UpdateUI(int tabIdx)
    {
        this.curTabIdx = tabIdx;
        this.uITabs.gameObject.SetActive(true);
        this.repIcon.sprite = DataManager.instance.iconAtlas.GetSprite((this.curCharacter != null && this.curCharacter.Uid == LobbyMain.repCharacterUid) ? "icon_star_on" : "icon_star_off");

        switch (tabIdx)
        {
            case 0:
                this.uICharacterInfo.UpdateUI(this.curCharacter);
                break;
            case 1:
                this.uICharacterLevel.UpdateUI(this.curCharacter);
                break;
            case 2:
                this.uICharacterEquipment.UpdateUI(this.curCharacter);
                break;
            case 3:
                this.uICharacterSkill.UpdateUI(this.curCharacter);
                break;
        }
    }

    private void ResetListUI()
    {
        this.characterList = DataManager.instance.ownedCharacters.ToList();
        this.characterList = this.characterList.OrderByDescending(x => x.Level).ToList();

        foreach (var icon in dicCharacterIcons.Values)
            Destroy(icon);
        this.dicCharacterIcons = new Dictionary<string, GameObject>();

        foreach (var character in this.characterList)
        {
            GameObject iconGo = Instantiate(characterIconPrefab, this.listGrid);
            iconGo.GetComponent<CharacterIcon>().Init(character, false, true);
            iconGo.GetComponent<Button>().onClick.AddListener(() => SelectCharcter(character));

            this.dicCharacterIcons.Add(character.Uid, iconGo);
        }
    }

    private void UpdateIcon()
    {
        dicCharacterIcons[this.curCharacter.Uid].GetComponent<CharacterIcon>().Init(this.curCharacter, true, true);
        ResetListUI();
    }

    private void RemoveCharacter()
    {
        /*아이콘 제거*/
        GameObject icon = this.dicCharacterIcons[this.curCharacter.Uid];
        this.dicCharacterIcons.Remove(this.curCharacter.Uid);
        Destroy(icon);

        /*모델 제거*/
        Destroy(this.curModel);
        this.curModel = null;

        /*캐릭터 리스트 설정*/
        this.characterList.Remove(this.curCharacter);

        /*대표 캐릭터 설정*/
        if (this.curCharacter.Uid == LobbyMain.repCharacterUid)
            LobbyMain.repCharacterUid = DataManager.instance.ownedCharacters[0].Uid;

        /*캐릭터 참조 초기화*/
        this.curCharacter = null;

        /*UI 설정*/
        this.repIcon.sprite = DataManager.instance.iconAtlas.GetSprite("icon_star_off");
        this.uITabs.gameObject.SetActive(false);

        /*게임 저장*/
        DataManager.instance.SaveGame();
    }
}
