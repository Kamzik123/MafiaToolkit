﻿/* Copyright (c) 2017 Rick (rick 'at' gibbed 'dot' us)
 * 
 * This software is provided 'as-is', without any express or implied
 * warranty. In no event will the authors be held liable for any damages
 * arising from the use of this software.
 * 
 * Permission is granted to anyone to use this software for any purpose,
 * including commercial applications, and to alter it and redistribute it
 * freely, subject to the following restrictions:
 * 
 * 1. The origin of this software must not be misrepresented; you must not
 *    claim that you wrote the original software. If you use this software
 *    in a product, an acknowledgment in the product documentation would
 *    be appreciated but is not required.
 * 
 * 2. Altered source versions must be plainly marked as such, and must not
 *    be misrepresented as being the original software.
 * 
 * 3. This notice may not be removed or altered from any source
 *    distribution.
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using System.Xml.XPath;
using Gibbed.Illusion.FileFormats;
using Gibbed.Illusion.FileFormats.Hashing;
using Gibbed.Illusion.ResourceFormats;
using Gibbed.IO;
using Gibbed.Mafia2.FileFormats.Archive;
using Gibbed.Mafia2.ResourceFormats;
using Mafia2;

namespace Gibbed.Mafia2.FileFormats
{
    public class ArchiveFile
    {
        public const uint Signature = 0x53445300; // 'SDS\0'

        #region Fields
        private Endian _Endian;
        private Archive.Platform _Platform;
        private uint _SlotRamRequired;
        private uint _SlotVramRequired;
        private uint _OtherRamRequired;
        private uint _OtherVramRequired;
        private byte[] _Unknown20;
        private readonly List<Archive.ResourceType> _ResourceTypes;
        private string _ResourceInfoXml;
        private readonly List<Archive.ResourceEntry> _ResourceEntries;
        private Dictionary<ulong, TextureResource> _TextureEntries;
        #endregion
        #region Properties
        public Endian Endian {
            get { return this._Endian; }
            set { this._Endian = value; }
        }
        public Archive.Platform Platform {
            get { return this._Platform; }
            set { this._Platform = value; }
        }
        public uint SlotRamRequired {
            get { return this._SlotRamRequired; }
            set { this._SlotRamRequired = value; }
        }
        public uint SlotVramRequired {
            get { return this._SlotVramRequired; }
            set { this._SlotVramRequired = value; }
        }
        public uint OtherRamRequired {
            get { return this._OtherRamRequired; }
            set { this._OtherRamRequired = value; }
        }
        public uint OtherVramRequired {
            get { return this._OtherVramRequired; }
            set { this._OtherVramRequired = value; }
        }
        public byte[] Unknown20 {
            get { return this._Unknown20; }
            set { this._Unknown20 = value; }
        }
        public List<Archive.ResourceType> ResourceTypes {
            get { return this._ResourceTypes; }
        }
        public string ResourceInfoXml {
            get { return this._ResourceInfoXml; }
            set { this._ResourceInfoXml = value; }
        }
        public List<Archive.ResourceEntry> ResourceEntries {
            get { return this._ResourceEntries; }
        }
        #endregion
        #region Constructors
        public ArchiveFile()
        {
            this._ResourceTypes = new List<Archive.ResourceType>();
            this._ResourceEntries = new List<Archive.ResourceEntry>();
        }
        #endregion
        #region Functions
        public void Serialize(Stream output, ArchiveSerializeOptions options)
        {
            var compress = (options & ArchiveSerializeOptions.Compress) != 0;

            var basePosition = output.Position;
            var endian = this._Endian;

            using (var data = new MemoryStream(12))
            {
                data.WriteValueU32(Signature, Endian.Big);
                data.WriteValueU32(19, endian);
                data.WriteValueU32((uint)this._Platform, Endian.Big);
                data.Flush();
                output.WriteFromMemoryStreamSafe(data, endian);
            }

            var headerPosition = output.Position;

            Archive.FileHeader fileHeader;
            output.Seek(56, SeekOrigin.Current);

            fileHeader.ResourceTypeTableOffset = (uint)(output.Position - basePosition);
            output.WriteValueS32(this._ResourceTypes.Count, endian);
            foreach (var resourceType in this._ResourceTypes)
            {
                resourceType.Write(output, endian);
            }

            var blockAlignment = (options & ArchiveSerializeOptions.OneBlock) != 0
                ? (uint)this._ResourceEntries.Sum(re => 30 + (re.Data == null ? 0 : re.Data.Length))
                : 0x4000;

            fileHeader.BlockTableOffset = (uint)(output.Position - basePosition);
            fileHeader.ResourceCount = 0;
            var blockStream = BlockWriterStream.ToStream(output, blockAlignment, endian, compress);
            foreach (var resourceEntry in this._ResourceEntries)
            {
                Archive.ResourceHeader resourceHeader;
                resourceHeader.TypeId = (uint)resourceEntry.TypeId;
                resourceHeader.Size = 30 + (uint)(resourceEntry.Data == null ? 0 : resourceEntry.Data.Length);
                resourceHeader.Version = resourceEntry.Version;
                resourceHeader.SlotRamRequired = resourceEntry.SlotRamRequired;
                resourceHeader.SlotVramRequired = resourceEntry.SlotVramRequired;
                resourceHeader.OtherRamRequired = resourceEntry.OtherRamRequired;
                resourceHeader.OtherVramRequired = resourceEntry.OtherVramRequired;

                using (var data = new MemoryStream())
                {
                    resourceHeader.Write(data, endian);
                    data.Flush();
                    blockStream.WriteFromMemoryStreamSafe(data, endian);
                }

                blockStream.WriteBytes(resourceEntry.Data);
                fileHeader.ResourceCount++;
            }

            blockStream.Flush();
            blockStream.Finish();

            fileHeader.XmlOffset = (uint)(output.Position - basePosition);
            if (string.IsNullOrEmpty(this._ResourceInfoXml) == false)
            {
                output.WriteString(this._ResourceInfoXml, Encoding.ASCII);
            }

            fileHeader.SlotRamRequired = this.SlotRamRequired;
            fileHeader.SlotVramRequired = this.SlotVramRequired;
            fileHeader.OtherRamRequired = this.OtherRamRequired;
            fileHeader.OtherVramRequired = this.OtherVramRequired;
            fileHeader.Flags = 1;
            fileHeader.Unknown20 = this._Unknown20 ?? new byte[16];

            output.Position = headerPosition;
            using (var data = new MemoryStream())
            {
                fileHeader.Write(data, endian);
                data.Flush();
                output.WriteFromMemoryStreamSafe(data, endian);
            }
        }
        public void Deserialize(Stream input)
        {
            var basePosition = input.Position;

            var magic = input.ReadValueU32(Endian.Big);
            if (magic != Signature)
            {
                throw new FormatException("unsupported archive version");
            }

            input.Position += 4; // skip version
            var platform = (Archive.Platform)input.ReadValueU32(Endian.Big);
            if (platform != Archive.Platform.PC &&
                platform != Archive.Platform.Xbox360 &&
                platform != Archive.Platform.PS3)
            {
                throw new FormatException("unsupported archive platform");
            }

            var endian = platform == Archive.Platform.PC ? Endian.Little : Endian.Big;

            input.Position = basePosition;

            uint version;
            using (var data = input.ReadToMemoryStreamSafe(12, endian))
            {
                data.Position += 4; // skip magic
                version = data.ReadValueU32(endian);
                data.Position += 4; // skip platform
            }

            if (version != 19)
            {
                throw new FormatException("unsupported archive version");
            }

            Archive.FileHeader fileHeader;
            using (var data = input.ReadToMemoryStreamSafe(52, endian))
            {
                fileHeader = Archive.FileHeader.Read(data, endian);
            }

            input.Position = basePosition + fileHeader.ResourceTypeTableOffset;
            var resourceTypeCount = input.ReadValueU32(endian);
            var resourceTypes = new Archive.ResourceType[resourceTypeCount];
            for (uint i = 0; i < resourceTypeCount; i++)
            {
                resourceTypes[i] = Archive.ResourceType.Read(input, endian);
            }

            input.Position = basePosition + fileHeader.BlockTableOffset;
            var blockStream = BlockReaderStream.FromStream(input, endian);

            var resources = new Archive.ResourceEntry[fileHeader.ResourceCount];
            for (uint i = 0; i < fileHeader.ResourceCount; i++)
            {
                Archive.ResourceHeader resourceHeader;
                using (var data = blockStream.ReadToMemoryStreamSafe(26, endian))
                {
                    resourceHeader = Archive.ResourceHeader.Read(data, endian);
                }

                if (resourceHeader.Size < 30)
                {
                    throw new FormatException();
                }

                resources[i] = new Archive.ResourceEntry()
                {
                    TypeId = (int)resourceHeader.TypeId,
                    Version = resourceHeader.Version,
                    Data = blockStream.ReadBytes((int)resourceHeader.Size - 30),
                    SlotRamRequired = resourceHeader.SlotRamRequired,
                    SlotVramRequired = resourceHeader.SlotVramRequired,
                    OtherRamRequired = resourceHeader.OtherRamRequired,
                    OtherVramRequired = resourceHeader.OtherVramRequired,
                };
            }

            input.Position = basePosition + fileHeader.XmlOffset;
            var xml = input.ReadString((int)(input.Length - input.Position), Encoding.ASCII);

            this._ResourceTypes.Clear();
            this._ResourceEntries.Clear();

            this._Endian = endian;
            this._Platform = platform;
            this._SlotRamRequired = fileHeader.SlotRamRequired;
            this._SlotVramRequired = fileHeader.SlotVramRequired;
            this._OtherRamRequired = fileHeader.OtherRamRequired;
            this._OtherVramRequired = fileHeader.OtherVramRequired;
            this._Unknown20 = (byte[])fileHeader.Unknown20.Clone();
            this._ResourceTypes.AddRange(resourceTypes);
            this._ResourceInfoXml = xml;
            this._ResourceEntries.AddRange(resources);
        }

        /// <summary>
        /// Build resources from given folder.
        /// </summary>
        /// <param name="folder"></param>
        public void BuildResources(string folder)
        {
            //TODO: MAKE THIS CLEANER
            string sdsFolder = folder;

            List<string> addedTypes = new List<string>();
            XmlDocument document = new XmlDocument();
            XmlDocument xmlDoc = new XmlDocument();
            XmlNode rootNode;

            try
            {
                document = new XmlDocument();
                document.Load(sdsFolder + "/SDSContent.xml");
                xmlDoc = new XmlDocument();
                rootNode = xmlDoc.CreateElement("xml");
            }
            catch
            {
                MessageBox.Show("Could not find 'SDSContent.xml'. Folder Path: " + sdsFolder + "/SDSContent.xml", "Game Explorer", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            xmlDoc.AppendChild(rootNode);

            XPathNavigator nav = document.CreateNavigator();
            var nodes = nav.Select("/SDSResource/ResourceEntry");
            while (nodes.MoveNext() == true)
            {
                int exists = -1;
                nodes.Current.MoveToFirstChild();
                string resourceType = nodes.Current.Value;

                if (addedTypes.Count == 0)
                {
                    exists = -1;
                }
                else
                {
                    for (int i = 0; i != addedTypes.Count; i++)
                    {
                        if (addedTypes[i] == resourceType)
                            exists = i;
                    }
                }

                if (exists == -1)
                {
                    ResourceType resource = new ResourceType();
                    resource.Name = nodes.Current.Value;
                    resource.Id = (uint)addedTypes.Count;
                    exists = addedTypes.Count;

                    //TODO
                    if (resource.Name == "IndexBufferPool" || resource.Name == "PREFAB")
                        resource.Parent = 3;
                    else if (resource.Name == "VertexBufferPool" || resource.Name == "NAV_OBJ_DATA")
                        resource.Parent = 2;
                    else if (resource.Name == "NAV_HPD_DATA")
                        resource.Parent = 1;

                    addedTypes.Add(nodes.Current.Value);
                    ResourceTypes.Add(resource);
                }

                XmlNode resourceNode = xmlDoc.CreateElement("ResourceInfo");
                XmlNode typeNameNode = xmlDoc.CreateElement("TypeName");
                typeNameNode.InnerText = resourceType;
                XmlNode sddescNode = xmlDoc.CreateElement("SourceDataDescription");

                ResourceEntry resourceEntry = new ResourceEntry();
                resourceEntry.TypeId = exists;

                if (resourceType == "FrameResource" ||
                    resourceType == "Effects" ||
                    resourceType == "PREFAB" ||
                    resourceType == "ItemDesc" ||
                    resourceType == "FrameNameTable" ||
                    resourceType == "Actors" ||
                    resourceType == "Collisions" ||
                    resourceType == "NAV_AIWORLD_DATA" ||
                    resourceType == "NAV_OBJ_DATA" ||
                    resourceType == "NAV_HPD_DATA" ||
                    resourceType == "Cutscene" ||
                    resourceType == "FxActor" ||
                    resourceType == "FxAnimSet" ||
                    resourceType == "Translokator" ||
                    resourceType == "AudioSectors" ||
                    resourceType == "Speech" ||
                    resourceType == "SoundTable" ||
                    resourceType == "AnimalTrafficPaths")
                {
                    nodes.Current.MoveToNext();
                    string file = nodes.Current.Value;
                    using (BinaryReader reader = new BinaryReader(File.Open(sdsFolder + "/" + file, FileMode.Open)))
                    {
                        resourceEntry.Data = reader.ReadBytes((int)reader.BaseStream.Length);
                    }

                    nodes.Current.MoveToNext();
                    resourceEntry.Version = Convert.ToUInt16(nodes.Current.Value);

                    sddescNode.InnerText = "not available";
                }
                else if(resourceType == "IndexBufferPool" || resourceType == "VertexBufferPool")
                {
                    nodes.Current.MoveToNext();
                    string file = nodes.Current.Value;
                    using (BinaryReader reader = new BinaryReader(File.Open(sdsFolder + "/" + file, FileMode.Open)))
                    {
                        reader.BaseStream.Position = 5;
                        resourceEntry.SlotVramRequired = (uint)reader.ReadInt32();
                        reader.BaseStream.Position = 0;
                        resourceEntry.Data = reader.ReadBytes((int)reader.BaseStream.Length);
                    }

                    nodes.Current.MoveToNext();
                    resourceEntry.Version = Convert.ToUInt16(nodes.Current.Value);

                    sddescNode.InnerText = "not available";
                }
                else if(resourceType == "EntityDataStorage")
                {
                    nodes.Current.MoveToNext();
                    string file = nodes.Current.Value;
                    using (BinaryReader reader = new BinaryReader(File.Open(sdsFolder + "/" + file, FileMode.Open)))
                    {
                        resourceEntry.Data = reader.ReadBytes((int)reader.BaseStream.Length);
                        resourceEntry.SlotRamRequired = (uint)reader.BaseStream.Length + 30;
                    }

                    nodes.Current.MoveToNext();
                    resourceEntry.Version = Convert.ToUInt16(nodes.Current.Value);

                    sddescNode.InnerText = "not available";
                }
                else if (resourceType == "Texture")
                {
                    MemoryStream data = new MemoryStream();
                    TextureResource resource;

                    byte[] texData;
                    nodes.Current.MoveToNext();
                    string file = nodes.Current.Value;
                    nodes.Current.MoveToNext();
                    byte hasMIP = Convert.ToByte(nodes.Current.Value);
                    nodes.Current.MoveToNext();
                    resourceEntry.Version = Convert.ToUInt16(nodes.Current.Value);

                    using (BinaryReader reader = new BinaryReader(File.Open(sdsFolder + "/" + file, FileMode.Open)))
                    {
                        texData = reader.ReadBytes((int)reader.BaseStream.Length);
                    }

                    resource = new TextureResource(FNV64.Hash(file), hasMIP, texData);
                    resourceEntry.SlotVramRequired = (uint)texData.Length - 128;

                    if (hasMIP == 1)
                    {
                        using (BinaryReader reader = new BinaryReader(File.Open(sdsFolder + "/MIP_" + file, FileMode.Open)))
                        {
                            resourceEntry.SlotVramRequired += (uint)(reader.BaseStream.Length-128);
                        }
                    }

                    resource.Serialize(resourceEntry.Version, data, Endian.Little);                
                    sddescNode.InnerText = file;
                    resourceEntry.Version = Convert.ToUInt16(nodes.Current.Value);
                    resourceEntry.Data = data.GetBuffer();
                }
                else if (resourceType == "Mipmap")
                {
                    MemoryStream data = new MemoryStream();
                    TextureResource resource;
                    byte[] texData;

                    nodes.Current.MoveToNext();
                    string file = nodes.Current.Value;
                    nodes.Current.MoveToNext();
                    resourceEntry.Version = Convert.ToUInt16(nodes.Current.Value);

                    using (BinaryReader reader = new BinaryReader(File.Open(sdsFolder + "/" + file, FileMode.Open)))
                        texData = reader.ReadBytes((int)reader.BaseStream.Length);

                    resource = new TextureResource(FNV64.Hash(file.Remove(0, 4)), 0, texData);
                    resource.SerializeMIP(resourceEntry.Version, data, Endian.Little);
                    sddescNode.InnerText = file.Remove(0, 4);
                    resourceEntry.Version = Convert.ToUInt16(nodes.Current.Value);
                    resourceEntry.Data = data.GetBuffer();
                }
                else if (resourceType == "Sound")
                {
                    List<byte> data = new List<byte>();
                    string file;
                    nodes.Current.MoveToNext();
                    file = nodes.Current.Value.Remove(nodes.Current.Value.Length - 4, 4);
                    data.Add((byte)file.Length);
                    for (int i = 0; i != file.Length; i++)
                        data.Add((byte)file[i]);

                    using (BinaryReader reader = new BinaryReader(File.Open(sdsFolder + "/" + file + ".fsb", FileMode.Open)))
                    {
                        resourceEntry.SlotRamRequired = 40;
                        resourceEntry.SlotVramRequired = (uint)reader.BaseStream.Length;
                        data.AddRange(BitConverter.GetBytes((int)reader.BaseStream.Length)); //?? WHAT DOES THIS DO?? CHECK LATER.
                        data.AddRange(reader.ReadBytes((int)reader.BaseStream.Length));
                    }
                    nodes.Current.MoveToNext();
                    resourceEntry.Version = Convert.ToUInt16(nodes.Current.Value);
                    resourceEntry.Data = data.ToArray();
                    sddescNode.InnerText = file;
                }
                else if (resourceType == "MemFile")
                {
                    //get file name from XML.
                    nodes.Current.MoveToNext();
                    string file = nodes.Current.Value;

                    //construct MemResource.
                    MemFileResource resource = new MemFileResource
                    {
                        Name = file,
                        Unk1 = 1
                    };
                    using (BinaryReader reader = new BinaryReader(File.Open(sdsFolder + "/" + file, FileMode.Open)))
                    {
                        resource.Data = reader.ReadBytes((int)reader.BaseStream.Length);
                        //resourceEntry.SlotRamRequired = (uint)reader.BaseStream.Length;
                    }

                    //Get version.
                    nodes.Current.MoveToNext();
                    resourceEntry.Version = Convert.ToUInt16(nodes.Current.Value);

                    //serialize.
                    MemoryStream stream = new MemoryStream();
                    resource.Serialize(resourceEntry.Version, stream, Endian.Little);

                    //set the data.
                    resourceEntry.Data = stream.GetBuffer();
                    sddescNode.InnerText = file;
                }
                else if (resourceType == "Script")
                {
                    nodes.Current.MoveToNext();
                    string path = nodes.Current.Value;
                    nodes.Current.MoveToNext();
                    int numScripts = Convert.ToInt32(nodes.Current.Value);

                    ScriptResource resource = new ScriptResource();
                    resource.Path = path;

                    for (int i = 0; i != numScripts; i++)
                    {
                        ScriptData data = new ScriptData();
                        nodes.Current.MoveToNext();
                        data.Name = nodes.Current.Value;
                        using (BinaryReader reader = new BinaryReader(File.Open(sdsFolder + data.Name, FileMode.Open)))
                        {
                            data.Data = reader.ReadBytes((int)reader.BaseStream.Length);
                        }
                        resource.Scripts.Add(data);
                    }
                    nodes.Current.MoveToNext();
                    ushort version = Convert.ToUInt16(nodes.Current.Value);

                    MemoryStream stream = new MemoryStream();
                    resource.Serialize(version, stream, Endian.Little);
                    resourceEntry.Version = version;
                    resourceEntry.Data = stream.GetBuffer();
                    sddescNode.InnerText = path;
                }
                else if (resourceType == "Animation2")
                {
                    nodes.Current.MoveToNext();
                    string file = nodes.Current.Value;
                    nodes.Current.MoveToNext();
                    resourceEntry.Version = Convert.ToUInt16(nodes.Current.Value);
                    using (BinaryReader reader = new BinaryReader(File.Open(sdsFolder + "/" + file, FileMode.Open)))
                    {
                        resourceEntry.Data = reader.ReadBytes((int)reader.BaseStream.Length);
                    }
                    sddescNode.InnerText = file.Remove(file.Length - 4, 4);
                }
                else if (resourceType == "Table")
                {
                    nodes.Current.MoveToNext();
                    string file = nodes.Current.Value;
                    using (BinaryReader reader = new BinaryReader(File.Open(sdsFolder + "/" + file, FileMode.Open)))
                    {
                        resourceEntry.Data = reader.ReadBytes((int)reader.BaseStream.Length);
                    }

                    nodes.Current.MoveToNext();
                    resourceEntry.Version = Convert.ToUInt16(nodes.Current.Value);

                    sddescNode.InnerText = file.Remove(file.Length - 4, 4);
                }
                else if (resourceType == "XML")
                {
                    nodes.Current.MoveToNext();
                    string file = nodes.Current.Value;

                    nodes.Current.MoveToNext();
                    string tag = nodes.Current.Value;

                    nodes.Current.MoveToNext();
                    bool unk1 = nodes.Current.ValueAsBoolean;

                    nodes.Current.MoveToNext();
                    bool unk3 = nodes.Current.ValueAsBoolean;

                    //need to do version early.
                    nodes.Current.MoveToNext();
                    resourceEntry.Version = Convert.ToUInt16(nodes.Current.Value);

                    XmlResource resource = new XmlResource
                    {
                        Name = file,
                        Content = sdsFolder + "/" + file + ".xml",
                        Tag = tag,
                        Unk1 = unk1,
                        Unk3 = unk3
                    };
                    MemoryStream stream = new MemoryStream();
                    resource.Serialize(resourceEntry.Version, stream, Endian.Little);

                    sddescNode.InnerText = file;

                    if (resource.Unk3)
                    {
                        using (BinaryReader reader = new BinaryReader(File.Open(sdsFolder + "/" + file + ".xml", FileMode.Open)))
                        {
                            resource.SerializeVersion3(resourceEntry.Version, stream, reader.ReadBytes((int)reader.BaseStream.Length));
                        }
                    }
                    resourceEntry.Data = stream.GetBuffer();
                }
                else
                {
                    if (resourceType == "")
                        continue;

                    MessageBox.Show("Did not pack type: " + resourceType, "Toolkit",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                resourceNode.AppendChild(typeNameNode);
                resourceNode.AppendChild(sddescNode);
                //resourceEntry.OtherRamRequired = 0;
                //resourceEntry.OtherVramRequired = 0;
                //resourceEntry.SlotRamRequired = 0;
                //resourceEntry.SlotVramRequired = 0;
                resourceNode.AppendChild(AddRamElement(xmlDoc, "SlotRamRequired", (int)resourceEntry.SlotRamRequired));
                resourceNode.AppendChild(AddRamElement(xmlDoc, "SlotVRamRequired", (int)resourceEntry.SlotVramRequired));
                resourceNode.AppendChild(AddRamElement(xmlDoc, "OtherRamRequired", (int)resourceEntry.OtherRamRequired));
                resourceNode.AppendChild(AddRamElement(xmlDoc, "OtherVramRequired", (int)resourceEntry.OtherVramRequired));
                rootNode.AppendChild(resourceNode);
                ResourceEntries.Add(resourceEntry);
                SlotRamRequired += resourceEntry.SlotRamRequired;
                SlotVramRequired += resourceEntry.SlotVramRequired;
                OtherRamRequired += resourceEntry.OtherRamRequired;
                OtherVramRequired += resourceEntry.OtherVramRequired;
            }

            ResourceInfoXml = xmlDoc.OuterXml;
        }

        /// <summary>
        /// Save resource data from given sds data.
        /// </summary>
        /// <param name="xml"></param>
        public void SaveResources(FileInfo file)
        {
            //get resources names...
            List<string> itemNames = new List<string>();
            if (string.IsNullOrEmpty(ResourceInfoXml) == false)
            {
                using (var reader = new StringReader(ResourceInfoXml))
                {
                    var doc = new XPathDocument(reader);
                    var nav = doc.CreateNavigator();
                    var nodes = nav.Select("/xml/ResourceInfo/SourceDataDescription");
                    while (nodes.MoveNext() == true)
                    {
                        itemNames.Add(nodes.Current.Value);
                    }
                    Log.WriteLine("Found all items; count is " + nodes.Count);
                }
            }
            else
            {
                //Fix for friends for life SDS files.
                MessageBox.Show("Detected SDS with no ResourceXML. I do not recommend repacking this SDS. It could cause crashes!", "Toolkit", MessageBoxButtons.OK, MessageBoxIcon.Information);
                Log.WriteLine("Detected SDS with no ResourceXML. I do not recommend repacking this SDS. It could cause crashes!", LoggingTypes.WARNING);
                for (int i = 0; i != ResourceEntries.Count; i++)
                {
                    itemNames.Add("unk_" + i);
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

            Log.WriteLine("Begin unpacking and saving files..");

            XmlWriter resourceXML = XmlWriter.Create(extractedPath + file.Name + "/SDSContent.xml", settings);
            resourceXML.WriteStartElement("SDSResource");

            //TODO Cleanup this code. It's awful. (V2 26/08/18, improved to use switch)
            for (int i = 0; i != ResourceEntries.Count; i++)
            {
                ResourceEntry entry = ResourceEntries[i];
                resourceXML.WriteStartElement("ResourceEntry");
                resourceXML.WriteElementString("Type", ResourceTypes[entry.TypeId].Name);
                string saveName = "";
                Log.WriteLine("Resource: " + i + ", name: " + itemNames[i] + ", type: " + entry.TypeId);

                switch (ResourceTypes[entry.TypeId].Name)
                {
                    case "Texture":
                        ReadTextureEntry(entry, resourceXML, itemNames[i]);
                        saveName = itemNames[i];
                        break;
                    case "Mipmap":
                        ReadMipmapEntry(entry, resourceXML, itemNames[i]);
                        saveName = "MIP_" + itemNames[i];
                        break;
                    case "IndexBufferPool":
                        saveName = ReadBasicEntry(resourceXML, "IndexBufferPool_" + i + ".ibp");
                        break;
                    case "VertexBufferPool":
                        saveName = ReadBasicEntry(resourceXML, "VertexBufferPool_" + i + ".vbp");
                        break;
                    case "AnimalTrafficPaths":
                        saveName = ReadBasicEntry(resourceXML, "AnimalTrafficPaths" + i + ".atp");
                        break;
                    case "FrameResource":
                        saveName = ReadBasicEntry(resourceXML, "FrameResource_" + i + ".fr");
                        break;
                    case "Effects":
                        saveName = ReadBasicEntry(resourceXML, "Effects_" + i + ".eff");
                        break;
                    case "FrameNameTable":
                        saveName = ReadBasicEntry(resourceXML, "FrameNameTable_" + i + ".fnt");
                        break;
                    case "EntityDataStorage":
                        saveName = ReadBasicEntry(resourceXML, "EntityDataStorage_" + i + ".eds");
                        break;
                    case "PREFAB":
                        saveName = ReadBasicEntry(resourceXML, "PREFAB_" + i + ".prf");
                        break;
                    case "ItemDesc":
                        saveName = ReadBasicEntry(resourceXML, "ItemDesc_" + i + ".ids");
                        break;
                    case "Actors":
                        saveName = ReadBasicEntry(resourceXML, "Actors_" + i + ".act");
                        break;
                    case "Collisions":
                        saveName = ReadBasicEntry(resourceXML, "Collisions_" + i + ".col");
                        break;
                    case "AudioSectors":
                        saveName = ReadBasicEntry(resourceXML, "AudioSectors_" + i + ".aus");
                        break;
                    case "SoundTable":
                        saveName = ReadBasicEntry(resourceXML, "SoundTable_" + i + ".stbl");
                        break;
                    case "Speech":
                        saveName = ReadBasicEntry(resourceXML, "Speech_" + i + ".spe");
                        break;
                    case "FxAnimSet":
                        saveName = ReadBasicEntry(resourceXML, "FxAnimSet_" + i + ".fas");
                        break;
                    case "FxActor":
                        saveName = ReadBasicEntry(resourceXML, "FxActor_" + i + ".fxa");
                        break;
                    case "Cutscene":
                        saveName = ReadBasicEntry(resourceXML, "Cutscene_" + i + ".cut");
                        break;
                    case "Translokator":
                        saveName = ReadBasicEntry(resourceXML, "Translokator_" + i + ".tra");
                        break;
                    case "Animation2":
                        saveName = ReadBasicEntry(resourceXML, "FxAnimSet_" + i + ".fas");
                        break;
                    case "NAV_AIWORLD_DATA":
                        saveName = ReadBasicEntry(resourceXML, "NAV_AIWORLD_DATA_" + i + ".nav");
                        break;
                    case "NAV_OBJ_DATA":
                        saveName = ReadBasicEntry(resourceXML, "NAV_OBJ_DATA_" + i + ".nov");
                        break;
                    case "NAV_HPD_DATA":
                        saveName = ReadBasicEntry(resourceXML, "NAV_HPD_DATA_" + i + ".nhv");
                        break;
                    case "Script":
                        ReadScriptEntry(entry, resourceXML, extractedPath + file.Name);
                        continue;
                    case "XML":
                        ReadXMLEntry(entry, resourceXML, itemNames[i], extractedPath + file.Name);
                        continue;
                    case "Sound":
                        ReadSoundEntry(entry, resourceXML, itemNames[i], extractedPath + file.Name);
                        saveName = itemNames[i] + ".fsb";
                        break;
                    case "MemFile":
                        ReadMemEntry(entry, resourceXML, itemNames[i], extractedPath + file.Name);
                        saveName = itemNames[i];
                        break;
                    case "Table":
                        ReadTableEntry(entry, resourceXML, "");
                        continue;
                    case "Animated Texture":
                        saveName = ReadBasicEntry(resourceXML, itemNames[i]);
                        break;
                    default:
                        MessageBox.Show("Found unknown type: " + ResourceTypes[(int)entry.TypeId].Name);
                        break;
                }

                resourceXML.WriteElementString("Version", entry.Version.ToString());
                using (BinaryWriter writer = new BinaryWriter(File.Open(extractedPath + file.Name + "/" + saveName, FileMode.Create)))
                {
                    writer.Write(entry.Data);
                }

                resourceXML.WriteEndElement();
            }

            resourceXML.WriteEndElement();
            resourceXML.Flush();
            resourceXML.Dispose();
        }

        public ResourceEntry ReadTextureEntry(ResourceEntry entry, XmlWriter resourceXML, string name)
        {
            TextureResource resource = new TextureResource();
            resource.Deserialize(entry.Version, new MemoryStream(entry.Data), Endian.Little);
            resourceXML.WriteElementString("File", name);
            resourceXML.WriteElementString("HasMIP", resource.HasMIP.ToString());
            entry.Data = resource.Data;
            return entry;
        }
        public ResourceEntry ReadMipmapEntry(ResourceEntry entry, XmlWriter resourceXML, string name)
        {
            TextureResource resource = new TextureResource();
            resource.DeserializeMIP(entry.Version, new MemoryStream(entry.Data), Endian.Little);
            resourceXML.WriteElementString("File", "MIP_" + name);
            entry.Data = resource.Data;
            return entry;
        }
        public string ReadBasicEntry(XmlWriter resourceXML, string name)
        {
            resourceXML.WriteElementString("File", name);
            return name;
        }
        public void ReadScriptEntry(ResourceEntry entry, XmlWriter resourceXML, string scriptDir)
        {
            ScriptResource resource = new ScriptResource();
            resource.Deserialize(entry.Version, new MemoryStream(entry.Data), Endian.Little);
            resourceXML.WriteElementString("File", resource.Path);
            resourceXML.WriteElementString("ScriptNum", resource.Scripts.Count.ToString());
            for (int x = 0; x != resource.Scripts.Count; x++)
            {
                string scrdir = scriptDir;
                string[] dirs = resource.Scripts[x].Name.Split('/');
                for (int z = 0; z != dirs.Length - 1; z++)
                {
                    scrdir += "/" + dirs[z];
                    Directory.CreateDirectory(scrdir);
                }

                using (BinaryWriter writer = new BinaryWriter(
                    File.Open(scriptDir + "/" + resource.Scripts[x].Name, FileMode.Create)))
                {
                    writer.Write(resource.Scripts[x].Data);
                }
                resourceXML.WriteElementString("Name", resource.Scripts[x].Name);
            }
            resourceXML.WriteElementString("Version", entry.Version.ToString());
            resourceXML.WriteEndElement(); //finish early.
        }
        public void ReadXMLEntry(ResourceEntry entry, XmlWriter resourceXML, string name, string xmlDir)
        {
            resourceXML.WriteElementString("File", name);
            XmlResource resource = new XmlResource();

            try
            {
                string[] dirs = name.Split('/');
                resource = new XmlResource();
                resource.Deserialize(entry.Version, new MemoryStream(entry.Data), Endian.Little);
                string xmldir = xmlDir;
                for (int z = 0; z != dirs.Length - 1; z++)
                {
                    xmldir += "/" + dirs[z];
                    Directory.CreateDirectory(xmldir);
                }
                if (!resource.Unk3)
                    File.WriteAllText(xmlDir + "/" + name + ".xml", resource.Content);
                else
                {
                    using (BinaryWriter writer = new BinaryWriter(File.Open(xmlDir + "/" + name + ".xml", FileMode.Create)))
                    {
                        writer.Write(entry.Data);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("ERROR CONVERTING XML: " + ex.Message);
            }
            resourceXML.WriteElementString("XMLTag", resource.Tag);
            resourceXML.WriteElementString("Unk1", Convert.ToByte(resource.Unk1).ToString());
            resourceXML.WriteElementString("Unk3", Convert.ToByte(resource.Unk3).ToString());
            resourceXML.WriteElementString("Version", entry.Version.ToString());
            resourceXML.WriteEndElement(); //finish early.
        }
        public void ReadSoundEntry(ResourceEntry entry, XmlWriter resourceXML, string name, string soundDir)
        {
            //Do resource first..
            SoundResource resource = new SoundResource();
            resource.Deserialize(entry.Data);
            entry.Data = resource.Data;

            string fileName = name + ".fsb";
            string[] dirs = name.Split('/');

            string sounddir = soundDir;
            for (int z = 0; z != dirs.Length - 1; z++)
            {
                sounddir += "/" + dirs[z];
                Directory.CreateDirectory(sounddir);
            }
            resourceXML.WriteElementString("File", fileName);
        }
        public void ReadMemEntry(ResourceEntry entry, XmlWriter resourceXML, string name, string memDIR)
        {
            MemFileResource resource = new MemFileResource();
            resource.Deserialize(entry.Data);
            entry.Data = resource.Data;

            string[] dirs = name.Split('/');

            string memdir = memDIR;
            for (int z = 0; z != dirs.Length - 1; z++)
            {
                memdir += "/" + dirs[z];
                Directory.CreateDirectory(memdir);
            }
            resourceXML.WriteElementString("File", name);
        }
        public ResourceEntry ReadTableEntry(ResourceEntry entry, XmlWriter resourceXML, string name)
        {
            TableResource resource = new TableResource();
            resource.Deserialize(entry.Version, new MemoryStream(entry.Data), Endian.Little);
            //todo extract individual tables.
            name = "Tables_" + ".tbl";
            resourceXML.WriteElementString("File", name);
            return entry;
        }

        private XmlNode AddRamElement(XmlDocument xmlDoc, string name, int num)
        {
            XmlNode node = xmlDoc.CreateElement(name);
            XmlAttribute attribute = xmlDoc.CreateAttribute("__type");
            attribute.Value = "Int";
            node.InnerText = num.ToString();
            node.Attributes.Append(attribute);

            return node;
        }
    }
    #endregion
}