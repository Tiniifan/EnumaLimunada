using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace IEGOModelConverter.Level5.Resource.XRES
{
    public class XRESSupport
    {
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct Header
        {
            public uint Magic;
            public short StringOffset;
            public short Unk1;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 0x0C)]
            public byte[] EmptyBlock1;
            public HeaderTable MaterialTypeUnk1;
            public HeaderTable Material1;
            public HeaderTable Material2;
            public HeaderTable MaterialSplit;
            public HeaderTable MaterialTypeUnk2;
            public int EmptyBlock2;
            public HeaderTable TextureData;
            public int EmptyBlock3;
            public HeaderTable TextureName;
            public HeaderTable Bone;
            public HeaderTable Animation1;
            public HeaderTable Animation2;
            public HeaderTable NodeTypeUnk1;
            public HeaderTable Shading;
            public HeaderTable NodeTypeUnk2;
            public HeaderTable BoundingBoxParameter;
            public HeaderTable AnimationSplit1;
            public HeaderTable AnimationSplit2;
            public HeaderTable NodeTypeUnk3;
            public HeaderTable Textproj;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct HeaderTable
        {
            public short DataOffset;
            public short Count;
        }

        public static Dictionary<RESType, int> TypeLength = new Dictionary<RESType, int>
        {
            {RESType.Bone, 8 },
            {RESType.Textproj, 8 },
            {RESType.BoundingBoxParameter, 8 },
            {RESType.Shading, 8 },
            {RESType.Material1, 8 },
            {RESType.Material2, 8 },
            {RESType.TextureName, 8 },
            {RESType.MaterialSplit, 32 },
            {RESType.TextureData, 224 },
            {RESType.Animation1, 8 },
            {RESType.Animation2, 8 },
            {RESType.AnimationSplit1, 8 },
            {RESType.AnimationSplit2, 8 },
        };

        public static List<RESType> TypeOrder = new List<RESType>
        {
            RESType.MaterialTypeUnk1,
            RESType.Material1,
            RESType.Material2,
            RESType.MaterialSplit,
            RESType.MaterialTypeUnk2,
            RESType.TextureData,
            RESType.TextureName,
            RESType.Bone,
            RESType.Animation1,
            RESType.Animation2,
            RESType.NodeTypeUnk1,
            RESType.Shading,
            RESType.NodeTypeUnk2,
            RESType.BoundingBoxParameter,
            RESType.AnimationSplit1,
            RESType.AnimationSplit2,
            RESType.NodeTypeUnk3,
            RESType.Textproj,
        };
    }
}
