using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

using Assets.GameProcessScripts;
using UnityEngine.UI;
using TMPro;
using Unity.Collections;

public class UIManager : NetworkBehaviour
{

    public static UIManager Instance { get; private set; }

    [Header("Prefubs")]
    [SerializeField] public GameObject HandLetterUI;
    [SerializeField] public GameObject DragLetterUI;
    [SerializeField] HandLetter letterPrefub;
    [SerializeField] NotificationPopUp notificationPopUp;

    [Header("UIElements")]
    [SerializeField] private Button explodeBtn;
    [SerializeField] private Button fixLettersBtn;
    [SerializeField] private Slider letterTimerBar;
    [SerializeField] private Button mainMenuBtn;
    [SerializeField] private Slider hpBar;

    [Header("Store")]
    [SerializeField] private TextMeshProUGUI availablePointsText;
    [SerializeField] private StoreBtn explodeStoreBtn;
    [SerializeField] private StoreBtn refreshStoreBtn;
    [SerializeField] private StoreBtn healStoreBtn;
    [SerializeField] private StoreBtn timerStoreBtn;

    [Space(10)]
    [SerializeField] private LetterStickUI[] tiles;

    private List<HandLetter> sentLetters;

    PlayersGameManager playersGameManager;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }

        explodeBtn.onClick.AddListener(() =>
        {
            playersGameManager.ExplodeServerRpc();
        });
        fixLettersBtn.onClick.AddListener(FixLetters);
        mainMenuBtn.onClick.AddListener(() =>
        {
            Loader.Load(Loader.Scene.MainMenuScene);
        });
    }

    private void Start()
    {
        playersGameManager = PlayersGameManager.Instance;

        letterTimerBar.maxValue = playersGameManager.GetDefaultLetterTimerCap();
        hpBar.maxValue = playersGameManager.GetMaxHp();
        hpBar.value = hpBar.maxValue;

        availablePointsText.text = $"Количество очков: {0}";
        explodeStoreBtn.SetActive(false);
        explodeStoreBtn.SetPrice(playersGameManager.GetExplodePrice());
        explodeStoreBtn.SetOnClickListener(() => { playersGameManager.ExplodeStoreServerRpc(); });
        refreshStoreBtn.SetActive(false);
        refreshStoreBtn.SetPrice(playersGameManager.GetRefreshPrice());
        refreshStoreBtn.SetOnClickListener(() => { Debug.Log("yay2"); });
        healStoreBtn.SetActive(false);
        healStoreBtn.SetPrice(playersGameManager.GetHealPrice());   
        healStoreBtn.SetOnClickListener(() => { Debug.Log("yay3"); });
        timerStoreBtn.SetActive(false);
        timerStoreBtn.SetPrice(playersGameManager.GetTimerPrice());
        timerStoreBtn.SetOnClickListener(() => { Debug.Log("yay4"); });

        sentLetters = new();
    }

    private void Update()
    {
        letterTimerBar.value += Time.deltaTime;
        if (letterTimerBar.value >= letterTimerBar.maxValue) letterTimerBar.value = 0;
    }

    [ClientRpc]
    public void SetAvailablePointsClientRpc(int points, ClientRpcParams clientRpcParams = default)
    {
        availablePointsText.text = $"Количество очков: {points}";
        explodeStoreBtn.SetActive(points >= playersGameManager.GetExplodePrice());
        refreshStoreBtn.SetActive(points >= playersGameManager.GetRefreshPrice());
        healStoreBtn.SetActive(points >= playersGameManager.GetHealPrice());
        timerStoreBtn.SetActive(points >= playersGameManager.GetTimerPrice());
    }

    public void FixLetters()
    {
        NetLetterInfoCollection updatedTiles = new();
        sentLetters.Clear();

        foreach (LetterStickUI tile in tiles)
        {
            if (tile.Letter != null && tile.Letter.Tile != null)
            {
                LetterStick fieldTile = tile.Letter.Tile;

                updatedTiles.Add(fieldTile.XCoord, fieldTile.YCoord, tile.Letter.Value);
                sentLetters.Add(tile.Letter);

                tile.Letter = null;
            }
        }

        TileFieldManager.Instance.UpdateFieldServerRpc(updatedTiles);
    }

    [ClientRpc]
    public void UpdateLetterTimerBarClientRpc(float timerValue, ClientRpcParams clientRpcParams = default)
    {
        letterTimerBar.value = timerValue;
    }

    [ClientRpc]
    public void UpdateHpBarClientRpc(float hpValue, ClientRpcParams clientRpcParams = default)
    {
        hpBar.value = hpValue;
    }


    [ClientRpc]
    public void GetLetterClientRpc(string letterValue, ClientRpcParams clientRpcParams = default)
    {
        foreach (LetterStickUI tile in tiles)
        {
            if (tile.Letter == null)
            {
                HandLetter letter = Instantiate(letterPrefub);

                letter.ChangeValue(letterValue);

                letter.StickUI(tile);
                return;
            }
        }
    }

    [ClientRpc]
    public void RemoveSentLettersClientRpc(ClientRpcParams clientRpcParams = default)
    {
        foreach (HandLetter letter in sentLetters)
        {
            letter.TileUI.Letter = null;
            Destroy(letter.gameObject);
        }
    }

    [ClientRpc]
    public void WordNotValidatedPopUpClientRpc(ClientRpcParams clientRpcParams = default)
    {
        foreach (HandLetter letterGameObject in sentLetters)
        {
            letterGameObject.ResetToUI();
        }

        NotificationPopUp popUp = Instantiate(notificationPopUp, gameObject.transform.parent);
        popUp.SetPopUpText("Некорректное слово");
        StartCoroutine(DeletePopUp(popUp));
    }

    [ClientRpc]
    public void ExplodeHandClientRpc(ClientRpcParams clientRpcParams = default)
    {
        foreach (LetterStickUI tile in tiles)
        {
            if (tile.Letter != null)
            {
                Destroy(tile.Letter.gameObject);
            }
        }
    }

    [ClientRpc]
    public void DedgeClientRpc(ClientRpcParams clientRpcParams = default)
    {
        NotificationPopUp popUp = Instantiate(notificationPopUp, gameObject.transform.parent);
        popUp.SetPopUpText("Omae wa mou... Shindeiru!");
        StartCoroutine(DeletePopUp(popUp));
    }

    [ClientRpc]
    public void MakeDedgeClientRpc(ClientRpcParams clientRpcParams = default)
    {
        NotificationPopUp popUp = Instantiate(notificationPopUp, gameObject.transform.parent);
        popUp.SetPopUpText("Ты погиб :( Dedge Sadge");
    }

    [ClientRpc]
    public void ShowPopUpClientRpc(string message, ClientRpcParams clientRpcParams = default)
    {
        NotificationPopUp popUp = Instantiate(notificationPopUp, gameObject.transform.parent);
        popUp.SetPopUpText(message);
        StartCoroutine(DeletePopUp(popUp));
    }

    [ClientRpc]
    public void ForceUpdateHandClientRpc(ClientRpcParams clientRpcParams = default)
    {
        foreach (LetterStickUI tile in tiles)
        {
            if (tile.Letter != null)
            {
                Destroy(tile.Letter.gameObject);
            }
        }

        // Update
    }

    IEnumerator DeletePopUp (NotificationPopUp popUp)
    {
        yield return new WaitForSeconds(3);

        Destroy(popUp.gameObject);
    }

}
