using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;

public class LobbyMain : MonoBehaviour
{
    public Button btnShop;
    public Button btnCharacter;
    public Button btnInventory;
    public Button btnWorldmap;
    public SceneLoading sceneLoading;

    public List<Button> tutorialImages;
    public GameObject tutorialImagesGo;

    public static string repCharacterUid;
    public Transform modelPos;

    private void Awake()
    {
        Init();
    }

    public void Init()
    {
        foreach (var img in tutorialImages)
            img.onClick.AddListener(() => { img.gameObject.SetActive(false); });
        tutorialImages[0].onClick.AddListener(() => {
            DataManager.instance.SaveGame();
        });
        this.tutorialImagesGo.SetActive(!File.Exists(Application.persistentDataPath + "/userinfo.json"));

        this.btnShop.onClick.AddListener(() => {
            SoundManager.PlaySFX("Pop (3)");
            App.instance.LoadScene(App.eSceneType.ShopScene);
        });
        this.btnCharacter.onClick.AddListener(() => {
            SoundManager.PlaySFX("Pop (3)");
            App.instance.LoadScene(App.eSceneType.CharacterScene);
        });
        this.btnInventory.onClick.AddListener(() => {
            SoundManager.PlaySFX("Pop (3)");
            App.instance.LoadScene(App.eSceneType.InventoryScene);
        });
        this.btnWorldmap.onClick.AddListener(() => {
            SoundManager.PlaySFX("Pop (3)");
            this.sceneLoading.gameObject.SetActive(true);
            this.sceneLoading.LoadScene(SceneLoading.eScene.WorldMapScene);
            //App.instance.LoadScene(App.eSceneType.WorldMapScene);
        });

        Character rep = DataManager.instance.ownedCharacters.Find(x => x.Uid == repCharacterUid);
        GameObject model = Instantiate(rep.model, this.modelPos);
        rep.UpdateModel(model);

        SoundManager.SetVolumeSFX(App.instance.fxVolume);
        SoundManager.SetVolumeMusic(App.instance.backgroundVolume);
    }
}
