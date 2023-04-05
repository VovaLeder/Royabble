using Assets.GameProcessScripts;
using System.IO;
using Unity.Netcode;
using UnityEngine;
using Common;

public class FixedLetter : NetworkBehaviour, ILetter
{
    public LetterStick Tile;

    public string Value;

    public bool hasOwner { get; private set; }
    public ulong ownerId;

    public (int i, int j) GetCoords()
    {
        return (Tile.XCoord, Tile.YCoord);
    }

    public void Stick(LetterStick tile)
    {
        if (Tile != null) Tile.Letter = null;
        Tile = tile;
        tile.Letter = this;
    }

    public void ChangeInfo(OwnedLetterInfo letterinfo)
    {
        Value = letterinfo.GetValue();

        setOwner(letterinfo.hasOwner, letterinfo.GetOwnerId());

        GetComponent<SpriteRenderer>().sprite =
                    Resources.Load(Path.Combine("Sprites", "Letters", "Russian", Value), typeof(Sprite)) as Sprite;
    }

    public void resetOwner()
    {
        hasOwner = false;
        GetComponent<SpriteRenderer>().color = new Color(1, 1, 1);
    }

    public void setOwner(bool hasOwner, ulong ownerId)
    {
        this.hasOwner = hasOwner;
        if (hasOwner) {
            this.ownerId = ownerId;
            GetComponent<SpriteRenderer>().color = PlayersManager.Instance.Colors[PlayersManager.Instance.GetPlayerData(ownerId).colorId];
        }
        else
        {
            resetOwner();
        }
    }

    public string GetValue()
    {
        return Value;
    }

}
