﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using System.Xml;
using System.Xml.XPath;
using Gibbed.Illusion.FileFormats.Hashing;
using Gibbed.Illusion.ResourceFormats;
using Gibbed.IO;
using Gibbed.Mafia2.FileFormats;
using Gibbed.Mafia2.FileFormats.Archive;
using Gibbed.Mafia2.ResourceFormats;

namespace Mafia2Tool
{
    public partial class GameExplorer : Form
    {
        public GameExplorer()
        {
            InitializeComponent();
            infoText.Text = "Loading..";
            BuildTreeView();
            infoText.Text = "Ready..";
        }

        /// <summary>
        /// Build TreeView from Mafia II's main directory.
        /// </summary>
        public void BuildTreeView()
        {
            IniFile ini = new IniFile();
            TreeNode rootTreeNode;

            DirectoryInfo dirInfo = new DirectoryInfo(ini.Read("MafiaII", "Directories"));

            //check if directory exists.
            if (!dirInfo.Exists)
                return;

            //check if launcher.exe exists.
            bool hasLauncher = false;
            foreach (FileInfo file in dirInfo.GetFiles())
            {
                if (file.Name == "launcher.exe" || file.Name == "launcher")
                    hasLauncher = true;
            }

            if (!hasLauncher)
                return;

            infoText.Text = "Building folders..";
            //build treeView.
            rootTreeNode = new TreeNode(dirInfo.Name);
            rootTreeNode.Tag = dirInfo;
            GetSubFolders(dirInfo.GetDirectories(), rootTreeNode);
            folderView.Nodes.Add(rootTreeNode);
            infoText.Text = "Done builidng folders..";
        }

        /// <summary>
        /// Build tree by adding sub folders to treeView1.
        /// </summary>
        /// <param name="directories">sub directories of root.</param>
        /// <param name="rootTreeNode">Node to apply the children</param>
        public void GetSubFolders(DirectoryInfo[] directories, TreeNode rootTreeNode)
        {
            TreeNode node;
            DirectoryInfo[] dirs;

            foreach (DirectoryInfo directory in directories)
            {
                node = new TreeNode(directory.Name);
                node.Tag = directory;
                node.ImageIndex = 0;
                dirs = directory.GetDirectories();
                if (dirs.Length != 0)
                    GetSubFolders(dirs, node);

                rootTreeNode.Nodes.Add(node);
            }
        }

        /// <summary>
        /// Clears listView1 and displays the current directory.
        /// </summary>
        /// <param name="directory">directory to show</param>
        private void OpenDirectory(DirectoryInfo directory)
        {
            infoText.Text = "Loading Directory..";
            fileListView.Items.Clear();
            ListViewItem.ListViewSubItem[] subItems;
            ListViewItem item = null;

            foreach (DirectoryInfo dir in directory.GetDirectories())
            {
                item = new ListViewItem(dir.Name, 0);
                item.Tag = dir;
                subItems = new ListViewItem.ListViewSubItem[]
                {
                    new ListViewItem.ListViewSubItem(item, "Directory"),
                    new ListViewItem.ListViewSubItem(item, ""),
                    new ListViewItem.ListViewSubItem(item,
                        dir.LastAccessTime.ToShortDateString())
                };
                item.SubItems.AddRange(subItems);
                fileListView.Items.Add(item);
            }

            foreach (FileInfo file in directory.GetFiles())
            {
                item = new ListViewItem(file.Name, DetermineFileIcon(file.Extension));
                item.Tag = file;

                if (file.Extension == ".sds")
                    fileListView.ContextMenuStrip = SDSContext;

                subItems = new ListViewItem.ListViewSubItem[]
                {
                    new ListViewItem.ListViewSubItem(item, DetermineFileType(file.Extension)),
                    new ListViewItem.ListViewSubItem(item, file.Length.ToString()),
                    new ListViewItem.ListViewSubItem(item,
                        file.LastAccessTime.ToShortDateString())
                };

                item.SubItems.AddRange(subItems);
                fileListView.Items.Add(item);
            }

            fileListView.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);
            infoText.Text = "Done loading directory.";
        }

        /// <summary>
        /// Pack an SDS from the FileInfo given.
        /// </summary>
        /// <param name="file">location of SDS.</param>
        private void PackSDS(FileInfo file)
        {
            infoText.Text = "Saving SDS..";
            ArchiveFile archiveFile = new ArchiveFile
            {
                Platform = Platform.PC
            };
            archiveFile.BuildResourceTypes(file.Directory.FullName + "/extracted/" + file.Name + "/SDSContent.xml");


        }

