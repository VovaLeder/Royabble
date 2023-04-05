using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VoidTools;

namespace VoidTools
{
    internal struct GuiContainer
    {
        internal string Path;
        internal List<GuiData> Data;

        internal GuiContainer(int _size)
        {
            Path = string.Empty;
            Data = new List<GuiData>(_size);
        }
    }

    internal struct GuiData
    {
        internal MessagePriorityLevel PriorityLevel;
        internal string FileName;
        internal string Message;
        internal int LineNumber;
    }
}
