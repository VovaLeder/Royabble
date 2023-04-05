using System;
using Unity.Netcode;

namespace Common
{
    public struct Pair : IEquatable<Pair>, INetworkSerializable
    {
        public int i;
        public int j;

        public bool Equals(Pair other)
        {
            return i == other.i && j == other.j;
        }

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref i);
            serializer.SerializeValue(ref j);
        }

        public void Deconstruct(out int i, out int j)
        {
            i = this.i;
            j = this.j;
        }
    }
}