        /// <summary>
        /// Open an SDS from the FileInfo given.
        /// </summary>
        /// <param name="file">location of SDS.</param>
        private void OpenSDS(FileInfo file)
        {
            infoText.Text = "Opening SDS..";
            fileListView.Items.Clear();
            ArchiveFile archiveFile;
            using (var input = File.OpenRead(file.FullName))
            {
                using (Stream data = ArchiveEncryption.Unwrap(input))
                {
                    archiveFile = new ArchiveFile();
                    archiveFile.Deserialize(data ?? input);
                }
            }

            List<string> itemNames = new List<string>();
            if (string.IsNullOrEmpty(archiveFile.ResourceInfoXml) == false)
            {
                using (var reader = new StringReader(archiveFile.ResourceInfoXml))
                {
                    var doc = new XPathDocument(reader);
                    var nav = doc.CreateNavigator();
                    var nodes = nav.Select("/xml/ResourceInfo/SourceDataDescription");
                    while (nodes.MoveNext() == true)
                    {
                        itemNames.Add(nodes.Current.Value);
                    }
                }
            }

            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = true;
            settings.IndentChars = ("\t");
            settings.OmitXmlDeclaration = true;

            string extractedPath = file.Directory.FullName + "/extracted/";

            if (!Directory.Exists(extractedPath))
                Directory.CreateDirectory(extractedPath);

            Directory.CreateDirectory(extractedPath + file.Name);

            XmlWriter resourceXML = XmlWriter.Create(extractedPath + file.Name + "/SDSContent.xml", settings);
            resourceXML.WriteStartElement("SDSResource");

            //TODO Cleanup this code. It's awful.
            for (int i = 0; i != archiveFile.ResourceEntries.Count; i++)
            {
                ResourceEntry entry = archiveFile.ResourceEntries[i];
                resourceXML.WriteStartElement("ResourceEntry");
                resourceXML.WriteElementString("Type", archiveFile.ResourceTypes[(int)entry.TypeId].Name);
                string saveName = "";

                if (archiveFile.ResourceTypes[(int) entry.TypeId].Name == "Texture")
                {
                    saveName = itemNames[i];
                    TextureResource resource = new TextureResource();
                    resource.Deserialize(entry.Version, new MemoryStream(entry.Data), Endian.Little);
                    resourceXML.WriteElementString("File", saveName);
                    resourceXML.WriteElementString("Unknown9", resource.Unknown9.ToString());
                    entry.Data = resource.Data;
                }
                else if (archiveFile.ResourceTypes[(int) entry.TypeId].Name == "Mipmap")
                {
                    saveName = "MIP_" + itemNames[i];
                    TextureResource resource = new TextureResource();
                    resource.DeserializeMIP(entry.Version, new MemoryStream(entry.Data), Endian.Little);
                    resourceXML.WriteElementString("File", saveName);
                    resourceXML.WriteElementString("Unknown9", resource.Unknown9.ToString());
                    entry.Data = resource.Data;
                }
                else if (archiveFile.ResourceTypes[(int) entry.TypeId].Name == "IndexBufferPool")
                {
                    saveName = "IndexBufferPool_" + i + ".ibp";
                    resourceXML.WriteElementString("File", saveName);
                }
                else if (archiveFile.ResourceTypes[(int) entry.TypeId].Name == "VertexBufferPool")
                {
                    saveName = "VertexBufferPool_" + i + ".vbp";
                    resourceXML.WriteElementString("File", saveName);
                }
                else if (archiveFile.ResourceTypes[(int) entry.TypeId].Name == "FrameResource")
                {
                    saveName = "FrameResource_" + i + ".fr";
                    resourceXML.WriteElementString("File", saveName);
                }
                else if (archiveFile.ResourceTypes[(int) entry.TypeId].Name == "Effects")
                {
                    saveName = "Effects_" + i + ".eff";
                    resourceXML.WriteElementString("File", saveName);
                }
                else if (archiveFile.ResourceTypes[(int) entry.TypeId].Name == "FrameNameTable")
                {
                    saveName = "FrameNameTable_" + i + ".fnt";
                    resourceXML.WriteElementString("File", saveName);
                }
                else if (archiveFile.ResourceTypes[(int) entry.TypeId].Name == "EntityDataStorage")
                {
                    saveName = "EntityDataStorage_" + i + ".eds";
                    resourceXML.WriteElementString("File", saveName);
                }
                else if (archiveFile.ResourceTypes[(int) entry.TypeId].Name == "PREFAB")
                {
                    saveName = "PREFAB_" + i + ".prf";
                    resourceXML.WriteElementString("File", saveName);
                }
                else if (archiveFile.ResourceTypes[(int) entry.TypeId].Name == "ItemDesc")
                {
                    saveName = "ItemDesc_" + i + ".ids";
                    resourceXML.WriteElementString("File", saveName);
                }
                else if (archiveFile.ResourceTypes[(int) entry.TypeId].Name == "Actors")
                {
                    saveName = "Actors_" + i + ".act";
                    resourceXML.WriteElementString("File", saveName);
                }
                else if (archiveFile.ResourceTypes[(int) entry.TypeId].Name == "Collisions")
                {
                    saveName = "Collisions_" + i + ".col";
                    resourceXML.WriteElementString("File", saveName);
                }
                else if (archiveFile.ResourceTypes[(int) entry.TypeId].Name == "AudioSectors")
                {
                    saveName = "AudioSectors_" + i + ".aus";
                    resourceXML.WriteElementString("File", saveName);
                }
                else if (archiveFile.ResourceTypes[(int) entry.TypeId].Name == "Script")
                {
                    ScriptResource resource = new ScriptResource();
                    resource.Deserialize(entry.Version, new MemoryStream(entry.Data), Endian.Little);

                    for (int x = 0; x != resource.Scripts.Count; x++)
                    {
                        string scrdir = extractedPath + file.Name;
                        string[] dirs = resource.Scripts[x].Name.Split('/');
                        for (int z = 0; z != dirs.Length - 1; z++)
                        {
                            scrdir += "/" + dirs[z];
                            Directory.CreateDirectory(scrdir);
                        }

                        using (BinaryWriter writer = new BinaryWriter(
                            File.Open(extractedPath + file.Name + "/" + resource.Scripts[x].Name, FileMode.Create)))
                        {
                            writer.Write(resource.Scripts[x].Data);
                        }
                    }

                    continue;
                }
                else if (archiveFile.ResourceTypes[(int) entry.TypeId].Name == "XML")
                {
                    try
                    {
                        saveName = itemNames[i];
                        string[] dirs = itemNames[i].Split('/');
                        XmlResource resource = new XmlResource();
                        resource.Deserialize(entry.Version, new MemoryStream(entry.Data), Endian.Little);
                        string xmldir = extractedPath + file.Name;
                        for (int z = 0; z != dirs.Length - 1; z++)
                        {
                            xmldir += "/" + dirs[z];
                            Directory.CreateDirectory(xmldir);
                        }

                        File.WriteAllText(extractedPath + file.Name + "/" + saveName + ".xml", resource.Content);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("ERROR CONVERTING XML: " + ex.Message);
                    }

                    continue;
                }
                else if (archiveFile.ResourceTypes[(int) entry.TypeId].Name == "Sound")
                {
                    //Do resource first..
                    SoundResource resource = new SoundResource();
                    resource.Deserialize(entry.Data, (byte) itemNames[i].Length);
                    entry.Data = resource.Data;

                    saveName = itemNames[i] + ".fsb";
                    string[] dirs = itemNames[i].Split('/');

                    string sounddir = extractedPath + file.Name;
                    for (int z = 0; z != dirs.Length - 1; z++)
                    {
                        sounddir += "/" + dirs[z];
                        Directory.CreateDirectory(sounddir);
                    }
                    resourceXML.WriteElementString("File", saveName);
                }
                else if (archiveFile.ResourceTypes[(int) entry.TypeId].Name == "MemFile")
                {
                    MemFileResource resource = new MemFileResource();
                    resource.Deserialize(entry.Data);
                    entry.Data = resource.Data;

                    saveName = itemNames[i];
                    string[] dirs = itemNames[i].Split('/');

                    string memdir = extractedPath + file.Name;
                    for (int z = 0; z != dirs.Length - 1; z++)
                    {
                        memdir += "/" + dirs[z];
                        Directory.CreateDirectory(memdir);
                    }
                }
                else if (archiveFile.ResourceTypes[(int) entry.TypeId].Name == "SoundTable")
                {
                    saveName = "SoundTable_" + i + ".stbl";
                }
                else if (archiveFile.ResourceTypes[(int) entry.TypeId].Name == "FxAnimSet")
                {
                    saveName = "FxAnimSet_" + i + ".fas";
                }
                else if (archiveFile.ResourceTypes[(int) entry.TypeId].Name == "FxActor")
                {
                    saveName = "FxActor_" + i + ".fxa";
                }
                else if (archiveFile.ResourceTypes[(int) entry.TypeId].Name == "Cutscene")
                {
                    saveName = "Cutscene_" + i + ".cut";
                }
                else if (archiveFile.ResourceTypes[(int) entry.TypeId].Name == "Translokator")
                {
                    saveName = "Translokator_" + i + ".tra";
                }
                else if (archiveFile.ResourceTypes[(int) entry.TypeId].Name == "Animation2")
                {
                    saveName = itemNames[i] + ".an2";
                }
                else if (archiveFile.ResourceTypes[(int) entry.TypeId].Name == "NAV_AIWORLD_DATA")
                {
                    saveName = "NAV_AIWORLD_DATA_" + i + ".nav";
                }
                else if (archiveFile.ResourceTypes[(int) entry.TypeId].Name == "NAV_OBJ_DATA")
                {
                    saveName = "NAV_OBJ_DATA_" + i + ".nov";
                }
                else if (archiveFile.ResourceTypes[(int) entry.TypeId].Name == "Table")
                {
                    TableResource resource = new TableResource();
                    resource.Deserialize(entry.Version, new MemoryStream(entry.Data), Endian.Little);
                    //todo extract individual tables.
                    saveName = "Tables_" + ".tbl";
                }
                else
                {
                    MessageBox.Show("Found unknown type: " + archiveFile.ResourceTypes[(int) entry.TypeId].Name);
                    saveName = "unknown.bin";
                }

                using (BinaryWriter writer =
                    new BinaryWriter(
                        File.Open(extractedPath + file.Name + "/" + saveName, FileMode.Create)))
                {
                    writer.Write(entry.Data);
                }

                resourceXML.WriteEndElement();
            }

            resourceXML.WriteEndElement();
            resourceXML.Flush();

            OpenDirectory(new DirectoryInfo(extractedPath + file.Name));
            infoText.Text = "Opened SDS..";
        }

