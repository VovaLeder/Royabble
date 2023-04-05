using Common;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.Netcode;

namespace Assets.GameProcessScripts
{
    public class TileLetterInfoCollection : INetworkSerializable
    {
        public List<TileLetterInfo> letters;
        public int Count => letters.Count;

        public TileLetterInfoCollection()
        {
            letters = new();
        }

        //public TileLetterInfo this[(int i, int j) pos]
        //{
        //    get { return letters.Find(x => (pos.i, pos.j) == x.GetPosition()); }
        //}

        public TileLetterInfo this[int i]
        {
            get { return letters[i]; }
        }

        public void Add(int i, int j, bool letterPresent, OwnedLetterInfo ownedLetterInfo)
        {
            letters.Add(new TileLetterInfo(i, j, letterPresent, ownedLetterInfo));
        }

        public void Add(TileLetterInfo info)
        {
            letters.Add(info);
        }

        //public bool ContainsKey((int i, int j) pos)
        //{
        //    return letters.Exists(x => (pos.i, pos.j) == x.GetPosition());
        //}

        public IEnumerator<TileLetterInfo> GetEnumerator()
        {
            return ((IEnumerable<TileLetterInfo>)letters).GetEnumerator();
        }

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            int count = letters.Count;
            serializer.SerializeValue(ref count);
            for (int i = 0; i < count; i++)
            {
                if (serializer.IsReader)
                {
                    letters.Add(new TileLetterInfo(0, 0, false, new()));
                }
                TileLetterInfo value = letters[i];
                serializer.SerializeValue(ref value);
                if (serializer.IsReader)
                {
                    letters[i] = value;
                }
            }
        }
    }
}
