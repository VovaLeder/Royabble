using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Unity.Netcode;
using UnityEngine;
using Common;

using Assets.GameProcessScripts;
using System.Linq;

public class TileFieldManager : NetworkBehaviour
{
    [SerializeField] bool CheckForWordCorrect;

    [SerializeField] List<float> zoneTimes;
    private int zoneCounter = 0;
    private float zoneTimer = 0;
    public static TileFieldManager Instance { get; private set; }

    [SerializeField] LetterStick tile;

    private const int NumberOfTiles = 32;
    private LetterStick[,] tiles = new LetterStick[NumberOfTiles, NumberOfTiles];

    private Pair YellowTile1 = new Pair { i = -1, j = -1};
    private Pair YellowTile2 = new Pair { i = NumberOfTiles, j = NumberOfTiles };
    private Pair RedTile1 = new Pair { i = -1, j = -1 };
    private Pair RedTile2 = new Pair { i = NumberOfTiles, j = NumberOfTiles };

    private string[] _ALL_WORDS;

    UIManager uiManager;
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
    }

    void Start()
    {
        CreateTileField();

        TextAsset txtAsset =
                    Resources.Load(Path.Combine("Dictionaries", "RussianDictionary"), typeof(TextAsset)) as TextAsset;
        _ALL_WORDS = txtAsset.text.Split('\n');

        uiManager = UIManager.Instance;
        playersGameManager = PlayersGameManager.Instance;
    }

    private void Update()
    {
        if (NetworkManager.Singleton.IsHost)
        {
            UpdateZoneInfo();
            CheckForPlayersInZone();
        }
    }

    private void CheckForPlayersInZone()
    {
        foreach (ulong playerId in NetworkManager.Singleton.ConnectedClientsIds)
        {
            bool isInZone = true;
            var playerNetwork = playersGameManager.GetPlayerNetworkData(playerId);

            if (!playerNetwork.alive) continue;

            foreach (var pair in playerNetwork.GetCurrentWord())
            {
                isInZone = false;
                if (pair.i <= RedTile1.i || pair.j <= RedTile1.j || pair.i >= RedTile2.i || pair.j >= RedTile2.j)
                {
                    isInZone = true;
                    break;
                }
            }
            if (isInZone)
            {
                playersGameManager.PlayerInZone(playerId);
            }
        }
    }

    private void UpdateZoneInfo()
    {
        if (zoneCounter < zoneTimes.Count)
        {
            zoneTimer += Time.deltaTime;

            if (zoneTimer >= zoneTimes[zoneCounter])
            {
                zoneCounter++;

                RedTile1 = YellowTile1;
                RedTile2 = YellowTile2;
                YellowTile1.i += 2; YellowTile1.j += 2;
                YellowTile2.i -= 2; YellowTile2.j -= 2;

                ChangeTilesColorsClientRpc(false, YellowTile1, YellowTile2);
                ChangeTilesColorsClientRpc(true, RedTile1, RedTile2);
            }
        }
    }

    public void CreateTileField()
    {
        for (int i = 0; i < NumberOfTiles; i++)
        {
            for (int j = 0; j < NumberOfTiles; j++)
            {
                tiles[i, j] = Instantiate<LetterStick>(tile, new Vector3(i * 1.06f, j * 1.06f, 0f), new Quaternion(0, 0, 0, 0), gameObject.transform);
                tiles[i, j].XCoord = i;
                tiles[i, j].YCoord = j;
            }
        }
    }

    [ClientRpc]
    void ChangeTilesColorsClientRpc(bool toRed, Pair tile1, Pair tile2)
    {
        for (int i = 0; i < NumberOfTiles; i++)
        {
            for (int j = 0; j < NumberOfTiles; j++)
            {
                if (i <= tile1.i || j <= tile1.j || i >= tile2.i || j >= tile2.j)
                {
                    tiles[i, j].ChangeTileColor(toRed);
                }
            }
        }
    }


    public void ExplodeField(List<Pair> playerWord)
    {
        int iMax = -1;               int jMax = -1;
        int iMin = NumberOfTiles;    int jMin = NumberOfTiles;
        foreach ((int iL, int jL) in playerWord)
        {
            if (iL < iMin) iMin = iL;
            if (iL > iMax) iMax = iL;
            if (jL < jMin) jMin = jL;
            if (jL > jMax) jMax = jL;
        }
        jMax += 2; if (jMax >= NumberOfTiles) jMax = NumberOfTiles - 1;
        iMax += 2; if (iMax >= NumberOfTiles) iMax = NumberOfTiles - 1;
        jMin -= 2; if (jMin < 0) jMin = 0;
        iMin -= 2; if (iMin < 0) iMin = 0;

        TileLetterInfoCollection tilesForUpdate = new();

        for (int i = iMin; i <= iMax; i++)
        {
            for (int j = jMin; j <= jMax; j++)
            {
                if (tiles[i, j].Letter is FixedLetter letter)
                {
                    if (!letter.hasOwner)
                    {
                        Destroy(letter.gameObject);
                        tiles[i, j].Letter = null;
                        tilesForUpdate.Add(new (i, j, false, new()));
                    }
                }
            }
        }
        UpdateTilesClientRPC(tilesForUpdate);
    }

    [ServerRpc(RequireOwnership = false)]
    public void UpdateFieldServerRpc(NetLetterInfoCollection updatedTiles, ServerRpcParams serverRpcParams = default)
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

        List<ulong> defeatedPlayers;
        HashSet<(int i, int j)> newPlayerWord;

        if (!playersGameManager.IsPlayerAlive(clientId))
        {
            uiManager.DedgeClientRpc(clientRpcParams);
            return;
        }

        if (updatedTiles.Count == 0)
        {
            return;
        }

        if (ValidateWord(updatedTiles, clientId, out defeatedPlayers, out newPlayerWord))
        {
            PlayerGameNetworkData gameInfo = playersGameManager.GetPlayerNetworkData(clientId);

            playersGameManager.ManageHandAfterWordValidating(clientId, updatedTiles);

            TileLetterInfoCollection ownedTilesObj = new();

            foreach ((int i, int j) in gameInfo.GetCurrentWord())
            {
                if (tiles[i, j].Letter is FixedLetter fixedLetter)
                {
                    fixedLetter.resetOwner();
                    ownedTilesObj.Add(
                        new(fixedLetter.GetCoords().i, fixedLetter.GetCoords().j, true, 
                            new(fixedLetter.GetCoords().i, fixedLetter.GetCoords().j, fixedLetter.Value, fixedLetter.hasOwner, fixedLetter.ownerId)
                            )
                        );
                }
            }

            foreach (var defeatedPlayerId in defeatedPlayers)
            {
                PlayerGameNetworkData defeatedPlayerGameData = playersGameManager.GetPlayerNetworkData(defeatedPlayerId);
                foreach ((int i, int j) in defeatedPlayerGameData.GetCurrentWord())
                {
                    if (tiles[i, j].Letter is FixedLetter fixedLetter)
                    {
                        fixedLetter.resetOwner();
                        ownedTilesObj.Add(
                            new(fixedLetter.GetCoords().i, fixedLetter.GetCoords().j, true, 
                                new(fixedLetter.GetCoords().i, fixedLetter.GetCoords().j, fixedLetter.Value, fixedLetter.hasOwner, fixedLetter.ownerId)
                                )
                            );
                    }

                    playersGameManager.KillPlayer(defeatedPlayerId, clientId);
                }
            }

            List<Pair> newCurrentWord = new();
            int gainedPoints = 0;
            foreach ((int i, int j) in newPlayerWord)
            {
                newCurrentWord.Add(new Pair { i=i, j=j });
                
                if (tiles[i, j].Letter is FixedLetter fixedLetter)
                {
                    gainedPoints += playersGameManager.GetLetterValue(fixedLetter.Value);

                    fixedLetter.setOwner(true, gameInfo.clientId);
                    ownedTilesObj.Add(
                        new(fixedLetter.GetCoords().i, fixedLetter.GetCoords().j, true, 
                            new(fixedLetter.GetCoords().i, fixedLetter.GetCoords().j, fixedLetter.Value, fixedLetter.hasOwner, fixedLetter.ownerId)
                            )
                        );
                }
                else
                {
                    gainedPoints += playersGameManager.GetLetterValue(updatedTiles[(i, j)].GetValue());
                }
            }
            playersGameManager.UpdatePlayerWord(newCurrentWord, gameInfo.clientId);
            playersGameManager.ChangePlayerPoints(gainedPoints, gameInfo.clientId);

            foreach (var updatedTile in updatedTiles)
            {
                ownedTilesObj.Add(
                    new(updatedTile.i, updatedTile.j, true, 
                        new(updatedTile.i, updatedTile.j, updatedTile.GetValue(), true, gameInfo.clientId)
                        )
                    );
            }

            UpdateTilesClientRPC(ownedTilesObj);
            return;
        }

        uiManager.WordNotValidatedPopUpClientRpc(clientRpcParams);
    }

    [ClientRpc]
    public void UpdateTilesClientRPC(TileLetterInfoCollection updatedTiles, ClientRpcParams clientRpcParams = default)
    {
        foreach (var updatedTile in updatedTiles)
        {
            if (updatedTile.letterPresent)
            {
                var letter = updatedTile.letter;
                tiles[updatedTile.i, updatedTile.j].CreateLetter(letter);
            }
            else
            {
                tiles[updatedTile.i, updatedTile.j].DeleteLetter();
            }
        }
    }

    public void UpdateField(ulong id)
    {
        TileLetterInfoCollection list = new();

        ClientRpcParams clientRpcParams = new ClientRpcParams
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIds = new ulong[] { id }
            }
        };

        for (int i = 0; i < NumberOfTiles; i++)
        {
            for (int j = 0; j < NumberOfTiles; j++)
            {
                var tile = tiles[i, j];
                if (tile.Letter is FixedLetter letter)
                {
                    list.Add(new(i, j, true, new(i, j, letter.Value, letter.hasOwner, letter.ownerId)));
                }
                else
                {
                    list.Add(new(i, j, false, new()));
                }

            }
            UpdateTilesClientRPC(list, clientRpcParams);
            list = new();
        }

    }

    public bool ValidateWord(NetLetterInfoCollection newWord, ulong playerId, out List<ulong> defeatedPlayers, out HashSet<(int i, int j)> newPlayerWord)
    {
        defeatedPlayers = new List<ulong>();
        newPlayerWord = new HashSet<(int i, int j)>();

        PlayerGameNetworkData playerInfo = playersGameManager.GetPlayerNetworkData(playerId);
        bool firstWord = playerInfo.currentWord.Length == 0;

        if (firstWord && newWord.Count != 1)
        {
            ClientRpcParams clientRpcParams = new ClientRpcParams
            {
                Send = new ClientRpcSendParams
                {
                    TargetClientIds = new ulong[] { playerId }
                }
            };
            uiManager.ShowPopUpClientRpc("Самое первое слово может быть только буквой (т_т)", clientRpcParams);
            return false;
        }

        bool wordConnectedToPlayer = firstWord;

        bool onlyOneLetter = newWord.Count == 1;
        bool verticalWord = !onlyOneLetter && newWord[0].i == newWord[1].i;

        int letterCount = 1;
        bool isFirstCheckedLetter = true;

        if (onlyOneLetter)
        {
            if (CheckForLetterOnField(newWord[0].i + 1, newWord[0].j) || CheckForLetterOnField(newWord[0].i - 1, newWord[0].j))
            {
                verticalWord = false;
            }
            else
            if (CheckForLetterOnField(newWord[0].i, newWord[0].j + 1) || CheckForLetterOnField(newWord[0].i, newWord[0].j - 1))
            {
                verticalWord = true;
            }
        }


        // Проверка на правильность составленных слов
        foreach (var newLetter in newWord)
        {
            (int i, int j) newPos = newLetter.GetPosition();
            newPlayerWord.Add(newLetter.GetPosition());

            string letter = newLetter.GetValue();

            if (CheckForLetterOnField(newPos.i, newPos.j))
            {
                return false;
            }

            StringBuilder JebTheBuilder = new StringBuilder(letter); // Brother of BobTheBuilder (from Srcipts/Common/UpdateFieldSerializer)

            // Проверка на слово слева-справа
            FixedLetter fieldLetter;
            (int i, int j) checkPos = (newPos.i, newPos.j);
            while (CheckForLetterOnField(++checkPos.i, checkPos.j) || newWord.ContainsKey(checkPos))
            {
                if (CheckForLetterOnField(checkPos.i, checkPos.j, out fieldLetter))
                {
                    if (fieldLetter.hasOwner)
                    {
                        if (fieldLetter.ownerId == playerId)
                        {
                            wordConnectedToPlayer = true;
                        }
                        else
                        {
                            if (firstWord)
                            {
                                return false;
                            }
                            defeatedPlayers.Add(fieldLetter.ownerId);
                        }
                    }

                    if (!verticalWord)
                    {
                        newPlayerWord.Add((checkPos.i, checkPos.j));
                    }

                    JebTheBuilder.Append(fieldLetter.GetValue());
                }
                else
                {
                    if (verticalWord) return false;
                    if (isFirstCheckedLetter) letterCount++;

                    JebTheBuilder.Append(newWord[checkPos].GetValue());
                }
            }
            checkPos = (newPos.i, newPos.j);
            while (CheckForLetterOnField(--checkPos.i, checkPos.j) || newWord.ContainsKey(checkPos))
            {
                if (CheckForLetterOnField(checkPos.i, checkPos.j, out fieldLetter))
                {
                    if (fieldLetter.hasOwner)
                    {
                        if (fieldLetter.ownerId == playerId)
                        {
                            wordConnectedToPlayer = true;
                        }
                        else
                        {
                            if (firstWord)
                            {
                                return false;
                            }
                            defeatedPlayers.Add(fieldLetter.ownerId);
                        }
                    }

                    if (!verticalWord)
                    {
                        newPlayerWord.Add((checkPos.i, checkPos.j));
                    }

                    JebTheBuilder.Insert(0, fieldLetter.GetValue());
                }
                else
                {
                    if (verticalWord) return false;
                    if (isFirstCheckedLetter) letterCount++;

                    JebTheBuilder.Insert(0, newWord[checkPos].GetValue());
                }
            }

            if (letterCount != 1 && letterCount != newWord.Count) return false;
            if (!(!(JebTheBuilder.Length > 1) || !CheckForWordCorrect || _ALL_WORDS.Contains(JebTheBuilder.ToString().ToLower())))
            {
                return false;
            }

            // Проверка на слово сверху-снизу
            JebTheBuilder = new StringBuilder(letter);
            checkPos = (newPos.i, newPos.j);
            while (CheckForLetterOnField(checkPos.i, --checkPos.j) || newWord.ContainsKey(checkPos))
            {
                if (CheckForLetterOnField(checkPos.i, checkPos.j, out fieldLetter))
                {
                    if (fieldLetter.hasOwner)
                    {
                        if (fieldLetter.ownerId == playerId)
                        {
                            wordConnectedToPlayer = true;
                        }
                        else
                        {
                            if (firstWord)
                            {
                                return false;
                            }
                            defeatedPlayers.Add(fieldLetter.ownerId);
                        }
                    }

                    if (verticalWord)
                    {
                        newPlayerWord.Add((checkPos.i, checkPos.j));
                    }

                    JebTheBuilder.Append(fieldLetter.GetValue());
                }
                else
                {
                    if (!verticalWord) return false;
                    if (isFirstCheckedLetter) letterCount++;

                    JebTheBuilder.Append(newWord[checkPos].GetValue());
                }
            }
            checkPos = (newPos.i, newPos.j);
            while (CheckForLetterOnField(checkPos.i, ++checkPos.j) || newWord.ContainsKey(checkPos))
            {
                if (CheckForLetterOnField(checkPos.i, checkPos.j, out fieldLetter))
                {
                    if (fieldLetter.hasOwner)
                    {
                        if (fieldLetter.ownerId == playerId)
                        {
                            wordConnectedToPlayer = true;
                        }
                        else
                        {
                            if (firstWord)
                            {
                                return false;
                            }
                            defeatedPlayers.Add(fieldLetter.ownerId);
                        }
                    }

                    if (verticalWord)
                    {
                        newPlayerWord.Add((checkPos.i, checkPos.j));
                    }

                    JebTheBuilder.Insert(0, fieldLetter.GetValue());
                }
                else
                {
                    if (!verticalWord) return false;
                    if (isFirstCheckedLetter) letterCount++;

                    JebTheBuilder.Insert(0, newWord[checkPos].GetValue());
                }
            }

            if (letterCount != newWord.Count) return false;
            if (!(
                    !(JebTheBuilder.Length > 1)
                    ||
                    !CheckForWordCorrect
                    ||
                    _ALL_WORDS.Contains(JebTheBuilder.ToString().ToLower()))
                )
            {
                return false;
            }

            isFirstCheckedLetter = false;
        }

        if (!wordConnectedToPlayer) return false;

        return true;
    }

    public bool CheckForLetterOnField(int i, int j, out FixedLetter letter)
    {
        letter = null;
        if (i < 0 || j < 0 || i > NumberOfTiles || j > NumberOfTiles)
        {
            return false;
        }
        if (tiles[i, j].Letter is FixedLetter fixedLetter)
        {
            letter = fixedLetter;
            return true;
        }
        return false;
    }

    public bool CheckForLetterOnField(int i, int j)
    {
        return tiles[i, j].Letter is FixedLetter;
    }

}
