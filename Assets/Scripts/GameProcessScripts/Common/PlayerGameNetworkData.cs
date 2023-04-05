using Common;
using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Netcode;

namespace Assets.GameProcessScripts
{
    public struct PlayerGameNetworkData: IEquatable<PlayerGameNetworkData>, INetworkSerializable
    {
        public ulong clientId;

        public FixedList512Bytes<Pair> currentWord;
        public bool alive;

        public List<Pair> GetCurrentWord()
        {
            List<Pair> word = new();
            for (int i = 0; i < currentWord.Length; i++)
            {
                word.Add(currentWord[i]);
            }
            return word;
        }

        public bool Equals(PlayerGameNetworkData other)
        {
            return clientId == other.clientId;
        }

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref clientId);

            int count = currentWord.Length;
            serializer.SerializeValue(ref count);
            for (int i = 0; i < count; i++)
            {
                if (serializer.IsReader)
                {
                    currentWord.Add(new Pair());
                }
                Pair value = currentWord[i];
                serializer.SerializeValue(ref value);
                if (serializer.IsReader)
                {
                    currentWord[i] = value;
                }
            }

            serializer.SerializeValue(ref alive);
        }

    }
}
