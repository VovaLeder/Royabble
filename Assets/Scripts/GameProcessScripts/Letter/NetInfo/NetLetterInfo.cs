using Common;
using System;
using Unity.Collections;
using Unity.Netcode;

namespace Assets.GameProcessScripts
{
    public struct NetLetterInfo: IEquatable<NetLetterInfo>, INetworkSerializable
    {
        public int i;
        public int j;

        public FixedString32Bytes value;

        public NetLetterInfo(int i, int j, string value)
        {
            this.i = i;
            this.j = j;
            this.value = value;
        }

        public bool Equals(NetLetterInfo other)
        {
            return i == other.i && j == other.j && value == other.value;
        }

        public (int i, int j) GetPosition()
        {
            return (i, j);
        }

        public string GetValue()
        {
            return value.ToString();
        }

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref i);
            serializer.SerializeValue(ref j);

            serializer.SerializeValue(ref value);
        }
    }
}
