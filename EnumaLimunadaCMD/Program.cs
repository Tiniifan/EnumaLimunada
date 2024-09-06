using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StudioElevenLib.Tools;
using StudioElevenLib.Level5.Resource.RES;
using StudioElevenLib.Level5.Resource.XRES;
using StudioElevenLib.Level5.Archive.XPCK;
using StudioElevenLib.Level5.Material;
using StudioElevenLib.Level5.Animation;
using StudioElevenLib.Level5.Camera.CMR2;
using StudioElevenLib.Level5.Camera.CMR1;

namespace EnumaLimunadaCMD
{
    internal class Program
    {
        private static uint[] KnownMaterialHashes = new uint[]
        {
            0xC0A58CCF,
            0x547E69F1,
            0xF3E59F75,
            0xBA6C9549
        };

        private static byte[] ConvertATR(byte[] data, bool isPlayer)
        {
            if (isPlayer)
            {
                return new byte[] { 0x41, 0x54, 0x52, 0x43, 0x30, 0x30, 0x00, 0x00, 0x0C, 0x00, 0x00, 0x00, 0x23, 0x02, 0x00, 0x00, 0x07, 0x40, 0x40, 0x00, 0x00, 0xFF, 0xC0, 0x81, 0x80, 0x01, 0x03, 0xC0, 0x02, 0x06, 0x00, 0x00, 0xF5, 0xD7, 0xE3, 0x1F, 0xB2, 0x98, 0xE1, 0xFC, 0xF5, 0xF8, 0xE1, 0x2C, 0x50, 0x55, 0x55, 0x55 };
            } else
            {
                return new byte[] { 0x41, 0x54, 0x52, 0x43, 0x30, 0x30, 0x00, 0x00, 0x0C, 0x00, 0x00, 0x00, 0x23, 0x02, 0x00, 0x00, 0x03, 0x40, 0xC0, 0xFF, 0x01, 0x00, 0x00, 0x00, 0xD5, 0x15, 0x15, 0x15, 0x7F, 0x5D, 0x55, 0x57, 0xFF, 0xFF, 0xFF, 0xF5 };
            }
        }

        private static byte[] ConvertMTR(byte[] data)
        {
            MTRC mtrc = new MTRC(data);

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

            byte[] resizedArray = new byte[mtrc.MTRCData.Length - 4];
            Array.Copy(mtrc.MTRCData, resizedArray, resizedArray.Length);
            mtrc.MTRCData = resizedArray;

            return mtrc.Save();
        }

        private static byte[] ConvertPRM(byte[] data)
        {
            int materialLibOffset = 0;
            uint materialHash = 0;

            using (BinaryDataReader prmReader = new BinaryDataReader(data))
            {
                prmReader.Seek(0x0C);
                materialLibOffset = prmReader.ReadValue<int>() + 8;
                prmReader.Seek(materialLibOffset);
                materialHash = prmReader.ReadValue<uint>();
            }

            using (BinaryDataWriter prmWriter = new BinaryDataWriter(data))
            {
                prmWriter.Seek(materialLibOffset);

                if (!KnownMaterialHashes.Contains(materialHash))
                {
                    prmWriter.Write(0xF3E59F75);
                }
            }

            return data;
        }

        private static byte[] ConvertAnimation(byte[] data)
        {
            using (MemoryStream stream = new MemoryStream(data))
            {
                AnimationManager animationManager = new AnimationManager(stream);
                animationManager.Version = "V1";
                return animationManager.Save();
            }
        }

        private static byte[] ConvertCamera(byte[] data)
        {
            CMR2 cmr2 = new CMR2(data);
            return new CMR1().Save(cmr2.HashName, cmr2.CamValues, cmr2.FrameCount, cmr2.CameraSpeed);
        }

        private static byte[] ConvertRES(byte[] data, bool flagEnabled, XPCK archive = null)
        {
            RES resCsGalaxy = new RES(data);
            XRES resGO = new XRES(resCsGalaxy.StringTable, resCsGalaxy.Items);

            if (flagEnabled)
            {
                if (!resGO.Items.ContainsKey(StudioElevenLib.Level5.Resource.RESType.BoundingBoxParameter))
                {
                    string[] flagsName = new string[] {
                                "bb_ref_bone",
                                "bb_size_x",
                                "bb_size_y",
                                "bb_size_z",
                                "bb_pos_x",
                                "bb_pos_y",
                                "bb_pos_z",
                                "bb_ratio_x",
                                "bb_ratio_y",
                                "bb_ratio_z",
                                "chr_flag",
                                "mesh_sort" };

                    // Add property files in xpck
                    if (archive != null)
                    {
                        int fileNameCount = 0;
                        foreach (string flagName in flagsName)
                        {
                            List<byte> fileContent = new List<byte>();

                            if (flagName == "bb_ref_bone")
                            {
                                fileContent.AddRange(BitConverter.GetBytes(0));
                            }
                            else
                            {
                                fileContent.AddRange(BitConverter.GetBytes(1));
                            }

                            fileContent.AddRange(BitConverter.GetBytes(Crc32.Compute(Encoding.UTF8.GetBytes(flagName))));

                            if (flagName == "bb_ref_bone" || flagName == "mesh_sort")
                            {
                                fileContent.AddRange(BitConverter.GetBytes(1));
                            }
                            else
                            {
                                fileContent.AddRange(BitConverter.GetBytes(0));
                            }

                            archive.Directory.Files.Add($"{fileNameCount.ToString().PadLeft(3, '0')}.cmn", new SubMemoryStream(fileContent.ToArray()));

                            fileNameCount++;
                        }
                    }
                    
                    // Add property in res
                    List<byte[]> propertyContent = new List<byte[]>() { };
                    foreach (string flagName in flagsName)
                    {
                        List<byte> bytes = new List<byte>();

                        bytes.AddRange(BitConverter.GetBytes(Crc32.Compute(Encoding.UTF8.GetBytes(flagName))));
                        bytes.AddRange(BitConverter.GetBytes(0));

                        propertyContent.Add(bytes.ToArray());
                    }

                    resGO.Items.Add(StudioElevenLib.Level5.Resource.RESType.BoundingBoxParameter, propertyContent);
                }
            }

            return resGO.Save();
        }

