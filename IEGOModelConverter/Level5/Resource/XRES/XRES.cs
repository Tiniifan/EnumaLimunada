using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IEGOModelConverter.Tools;
using IEGOModelConverter.Level5.Compression;
using IEGOModelConverter.Level5.Compression.NoCompression;

namespace IEGOModelConverter.Level5.Resource.XRES
{
    public class XRES : IResource
    {
        public string Name => "XRES";

        public List<string> StringTable { get; set; }

        public Dictionary<RESType, List<byte[]>> Items { get; set; }

        public XRES(Stream stream)
        {
            StringTable = new List<string>();
            Items = new Dictionary<RESType, List<byte[]>>();

            using (BinaryDataReader reader = new BinaryDataReader(Compressor.Decompress(stream)))
            {
                XRESSupport.Header header = reader.ReadStruct<XRESSupport.Header>();

                reader.Seek(header.StringOffset);
                using (BinaryDataReader textReader = new BinaryDataReader(reader.GetSection((int)(reader.Length - reader.Position))))
                {
                    while (textReader.Position < textReader.Length)
                    {
                        string name = textReader.ReadString(Encoding.UTF8);

                        if (name != "" && name != " ")
                        {
                            StringTable.Add(name);
                        }
                    }
                }

                Items.Add(RESType.Bone, ReadType(reader, header.Bone, RESType.Bone));
                Items.Add(RESType.Textproj, ReadType(reader, header.Textproj, RESType.Textproj));
                Items.Add(RESType.BoundingBoxParameter, ReadType(reader, header.BoundingBoxParameter, RESType.BoundingBoxParameter));
                Items.Add(RESType.Shading, ReadType(reader, header.Shading, RESType.Shading));
                Items.Add(RESType.Material1, ReadType(reader, header.Material1, RESType.Material1));
                Items.Add(RESType.Material2, ReadType(reader, header.Material2, RESType.Material2));
                Items.Add(RESType.TextureName, ReadType(reader, header.TextureName, RESType.TextureName));
                Items.Add(RESType.MaterialSplit, ReadType(reader, header.MaterialSplit, RESType.MaterialSplit));
                Items.Add(RESType.TextureData, ReadType(reader, header.TextureData, RESType.TextureData));
                Items.Add(RESType.Animation1, ReadType(reader, header.Animation1, RESType.Animation1));
                Items.Add(RESType.Animation2, ReadType(reader, header.Animation2, RESType.Animation2));
                Items.Add(RESType.AnimationSplit1, ReadType(reader, header.AnimationSplit1, RESType.AnimationSplit1));
                Items.Add(RESType.AnimationSplit2, ReadType(reader, header.AnimationSplit2, RESType.AnimationSplit2));
            }
        }

        public XRES(List<string> stringTable, Dictionary<RESType, List<byte[]>> items)
        {
            StringTable = stringTable;
            Items = items;
        }

        public void Save(string fileName)
        {
            using (FileStream stream = new FileStream(fileName, FileMode.Create, FileAccess.Write))
            {
                BinaryDataWriter writerComp = new BinaryDataWriter(stream);

                using (MemoryStream memoryStream = new MemoryStream())
                {
                    BinaryDataWriter writerDecomp = new BinaryDataWriter(memoryStream);

                    // Fix items size
                    for (int i = 0; i < XRESSupport.TypeOrder.Count; i++)
                    {
                        RESType resType = XRESSupport.TypeOrder[i];

                        if (Items.ContainsKey(resType))
                        {
                            for (int j = 0; j < Items[resType].Count; j++)
                            {
                                if (XRESSupport.TypeLength[resType] != Items[resType][j].Length)
                                {
                                    byte[] resizedArray = new byte[XRESSupport.TypeLength[resType]];
                                    Array.Copy(Items[resType][j], resizedArray, Math.Min(XRESSupport.TypeLength[resType], Items[resType][j].Length));
                                    Items[resType][j] = resizedArray;
                                }
                            }
                        }
                    }

                    int stringOffset = 0x64 + Items.Values.SelectMany(itemData => itemData).Sum(byteArray => byteArray.Length);

                    // Header
                    writerDecomp.Write(0x53455258);
                    writerDecomp.Write((short)stringOffset);
                    writerDecomp.Write((short)1);
                    writerDecomp.Write(new byte[0x0C]);

                    // Header table
                    int dataOffset = 0x64;
                    for (int i = 0; i < XRESSupport.TypeOrder.Count; i++)
                    {
                        RESType resType = XRESSupport.TypeOrder[i];

                        if (Items.ContainsKey(resType))
                        {
                            writerDecomp.Write((short)dataOffset);
                            writerDecomp.Write((short)Items[resType].Count);
                            dataOffset += Items[resType].Select(itemData => itemData).Sum(byteArray => byteArray.Length);
                        }
                        else
                        {
                            writerDecomp.Write((short)0x64);
                            writerDecomp.Write((short)0);
                        }

                        if (i == 4 || i == 5)
                        {
                            writerDecomp.Write(0);
                        }
                    }

                    // Data
                    for (int i = 0; i < XRESSupport.TypeOrder.Count; i++)
                    {
                        RESType resType = XRESSupport.TypeOrder[i];

                        if (Items.ContainsKey(resType))
                        {
                            writerDecomp.Write(Items[resType].SelectMany(bytes => bytes).ToArray());
                        }
                    }

                    // String table
                    for (int i = 0; i < StringTable.Count; i++)
                    {
                        writerDecomp.Write(Encoding.UTF8.GetBytes(StringTable[i]));
                        writerDecomp.Write((byte)0);
                    }

                    writerDecomp.WriteAlignment(4);

                    // Compress
                    writerComp.Write(new NoCompression().Compress(memoryStream.ToArray()));
                }
            }
        }

