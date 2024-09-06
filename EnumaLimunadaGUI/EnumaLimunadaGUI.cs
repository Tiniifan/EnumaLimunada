using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.WindowsAPICodePack.Dialogs;
using StudioElevenLib.Tools;
using StudioElevenLib.Level5.Resource.RES;
using StudioElevenLib.Level5.Resource.XRES;
using StudioElevenLib.Level5.Archive.XPCK;
using StudioElevenLib.Level5.Material;
using StudioElevenLib.Level5.Animation;
using StudioElevenLib.Level5.Camera.CMR2;
using StudioElevenLib.Level5.Camera.CMR1;

namespace EnumaLimunadaGUI
{
    public partial class EnumaLimunadaGUI : Form
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
            }
            else
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

        public EnumaLimunadaGUI()
        {
            InitializeComponent();
        }

        private void EnumaLimunadaGUI_DragDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);

                string[] supportedExtensions = { ".atr", ".mtr", ".prm", ".mtn2", ".imm2", ".mtm2", ".cmr2", ".pck", ".xc", ".xv" };

                foreach (string file in files)
                {
                    string dragPath = Path.GetFullPath(file);
                    string dragExt = Path.GetExtension(file).ToLower();
                    string fileName = Path.GetFileName(file).ToLower();

                    // Check if it's named RES.bin specifically
                    if (fileName == "res.bin" || supportedExtensions.Contains(dragExt))
                    {
                        selectedFileListBox.Items.Add(dragPath);
                    }
                }
            }
        }

        private void EnumaLimunadaGUI_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop)) e.Effect = DragDropEffects.Copy;
        }

        private void OpenButton_Click(object sender, EventArgs e)
        {
            CommonOpenFileDialog dialog = new CommonOpenFileDialog();
            dialog.IsFolderPicker = true;

            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                selectedFolderTextBox.Text = dialog.FileName;
                runButton.Enabled = true;
            }
        }

        private void AddButton_Click(object sender, EventArgs e)
        {
            string[] supportedExtensions = { ".atr", ".mtr", ".prm", ".mtn2", ".imm2", ".mtm2", ".cmr2", ".pck", ".xc", ".xv" };

            string filter = "Supported Files (" + string.Join(", ", supportedExtensions.Select(ext => "*" + ext)) + ")|" +
                string.Join(";", supportedExtensions.Select(ext => "*" + ext)) + "|" +
                "RES.bin (RES.bin)|RES.bin|" +
                "All Files (*.*)|*.*";

            openFileDialog1.Filter = filter;
            openFileDialog1.FilterIndex = 1;
            openFileDialog1.Multiselect = true;

            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                string[] selectedFiles = openFileDialog1.FileNames;

                foreach (string filePath in selectedFiles)
                {
                    selectedFileListBox.Items.Add(filePath);
                }
            }
        }

        private void RemoveButton_Click(object sender, EventArgs e)
        {
            if (selectedFileListBox.SelectedIndex < 0) return;

            selectedFileListBox.Items.RemoveAt(selectedFileListBox.SelectedIndex);
        }

        private void SelectedFileListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (selectedFileListBox.SelectedIndex < 0)
            {
                removeButton.Enabled = false;
                return;
            } else
            {
                removeButton.Enabled = true;
            }

        }

        private void RunButton_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(selectedFolderTextBox.Text))
            {
                MessageBox.Show("Please enter a valid folder path.");
                return;
            }

            if (!Directory.Exists(selectedFolderTextBox.Text))
            {
                MessageBox.Show("Directory doesn't exist.");
                return;
            }

            logTextBox.Text = "";
            logTextBox.Visible = true;
            bool flagsEnabled = addIEGOFlagCheckBox.Checked;
            bool isPlayer = addPlayerFlagCheckBox.Checked;

            foreach (string filePath in selectedFileListBox.Items.Cast<string>().ToArray())
            {
                byte[] outputData;
                string outputFilePath = Path.Combine(selectedFolderTextBox.Text, Path.GetFileName(filePath));

                if (!File.Exists(filePath)) continue;

                string extension = Path.GetExtension(filePath);
                logTextBox.Text += $"Start to convert {Path.GetFileName(filePath)}\n";
                logTextBox.Update();

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
                else if (extension == ".pck" || extension == ".xc" || extension == ".xv")
                {
                    outputData = ConvertArchive(new XPCK(new FileStream(filePath, FileMode.Open, FileAccess.Read)), flagsEnabled, isPlayer);
                } else
                {
                    continue;
                }

                logTextBox.Text += $"Convert succesfull {filePath}! Save on {outputFilePath}\n";
                logTextBox.Update();
                File.WriteAllBytes(outputFilePath, outputData);
            }

            MessageBox.Show("Done!");

            logTextBox.Visible = false;
        }
    }
}
