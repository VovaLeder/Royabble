using Common;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using Unity.Services.Authentication;
using UnityEngine;
using UnityEngine.UI;

public class LobbyPlayer : MonoBehaviour
{
    [SerializeField] private int playerIndex;
    [SerializeField] private GameObject readyGameObject;
    [SerializeField] private TextMeshProUGUI playerNameText;
    [SerializeField] private Image image;
    [SerializeField] private Button kickBtn;

    private void Awake()
    {
        if (kickBtn != null)
        {
            kickBtn.onClick.AddListener(() =>
            {
                PlayerData data = PlayersManager.Instance.GetPlayerDataFromIndex(playerIndex);
                LobbyManager.Instance.KickPlayer(data.playerId.ToString());
                PlayersManager.Instance.KickPlayer(data.clientId);
            });
        }
    }

    private void Start()
    {
        PlayersManager.Instance.OnPlayerDataListChanged += PlayersManager_OnPlayerDataListChanged;
        LobbyReady.Instance.OnReadyChanged += LobbyReady_OnReadyChanged;

        if (kickBtn != null)
        {
            kickBtn.gameObject.SetActive(NetworkManager.Singleton.IsHost);
        }

        UpdatePlayer();
    }

    private void OnDestroy()
    {
        PlayersManager.Instance.OnPlayerDataListChanged -= PlayersManager_OnPlayerDataListChanged;
        LobbyReady.Instance.OnReadyChanged -= LobbyReady_OnReadyChanged;
    }

    private void LobbyReady_OnReadyChanged(object sender, EventArgs e)
    {
        UpdatePlayer();
    }

    private void PlayersManager_OnPlayerDataListChanged(object sender, EventArgs e)
    {
        UpdatePlayer();
    }

    private void UpdatePlayer()
    {
        if (PlayersManager.Instance.IsPlayerIndexConnected(playerIndex)) 
        {
            Show();

            PlayerData playerInfo = PlayersManager.Instance.GetPlayerDataFromIndex(playerIndex);

            readyGameObject.SetActive(LobbyReady.Instance.IsPlayerReady(playerInfo.clientId));

            playerNameText.text = playerInfo.nickname.ToString();

            image.color = PlayersManager.Instance.GetColor(playerInfo.colorId);
        }
        else
        {
            Hide();
        }
    }

    public void Show()
    {
        gameObject.SetActive(true);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
}
