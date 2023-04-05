using System.Collections.Generic;
using TMPro;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UI;

public class LobbySelectUI : MonoBehaviour
{

    [SerializeField] private Button mainMenyBtn;
    [SerializeField] private Button createLobbyBtn;
    [SerializeField] private Button quickJoinBtn;
    [SerializeField] private Button joinByCodeBtn;
    [SerializeField] private TMP_InputField lobbyCodeInputField;
    [SerializeField] private TMP_InputField playerNameInputField;
    [SerializeField] private LobbyCreateUI lobbyCreateUI;

    [SerializeField] private Transform lobbyContainer;
    [SerializeField] private Transform lobbyTemplate;


    void Awake() 
    {
        mainMenyBtn.onClick.AddListener(() => {
            Loader.Load(Loader.Scene.MainMenuScene);
        });
        createLobbyBtn.onClick.AddListener(() => {
            lobbyCreateUI.Show();
        });
        quickJoinBtn.onClick.AddListener(() => {
            LobbyManager.Instance.QuickJoin();
        });
        joinByCodeBtn.onClick.AddListener(() => {
            LobbyManager.Instance.JoinByCode(lobbyCodeInputField.text);
        });

        lobbyTemplate.gameObject.SetActive(false);
    }

    private void Start()
    {
        playerNameInputField.text = PlayersManager.Instance.GetPlayerName();
        playerNameInputField.onValueChanged.AddListener((string newText) =>
        {
            PlayersManager.Instance.SetPlayerName(newText);
        });

        LobbyManager.Instance.OnLobbyListChanged += LobbyManager_OnLobbyListChanged;
        UpdateLobbyList(new List<Lobby>());
    }

    private void OnDestroy()
    {
        LobbyManager.Instance.OnLobbyListChanged -= LobbyManager_OnLobbyListChanged;
    }

    private void LobbyManager_OnLobbyListChanged(object sender, LobbyManager.OnLobbyListChangedEventArgs e)
    {
        UpdateLobbyList(e.lobbyList);
    }

    private void UpdateLobbyList(List<Lobby> lobbyList)
    {
        foreach (Transform child in lobbyContainer)
        {
            if (child == lobbyTemplate) continue;
            Destroy(child.gameObject);
        }

        foreach (Lobby lobby in lobbyList)
        {
            Transform lobbyTransform = Instantiate(lobbyTemplate, lobbyContainer);
            lobbyTransform.gameObject.SetActive(true);
            lobbyTransform.GetComponent<LobbyListSingleUI>().SetLobby(lobby);
        }
    }
}
