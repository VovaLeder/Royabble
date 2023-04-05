using Unity.Netcode;
using Unity.VisualScripting;

namespace Assets.GameProcessScripts
{
    public struct TileLetterInfo: INetworkSerializable
    {
        public int i;
        public int j;
        public bool letterPresent;

        public OwnedLetterInfo letter;

        public TileLetterInfo(int i, int j, bool letterPresent, OwnedLetterInfo info)
        {
            this.i = i;
            this.j = j;
            this.letterPresent = letterPresent;
            if (letterPresent)
            {
                letter = info.Clone();
            }
            else
            {
                letter = new();
            }
            
        }

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref i);
            serializer.SerializeValue(ref j);

            serializer.SerializeValue(ref letterPresent);
            serializer.SerializeValue(ref letter);
        }
    }
}
