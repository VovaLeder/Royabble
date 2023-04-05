using Assets.GameProcessScripts;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

namespace Common
{
    public struct PlayerData: IEquatable<PlayerData>, INetworkSerializable
    {
        public ulong clientId;
        public FixedString64Bytes playerId;

        public int colorId;
        public FixedString64Bytes nickname;

        public PlayerData(ulong clientId, FixedString64Bytes nickname, int colorId, FixedString64Bytes playerId)
        {
            this.clientId = clientId;
            this.playerId = playerId;

            this.colorId = colorId;
            this.nickname = nickname;
        }

        public PlayerData Clone()
        {
            return new PlayerData(clientId, nickname, colorId, playerId);
        }

        public bool Equals(PlayerData other)
        {
            return clientId == other.clientId && playerId == other.playerId;
        }

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref clientId);
            serializer.SerializeValue(ref playerId);

            serializer.SerializeValue(ref colorId);
            serializer.SerializeValue(ref nickname);
        }
    }
}
