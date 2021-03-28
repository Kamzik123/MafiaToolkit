﻿using System;
using System.IO;
using Utils.StringHelpers;

namespace ResourceTypes.Navigation
{
    //For AIWorld
    //Type 1: AI Group
    //Type 2: AI World Part
    //Type 3: AI Nav Point
    //Type 4: AI Cover Part
    //Type 5: AI Anim Point
    //Type 6: AI User Area
    //Type 7: AI Path Object
    //Type 8: AI Action Point
    //Type 9: AI Action Point 2?
    //Type 10: AI Action Point 3?
    //Type 11: AI Hiding Place
    //Type 12: AI Action Point 4?
    public class NAVData
    {
        //unk01_flags could be types; AIWORLDS seem to have 1005, while OBJDATA is 3604410608.
        FileInfo file;

        int fileSize; //size - 4;
        uint unk01_flags; //possibly flags?
        string fileName;
        public object data;


        public NAVData(BinaryReader reader)
        {
            ReadFromFile(reader);
        }
        public NAVData(FileInfo info)
        {
            file = info;

            using (BinaryReader reader = new BinaryReader(File.Open(info.FullName, FileMode.Open)))
            {
                ReadFromFile(reader);
            }

            WriteToFile();
        }

        public void WriteToFile()
        {
            var backup = file.FullName.Insert(file.FullName.Length - 4, "_test");
            using (NavigationWriter writer = new NavigationWriter(File.Open(backup, FileMode.Create)))
            {
                WriteToFile(writer);
            }
        }

        public void ReadFromFile(BinaryReader reader)
        {
            fileSize = reader.ReadInt32();
            unk01_flags = reader.ReadUInt32();

            //file name seems to be earlier.
            if (unk01_flags == 3604410608)
            {
                int nameSize = reader.ReadInt32();
                fileName = new string(reader.ReadChars(nameSize));

                long start = reader.BaseStream.Position;
                string hpdString = new string(reader.ReadChars(11));
                reader.ReadByte();
                int hpdVersion = reader.ReadInt32();
                if (hpdString == "Kynogon HPD" && hpdVersion == 2)
                {
                    data = new HPDData();
                    (data as HPDData).ReadFromFile(reader);
                }
                else
                {
                    reader.BaseStream.Seek(start, SeekOrigin.Begin);
                    data = new OBJData(reader);
                }
            }
            else if (unk01_flags == 1005)
            {
                data = new AIWorld(reader);
            }
            else
            {
                throw new Exception("Found unexpected type: " + unk01_flags);
            }
        }

        public void WriteToFile(NavigationWriter writer)
        {
            writer.Write(-1); //file size
            writer.Write(unk01_flags);

            if (unk01_flags == 3604410608)
            {
                writer.Write(fileName.Length);
                StringHelpers.WriteString(writer, fileName, false);
                if (data is OBJData)
                {
                    (data as OBJData).WriteToFile(writer);
                }
                else if(data is HPDData)
                {
                    WriteHPD(writer);

                    // Our HPD file here actually should subtract 12 of the total.
                    writer.BaseStream.Position = 0;
                    writer.Write((uint)(writer.BaseStream.Length - 12));
                    return;
                }
            }

            writer.BaseStream.Position = 0;
            writer.Write((uint)(writer.BaseStream.Length - 4));
        }

        private void WriteHPD(BinaryWriter writer)
        {
            var hpd = (data as HPDData);

            StringHelpers.WriteString(writer, "Kynogon HPD");
            writer.Write(2); // HPD Version is always 2.
            hpd.WriteToFile(writer);
        }
    }
}