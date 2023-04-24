using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class LobbyReady : NetworkBehaviour
{
    [SerializeField] private Button playBtn;

    public static LobbyReady Instance { get; private set; }

    public event EventHandler OnReadyChanged;
    private Dictionary<ulong, bool> playerReadyDictionary;

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

        playerReadyDictionary = new Dictionary<ulong, bool>();
    }

    private void Start()
    {
        if (!NetworkManager.Singleton.IsHost)
        {
            playBtn.gameObject.SetActive(false);
        }
        else
        {
            NetworkManager.Singleton.OnClientDisconnectCallback += Singleton_OnClientDisconnectCallback;
            NetworkManager.Singleton.OnClientConnectedCallback += Singleton_OnClientConnectedCallback;
            playBtn.interactable = false;
        }
    }

    public override void OnDestroy()
    {
        base.OnDestroy();
        if (NetworkManager.Singleton.IsHost)
        {
            NetworkManager.Singleton.OnClientDisconnectCallback -= Singleton_OnClientDisconnectCallback;
            NetworkManager.Singleton.OnClientConnectedCallback -= Singleton_OnClientConnectedCallback;
        }
    }

    private void Singleton_OnClientDisconnectCallback(ulong id)
    {
        CheckForReadyPlayers(true);
    }

    private void Singleton_OnClientConnectedCallback(ulong id)
    {
        ClientRpcParams clientRpcParams = new ClientRpcParams
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIds = new ulong[] { id }
            }
        };

        var sendedDictionary = playerReadyDictionary.AsEnumerable();
        foreach (var kvp in playerReadyDictionary)
        {
            SetPlayerReadyForClientRpc(kvp.Key, kvp.Value, clientRpcParams);
        }
    }

    public void SetPlayerReady()
    {
        SetPlayerReadyServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    private void SetPlayerReadyServerRpc(ServerRpcParams serverRpcParams = default)
    {
        ulong senderId = serverRpcParams.Receive.SenderClientId;

        playerReadyDictionary[senderId] = playerReadyDictionary.ContainsKey(senderId) ? !playerReadyDictionary[senderId] : true;
        SetPlayerReadyClientRpc(senderId, playerReadyDictionary[senderId]);

        CheckForReadyPlayers();
    }

    private void CheckForReadyPlayers(bool onClientDisconnect = false)
    {
        bool allClientsReady = true;

        foreach (ulong clientId in NetworkManager.Singleton.ConnectedClientsIds)
        {
            if (!IsPlayerReady(clientId) || playerReadyDictionary.Where(kvp => NetworkManager.Singleton.ConnectedClientsIds.Contains(kvp.Key) && kvp.Value).Count() < (onClientDisconnect ? 3 : 2))
            {
                allClientsReady = false;
                break;
            }
        }

        playBtn.interactable = allClientsReady;
    }

    [ClientRpc]
    private void SetPlayerReadyClientRpc(ulong clientId, bool isReady)
    {
        playerReadyDictionary[clientId] = isReady;

        OnReadyChanged.Invoke(this, EventArgs.Empty);
    }

    [ClientRpc]
    private void SetPlayerReadyForClientRpc(ulong clientId, bool isReady, ClientRpcParams clientRpcParams = default)
    {
        playerReadyDictionary[clientId] = isReady;

        OnReadyChanged.Invoke(this, EventArgs.Empty);
    }


    public bool IsPlayerReady(ulong clientId)
    {
         return playerReadyDictionary.ContainsKey(clientId) && playerReadyDictionary[clientId];
    }
}