        /// <summary>
        /// Check extension and return file type string.
        /// </summary>
        /// <param name="extension">extension of file.</param>
        private string DetermineFileType(string extension)
        {
            //TODO. Sort extensions with localisations.
            switch (extension)
            {
                case ".sds":
                    return "SDS Archive";
                case ".dds":
                    return "Direct-Draw Surface";
                case ".exe":
                    return "Executable";
                case ".dll":
                    return "Dynamic-Link Library";
                case ".mtl":
                    return "Material Library";
                case "": //fix for content files.
                    return "File";
                default:
                    return extension.Remove(0, 1).ToUpper();
            }
        }

        /// <summary>
        /// Use the file extension to determine to file icon.
        /// </summary>
        /// <param name="extension">file extension</param>
        /// <returns></returns>
        private int DetermineFileIcon(string extension)
        {
            switch (extension)
            {
                case ".exe":
                    return 3;
                case ".dll":
                    return 2;
                default:
                    return 1;
            }
        }

        void treeView1_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            TreeNode selectedNode = e.Node;
            OpenDirectory((DirectoryInfo) selectedNode.Tag);
        }

        private void listView1_ItemActivate(object sender, EventArgs e)
        {
            ListViewItem item = fileListView.SelectedItems[0];
            MaterialTool mTool;

            if (item.SubItems[1].Text == "Directory")
                OpenDirectory((DirectoryInfo) item.Tag);
            else if (item.SubItems[1].Text == "Material Library")
                mTool = new MaterialTool((FileInfo) item.Tag);
            else if (item.SubItems[1].Text == "SDS Archive")
                OpenSDS((FileInfo) item.Tag);
        }

        private void ContextSDSPack_Click(object sender, EventArgs e)
        {
            PackSDS(fileListView.SelectedItems[0].Tag as FileInfo);
        }

        private void ContextSDSUnpack_Click(object sender, EventArgs e)
        {
            OpenSDS(fileListView.SelectedItems[0].Tag as FileInfo);
        }

        private void listView1_MouseDown(object sender, MouseEventArgs e)
        {
            bool match = false;

            if (e.Button == System.Windows.Forms.MouseButtons.Right)
            {
                foreach (ListViewItem item in fileListView.Items)
                {
                    if (item.Bounds.Contains(e.Location))
                    {
                        if (item.Tag is FileInfo)
                        {
                            FileInfo info = (FileInfo) item.Tag;
                            if (info.Extension == ".sds")
                            {
                                fileListView.ContextMenuStrip = SDSContext;
                            }
                            else
                            {
                                fileListView.ContextMenuStrip = null;
                                match = false;
                                break;
                            }
                        }
                        else
                        {
                            match = false;
                            break;
                        }
                        match = true;
                        break;
                    }
                }
                if (match)
                {
                    fileListView.ContextMenuStrip.Show(fileListView, e.Location);
                }
                else
                {
                    //Show listViews context menu
                }

            }

        }
    }
}
