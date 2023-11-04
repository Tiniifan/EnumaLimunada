using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IEGOModelConverter.Tools;
using IEGOModelConverter.Level5.Resource.RES;
using IEGOModelConverter.Level5.Resource.XRES;
using IEGOModelConverter.Level5.Archive.XPCK;
using IEGOModelConverter.Level5.Material;

namespace IEGOModelConverter
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            if (args.Length < 1)
            {
                Console.WriteLine("Usage:");
                Console.WriteLine("  ConvertXPCKToIEGO.exe <XPCKFilePath> : Convert the XPCK file to the IEGO format.");
                return;
            }

            string xpckFilePath = args[0];

            if (!File.Exists(xpckFilePath))
            {
                Console.WriteLine($"The file {xpckFilePath} does not exist.");
                return;
            }

            string outputFolderPath = Path.Combine(Path.GetDirectoryName(xpckFilePath), Path.GetFileNameWithoutExtension(xpckFilePath) + $"_{Path.GetExtension(xpckFilePath).Substring(1)}");

            if (!Directory.Exists(outputFolderPath))
            {
                Directory.CreateDirectory(outputFolderPath);
            }

            XPCK archive = new XPCK(new FileStream(xpckFilePath, FileMode.Open, FileAccess.Read));

            foreach (KeyValuePair<string, SubMemoryStream> file in archive.Directory.Files)
            {
                if (file.Key.EndsWith(".atr"))
                {
                    file.Value.Read();

                    file.Value.ByteContent = new byte[] { 0x41, 0x54, 0x52, 0x43, 0x30, 0x30, 0x00, 0x00, 0x0C, 0x00, 0x00, 0x00, 0x23, 0x02, 0x00, 0x00, 0x03, 0x40, 0xC0, 0xFF, 0x01, 0x00, 0x00, 0x00, 0xD5, 0x15, 0x15, 0x15, 0x7F, 0x5D, 0x55, 0x57, 0xFF, 0xFF, 0xFF, 0xF5 };

                    File.WriteAllBytes(outputFolderPath + "/" + file.Key, file.Value.ByteContent);
                }
                else if (file.Key.EndsWith(".mtr"))
                {
                    file.Value.Read();

                    MTRC mtrc = new MTRC(file.Value.ByteContent);

                    using (BinaryDataWriter writer = new BinaryDataWriter(mtrc.MTRCData))
                    {
                        writer.Seek(0x80);
                        writer.WriteStruct<float>(0.5f);
                        writer.WriteStruct<float>(0.5f);
                        writer.WriteStruct<float>(0.5f);

                        writer.Seek(0xA4);
                        writer.WriteStruct<float>(0.5f);
                        writer.WriteStruct<float>(0.5f);
                        writer.WriteStruct<float>(0.5f);
                    }

                    byte[] resizedArray = new byte[228];
                    Array.Copy(mtrc.MTRCData, resizedArray, 228);
                    mtrc.MTRCData = resizedArray;

                    file.Value.ByteContent = mtrc.Save();

                    File.WriteAllBytes(outputFolderPath + "/" + file.Key, file.Value.ByteContent);
                }
                else if (file.Key.EndsWith(".prm"))
                {
                    file.Value.Read();

                    int materialLibOffset = 0;
                    using (BinaryDataReader prmReader = new BinaryDataReader(file.Value.ByteContent))
                    {
                        prmReader.Seek(0x0C);
                        materialLibOffset = prmReader.ReadValue<int>() + 8;
                    }

                    using (BinaryDataWriter prmWriter = new BinaryDataWriter(file.Value.ByteContent))
                    {
                        prmWriter.Seek(materialLibOffset);
                        prmWriter.Write(0xF3E59F75);
                    }

                    File.WriteAllBytes(outputFolderPath + "/" + file.Key, file.Value.ByteContent);
                }
                else if (file.Key == "RES.bin")
                {
                    file.Value.Read();

                    RES resCsGalaxy = new RES(file.Value.ByteContent);
                    XRES resGO = new XRES(resCsGalaxy.StringTable, resCsGalaxy.Items);

                    file.Value.ByteContent = resGO.Save();

                    File.WriteAllBytes(outputFolderPath + "/" + file.Key, file.Value.ByteContent);
                }
            }

            Console.WriteLine("Conversion completed successfully, check: ", outputFolderPath);
        }
    }
}
