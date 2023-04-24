using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;
using Common;
using System;
using UnityEngine.SceneManagement;
using Unity.Services.Authentication;

public class PlayersManager : NetworkBehaviour
{
    [SerializeField] public List<Color> Colors;
    public const int MAX_PLAYER_AMOUNT = 6;
    const string PLAYER_PREFS_PLAYER_NAME = "PlayerName";

    private string playerName;

    public static PlayersManager Instance { get; private set; }

    NetworkList<PlayerData> playerDataNetworkList;

    public event EventHandler OnTryingToJoinGame;
    public event EventHandler OnFailedToJoinGame;

    public event EventHandler OnPlayerDataListChanged;

    public bool IsPlayerIndexConnected(int playerIndex)
    {
        return playerIndex < playerDataNetworkList.Count;
    }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }

        playerName = PlayerPrefs.GetString(PLAYER_PREFS_PLAYER_NAME, "Royabbler" + UnityEngine.Random.Range(100, 10000));

        playerDataNetworkList = new NetworkList<PlayerData>();
        playerDataNetworkList.OnListChanged += PlayerDatasNetwork_OnListChanged;

        DontDestroyOnLoad(gameObject);
    }

    public void KickPlayer(ulong clietnId)
    {
        NetworkManager.Singleton.DisconnectClient(clietnId);
        NetworkManager_Server_OnClientDisconnectCallback(clietnId);
    }

    public string GetPlayerName()
    {
        return playerName;
    }

    public void SetPlayerName(string playerName)
    {
        this.playerName = playerName;

        PlayerPrefs.SetString(PLAYER_PREFS_PLAYER_NAME, playerName);
    }

    private void PlayerDatasNetwork_OnListChanged(NetworkListEvent<PlayerData> changeEvent)
    {
        OnPlayerDataListChanged?.Invoke(this, EventArgs.Empty);
    }

    public Color GetColor(int colorId)
    {
        return Colors[colorId];
    }
    public void ChangeColor(int colorId)
    {
        ChangeColorServerRpc(colorId);
    }

    [ServerRpc(RequireOwnership = false)]
    private void ChangeColorServerRpc(int colorId, ServerRpcParams serverRpcParams = default)
    {
        if (!IsColorAvailable(colorId)) return;

        int playerInfoIndex = GetPlayerIndex(serverRpcParams.Receive.SenderClientId);

        PlayerData playerInfo = playerDataNetworkList[playerInfoIndex];
        playerInfo.colorId = colorId;
        playerDataNetworkList[playerInfoIndex] = playerInfo;
    }

    private bool IsColorAvailable(int colorId)
    {
        for (int i = 0; i < playerDataNetworkList.Count; i++)
        {
            if (playerDataNetworkList[i].colorId == colorId) return false;
        }
        return true;
    }

    private int GetFirstUnusedColorId()
    {
        for (int i = 0; i < Colors.Count; i++)
        {
            if (IsColorAvailable(i)) return i;
        }
        return -1;
    }


    private void Unsubscribe()
    {
        NetworkManager.Singleton.OnClientConnectedCallback -= NetworkManager_Server_OnClientConnectedCallback;
        NetworkManager.Singleton.OnClientDisconnectCallback -= NetworkManager_Server_OnClientDisconnectCallback;

        NetworkManager.Singleton.OnClientConnectedCallback -= NetworkManager_Client_OnClientConnectedCallback;
        NetworkManager.Singleton.OnClientDisconnectCallback += NetworkManager_Client_OnClientDisconnectCallback;
    }

    public void StartHost()
    {
        Unsubscribe();

        NetworkManager.Singleton.ConnectionApprovalCallback = NetworkManager_ConnectionApprovalCallback;

        NetworkManager.Singleton.OnClientConnectedCallback += NetworkManager_Server_OnClientConnectedCallback;
        NetworkManager.Singleton.OnClientDisconnectCallback += NetworkManager_Server_OnClientDisconnectCallback;

        NetworkManager.Singleton.StartHost();

        playerDataNetworkList.Clear();
        NetworkManager_Server_OnClientConnectedCallback(NetworkManager.Singleton.LocalClientId);
    }

    private void NetworkManager_Server_OnClientDisconnectCallback(ulong clientId)
    {
        for (int i = 0; i < playerDataNetworkList.Count; i++)
        {
            PlayerData playerInfo = playerDataNetworkList[i];
            if (playerInfo.clientId == clientId)
            {
                playerDataNetworkList.RemoveAt(i);
            }
        }
    }

    private void NetworkManager_Server_OnClientConnectedCallback(ulong clientId)
    {
        playerDataNetworkList.Add(new PlayerData
        {
            clientId = clientId,
            colorId = GetFirstUnusedColorId(),
            nickname = "DefaultNickname",
        });

        SetPlayerNameServerRpc(GetPlayerName());
        SetPlayerIdServerRpc(AuthenticationService.Instance.PlayerId);
    }

    public void StartClient()
    {
        Unsubscribe();

        OnTryingToJoinGame?.Invoke(this, EventArgs.Empty);

        NetworkManager.Singleton.OnClientDisconnectCallback += NetworkManager_Client_OnClientDisconnectCallback;
        NetworkManager.Singleton.OnClientConnectedCallback += NetworkManager_Client_OnClientConnectedCallback;
        NetworkManager.Singleton.StartClient();
    }

    private void NetworkManager_Client_OnClientDisconnectCallback(ulong obj)
    {
        OnFailedToJoinGame?.Invoke(this, EventArgs.Empty);
    }

    private void NetworkManager_Client_OnClientConnectedCallback(ulong obj)
    {
        SetPlayerNameServerRpc(GetPlayerName());
        SetPlayerIdServerRpc(AuthenticationService.Instance.PlayerId);
    }

    [ServerRpc(RequireOwnership = false)]
    private void SetPlayerNameServerRpc(string playerName, ServerRpcParams serverRpcParams = default)
    {
        int playerInfoIndex = GetPlayerIndex(serverRpcParams.Receive.SenderClientId);

        PlayerData playerInfo = playerDataNetworkList[playerInfoIndex];
        playerInfo.nickname = playerName;
        playerDataNetworkList[playerInfoIndex] = playerInfo;
    }

    [ServerRpc(RequireOwnership = false)]
    private void SetPlayerIdServerRpc(string playerId, ServerRpcParams serverRpcParams = default)
    {
        int playerInfoIndex = GetPlayerIndex(serverRpcParams.Receive.SenderClientId);

        PlayerData playerInfo = playerDataNetworkList[playerInfoIndex];
        playerInfo.playerId = playerId;
        playerDataNetworkList[playerInfoIndex] = playerInfo;
    }

    private void NetworkManager_ConnectionApprovalCallback(NetworkManager.ConnectionApprovalRequest connectionApprovalRequest, NetworkManager.ConnectionApprovalResponse connectionApprovalResponse)
    {
        if (SceneManager.GetActiveScene().name == Loader.Scene.GameScene.ToString())
        {
            connectionApprovalResponse.Approved = false;
            connectionApprovalResponse.Reason = "Игра уже начата.";
            return;
        }

        if (NetworkManager.Singleton.ConnectedClientsIds.Count >= MAX_PLAYER_AMOUNT)
        {
            connectionApprovalResponse.Approved = false;
            connectionApprovalResponse.Reason = "Лобби заполнено.";
            return;
        }

        connectionApprovalResponse.Approved = true;
    }
    
    public PlayerData GetPlayerDataFromIndex(int playerIndex)
    {
        return playerDataNetworkList[playerIndex];
    }
    public int GetPlayerIndex(ulong clientId)
    {
        for (int i = 0; i < playerDataNetworkList.Count; i++)
        {
            if (playerDataNetworkList[i].clientId == clientId) return i;
        }
        return -1;
    }
    public PlayerData GetPlayerData(ulong id)
    {
        for (int i = 0; i < playerDataNetworkList.Count; i++)
        {
            if (playerDataNetworkList[i].clientId == id) return playerDataNetworkList[i];
        }
        return default;
    }
    public PlayerData GetPlayerData()
    {
        return GetPlayerData(NetworkManager.Singleton.LocalClientId);
    }

    public List<PlayerData> GetPlayersData()
    {
        List<PlayerData> playersData = new List<PlayerData>();
        for (int i = 0; i < playerDataNetworkList.Count; i++)
        {
            playersData.Add(playerDataNetworkList[i]);
        }
        return playersData;
    }
}
