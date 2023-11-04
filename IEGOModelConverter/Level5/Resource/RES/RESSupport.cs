using System.Runtime.InteropServices;

namespace IEGOModelConverter.Level5.Resource.RES
{
    public class RESSupport
    {
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct Header
        {
            public long Magic;
            public short _stringOffset;
            public short Unk1;
            private short _materialTableOffset;
            public short MaterialTableCount;
            private short _nodeOffset;
            public short NodeCount;

            public int StringOffset => _stringOffset << 2;
            public int MaterialTableOffset => _materialTableOffset << 2;
            public int NodeOffset => _nodeOffset << 2;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct HeaderTable
        {
            private short _dataOffset;
            public short Count;
            public short Type;
            public short Length;

            public int DataOffset => _dataOffset << 2;
        }
    }
}
