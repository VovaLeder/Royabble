using Assets.GameProcessScripts;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using Common;
using Unity.Collections;

public class PlayersGameManager : NetworkBehaviour
{
    const string Letters = "¿¿¿¿¿¿¿¿¿¿¡¡¡¬¬¬¬¬√√√ƒƒƒƒƒ≈≈≈≈≈≈≈≈≈∆∆««»»»»»»»»…………      ÀÀÀÀÃÃÃÃÃÕÕÕÕÕÕÕÕŒŒŒŒŒŒŒŒŒŒœœœœœœ––––––——————“““““”””‘’’÷◊◊ÿŸ⁄€€‹‹›ﬁﬂﬂﬂ";

    Dictionary<string, int> LetterValues = new() {
        {"¿", 1 },
        {"¡", 3 },
        {"¬", 2 },
        {"√", 3 },
        {"ƒ", 2 },
        {"≈", 1 },
        {"∆", 5 },
        {"«", 5 },
        {"»", 1 },
        {"…", 2 },
        {" ", 2 },
        {"À", 2 },
        {"Ã", 2 },
        {"Õ", 1 },
        {"Œ", 1 },
        {"œ", 2 },
        {"–", 2 },
        {"—", 2 },
        {"“", 2 },
        {"”", 3 },
        {"‘", 10 },
        {"’", 5 },
        {"÷", 10 },
        {"◊", 5 },
        {"ÿ", 10 },
        {"Ÿ", 10 },
        {"⁄", 10 },
        {"€", 5 },
        {"‹", 5 },
        {"›", 10 },
        {"ﬁ", 10 },
        {"ﬂ", 5 },
        };

    [SerializeField] private float defaultLetterTimerCap;
    [SerializeField] private float MAX_HP;

    [SerializeField] private int explodePrice;
    [SerializeField] private int refreshPrice;
    [SerializeField] private int healPrice;
    [Range(0, 100)]
    [SerializeField] private float healAmount;
    [SerializeField] private int timerPrice;
    [Range(0.5f, 1)]
    [SerializeField] private float timerDecreaseMultiplyer;

    private float updateHpBarTimer;
    private float updateHpBarTimerCap;

    public static PlayersGameManager Instance { get; private set; }

    NetworkList<PlayerGameNetworkData> playerGameDataNetworkList;
    List<PlayerGameData> playerGameDataList;

    bool gameIsGoing;

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

        updateHpBarTimer = 0;
        updateHpBarTimerCap = 1;

        playerGameDataList = new();
        playerGameDataNetworkList = new NetworkList<PlayerGameNetworkData>();

