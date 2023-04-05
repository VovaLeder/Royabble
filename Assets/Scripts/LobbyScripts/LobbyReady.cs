using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.EventSystems;
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
            playBtn.interactable = false;
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

        bool allClientsReady = true;
        
        foreach (ulong clientId in NetworkManager.Singleton.ConnectedClientsIds)
        {
            if (!playerReadyDictionary.ContainsKey(clientId) || !playerReadyDictionary[clientId]) {
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

    
    public bool IsPlayerReady(ulong clientId)
    {
         return playerReadyDictionary.ContainsKey(clientId) && playerReadyDictionary[clientId];
    }
}
