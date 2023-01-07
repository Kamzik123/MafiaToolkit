using System.ComponentModel;
using System.IO;
using System.Windows;
using Utils.Extensions;
using ResourceTypes.Actors;
using Utils.Helpers.Reflection;
using System.Xml.Linq;
using System.Collections.Generic;
using Gibbed.Squish;
using System;
using System.Linq;

namespace ResourceTypes.EntityDataStorage
{
    public class EntityDataStorageLoader
    {
        [ReadOnly(true)]
        public ActorEDSTypes EntityType { get; set; }
        [ReadOnly(true)]
        public int TableSize { get; set; }
        public ulong Hash { get; set; }
        public ulong[] TableHashes { get; set; }
        [Browsable(false)]
        public IActorExtraDataInterface[] Tables { get; set; }

        public EntityDataStorageLoader()
        {
            Tables = new IActorExtraDataInterface[0];
        }

        public void ReadFromFile(string fileName, bool isBigEndian)
        {
            Dictionary<string, dynamic> OverridesMin = new Dictionary<string, dynamic>();
            Dictionary<string, dynamic> OverridesMax = new Dictionary<string, dynamic>();
            Dictionary<string, Array> OverridesArrays = new Dictionary<string, Array>();
            Dictionary<int, int[]> SoundCategories = new Dictionary<int, int[]>();
            Dictionary<string, int> Categories = new Dictionary<string, int>();
            Dictionary<string, string> Sounds = new Dictionary<string, string>();
            string[] lines = File.ReadAllLines("Resources\\EDS_Override.txt");
            string[] SoundsByCategory = File.ReadAllLines("Resources\\SoundsByCategory.txt");

            int CurrentIndex = 0;
            List<int> CurrentValues = new List<int>();
            foreach (string line in SoundsByCategory)
            {
                if (line.Contains("Category"))
                {
                    SoundCategories.Add(CurrentIndex, CurrentValues.ToArray());
                    CurrentIndex = int.Parse(line.Replace("Category - ", ""));
                    CurrentValues = new List<int>();
                }
                else if (line.Contains("Sound"))
                {
                    CurrentValues.Add(int.Parse(line.Replace("Sound - ", "")));
                }
            }

            foreach (string line in lines)
            {
                string[] values = line.Split(" ");

                if (values[1].Contains("[]"))
                {
                    string typeName = values[1].Replace("[]", "");
                    Type type = System.Type.GetType("System." + typeName);
                    string[] arrayValues = values[2].Split("|");
                    Array ArrayObject = Array.CreateInstance(type, arrayValues.Length);

                    for (int i = 0; i < arrayValues.Length; i++)
                    {
                        ArrayObject.SetValue(ConverterUtils.ConvertObjectToValue(type, arrayValues[i]), i);
                    }

                    OverridesArrays.Add(values[0], ArrayObject);
                }
                else if (values[1].Equals("category", StringComparison.InvariantCultureIgnoreCase))
                {
                    Categories.Add(values[0], -1);
                }
                else if (values[1].Equals("sound", StringComparison.InvariantCultureIgnoreCase))
                {
                    Sounds.Add(values[0], values[2]);
                }
                else
                {
                    OverridesMin.Add(values[0], ConverterUtils.ConvertObjectToValue(Type.GetType("System." + values[1]), values[2]));
                    OverridesMax.Add(values[0], ConverterUtils.ConvertObjectToValue(Type.GetType("System." + values[1]), values[3]));
                }
            }

            using (var fileStream = new MemoryStream(File.ReadAllBytes(fileName)))
            {
                EntityType = (ActorEDSTypes)fileStream.ReadInt32(isBigEndian);

                Hash = fileStream.ReadUInt64(isBigEndian);
                TableSize = fileStream.ReadInt32(isBigEndian);

                uint numTables = fileStream.ReadUInt32(isBigEndian);
                TableHashes = new ulong[numTables];
                Tables = new IActorExtraDataInterface[numTables];

                // Iterate and read table hashes
                for (int i = 0; i < numTables; i++)
                {
                    TableHashes[i] = fileStream.ReadUInt64(isBigEndian);
                }

                Directory.CreateDirectory("EDSFiles");

                // Iterate and Read all tables
                for (int i = 0; i < numTables; i++)
                {
                    using (MemoryStream stream = new MemoryStream(fileStream.ReadBytes(TableSize)))
                    {
                        var item = ActorFactory.LoadEntityDataStorage(EntityType, stream, isBigEndian, OverridesMin, OverridesMax, OverridesArrays, SoundCategories, Categories, Sounds);
                        Tables[i] = item;
                    }
                }
            }
        }

        public void WriteToFile(string fileName, bool isBigEndian)
        {
            using (var fileStream = new MemoryStream())
            {
                fileStream.Write((int)EntityType, isBigEndian);
                fileStream.Write(Hash, isBigEndian);
                fileStream.Write(TableSize, isBigEndian);
                fileStream.Write(Tables.Length, isBigEndian);

                // Write file hashes
                for(int i = 0; i < Tables.Length; i++)
                {
                    fileStream.Write(TableHashes[i], isBigEndian);
                }

                // Iterate through each table and write their data into bespoke streams, 
                // then dump into main stream.
                for (int i = 0; i < Tables.Length; i++)
                {
                    using (MemoryStream stream = new MemoryStream())
                    {
                        Tables[i].WriteToFile(stream, isBigEndian);
                        fileStream.Write(stream.ToArray());
                    }
                }

                File.WriteAllBytes(fileName, fileStream.ToArray());
            }
        }

        public void ConvertToXML(string Filename)
        {
            XElement Root = ReflectionHelpers.ConvertPropertyToXML(this);
            Root.Save(Filename);
        }

        public void ConvertFromXML(string Filename)
        {
            XElement LoadedDoc = XElement.Load(Filename);
            EntityDataStorageLoader FileContents = ReflectionHelpers.ConvertToPropertyFromXML<EntityDataStorageLoader>(LoadedDoc);

            // Copy data taken from loaded XML
            EntityType = FileContents.EntityType;
            TableSize = FileContents.TableSize;
            Hash = FileContents.Hash;
            TableHashes = FileContents.TableHashes;
            Tables = FileContents.Tables;
        }
    }
}