﻿using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using SharpDX;
using Utils.Extensions;
using Utils.SharpDXExtensions;

namespace ResourceTypes.Cutscene.AnimEntities
{
    public class AeSpotLightWrapper : AnimEntityWrapper
    {
        [TypeConverter(typeof(ExpandableObjectConverter))]
        public AeSpotLight SpotLightEntity { get; set; }
        [TypeConverter(typeof(ExpandableObjectConverter))]
        public AeSpotLightTarget SpotLightTargetEntity { get; set; }

        public AeSpotLightWrapper() : base()
        {
            SpotLightEntity = new AeSpotLight();
            SpotLightTargetEntity = new AeSpotLightTarget();
            AnimEntityData = new AeSpotLightData();
        }

        public override void ReadFromFile(MemoryStream stream, bool isBigEndian)
        {
            base.ReadFromFile(stream, isBigEndian);
            SpotLightEntity.ReadFromFile(stream, isBigEndian);
            SpotLightTargetEntity.ReadFromFile(stream, isBigEndian);
        }

        public override void WriteToFile(MemoryStream stream, bool isBigEndian)
        {
            base.WriteToFile(stream, isBigEndian);
            SpotLightEntity.WriteToFile(stream, isBigEndian);
            SpotLightTargetEntity.WriteToFile(stream, isBigEndian);
        }

        public override AnimEntityTypes GetEntityType()
        {
            return AnimEntityTypes.AeSpotLight;
        }
    }

    //AeSpotLight
    public class AeSpotLight : AnimEntity
    {
        public byte Unk05 { get; set; }
        public int Unk06 { get; set; }
        public int Unk07 { get; set; }     
        public Matrix Transform { get; set; }
        public int Unk09 { get; set; }
        public int UnknownSize { get; set; }
        public float[] Unk08 { get; set; }
        public int Unk11 { get; set; }
        public int Unk12 { get; set; }
        public string Name33 { get; set; }
        public float[] Unk14 { get; set; }
        public string[] Unk15 { get; set; }

        // Only available if UnknownSize == 1
        public int Type_1_Unk0 { get; set; }

        public override void ReadFromFile(MemoryStream stream, bool isBigEndian)
        {
            base.ReadFromFile(stream, isBigEndian);
            Unk05 = stream.ReadByte8();
            Unk06 = stream.ReadInt32(isBigEndian);
            Unk07 = stream.ReadInt32(isBigEndian);
            Transform = MatrixExtensions.ReadFromFile(stream, isBigEndian);
            Unk09 = stream.ReadInt32(isBigEndian);
            UnknownSize = stream.ReadInt32(isBigEndian);
            Unk08 = new float[12];
            for (int i = 0; i < 12; i++)
            {
                Unk08[i] = stream.ReadSingle(isBigEndian);
            }
            Unk11 = stream.ReadInt32(isBigEndian);
            Unk12 = stream.ReadInt32(isBigEndian);
            Name33 = stream.ReadString16(isBigEndian);

            if(UnknownSize == 2)
            {
                Unk14 = new float[20];
                for (int i = 0; i < 20; i++)
                {
                    Unk14[i] = stream.ReadSingle(isBigEndian);
                }
                Unk15 = new string[3];
                for (int i = 0; i < 3; i++)
                {
                    Unk15[i] = stream.ReadString16(isBigEndian);
                }
            }
            else if(UnknownSize == 1)
            {
                Type_1_Unk0 = stream.ReadInt32(isBigEndian);

                Unk14 = new float[21];
                for (int i = 0; i < 21; i++)
                {
                    Unk14[i] = stream.ReadSingle(isBigEndian);
                }
                Unk15 = new string[1];
                for (int i = 0; i < 1; i++)
                {
                    Unk15[i] = stream.ReadString16(isBigEndian);
                }
            }
        }

        public override void WriteToFile(MemoryStream stream, bool isBigEndian)
        {
            base.WriteToFile(stream, isBigEndian);
            stream.WriteByte(Unk05);
            stream.Write(Unk06, isBigEndian);
            stream.Write(Unk07, isBigEndian);
            Transform.WriteToFile(stream, isBigEndian);
            stream.Write(Unk09, isBigEndian);
            stream.Write(UnknownSize, isBigEndian);
            foreach (var Value in Unk08)
            {
                stream.Write(Value, isBigEndian);
            }
            stream.Write(Unk11, isBigEndian);
            stream.Write(Unk12, isBigEndian);
            stream.WriteString16(Name33, isBigEndian);

            if(UnknownSize == 2)
            {
                foreach (var Value in Unk14)
                {
                    stream.Write(Value, isBigEndian);
                }
                foreach (var Value in Unk15)
                {
                    stream.WriteString16(Value, isBigEndian);
                }
            }
            else if (UnknownSize == 1)
            {
                stream.Write(Type_1_Unk0, isBigEndian);

                foreach (var Value in Unk14)
                {
                    stream.Write(Value, isBigEndian);
                }
                foreach (var Value in Unk15)
                {
                    stream.WriteString16(Value, isBigEndian);
                }
            }

            UpdateSize(stream, isBigEndian);
        }
        public override AnimEntityTypes GetEntityType()
        {
            return AnimEntityTypes.AeSpotLight;
        }
    }

    public class AeSpotLightData : AeBaseData
    {
        public byte Unk02 { get; set; }
        public int Unk03 { get; set; }
        public override void ReadFromFile(MemoryStream stream, bool isBigEndian)
        {
            base.ReadFromFile(stream, isBigEndian);
            Debug.Assert(stream.Position != stream.Length, "I've read the parent class data, although i've hit the eof!");

            Unk02 = stream.ReadByte8();

            // Could be an array of integers
            if (Unk02 == 1)
            {
                Unk03 = stream.ReadInt32(isBigEndian);
            }
            else if (Unk02 != 0)
            {
                Console.WriteLine("oof");
            }
        }

        public override void WriteToFile(MemoryStream stream, bool isBigEndian)
        {
            base.WriteToFile(stream, isBigEndian);
            stream.WriteByte(Unk02);

            // Could be an array of integers
            if (Unk02 == 1)
            {
                stream.Write(Unk03, isBigEndian);
            }
            else if (Unk02 != 0)
            {
                Console.WriteLine("oof");
            }
        }
    }
}
