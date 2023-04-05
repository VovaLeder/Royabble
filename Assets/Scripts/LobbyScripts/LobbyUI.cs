using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LobbyUI : MonoBehaviour
{

    [SerializeField] private Button mainMenyBtn;
    [SerializeField] private Button setReadyBtn;
    [SerializeField] private Button playBtn;

    [SerializeField] private TextMeshProUGUI lobbyNameText;
    [SerializeField] private TextMeshProUGUI lobbyCodeText;


    void Awake() 
    {
        mainMenyBtn.onClick.AddListener(() => {
            LobbyManager.Instance.LeaveLobby();
            Loader.Load(Loader.Scene.MainMenuScene);
        });
        setReadyBtn.onClick.AddListener(() => {
            LobbyReady.Instance.SetPlayerReady();
        });
        playBtn.onClick.AddListener(() => {
            LobbyManager.Instance.DeleteLobby();
            Loader.LoadNetwork(Loader.Scene.GameScene);
        });
    }

    private void Start()
    {
        if (!NetworkManager.Singleton.IsHost) SetPlayBtnActive(false);

        Lobby lobby = LobbyManager.Instance.joinedLobby;
        lobbyNameText.text = $"ָלÿ כמבבט: {lobby.Name}";
        lobbyCodeText.text = $"Êמה כמבבט: {lobby.LobbyCode}";
    }

    public void SetPlayBtnActive(bool active)
    {
        playBtn.gameObject.SetActive(active);
    }
}
