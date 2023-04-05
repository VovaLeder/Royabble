using Assets.GameProcessScripts;
using Common;
using Unity.Collections;
using Unity.Netcode;

namespace Assets.GameProcessScripts
{
    public struct OwnedLetterInfo: INetworkSerializable
    {
        public int i;
        public int j;

        public FixedString32Bytes value;

        public bool hasOwner;
        public ulong ownerId;

        public OwnedLetterInfo(int i, int j, FixedString32Bytes value, bool hasOwner, ulong ownerId)
        {
            this.i = i;
            this.j = j;
            this.value = value;
            this.hasOwner = hasOwner;
            this.ownerId = ownerId;
        }

        public (int i, int j) GetPosition()
        {
            return (i, j);
        }

        public ulong GetOwnerId()
        {
            return ownerId;
        }

        public ulong SetOwnerId(ulong info)
        {
            return ownerId = info;
        }

        public string GetValue()
        {
            return value.ToString();
        }

        public OwnedLetterInfo Clone()
        {
            return new(i, j, value, hasOwner, ownerId);
        }

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref i);
            serializer.SerializeValue(ref j);

            serializer.SerializeValue(ref value);
            serializer.SerializeValue(ref hasOwner);
            serializer.SerializeValue(ref ownerId);
        }
    }
}
