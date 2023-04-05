using Common;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.Netcode;

namespace Assets.GameProcessScripts
{
    public class NetLetterInfoCollection: INetworkSerializable
    {
        public List<NetLetterInfo> letters;
        public int Count => letters.Count;

        public NetLetterInfoCollection()
        {
            letters = new();
        }

        public NetLetterInfo this[(int i, int j) pos]
        {
            get { return letters.Find(x => (pos.i, pos.j) == x.GetPosition()); }
        }

        public NetLetterInfo this[int i]
        {
            get { return letters[i]; }
        }

        public void Add(int i, int j, string value)
        {
            letters.Add(new NetLetterInfo(i, j, value));
        }

        public bool ContainsKey((int i, int j) pos)
        {
            return letters.Exists(x => (pos.i, pos.j) == x.GetPosition());
        }

        public IEnumerator<NetLetterInfo> GetEnumerator()
        {
            return ((IEnumerable<NetLetterInfo>)letters).GetEnumerator();
        }

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            int count = letters.Count;
            serializer.SerializeValue(ref count);
            for (int i = 0; i < count; i++)
            {
                if (serializer.IsReader)
                {
                    letters.Add(new NetLetterInfo(0, 0, "A"));
                }
                NetLetterInfo value = letters[i];
                serializer.SerializeValue(ref value);
                if (serializer.IsReader)
                {
                    letters[i] = value;
                }
            }
        }
    }
}
