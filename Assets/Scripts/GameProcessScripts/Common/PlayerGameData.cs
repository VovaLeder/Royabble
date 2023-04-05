using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace Assets.GameProcessScripts
{
    public class PlayerGameData
    {
        public ulong clientId;

        public List<string> hand;

        public float hp;

        public int points;

        public int availablePoints;

        public float letterTimer;
        public float letterTimerCap;

        public PlayerGameData(ulong id)
        {
            this.clientId = id;

            hand = new();

            hp = PlayersGameManager.Instance.GetMaxHp();

            letterTimer = 0;
            letterTimerCap = PlayersGameManager.Instance.GetDefaultLetterTimerCap();
        }
    }
}
