using System.ComponentModel;
using System.IO;
using Toolkit.Mathematics;
using Utils.Extensions;

namespace ResourceTypes.Cutscene.AnimEntities
{
    public class AeFrameWrapper : AnimEntityWrapper
    {
        [TypeConverter(typeof(ExpandableObjectConverter))]
        public AeFrame FrameEntity { get; set; }

        public AeFrameWrapper() : base()
        {
            FrameEntity = new AeFrame();
            AnimEntityData = new AeFrameData();
        }

        public override void ReadFromFile(MemoryStream stream, bool isBigEndian)
        {
            base.ReadFromFile(stream, isBigEndian);
            FrameEntity.ReadFromFile(stream, isBigEndian);
        }

        public override void WriteToFile(MemoryStream stream, bool isBigEndian)
        {
            base.WriteToFile(stream, isBigEndian);
            FrameEntity.WriteToFile(stream, isBigEndian);
        }

        public override AnimEntityTypes GetEntityType()
        {
            return AnimEntityTypes.AeFrame;
        }
    }

    // TODO: I don't really understand this data; we need to understand it though.
    // It looks to reference prior hashes which can be found in the base class, 
    // and then stores hashes/transforms for these objects? Unknown though, most fail to save.
    public class AeFrame : AnimEntity
    {
        public byte Unk00 { get; set; }
        public ulong ParentHash { get; set; }
        public Matrix44 ParentOffset { get; set; } = new();
        public ulong RootHash { get; set; }
        public Matrix44 RootOffset { get; set; } = new();
        public override void ReadFromFile(MemoryStream stream, bool isBigEndian)
        {
            base.ReadFromFile(stream, isBigEndian);
            //UnknownData = stream.ReadBytes((int)Size-8);
            
            Unk00 = stream.ReadByte8();

            ParentHash = stream.ReadUInt64(isBigEndian);

            if (ParentHash != 0)
            {
                RootHash = stream.ReadUInt64(isBigEndian);
            }
                
            ParentOffset.ReadFromFile(stream, isBigEndian);

            if (ParentHash != 0)
            {
                RootOffset.ReadFromFile(stream, isBigEndian);
            }
        }

        public override void WriteToFile(MemoryStream stream, bool isBigEndian)
        {
            base.WriteToFile(stream, isBigEndian);

            stream.WriteByte(Unk00);

            stream.Write(ParentHash, isBigEndian);

            if (ParentHash != 0)
            {
                stream.Write(RootHash, isBigEndian);
            }
            
            ParentOffset.WriteToFile(stream, isBigEndian);

            if (ParentHash != 0)
            {
                RootOffset.WriteToFile(stream, isBigEndian);
            }

            UpdateSize(stream, isBigEndian);
        }

        public override AnimEntityTypes GetEntityType()
        {
            return AnimEntityTypes.AeFrame;
        }
    }

    public class AeFrameData : AeBaseData
    {
        public int[] UnkInts { get; set; } = new int[0];
        public override void ReadFromFile(MemoryStream stream, bool isBigEndian)
        {
            base.ReadFromFile(stream, isBigEndian);

            int Count = stream.ReadInt32(isBigEndian) * 2;
            UnkInts = new int[Count];

            for (int i = 0; i < Count; i++)
            {
                UnkInts[i] = stream.ReadInt32(isBigEndian);
            }
        }

        public override void WriteToFile(MemoryStream stream, bool isBigEndian)
        {
            base.WriteToFile(stream, isBigEndian);

            stream.Write((int)(UnkInts.Length / 2), isBigEndian);

            foreach (var val in UnkInts)
            {
                stream.Write(val, isBigEndian);
            }
        }
    }
}
