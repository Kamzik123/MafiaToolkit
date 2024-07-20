using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Numerics;
using System.Windows.Markup;
using System.Windows.Shapes;
using Toolkit.Mathematics;
using Utils.Extensions;
using Utils.Settings;
using Utils.VorticeUtils;

namespace ResourceTypes.Cutscene.CurveParams
{
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class CurveAnimParam : ICurveParam
    {
        public CurveAnimParam()
        {

        }

        public CurveAnimParam(BinaryReader br)
        {
            Read(br);
        }

        public override void Read(BinaryReader br)
        {
            base.Read(br);
        }

        public override void Write(BinaryWriter bw)
        {
            base.Write(bw);
        }

        public override int GetParamType()
        {
            return base.GetParamType();
        }

        public override string ToString()
        {
            return GetType().Name;
        }
    }

    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class FloatLinear : CurveAnimParam
    {
        [TypeConverter(typeof(ExpandableObjectConverter))]
        public class FrameData
        {
            public int StartFrame { get; set; }
            public int EndFrame { get; set; }
            public bool Unk00 { get; set; } = true;
            public float Value { get; set; } = 0.0f;
            public FrameData()
            {

            }

            public FrameData(BinaryReader br)
            {
                Read(br);
            }

            public void Read(BinaryReader br)
            {
                StartFrame = br.ReadInt32();
                EndFrame = br.ReadInt32();
                Unk00 = br.ReadBoolean();
                Value = br.ReadSingle();
            }

            public void Write(BinaryWriter bw)
            {
                bw.Write(StartFrame);
                bw.Write(EndFrame);
                bw.Write(Unk00);
                bw.Write(Value);
            }
        }

        public FrameData[] Data { get; set; } = new FrameData[0];
        public short Unk00 { get; set; }

        public FloatLinear()
        {

        }

        public FloatLinear(BinaryReader br)
        {
            Read(br);
        }

        public override void Read(BinaryReader br)
        {
            base.Read(br);
            int Count = br.ReadInt32();
            Data = new FrameData[Count];

            for (int i = 0; i < Data.Length; i++)
            {
                Data[i] = new(br);
            }

            Unk00 = br.ReadInt16();
        }

        public override void Write(BinaryWriter bw)
        {
            base.Write(bw);
            bw.Write(Data.Length);

            foreach (var data in Data)
            {
                data.Write(bw);
            }

            bw.Write(Unk00);
        }

        public override int GetParamType()
        {
            return 0;
        }

        public override string ToString()
        {
            return GetType().BaseType.Name + "::" + GetType().Name + " Frames: " + Data.Length;
        }
    }

    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class FloatBezier : CurveAnimParam
    {
        [TypeConverter(typeof(ExpandableObjectConverter))]
        public class FrameData
        {
            public int StartFrame { get; set; }
            public int EndFrame { get; set; }
            public bool Unk00 { get; set; } = true;
            public int Unk01 { get; set; }
            [TypeConverter(typeof(Vector2Converter))]
            public Vector2 AnchorA { get; set; } = Vector2.Zero;
            [TypeConverter(typeof(Vector2Converter))]
            public Vector2 AnchorB { get; set; } = Vector2.Zero;
            public float Value { get; set; } = 0.0f;
            public FrameData()
            {

            }

            public FrameData(BinaryReader br)
            {
                Read(br);
            }

            public void Read(BinaryReader br)
            {
                StartFrame = br.ReadInt32();
                EndFrame = br.ReadInt32();
                Unk00 = br.ReadBoolean();
                Unk01 = br.ReadInt32();
                AnchorA = Vector2Extenders.ReadFromFile(br);
                AnchorB = Vector2Extenders.ReadFromFile(br);
                Value = br.ReadSingle();
            }

            public void Write(BinaryWriter bw)
            {
                bw.Write(StartFrame);
                bw.Write(EndFrame);
                bw.Write(Unk00);
                bw.Write(Unk01);
                AnchorA.WriteToFile(bw);
                AnchorB.WriteToFile(bw);
                bw.Write(Value);
            }
        }

        public FrameData[] Data { get; set; } = new FrameData[0];
        public short Unk00 { get; set; }

        public FloatBezier()
        {

        }

        public FloatBezier(BinaryReader br)
        {
            Read(br);
        }

        public override void Read(BinaryReader br)
        {
            base.Read(br);
            int Count = br.ReadInt32();
            Data = new FrameData[Count];

            for (int i = 0; i < Data.Length; i++)
            {
                Data[i] = new(br);
            }

            Unk00 = br.ReadInt16();
        }

        public override void Write(BinaryWriter bw)
        {
            base.Write(bw);
            bw.Write(Data.Length);

            foreach (var data in Data)
            {
                data.Write(bw);
            }

            bw.Write(Unk00);
        }

        public override int GetParamType()
        {
            return 1;
        }

        public override string ToString()
        {
            return GetType().BaseType.Name + "::" + GetType().Name + " Frames: " + Data.Length;
        }
    }

    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class FloatTCB : CurveAnimParam
    {
        [TypeConverter(typeof(ExpandableObjectConverter))]
        public class FrameData
        {
            public int StartFrame { get; set; }
            public int EndFrame { get; set; }
            public bool Unk00 { get; set; } = true;
            public int Unk01 { get; set; }
            [TypeConverter(typeof(Vector2Converter))]
            public Vector2 Position { get; set; } = Vector2.Zero;
            [TypeConverter(typeof(Vector2Converter))]
            public Vector2 PreviousTangent { get; set; } = Vector2.Zero;
            [TypeConverter(typeof(Vector2Converter))]
            public Vector2 NextTangent { get; set; } = Vector2.Zero;
            public float Value { get; set; } = 0.0f;
            public FrameData()
            {

            }

            public FrameData(BinaryReader br)
            {
                Read(br);
            }

            public void Read(BinaryReader br)
            {
                StartFrame = br.ReadInt32();
                EndFrame = br.ReadInt32();
                Unk00 = br.ReadBoolean();
                Unk01 = br.ReadInt32();
                Position = Vector2Extenders.ReadFromFile(br);
                PreviousTangent = Vector2Extenders.ReadFromFile(br);
                NextTangent = Vector2Extenders.ReadFromFile(br);
                Value = br.ReadSingle();
            }

            public void Write(BinaryWriter bw)
            {
                bw.Write(StartFrame);
                bw.Write(EndFrame);
                bw.Write(Unk00);
                bw.Write(Unk01);
                Position.WriteToFile(bw);
                PreviousTangent.WriteToFile(bw);
                NextTangent.WriteToFile(bw);
                bw.Write(Value);
            }
        }

        public FrameData[] Data { get; set; } = new FrameData[0];
        public short Unk00 { get; set; }

        public FloatTCB()
        {

        }

        public FloatTCB(BinaryReader br)
        {
            Read(br);
        }

        public override void Read(BinaryReader br)
        {
            base.Read(br);
            int Count = br.ReadInt32();
            Data = new FrameData[Count];

            for (int i = 0; i < Data.Length; i++)
            {
                Data[i] = new(br);
            }

            Unk00 = br.ReadInt16();
        }

        public override void Write(BinaryWriter bw)
        {
            base.Write(bw);
            bw.Write(Data.Length);

            foreach (var data in Data)
            {
                data.Write(bw);
            }

            bw.Write(Unk00);
        }

        public override int GetParamType()
        {
            return 2;
        }

        public override string ToString()
        {
            return GetType().BaseType.Name + "::" + GetType().Name + " Frames: " + Data.Length;
        }
    }

    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class Vector2Linear : CurveAnimParam
    {
        [TypeConverter(typeof(ExpandableObjectConverter))]
        public class FrameData
        {
            public int StartFrame { get; set; }
            public int EndFrame { get; set; }
            public bool Unk00 { get; set; } = true;
            [TypeConverter(typeof(Vector2Converter))]
            public Vector2 Value { get; set; } = Vector2.Zero;
            public FrameData()
            {

            }

            public FrameData(BinaryReader br)
            {
                Read(br);
            }

            public void Read(BinaryReader br)
            {
                StartFrame = br.ReadInt32();
                EndFrame = br.ReadInt32();
                Unk00 = br.ReadBoolean();
                Value = Vector2Extenders.ReadFromFile(br);
            }

            public void Write(BinaryWriter bw)
            {
                bw.Write(StartFrame);
                bw.Write(EndFrame);
                bw.Write(Unk00);
                Value.WriteToFile(bw);
            }
        }

        public FrameData[] Data { get; set; } = new FrameData[0];
        public short Unk00 { get; set; }

        public Vector2Linear()
        {

        }

        public Vector2Linear(BinaryReader br)
        {
            Read(br);
        }

        public override void Read(BinaryReader br)
        {
            base.Read(br);
            int Count = br.ReadInt32();
            Data = new FrameData[Count];

            for (int i = 0; i < Data.Length; i++)
            {
                Data[i] = new(br);
            }

            Unk00 = br.ReadInt16();
        }

        public override void Write(BinaryWriter bw)
        {
            base.Write(bw);
            bw.Write(Data.Length);

            foreach (var data in Data)
            {
                data.Write(bw);
            }

            bw.Write(Unk00);
        }

        public override int GetParamType()
        {
            return 3;
        }

        public override string ToString()
        {
            return GetType().BaseType.Name + "::" + GetType().Name + " Frames: " + Data.Length;
        }
    }

    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class Vector2Bezier : CurveAnimParam
    {
        [TypeConverter(typeof(ExpandableObjectConverter))]
        public class FrameData
        {
            public int StartFrame { get; set; }
            public int EndFrame { get; set; }
            public bool Unk00 { get; set; } = true;
            public int Unk01 { get; set; }
            [TypeConverter(typeof(Vector2Converter))]
            public Vector2 AnchorA { get; set; } = Vector2.Zero;
            [TypeConverter(typeof(Vector2Converter))]
            public Vector2 AnchorB { get; set; } = Vector2.Zero;
            [TypeConverter(typeof(Vector2Converter))]
            public Vector2 AnchorC { get; set; } = Vector2.Zero;
            [TypeConverter(typeof(Vector2Converter))]
            public Vector2 AnchorD { get; set; } = Vector2.Zero;
            [TypeConverter(typeof(Vector2Converter))]
            public Vector2 Value { get; set; } = Vector2.Zero;
            public FrameData()
            {

            }

            public FrameData(BinaryReader br)
            {
                Read(br);
            }

            public void Read(BinaryReader br)
            {
                StartFrame = br.ReadInt32();
                EndFrame = br.ReadInt32();
                Unk00 = br.ReadBoolean();
                Unk01 = br.ReadInt32();
                AnchorA = Vector2Extenders.ReadFromFile(br);
                AnchorB = Vector2Extenders.ReadFromFile(br);
                AnchorC = Vector2Extenders.ReadFromFile(br);
                AnchorD = Vector2Extenders.ReadFromFile(br);
                Value = Vector2Extenders.ReadFromFile(br);
            }

            public void Write(BinaryWriter bw)
            {
                bw.Write(StartFrame);
                bw.Write(EndFrame);
                bw.Write(Unk00);
                bw.Write(Unk01);
                AnchorA.WriteToFile(bw);
                AnchorB.WriteToFile(bw);
                AnchorC.WriteToFile(bw);
                AnchorD.WriteToFile(bw);
                Value.WriteToFile(bw);
            }
        }

        public FrameData[] Data { get; set; } = new FrameData[0];
        public short Unk00 { get; set; }

        public Vector2Bezier()
        {

        }

        public Vector2Bezier(BinaryReader br)
        {
            Read(br);
        }

        public override void Read(BinaryReader br)
        {
            base.Read(br);
            int Count = br.ReadInt32();
            Data = new FrameData[Count];

            for (int i = 0; i < Data.Length; i++)
            {
                Data[i] = new(br);
            }

            Unk00 = br.ReadInt16();
        }

        public override void Write(BinaryWriter bw)
        {
            base.Write(bw);
            bw.Write(Data.Length);

            foreach (var data in Data)
            {
                data.Write(bw);
            }

            bw.Write(Unk00);
        }

        public override int GetParamType()
        {
            return 4;
        }

        public override string ToString()
        {
            return GetType().BaseType.Name + "::" + GetType().Name + " Frames: " + Data.Length;
        }
    }

    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class Vector2TCB : CurveAnimParam
    {
        [TypeConverter(typeof(ExpandableObjectConverter))]
        public class FrameData
        {
            public int StartFrame { get; set; }
            public int EndFrame { get; set; }
            public bool Unk00 { get; set; } = true;
            public int Unk01 { get; set; }
            [TypeConverter(typeof(Vector3Converter))]
            public Vector3 Position { get; set; } = Vector3.Zero;
            [TypeConverter(typeof(Vector3Converter))]
            public Vector3 PreviousTangent { get; set; } = Vector3.Zero;
            [TypeConverter(typeof(Vector3Converter))]
            public Vector3 NextTangent { get; set; } = Vector3.Zero;
            [TypeConverter(typeof(Vector2Converter))]
            public Vector2 Value { get; set; } = Vector2.Zero;
            public FrameData()
            {

            }

            public FrameData(BinaryReader br)
            {
                Read(br);
            }

            public void Read(BinaryReader br)
            {
                StartFrame = br.ReadInt32();
                EndFrame = br.ReadInt32();
                Unk00 = br.ReadBoolean();
                Unk01 = br.ReadInt32();
                Position = Vector3Utils.ReadFromFile(br);
                PreviousTangent = Vector3Utils.ReadFromFile(br);
                NextTangent = Vector3Utils.ReadFromFile(br);
                Value = Vector2Extenders.ReadFromFile(br);
            }

            public void Write(BinaryWriter bw)
            {
                bw.Write(StartFrame);
                bw.Write(EndFrame);
                bw.Write(Unk00);
                bw.Write(Unk01);
                Position.WriteToFile(bw);
                PreviousTangent.WriteToFile(bw);
                NextTangent.WriteToFile(bw);
                Value.WriteToFile(bw);
            }
        }

        public FrameData[] Data { get; set; } = new FrameData[0];
        public short Unk00 { get; set; }

        public Vector2TCB()
        {

        }

        public Vector2TCB(BinaryReader br)
        {
            Read(br);
        }

        public override void Read(BinaryReader br)
        {
            base.Read(br);
            int Count = br.ReadInt32();
            Data = new FrameData[Count];

            for (int i = 0; i < Data.Length; i++)
            {
                Data[i] = new(br);
            }

            Unk00 = br.ReadInt16();
        }

        public override void Write(BinaryWriter bw)
        {
            base.Write(bw);
            bw.Write(Data.Length);

            foreach (var data in Data)
            {
                data.Write(bw);
            }

            bw.Write(Unk00);
        }

        public override int GetParamType()
        {
            return 5;
        }

        public override string ToString()
        {
            return GetType().BaseType.Name + "::" + GetType().Name + " Frames: " + Data.Length;
        }
    }

    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class Vector3Linear : CurveAnimParam
    {
        [TypeConverter(typeof(ExpandableObjectConverter))]
        public class FrameData
        {
            public int StartFrame { get; set; }
            public int EndFrame { get; set; }
            public bool Unk00 { get; set; } = true;
            [TypeConverter(typeof(Vector3Converter))]
            public Vector3 Value { get; set; } = Vector3.Zero;
            public FrameData()
            {

            }

            public FrameData(BinaryReader br)
            {
                Read(br);
            }

            public void Read(BinaryReader br)
            {
                StartFrame = br.ReadInt32();
                EndFrame = br.ReadInt32();
                Unk00 = br.ReadBoolean();
                Value = Vector3Utils.ReadFromFile(br);
            }

            public void Write(BinaryWriter bw)
            {
                bw.Write(StartFrame);
                bw.Write(EndFrame);
                bw.Write(Unk00);
                Value.WriteToFile(bw);
            }
        }

        public FrameData[] Data { get; set; } = new FrameData[0];
        public short Unk00 { get; set; }

        public Vector3Linear()
        {

        }

        public Vector3Linear(BinaryReader br)
        {
            Read(br);
        }

        public override void Read(BinaryReader br)
        {
            base.Read(br);
            int Count = br.ReadInt32();
            Data = new FrameData[Count];

            for (int i = 0; i < Data.Length; i++)
            {
                Data[i] = new(br);
            }

            Unk00 = br.ReadInt16();
        }

        public override void Write(BinaryWriter bw)
        {
            base.Write(bw);
            bw.Write(Data.Length);

            foreach (var data in Data)
            {
                data.Write(bw);
            }

            bw.Write(Unk00);
        }

        public override int GetParamType()
        {
            return 6;
        }

        public override string ToString()
        {
            return GetType().BaseType.Name + "::" + GetType().Name + " Frames: " + Data.Length;
        }
    }

    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class Vector3Bezier : CurveAnimParam
    {
        [TypeConverter(typeof(ExpandableObjectConverter))]
        public class FrameData
        {
            public int StartFrame { get; set; }
            public int EndFrame { get; set; }
            public bool Unk00 { get; set; } = true;
            public int Unk01 { get; set; }
            [TypeConverter(typeof(Vector4Converter))]
            public Vector4 AnchorA { get; set; } = Vector4.Zero;
            [TypeConverter(typeof(Vector4Converter))]
            public Vector4 AnchorB { get; set; } = Vector4.Zero;
            [TypeConverter(typeof(Vector4Converter))]
            public Vector4 AnchorC { get; set; } = Vector4.Zero;
            [TypeConverter(typeof(Vector3Converter))]
            public Vector3 Value { get; set; } = Vector3.Zero;
            public FrameData()
            {

            }

            public FrameData(BinaryReader br)
            {
                Read(br);
            }

            public void Read(BinaryReader br)
            {
                StartFrame = br.ReadInt32();
                EndFrame = br.ReadInt32();
                Unk00 = br.ReadBoolean();
                Unk01 = br.ReadInt32();
                AnchorA = Vector4Extenders.ReadFromFile(br);
                AnchorB = Vector4Extenders.ReadFromFile(br);
                AnchorC = Vector4Extenders.ReadFromFile(br);
                Value = Vector3Utils.ReadFromFile(br);
            }

            public void Write(BinaryWriter bw)
            {
                bw.Write(StartFrame);
                bw.Write(EndFrame);
                bw.Write(Unk00);
                bw.Write(Unk01);
                AnchorA.WriteToFile(bw);
                AnchorB.WriteToFile(bw);
                AnchorC.WriteToFile(bw);
                Value.WriteToFile(bw);
            }
        }

        public FrameData[] Data { get; set; } = new FrameData[0];
        public short Unk00 { get; set; }

        public Vector3Bezier()
        {

        }

        public Vector3Bezier(BinaryReader br)
        {
            Read(br);
        }

        public override void Read(BinaryReader br)
        {
            base.Read(br);
            int Count = br.ReadInt32();
            Data = new FrameData[Count];

            for (int i = 0; i < Data.Length; i++)
            {
                Data[i] = new(br);
            }

            Unk00 = br.ReadInt16();
        }

        public override void Write(BinaryWriter bw)
        {
            base.Write(bw);
            bw.Write(Data.Length);

            foreach (var data in Data)
            {
                data.Write(bw);
            }

            bw.Write(Unk00);
        }

        public override int GetParamType()
        {
            return 7;
        }

        public override string ToString()
        {
            return GetType().BaseType.Name + "::" + GetType().Name + " Frames: " + Data.Length;
        }
    }

    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class Vector3TCB : CurveAnimParam
    {
        [TypeConverter(typeof(ExpandableObjectConverter))]
        public class FrameData
        {
            public int StartFrame { get; set; }
            public int EndFrame { get; set; }
            public bool Unk00 { get; set; } = true;
            public int Unk01 { get; set; }
            [TypeConverter(typeof(Vector3Converter))]
            public Vector3 Position { get; set; } = Vector3.Zero;
            [TypeConverter(typeof(Vector3Converter))]
            public Vector3 PreviousTangent { get; set; } = Vector3.Zero;
            [TypeConverter(typeof(Vector3Converter))]
            public Vector3 NextTangent { get; set; } = Vector3.Zero;
            [TypeConverter(typeof(Vector3Converter))]
            public Vector3 Value { get; set; } = Vector3.Zero;
            public FrameData()
            {

            }

            public FrameData(BinaryReader br)
            {
                Read(br);
            }

            public void Read(BinaryReader br)
            {
                StartFrame = br.ReadInt32();
                EndFrame = br.ReadInt32();
                Unk00 = br.ReadBoolean();
                Unk01 = br.ReadInt32();
                Position = Vector3Utils.ReadFromFile(br);
                PreviousTangent = Vector3Utils.ReadFromFile(br);
                NextTangent = Vector3Utils.ReadFromFile(br);
                Value = Vector3Utils.ReadFromFile(br);
            }

            public void Write(BinaryWriter bw)
            {
                bw.Write(StartFrame);
                bw.Write(EndFrame);
                bw.Write(Unk00);
                bw.Write(Unk01);
                Position.WriteToFile(bw);
                PreviousTangent.WriteToFile(bw);
                NextTangent.WriteToFile(bw);
                Value.WriteToFile(bw);
            }
        }

        public FrameData[] Data { get; set; } = new FrameData[0];
        public short Unk00 { get; set; }

        public Vector3TCB()
        {

        }

        public Vector3TCB(BinaryReader br)
        {
            Read(br);
        }

        public override void Read(BinaryReader br)
        {
            base.Read(br);
            int Count = br.ReadInt32();
            Data = new FrameData[Count];

            for (int i = 0; i < Data.Length; i++)
            {
                Data[i] = new(br);
            }

            Unk00 = br.ReadInt16();
        }

        public override void Write(BinaryWriter bw)
        {
            base.Write(bw);
            bw.Write(Data.Length);

            foreach (var data in Data)
            {
                data.Write(bw);
            }

            bw.Write(Unk00);
        }

        public override int GetParamType()
        {
            return 8;
        }

        public override string ToString()
        {
            return GetType().BaseType.Name + "::" + GetType().Name + " Frames: " + Data.Length;
        }
    }

    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class QuaternionLinear : CurveAnimParam
    {
        [TypeConverter(typeof(ExpandableObjectConverter))]
        public class FrameData
        {
            public int StartFrame { get; set; }
            public int EndFrame { get; set; }
            public bool Unk00 { get; set; } = true;
            [TypeConverter(typeof(QuaternionConverter))]
            public Quaternion Value { get; set; } = Quaternion.Identity;
            public FrameData()
            {

            }

            public FrameData(BinaryReader br)
            {
                Read(br);
            }

            public void Read(BinaryReader br)
            {
                StartFrame = br.ReadInt32();
                EndFrame = br.ReadInt32();
                Unk00 = br.ReadBoolean();
                Value = QuaternionExtensions.ReadFromFile(br);
            }

            public void Write(BinaryWriter bw)
            {
                bw.Write(StartFrame);
                bw.Write(EndFrame);
                bw.Write(Unk00);
                Value.WriteToFile(bw);
            }
        }

        public FrameData[] Data { get; set; } = new FrameData[0];
        public short Unk00 { get; set; } = 1;

        public QuaternionLinear()
        {

        }

        public QuaternionLinear(BinaryReader br)
        {
            Read(br);
        }

        public override void Read(BinaryReader br)
        {
            base.Read(br);
            int Count = br.ReadInt32();
            Data = new FrameData[Count];

            for (int i = 0; i < Data.Length; i++)
            {
                Data[i] = new(br);
            }

            Unk00 = br.ReadInt16();
        }

        public override void Write(BinaryWriter bw)
        {
            base.Write(bw);

            List<FrameData> newData = new();
            Matrix44Converter converter = new();

            string path = ToolkitSettings.AssetPaths[ToolkitSettings.CurrentAsset];

            if (!ToolkitSettings.AssetTypes[ToolkitSettings.CurrentAsset].pos)
            {
                goto Skip;
            }

            foreach (var line in File.ReadAllLines(path))
            {
                if (string.IsNullOrEmpty(line))
                {
                    continue;
                }

                string[] vals = line.Split('|');

                int frame = int.Parse(vals[0]) - 1;

                Matrix44 matrix = (Matrix44)converter.ConvertFromInvariantString(vals[1]);
                System.Numerics.Matrix4x4 m = System.Numerics.Matrix4x4.Identity;

                m.M11 = matrix.M11;
                m.M12 = matrix.M12;
                m.M13 = matrix.M13;
                m.M14 = matrix.M14;
                m.M21 = matrix.M21;
                m.M22 = matrix.M22;
                m.M23 = matrix.M23;
                m.M24 = matrix.M24;
                m.M31 = matrix.M31;
                m.M32 = matrix.M32;
                m.M33 = matrix.M33;
                m.M34 = matrix.M34;
                m.M41 = matrix.M41;
                m.M42 = matrix.M42;
                m.M43 = matrix.M43;
                m.M44 = matrix.M44;

                var quat = Quaternion.CreateFromRotationMatrix(m);

                FrameData f = new();

                f.StartFrame = frame;
                f.EndFrame = frame;
                f.Unk00 = true;
                f.Value = quat;

                newData.Add(f);
            }

        Skip:;

            Data = newData.ToArray();

            bw.Write(Data.Length);

            foreach (var data in Data)
            {
                data.Write(bw);
            }

            bw.Write(Unk00);
        }

        public override int GetParamType()
        {
            return 9;
        }

        public override string ToString()
        {
            return GetType().BaseType.Name + "::" + GetType().Name + " Frames: " + Data.Length;
        }
    }

    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class QuaternionBezier : CurveAnimParam
    {
        [TypeConverter(typeof(ExpandableObjectConverter))]
        public class FrameData
        {
            public int StartFrame { get; set; }
            public int EndFrame { get; set; }
            public bool Unk00 { get; set; } = true;
            [TypeConverter(typeof(QuaternionConverter))]
            public Quaternion Value { get; set; } = Quaternion.Identity;
            public FrameData()
            {

            }

            public FrameData(BinaryReader br)
            {
                Read(br);
            }

            public void Read(BinaryReader br)
            {
                StartFrame = br.ReadInt32();
                EndFrame = br.ReadInt32();
                Unk00 = br.ReadBoolean();
                Value = QuaternionExtensions.ReadFromFile(br);
            }

            public void Write(BinaryWriter bw)
            {
                bw.Write(StartFrame);
                bw.Write(EndFrame);
                bw.Write(Unk00);
                Value.WriteToFile(bw);
            }
        }

        public FrameData[] Data { get; set; } = new FrameData[0];
        public short Unk00 { get; set; }

        public QuaternionBezier()
        {

        }

        public QuaternionBezier(BinaryReader br)
        {
            Read(br);
        }

        public override void Read(BinaryReader br)
        {
            base.Read(br);
            int Count = br.ReadInt32();
            Data = new FrameData[Count];

            for (int i = 0; i < Data.Length; i++)
            {
                Data[i] = new(br);
            }

            Unk00 = br.ReadInt16();
        }

        public override void Write(BinaryWriter bw)
        {
            base.Write(bw);
            bw.Write(Data.Length);

            foreach (var data in Data)
            {
                data.Write(bw);
            }

            bw.Write(Unk00);
        }

        public override int GetParamType()
        {
            return 10;
        }

        public override string ToString()
        {
            return GetType().BaseType.Name + "::" + GetType().Name + " Frames: " + Data.Length;
        }
    }

    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class QuaternionTCB : CurveAnimParam
    {
        [TypeConverter(typeof(ExpandableObjectConverter))]
        public class FrameData
        {
            public int StartFrame { get; set; }
            public int EndFrame { get; set; }
            public bool Unk00 { get; set; } = true;
            [TypeConverter(typeof(QuaternionConverter))]
            public Quaternion Value { get; set; } = Quaternion.Identity;
            public FrameData()
            {

            }

            public FrameData(BinaryReader br)
            {
                Read(br);
            }

            public void Read(BinaryReader br)
            {
                StartFrame = br.ReadInt32();
                EndFrame = br.ReadInt32();
                Unk00 = br.ReadBoolean();
                Value = QuaternionExtensions.ReadFromFile(br);
            }

            public void Write(BinaryWriter bw)
            {
                bw.Write(StartFrame);
                bw.Write(EndFrame);
                bw.Write(Unk00);
                Value.WriteToFile(bw);
            }
        }

        public FrameData[] Data { get; set; } = new FrameData[0];
        public short Unk00 { get; set; }

        public QuaternionTCB()
        {

        }

        public QuaternionTCB(BinaryReader br)
        {
            Read(br);
        }

        public override void Read(BinaryReader br)
        {
            base.Read(br);
            int Count = br.ReadInt32();
            Data = new FrameData[Count];

            for (int i = 0; i < Data.Length; i++)
            {
                Data[i] = new(br);
            }

            Unk00 = br.ReadInt16();
        }

        public override void Write(BinaryWriter bw)
        {
            base.Write(bw);
            bw.Write(Data.Length);

            foreach (var data in Data)
            {
                data.Write(bw);
            }

            bw.Write(Unk00);
        }

        public override int GetParamType()
        {
            return 11;
        }

        public override string ToString()
        {
            return GetType().BaseType.Name + "::" + GetType().Name + " Frames: " + Data.Length;
        }
    }

    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class PositionXYZ : CurveAnimParam
    {
        [TypeConverter(typeof(ExpandableObjectConverter))]
        public class FrameDataWrapper
        {
            [TypeConverter(typeof(ExpandableObjectConverter))]
            public class FrameData
            {
                public int StartFrame { get; set; }
                public int EndFrame { get; set; }
                public bool Unk00 { get; set; } = true;
                public FrameData()
                {

                }

                public FrameData(BinaryReader br)
                {
                    Read(br);
                }

                public virtual void Read(BinaryReader br)
                {
                    StartFrame = br.ReadInt32();
                    EndFrame = br.ReadInt32();
                    Unk00 = br.ReadBoolean();
                }

                public virtual void Write(BinaryWriter bw)
                {
                    bw.Write(StartFrame);
                    bw.Write(EndFrame);
                    bw.Write(Unk00);
                }
            }

            [TypeConverter(typeof(ExpandableObjectConverter))]
            public class FloatData : FrameData
            {
                public float Value { get; set; }
                public FloatData()
                {

                }

                public FloatData(BinaryReader br)
                {
                    Read(br);
                }

                public override void Read(BinaryReader br)
                {
                    base.Read(br);
                    Value = br.ReadSingle();
                }

                public override void Write(BinaryWriter bw)
                {
                    base.Write(bw);
                    bw.Write(Value);
                }
            }

            [TypeConverter(typeof(ExpandableObjectConverter))]
            public class VectorData : FrameData
            {
                public int Unk01 { get; set; }
                [TypeConverter(typeof(Vector4Converter))]
                public Vector4 Unk02 { get; set; } = Vector4.Zero;
                public float Value { get; set; }
                public VectorData()
                {

                }

                public VectorData(BinaryReader br)
                {
                    Read(br);
                }

                public override void Read(BinaryReader br)
                {
                    base.Read(br);
                    Unk01 = br.ReadInt32();
                    Unk02 = Vector4Extenders.ReadFromFile(br);
                    Value = br.ReadSingle();
                }

                public override void Write(BinaryWriter bw)
                {
                    base.Write(bw);
                    bw.Write(Unk01);
                    Unk02.WriteToFile(bw);
                    bw.Write(Value);
                }
            }

            public int Type { get; set; }
            public FrameData[] Data { get; set; } = new FloatData[0];

            public FrameDataWrapper()
            {

            }

            public FrameDataWrapper(BinaryReader br)
            {
                Read(br);
            }

            public void Read(BinaryReader br)
            {
                Type = br.ReadInt32();
                int Count = br.ReadInt32();

                switch (Type)
                {
                    case 0:
                        Data = new FloatData[Count];

                        for (int i = 0; i < Data.Length; i++)
                        {
                            Data[i] = new FloatData(br);
                        }
                        break;

                    case 1:
                        Data = new VectorData[Count];

                        for (int i = 0; i < Data.Length; i++)
                        {
                            Data[i] = new VectorData(br);
                        }
                        break;
                }
            }

            public void Write(BinaryWriter bw)
            {
                bw.Write(Type);
                bw.Write(Data.Length);

                for (int i = 0; i < Data.Length; i++)
                {
                    switch (Type)
                    {
                        case 0:
                            if (Data[i] is not FloatData)
                            {
                                Data[i] = new FloatData();
                            }
                            break;

                        case 1:
                            if (Data[i] is not VectorData)
                            {
                                Data[i] = new VectorData();
                            }
                            break;
                    }

                    Data[i].Write(bw);
                }
            }
        }
        
        public int Unk00 { get; set; }
        public FrameDataWrapper X { get; set; } = new FrameDataWrapper();
        public FrameDataWrapper Y { get; set; } = new FrameDataWrapper();
        public FrameDataWrapper Z { get; set; } = new FrameDataWrapper();
        public short Unk01 { get; set; }

        public PositionXYZ()
        {

        }

        public PositionXYZ(BinaryReader br)
        {
            Read(br);
        }

        public override void Read(BinaryReader br)
        {
            base.Read(br);
            Unk00 = br.ReadInt32();
            X = new(br);
            Y = new(br);
            Z = new(br);
            Unk01 = br.ReadInt16();
        }

        public override void Write(BinaryWriter bw)
        {
            base.Write(bw);
            bw.Write(Unk00);

            List<FrameDataWrapper.FloatData> XData = new();
            List<FrameDataWrapper.FloatData> YData = new();
            List<FrameDataWrapper.FloatData> ZData = new();

            Matrix44Converter converter = new();

            string path = ToolkitSettings.AssetPaths[ToolkitSettings.CurrentAsset];
            var offset = ToolkitSettings.AssetPosOffsets[ToolkitSettings.CurrentAsset];

            if (!ToolkitSettings.AssetTypes[ToolkitSettings.CurrentAsset].pos)
            {
                goto Skip;
            }

            foreach (var line in File.ReadAllLines(path))
            {
                if (string.IsNullOrEmpty(line))
                {
                    continue;
                }

                string[] vals = line.Split('|');

                int frame = int.Parse(vals[0]) - 1;

                Matrix44 matrix = (Matrix44)converter.ConvertFromInvariantString(vals[1]);
                System.Numerics.Matrix4x4 m = System.Numerics.Matrix4x4.Identity;

                m.M11 = matrix.M11;
                m.M12 = matrix.M21;
                m.M13 = matrix.M31;
                m.M14 = matrix.M41;
                m.M21 = matrix.M12;
                m.M22 = matrix.M22;
                m.M23 = matrix.M32;
                m.M24 = matrix.M42;
                m.M31 = matrix.M13;
                m.M32 = matrix.M23;
                m.M33 = matrix.M33;
                m.M34 = matrix.M43;
                m.M41 = matrix.M14;
                m.M42 = matrix.M24;
                m.M43 = matrix.M34;
                m.M44 = matrix.M44;

                var t = m.Translation;

                FrameDataWrapper.FloatData x = new();
                FrameDataWrapper.FloatData y = new();
                FrameDataWrapper.FloatData z = new();

                x.StartFrame = frame;
                x.EndFrame = frame;
                x.Unk00 = true;
                x.Value = t.X + offset.x;

                y.StartFrame = frame;
                y.EndFrame = frame;
                y.Unk00 = true;
                y.Value = t.Y + offset.y;

                z.StartFrame = frame;
                z.EndFrame = frame;
                z.Unk00 = true;
                z.Value = t.Z + offset.z;

                XData.Add(x);
                YData.Add(y);
                ZData.Add(z);
            }

        Skip:;

            X.Type = 0;
            Y.Type = 0;
            Z.Type = 0;

            X.Data = XData.ToArray();
            Y.Data = YData.ToArray();
            Z.Data = ZData.ToArray();

            X.Write(bw);
            Y.Write(bw);
            Z.Write(bw);
            bw.Write(Unk01);
        }

        public override int GetParamType()
        {
            return 27;
        }

        public override string ToString()
        {
            return GetType().BaseType.Name + "::" + GetType().Name;
        }
    }

    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class EulerXYZ : CurveAnimParam
    {
        [TypeConverter(typeof(ExpandableObjectConverter))]
        public class FrameDataWrapper
        {
            [TypeConverter(typeof(ExpandableObjectConverter))]
            public class FrameData
            {
                public int StartFrame { get; set; }
                public int EndFrame { get; set; }
                public bool Unk00 { get; set; } = true;
                public FrameData()
                {

                }

                public FrameData(BinaryReader br)
                {
                    Read(br);
                }

                public virtual void Read(BinaryReader br)
                {
                    StartFrame = br.ReadInt32();
                    EndFrame = br.ReadInt32();
                    Unk00 = br.ReadBoolean();
                }

                public virtual void Write(BinaryWriter bw)
                {
                    bw.Write(StartFrame);
                    bw.Write(EndFrame);
                    bw.Write(Unk00);
                }
            }

            [TypeConverter(typeof(ExpandableObjectConverter))]
            public class FloatData : FrameData
            {
                public float Value { get; set; }
                public FloatData()
                {

                }

                public FloatData(BinaryReader br)
                {
                    Read(br);
                }

                public override void Read(BinaryReader br)
                {
                    base.Read(br);
                    Value = br.ReadSingle();
                }

                public override void Write(BinaryWriter bw)
                {
                    base.Write(bw);
                    bw.Write(Value);
                }
            }

            [TypeConverter(typeof(ExpandableObjectConverter))]
            public class QuaternionData : FrameData
            {
                public int Unk01 { get; set; }
                [TypeConverter(typeof(QuaternionConverter))]
                public Quaternion Unk02 { get; set; } = Quaternion.Identity;
                public float Value { get; set; }
                public QuaternionData()
                {

                }

                public QuaternionData(BinaryReader br)
                {
                    Read(br);
                }

                public override void Read(BinaryReader br)
                {
                    base.Read(br);
                    Unk01 = br.ReadInt32();
                    Unk02 = QuaternionExtensions.ReadFromFile(br);
                    Value = br.ReadSingle();
                }

                public override void Write(BinaryWriter bw)
                {
                    base.Write(bw);
                    bw.Write(Unk01);
                    Unk02.WriteToFile(bw);
                    bw.Write(Value);
                }
            }

            public int Type { get; set; }
            public FrameData[] Data { get; set; } = new FloatData[0];

            public FrameDataWrapper()
            {

            }

            public FrameDataWrapper(BinaryReader br)
            {
                Read(br);
            }

            public void Read(BinaryReader br)
            {
                Type = br.ReadInt32();
                int Count = br.ReadInt32();

                switch (Type)
                {
                    case 0:
                        Data = new FloatData[Count];

                        for (int i = 0; i < Data.Length; i++)
                        {
                            Data[i] = new FloatData(br);
                        }
                        break;

                    case 1:
                        Data = new QuaternionData[Count];

                        for (int i = 0; i < Data.Length; i++)
                        {
                            Data[i] = new QuaternionData(br);
                        }
                        break;
                }
            }

            public void Write(BinaryWriter bw)
            {
                bw.Write(Type);
                bw.Write(Data.Length);

                for (int i = 0; i < Data.Length; i++)
                {
                    switch (Type)
                    {
                        case 0:
                            if (Data[i] is not FloatData)
                            {
                                Data[i] = new FloatData();
                            }
                            break;

                        case 1:
                            if (Data[i] is not QuaternionData)
                            {
                                Data[i] = new QuaternionData();
                            }
                            break;
                    }

                    Data[i].Write(bw);
                }
            }
        }

        public int Unk00 { get; set; }
        public FrameDataWrapper X { get; set; } = new FrameDataWrapper();
        public FrameDataWrapper Y { get; set; } = new FrameDataWrapper();
        public FrameDataWrapper Z { get; set; } = new FrameDataWrapper();
        public short Unk01 { get; set; }

        public EulerXYZ()
        {

        }

        public EulerXYZ(BinaryReader br)
        {
            Read(br);
        }

        public override void Read(BinaryReader br)
        {
            base.Read(br);
            Unk00 = br.ReadInt32();
            X = new(br);
            Y = new(br);
            Z = new(br);
            Unk01 = br.ReadInt16();
        }

        public override void Write(BinaryWriter bw)
        {
            base.Write(bw);
            bw.Write(Unk00);

            List<FrameDataWrapper.FloatData> XData = new();
            List<FrameDataWrapper.FloatData> YData = new();
            List<FrameDataWrapper.FloatData> ZData = new();

            Matrix44Converter converter = new();

            string path = ToolkitSettings.AssetPaths[ToolkitSettings.CurrentAsset];

            if (!ToolkitSettings.AssetTypes[ToolkitSettings.CurrentAsset].rot)
            {
                goto Skip;
            }

            foreach (var line in File.ReadAllLines(path))
            {
                if (string.IsNullOrEmpty(line))
                {
                    continue;
                }

                string[] vals = line.Split('|');

                int frame = int.Parse(vals[0]) - 1;

                Matrix44 matrix = (Matrix44)converter.ConvertFromInvariantString(vals[1]);
                System.Numerics.Matrix4x4 m = System.Numerics.Matrix4x4.Identity;

                m.M11 = matrix.M11;
                m.M12 = matrix.M21;
                m.M13 = matrix.M31;
                m.M14 = matrix.M41;
                m.M21 = matrix.M12;
                m.M22 = matrix.M22;
                m.M23 = matrix.M32;
                m.M24 = matrix.M42;
                m.M31 = matrix.M13;
                m.M32 = matrix.M23;
                m.M33 = matrix.M33;
                m.M34 = matrix.M43;
                m.M41 = matrix.M14;
                m.M42 = matrix.M24;
                m.M43 = matrix.M34;
                m.M44 = matrix.M44;

                matrix = new(m);

                var t = matrix.Euler;

                FrameDataWrapper.FloatData x = new();
                FrameDataWrapper.FloatData y = new();
                FrameDataWrapper.FloatData z = new();

                x.StartFrame = frame;
                x.EndFrame = frame;
                x.Unk00 = true;
                x.Value = t.X;

                y.StartFrame = frame;
                y.EndFrame = frame;
                y.Unk00 = true;
                y.Value = t.Y;

                z.StartFrame = frame;
                z.EndFrame = frame;
                z.Unk00 = true;
                z.Value = t.Z;

                XData.Add(x);
                YData.Add(y);
                ZData.Add(z);
            }

        Skip:;

            X.Type = 0;
            Y.Type = 0;
            Z.Type = 0;

            X.Data = XData.ToArray();
            Y.Data = YData.ToArray();
            Z.Data = ZData.ToArray();

            X.Write(bw);
            Y.Write(bw);
            Z.Write(bw);
            bw.Write(Unk01);
        }

        public override int GetParamType()
        {
            return 28;
        }

        public override string ToString()
        {
            return GetType().BaseType.Name + "::" + GetType().Name;
        }
    }
}
