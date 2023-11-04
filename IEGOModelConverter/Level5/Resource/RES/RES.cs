using System.IO;
using System.Text;
using System.Collections.Generic;
using IEGOModelConverter.Tools;
using IEGOModelConverter.Level5.Compression;

namespace IEGOModelConverter.Level5.Resource.RES
{
    public class RES : IResource
    {
        public string Name => "RES";

        public List<string> StringTable { get; set; }

        public Dictionary<RESType, List<byte[]>> Items { get; set; }

        public RES(Stream stream)
        {
            StringTable = new List<string>();
            Items = new Dictionary<RESType, List<byte[]>>();

            using (BinaryDataReader reader = new BinaryDataReader(Compressor.Decompress(stream))) 
            {
                RESSupport.Header header = reader.ReadStruct<RESSupport.Header>();

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

                ReadSectionTable(reader, header.MaterialTableOffset, header.MaterialTableCount);
                ReadSectionTable(reader, header.NodeOffset, header.NodeCount);
            }
        }

        public RES(byte[] data)
        {
            StringTable = new List<string>();
            Items = new Dictionary<RESType, List<byte[]>>();

            using (BinaryDataReader reader = new BinaryDataReader(Compressor.Decompress(data)))
            {
                RESSupport.Header header = reader.ReadStruct<RESSupport.Header>();

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

                ReadSectionTable(reader, header.MaterialTableOffset, header.MaterialTableCount);
                ReadSectionTable(reader, header.NodeOffset, header.NodeCount);
            }
        }

        public RES(List<string> stringTable, Dictionary<RESType, List<byte[]>> items)
        {
            StringTable = stringTable;
            Items = items;
        }

        public void Save(string filepath)
        {

        }

        private void ReadSectionTable(BinaryDataReader reader, int tableOffset, int tableCount)
        {
            for (int i = 0; i < tableCount; i++)
            {
                reader.Seek(tableOffset + i * 8);

                RESSupport.HeaderTable headerTable = reader.ReadStruct<RESSupport.HeaderTable>();

                if (!Items.ContainsKey((RESType)headerTable.Type))
                {
                    Items.Add((RESType)headerTable.Type, new List<byte[]>());
                }

                for (int j = 0; j < headerTable.Count; j++)
                {
                    reader.Seek((uint)(headerTable.DataOffset + j * headerTable.Length));
                    Items[(RESType)headerTable.Type].Add(reader.GetSection(headerTable.Length));
                }
            }
        }
    }
}