        gameIsGoing = true;
        //TODO playerGameDataNetworkList.OnListChanged += playerGameDataNetwork_OnListChanged;
    }

    private void Start()
    {
        if (NetworkManager.Singleton.IsHost)
        {
            foreach (PlayerData playerGameData in PlayersManager.Instance.GetPlayersData())
            {
                playerGameDataList.Add(new PlayerGameData(playerGameData.clientId));
                playerGameDataNetworkList.Add(new PlayerGameNetworkData
                {
                    clientId = playerGameData.clientId,
                    currentWord = new(),
                    alive = true,
                });

                ClientRpcParams clientRpcParams = new ClientRpcParams
                {
                    Send = new ClientRpcSendParams
                    {
                        TargetClientIds = new ulong[] { playerGameData.clientId }
                    }
                };

                StatisticsManager.Instance.AddGameToCurrentUserClientRpc(clientRpcParams);
            }
        }
    }

    void Update()
    {
        if (gameIsGoing)
        {
            if (NetworkManager.Singleton.IsHost) UpdateHandsInfos();

            if (TimeToUpdateHpBar())
            {
                updateHpBarTimer = 0;
            }
            updateHpBarTimer += Time.deltaTime;
        }
    }

    private bool TimeToUpdateHpBar()
    {
        return updateHpBarTimer > updateHpBarTimerCap;
    }

    public int GetLetterValue(string letter)
    {
        return LetterValues[letter];
    }

    public int GetExplodePrice()
    {
        return explodePrice;
    }
    public int GetRefreshPrice()
    {
        return refreshPrice;
    }
    public int GetHealPrice()
    {
        return healPrice;
    }
    public int GetTimerPrice()
    {
        return timerPrice;
    }

    public float GetDefaultLetterTimerCap()
    {
        return defaultLetterTimerCap;
    }

    public float GetMaxHp()
    {
        return MAX_HP;
    }

    [ServerRpc(RequireOwnership = false)]
    public void ExplodeServerRpc(ServerRpcParams serverRpcParams = default)
    {
        var clientId = serverRpcParams.Receive.SenderClientId;
        if (!NetworkManager.ConnectedClients.ContainsKey(clientId))
        {
            return;
        }
        ClientRpcParams clientRpcParams = new ClientRpcParams
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIds = new ulong[] { clientId }
            }
        };

        if (!IsPlayerAlive(clientId))
        {
            UIManager.Instance.DedgeClientRpc(clientRpcParams);
            return;
        }

        PlayerGameNetworkData playerGameInfo = GetPlayerNetworkData(clientId);
        PlayerGameData hand = GetPlayerGameData(clientId);
        if (hand.hand.Count != 7)
        {
            UIManager.Instance.ShowPopUpClientRpc("“ÓÎ¸ÍÓ Ò 7 ·ÛÍ‚‡ÏË ^_^", clientRpcParams);
            return;
        }

        TileFieldManager.Instance.ExplodeField(playerGameInfo.GetCurrentWord());
        ExplodeHand(clientId);
    }

    [ServerRpc(RequireOwnership = false)]
    public void ExplodeStoreServerRpc(ServerRpcParams serverRpcParams = default)
    {
        var clientId = serverRpcParams.Receive.SenderClientId;
        if (!NetworkManager.ConnectedClients.ContainsKey(clientId))
        {
            return;
        }
        ClientRpcParams clientRpcParams = new ClientRpcParams
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIds = new ulong[] { clientId }
            }
        };

        if (!IsPlayerAlive(clientId))
        {
            UIManager.Instance.DedgeClientRpc(clientRpcParams);
            return;
        }

        PlayerGameNetworkData playerGameNetworkInfo = GetPlayerNetworkData(clientId);
        PlayerGameData playerGameInfo = GetPlayerGameData(clientId);
        if (playerGameInfo.availablePoints < explodePrice)
        {
            UIManager.Instance.ShowPopUpClientRpc($"ÕÛÊÌÓ {explodePrice} Ó˜ÍÓ‚, ˜ÚÓ·˚ ËÒÔÓÎ¸ÁÓ‚‡Ú¸ ÔÂ‰ÏÂÚ", clientRpcParams);
            return;
        }

        TileFieldManager.Instance.ExplodeField(playerGameNetworkInfo.GetCurrentWord());
        ChangePlayerPoints(-explodePrice, clientId);
    }

    public int GetPlayerIndex(ulong clientId)
    {
        for (int i = 0; i < playerGameDataNetworkList.Count; i++)
        {
            if (playerGameDataNetworkList[i].clientId == clientId) return i;
        }
        return -1;
    }
    public PlayerGameNetworkData GetPlayerNetworkData(ulong id)
    {
        for (int i = 0; i < playerGameDataNetworkList.Count; i++)
        {
            if (playerGameDataNetworkList[i].clientId == id) return playerGameDataNetworkList[i];
        }
        return default;
    }

    public PlayerGameData GetPlayerGameData(ulong id)
    {
        return playerGameDataList.FirstOrDefault(x => x.clientId == id);
    }

    public void ChangePlayerPoints(int pointsShift, ulong id)
    {
        StatisticsManager statisticsManager = StatisticsManager.Instance;

        ClientRpcParams clientPlayerRpcParams = new ClientRpcParams
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIds = new ulong[] { id }
            }
        };

        var playerGameData = GetPlayerGameData(id);
        playerGameData.availablePoints += pointsShift;
        if (pointsShift > 0)
        {
            statisticsManager.AddEarnedPointsToCurrentUserClientRpc(pointsShift, clientPlayerRpcParams);
            playerGameData.points += pointsShift;
        }
        else
        {
            statisticsManager.AddSpentPointsToCurrentUserClientRpc(-pointsShift, clientPlayerRpcParams);
        }

        UIManager.Instance.SetAvailablePointsClientRpc(playerGameData.availablePoints, clientPlayerRpcParams);
    }

    public void KillPlayer(ulong idKilled, ulong idKiller)
    {
        KillPlayer(idKilled);

        ClientRpcParams killerPlayerRpcParams = new ClientRpcParams
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIds = new ulong[] { idKiller }
            }
        };

        StatisticsManager.Instance.AddKilledPlayerToCurrentUserClientRpc(killerPlayerRpcParams);
    }

    public void KillPlayer(ulong id)
    {
        int index = GetPlayerIndex(id);
        PlayerGameNetworkData playerInfo = playerGameDataNetworkList[index];
        playerInfo.alive = false;
        playerGameDataNetworkList[index] = playerInfo;

        ClientRpcParams defeatedPlayerRpcParams = new ClientRpcParams
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIds = new ulong[] { id }
            }
        };

        UIManager.Instance.ShowOverlapMessageClientRpc("¬˚ ÏÂÚ‚˚, ÎÓÎ :)", defeatedPlayerRpcParams);

        var playersAlive = 0;
        foreach (var playerGameNetworkData in playerGameDataNetworkList)
        {
            if (playerGameNetworkData.alive)
            {
                playerInfo = playerGameNetworkData;
                playersAlive++;
            }
        }
        if (playersAlive < 2)
        {
            ClientRpcParams clientRpcParams = new ClientRpcParams
            {
                Send = new ClientRpcSendParams
                {
                    TargetClientIds = new ulong[] { playerInfo.clientId }
                }
            };

            gameIsGoing = false;
            StatisticsManager.Instance.AddWonGameToCurrentUserClientRpc(clientRpcParams);
            UIManager.Instance.ShowOverlapMessageClientRpc("œÓ·Â‰ËÚÂÎ¸. Ã≈√¿ ’Œ–Œÿ", clientRpcParams);
        }
    }

    public bool IsPlayerAlive(ulong id)
    {
        return GetPlayerNetworkData(id).alive;
    }

    public void UpdatePlayerWord(List<Pair> newWord, ulong clientId)
    {
        FixedList512Bytes<Pair> newFixedWord = new();
        foreach (Pair pair in newWord)
        {
            newFixedWord.Add(pair);
        }

        int playerInfoIndex = GetPlayerIndex(clientId);

        PlayerGameNetworkData playerInfo = playerGameDataNetworkList[playerInfoIndex];
        playerInfo.currentWord = newFixedWord;
        playerGameDataNetworkList[playerInfoIndex] = playerInfo;

        ClientRpcParams defeatedPlayerRpcParams = new ClientRpcParams
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIds = new ulong[] { clientId }
            }
        };

        StatisticsManager.Instance.AddComposedWordToCurrentUserClientRpc(defeatedPlayerRpcParams);
    }

    #region handManaging

    void UpdateHandsInfos()
    {
        foreach (var playerGameData in playerGameDataList)
        {
            if (!IsPlayerAlive(playerGameData.clientId)) continue;

            ClientRpcParams clientRpcParams = new ClientRpcParams
            {
                Send = new ClientRpcSendParams
                {
                    TargetClientIds = new ulong[] { playerGameData.clientId }
                }
            };

            playerGameData.letterTimer += Time.deltaTime;

            if (playerGameData.letterTimer > playerGameData.letterTimerCap)
            {
                playerGameData.letterTimer = 0;
                UIManager.Instance.UpdateLetterTimerBarClientRpc(playerGameData.letterTimer, playerGameData.letterTimerCap, clientRpcParams);
                if (playerGameData.hand.Count < 7)
                {
                    string randomLetter = Letters[Random.Range(0, Letters.Length)].ToString();

                    playerGameData.hand.Add(randomLetter);
                    UIManager.Instance.GetLetterClientRpc(randomLetter, clientRpcParams);
                }
            }
        }
    }

    public void ManageHandAfterWordValidating(ulong clientId, NetLetterInfoCollection updatedTiles)
    {
        PlayerGameNetworkData gameInfo = GetPlayerNetworkData(clientId);

        if (gameInfo.currentWord.Length == 0)
        {
            ExplodeHand(clientId);
        }
        else
        {
            List<string> letters = new();
            foreach (var updatedTile in updatedTiles)
            {
                letters.Add(updatedTile.GetValue());
            }

            RemoveLetters(clientId, letters);
        }
    }

    public void RemoveLetters(ulong id, List<string> letters)
    {
        PlayerGameData playerGameData = playerGameDataList.Find(x => x.clientId == id);
        ClientRpcParams clientRpcParams = new ClientRpcParams
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIds = new ulong[] { id }
            }
        };
        foreach (string letter in letters)
        {
            playerGameData.hand.Remove(letter);
        }
        UIManager.Instance.RemoveSentLettersClientRpc(clientRpcParams);
        
        if (playerGameData.hand.Count == 0)
        {
            for (int i = 0; i < 7; i++)
            {
                string randomLetter = Letters[Random.Range(0, Letters.Length)].ToString();
                UIManager.Instance.GetLetterClientRpc(randomLetter, clientRpcParams);
                playerGameData.hand.Add(randomLetter);
            }
        }
    }

    public void ForceHandUpdate(ulong clientId)
    {
        ClientRpcParams clientRpcParams = new ClientRpcParams
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIds = new ulong[] { clientId }
            }
        };
        UIManager.Instance.ForceUpdateHandClientRpc(clientRpcParams);
    }

    public void ExplodeHand(ulong clientId)
    {
        GetPlayerGameData(clientId).hand = new();
        ClientRpcParams clientRpcParams = new ClientRpcParams
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIds = new ulong[] { clientId }
            }
        };
        UIManager.Instance.ExplodeHandClientRpc(clientRpcParams);
    }

    [ServerRpc(RequireOwnership = false)]
    public void RefreshHandServerRpc(ServerRpcParams serverRpcParams = default)
    {
        ulong clientId = serverRpcParams.Receive.SenderClientId;

        if (!NetworkManager.ConnectedClients.ContainsKey(clientId))
        {
            return;
        }

        ClientRpcParams clientRpcParams = new ClientRpcParams
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIds = new ulong[] { clientId }
            }
        };
        PlayerGameData playerGameData = GetPlayerGameData(clientId);

        if (playerGameData.availablePoints < refreshPrice)
        {
            UIManager.Instance.ShowPopUpClientRpc($"ÕÛÊÌÓ {refreshPrice} Ó˜ÍÓ‚, ˜ÚÓ·˚ ËÒÔÓÎ¸ÁÓ‚‡Ú¸ ÔÂ‰ÏÂÚ", clientRpcParams);
            return;
        }

        UIManager.Instance.ExplodeHandClientRpc(clientRpcParams);
        playerGameData.hand.Clear();

        for (int i = 0; i < 7; i++)
        {
            string randomLetter = Letters[Random.Range(0, Letters.Length)].ToString();
            UIManager.Instance.GetLetterClientRpc(randomLetter, clientRpcParams);
            playerGameData.hand.Add(randomLetter);
        }
        ChangePlayerPoints(-refreshPrice, clientId);
    }

    #endregion

    public void PlayerInZone(ulong playerId)
    {
        ChangePlayerHp(playerId, -Time.deltaTime);
    }

    private void ChangePlayerHp(ulong playerId, float value, bool updateHealthBar = false)
    {
        ClientRpcParams clientRpcParams = new ClientRpcParams
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIds = new ulong[] { playerId }
            }
        };
        var playerGame = GetPlayerGameData(playerId);

        playerGame.hp += value;
        if (playerGame.hp < 0)
        {
            KillPlayer(playerId);
        }
        if (playerGame.hp > MAX_HP)
        {
            playerGame.hp = MAX_HP;
        }

        if (TimeToUpdateHpBar() || updateHealthBar)
        {
            UIManager.Instance.UpdateHpBarClientRpc(playerGame.hp, clientRpcParams);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void HealServerRpc(ServerRpcParams serverRpcParams = default)
    {
        ulong clientId = serverRpcParams.Receive.SenderClientId;

        if (!NetworkManager.ConnectedClients.ContainsKey(clientId))
        {
            return;
        }

        ClientRpcParams clientRpcParams = new ClientRpcParams
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIds = new ulong[] { clientId }
            }
        };
        PlayerGameData playerGameData = GetPlayerGameData(clientId);

        if (playerGameData.availablePoints < refreshPrice)
        {
            UIManager.Instance.ShowPopUpClientRpc($"ÕÛÊÌÓ {healPrice} Ó˜ÍÓ‚, ˜ÚÓ·˚ ËÒÔÓÎ¸ÁÓ‚‡Ú¸ ÔÂ‰ÏÂÚ", clientRpcParams);
            return;
        }

        ChangePlayerHp(clientId, healAmount, true);

        ChangePlayerPoints(-healPrice, clientId);
    }

    [ServerRpc(RequireOwnership = false)]
    public void DecreaseTimerServerRpc(ServerRpcParams serverRpcParams = default)
    {
        ulong clientId = serverRpcParams.Receive.SenderClientId;

        if (!NetworkManager.ConnectedClients.ContainsKey(clientId))
        {
            return;
        }

        ClientRpcParams clientRpcParams = new ClientRpcParams
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIds = new ulong[] { clientId }
            }
        };
        PlayerGameData playerGameData = GetPlayerGameData(clientId);

        if (playerGameData.availablePoints < refreshPrice)
        {
            UIManager.Instance.ShowPopUpClientRpc($"ÕÛÊÌÓ {timerPrice} Ó˜ÍÓ‚, ˜ÚÓ·˚ ËÒÔÓÎ¸ÁÓ‚‡Ú¸ ÔÂ‰ÏÂÚ", clientRpcParams);
            return;
        }


        playerGameData.letterTimerCap *= timerDecreaseMultiplyer;


        ChangePlayerPoints(-timerPrice, clientId);
    }


}