        public byte[] Save()
        {
            using (MemoryStream fileStream = new MemoryStream())
            {
                BinaryDataWriter writerComp = new BinaryDataWriter(fileStream);

                using (MemoryStream memoryStream = new MemoryStream())
                {
                    BinaryDataWriter writerDecomp = new BinaryDataWriter(memoryStream);

                    // Fix items size
                    for (int i = 0; i < XRESSupport.TypeOrder.Count; i++)
                    {
                        RESType resType = XRESSupport.TypeOrder[i];

                        if (Items.ContainsKey(resType))
                        {
                            for (int j = 0; j < Items[resType].Count; j++)
                            {
                                if (XRESSupport.TypeLength[resType] != Items[resType][j].Length)
                                {
                                    byte[] resizedArray = new byte[XRESSupport.TypeLength[resType]];
                                    Array.Copy(Items[resType][j], resizedArray, Math.Min(XRESSupport.TypeLength[resType], Items[resType][j].Length));
                                    Items[resType][j] = resizedArray;
                                }
                            }
                        }
                    }

                    int stringOffset = 0x64 + Items.Values.SelectMany(itemData => itemData).Sum(byteArray => byteArray.Length);

                    // Header
                    writerDecomp.Write(0x53455258);
                    writerDecomp.Write((short)stringOffset);
                    writerDecomp.Write((short)1);
                    writerDecomp.Write(new byte[0x0C]);

                    // Header table
                    int dataOffset = 0x64;
                    for (int i = 0; i < XRESSupport.TypeOrder.Count; i++)
                    {
                        RESType resType = XRESSupport.TypeOrder[i];

                        if (Items.ContainsKey(resType))
                        {
                            writerDecomp.Write((short)dataOffset);
                            writerDecomp.Write((short)Items[resType].Count);
                            dataOffset += Items[resType].Select(itemData => itemData).Sum(byteArray => byteArray.Length);
                        }
                        else
                        {
                            writerDecomp.Write((short)0x64);
                            writerDecomp.Write((short)0);
                        }

                        if (i == 4 || i == 5)
                        {
                            writerDecomp.Write(0);
                        }
                    }

                    // Data
                    for (int i = 0; i < XRESSupport.TypeOrder.Count; i++)
                    {
                        RESType resType = XRESSupport.TypeOrder[i];

                        if (Items.ContainsKey(resType))
                        {
                            writerDecomp.Write(Items[resType].SelectMany(bytes => bytes).ToArray());
                        }
                    }

                    // String table
                    for (int i = 0; i < StringTable.Count; i++)
                    {
                        writerDecomp.Write(Encoding.UTF8.GetBytes(StringTable[i]));
                        writerDecomp.Write((byte)0);
                    }

                    writerDecomp.WriteAlignment(4);

                    // Compress
                    writerComp.Write(new NoCompression().Compress(memoryStream.ToArray()));
                }

                return fileStream.ToArray();
            }
        }

        private List<byte[]> ReadType(BinaryDataReader reader, XRESSupport.HeaderTable headerTable, RESType type)
        {
            List<byte[]> output = new List<byte[]>();

            for (int i = 0; i < headerTable.Count; i++)
            {
                reader.Seek((uint)(headerTable.DataOffset + i * XRESSupport.TypeLength[type]));
                output.Add(reader.GetSection(XRESSupport.TypeLength[type]));
            }

            return output;
        }
    }
}