        private static byte[] ConvertArchive(XPCK archive, bool flagEnabled, bool isPlayer)
        {
            for (int i = 0; i < archive.Directory.Files.Count; i++)
            {
                var file = archive.Directory.Files.ElementAt(i);

                if (file.Key.EndsWith(".atr"))
                {
                    file.Value.Read();
                    file.Value.ByteContent = ConvertATR(file.Value.ByteContent, isPlayer);
                }
                else if (file.Key.EndsWith(".mtr"))
                {
                    file.Value.Read();
                    file.Value.ByteContent = ConvertMTR(file.Value.ByteContent);
                }
                else if (file.Key.EndsWith(".prm"))
                {
                    file.Value.Read();
                    file.Value.ByteContent = ConvertPRM(file.Value.ByteContent);
                }
                else if (file.Key.EndsWith(".mtn2") || file.Key.EndsWith(".imm2") || file.Key.EndsWith(".mtm2"))
                {
                    file.Value.Read();
                    file.Value.ByteContent = ConvertAnimation(file.Value.ByteContent);
                }
                else if (file.Key.EndsWith(".cmr2"))
                {
                    file.Value.Read();
                    file.Value.ByteContent = ConvertCamera(file.Value.ByteContent);
                }
                else if (file.Key == "RES.bin")
                {
                    file.Value.Read();
                    file.Value.ByteContent = ConvertRES(file.Value.ByteContent, flagEnabled, archive);
                }
                else if (file.Key.EndsWith(".xc") || file.Key.EndsWith(".xv"))
                {
                    file.Value.Read();
                    XPCK childArchive = new XPCK(file.Value.ByteContent);
                    file.Value.ByteContent = ConvertArchive(childArchive, flagEnabled, isPlayer);
                }
            }

            return archive.Save();
        }

        public static void Main(string[] args)
        {
            if (args.Length < 1)
            {
                Console.WriteLine("EnumaLimunadaCMD.exe <FilePath> [options]");
                Console.WriteLine("Convert the specified file to the appropriate IEGO format based on its extension.");
                Console.WriteLine();
                Console.WriteLine("Supported formats:");
                Console.WriteLine("  .atr           : Convert ATR files (supports -player option).");
                Console.WriteLine("  .mtr           : Convert MTR files.");
                Console.WriteLine("  .prm           : Convert PRM files.");
                Console.WriteLine("  .mtn2          : Convert animation files (MTN2 format).");
                Console.WriteLine("  .imm2          : Convert animation files (IMM2 format).");
                Console.WriteLine("  .mtm2          : Convert animation files (MTM2 format).");
                Console.WriteLine("  .cmr2          : Convert CMR2 camera files.");
                Console.WriteLine("  RES.bin        : Convert RES files (supports -flags option for adding properties).");
                Console.WriteLine("  .pck/.xc/.xv   : Convert XPCK archives (supports -flags and -player options).");
                Console.WriteLine();
                Console.WriteLine("Options:");
                Console.WriteLine("  -flags   : Force adding IEGO properties to archives or RES files.");
                Console.WriteLine("  -player  : Add player-specific properties to models (only applicable to .atr files or XPCK archives).");
                return;
            }

            string filePath = args[0];
            string extension = Path.GetExtension(filePath);

            bool flagsEnabled = false;
            if (args.Length > 1)
            {
                flagsEnabled = args[1] == "-flags";
            }

            bool isPlayer = false;
            if (args.Length > 2)
            {
                isPlayer = args[2] == "-player";
            }

            if (!File.Exists(filePath))
            {
                Console.WriteLine($"The file {filePath} does not exist.");
                return;
            }

            string outputFolderPath = Path.Combine(Path.GetDirectoryName(filePath), "converted");

            if (!Directory.Exists(outputFolderPath))
            {
                Directory.CreateDirectory(outputFolderPath);
            }

            byte[] outputData;
            string outputFilePath = Path.Combine(outputFolderPath, Path.GetFileName(filePath));

            if (extension == ".atr")
            {
                outputData = ConvertATR(File.ReadAllBytes(filePath), isPlayer);
            } 
            else if (extension == ".mtr")
            {
                outputData = ConvertMTR(File.ReadAllBytes(filePath));
            }
            else if (extension == ".prm")
            {
                outputData = ConvertPRM(File.ReadAllBytes(filePath));
            }
            else if (extension == ".mtn2" || extension == ".imm2" || extension == ".mtm2")
            {
                outputData = ConvertAnimation(File.ReadAllBytes(filePath));
            }
            else if (extension == ".cmr2")
            {
                outputData = ConvertCamera(File.ReadAllBytes(filePath));
            }
            else if (Path.GetFileName(filePath) == "RES.bin")
            {
                outputData = ConvertRES(File.ReadAllBytes(filePath), flagsEnabled);
            }
            else if (extension == ".pck"|| extension == ".xc" || extension == ".xv")
            {
               outputData = ConvertArchive(new XPCK(new FileStream(filePath, FileMode.Open, FileAccess.Read)), flagsEnabled, isPlayer);
            } 
            else
            {
                Console.WriteLine("Unsupported file format");
                return;
            }

            File.WriteAllBytes(outputFilePath, outputData);
            
            Console.WriteLine($"Conversion completed successfully, check: {outputFilePath}");
        }
    }
}
