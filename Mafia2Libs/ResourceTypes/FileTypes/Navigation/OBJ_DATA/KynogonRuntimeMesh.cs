using SharpDX;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using Utils.SharpDXExtensions;
using Utils.StringHelpers;

namespace ResourceTypes.Navigation
{
    public struct Unk10DataSet
    {
        public BoundingBox B1;
        public int UnkOffset;
        public int Unk20;
    }

    public struct Unk12DataSet
    {
        public BoundingBox B1;
        public int Unk01; //-1?
        public int Unk02; //-1?
        public float Unk03;
        public float Unk04;
        public float Unk05;
    }

    public struct Unk14DataSet
    {
        public int Offset;
        public Vector3[] Points;
    }

    public struct Unk18DataSet
    {
        public float Unk0;
        public float Unk1;
        public float Unk2;
        public int Offset;
        public Vector3[] Points;
    }

    public struct UnkSet0
    {
        public float X;
        public float Y;
        public int Offset;

        //This data is after each segment is listed.
        public int cellUnk0;
        public int cellUnk1;
        public int cellUnk2;
        public int cellUnk3;
        public float cellUnk4;
        public float cellUnk5;
        public float cellUnk6;
        public float cellUnk7;
        public int cellUnk8;
        public int cellUnk9;
        public int NumUnk10Boxes;
        public int cellUnk11;
        public int NumUnk12Boxes;
        public int cellUnk13;
        public int NumUn14Boxes;
        public int cellUnk15;
        public int NumEdges;
        public int cellUnk17;
        public int NumUnk18Boxes;
        public int cellUnk19;
        public Unk10DataSet[] unk10Boxes;
        public Unk12DataSet[] unk12Boxes;
        public int[] unk14Offsets;
        public int unk14End;
        public Unk14DataSet[] unk14Boxes;
        public byte[] unk14Data;
        public int[] unk16Offsets;
        public int EdgesDataEndOffset;
        public BoundingBox[] EdgeBoxes;
        public Unk18DataSet[] unk18Set;
        public int unk18End;

    }

    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class KynogonRuntimeMesh
    {
        public class Cell
        {
            public UnkSet0[] Sets;
            public int Offset;

            public override string ToString()
            {
                return string.Format("Offset: {0}, Set Count {1}", Offset, Sets.Length);
            }
        }

        public int Unk0 { get; set; }
        public int Unk1 { get; set; }
        public float Unk2 { get; set; }
        public float Unk3 { get; set; }
        public Vector2 BoundMin { get; set; }
        public Vector2 BoundMax { get; set; }
        public int CellSizeX { get; set; }
        public int CellSizeY { get; set; }
        public float Radius { get; set; }
        public int Unk4;
        public int Height { get; set; }

        public int Offset;
        public int[] Grid;
        private uint end;
        public Cell[] Cells;

        public void ReadFromFile(BinaryReader reader)
        {
            //KynogonRuntimeMesh
            long mesh_pos = reader.BaseStream.Position;
            string magicName = new string(reader.ReadChars(18));
            if (magicName != "KynogonRuntimeMesh")
            {
                throw new FormatException("Did not find KynogonRuntimeMesh");
            }

            short mesh_unk0 = reader.ReadInt16();
            int magicVersion = reader.ReadInt32();

            if (magicVersion != 2)
            {
                throw new FormatException("Version did not equal 2");
            }

            Unk0 = reader.ReadInt32();
            Unk1 = reader.ReadInt32();
            Unk2 = reader.ReadSingle();
            Unk3 = reader.ReadSingle();

            float boundMinX = reader.ReadSingle();
            float boundMaxX = reader.ReadSingle();
            float boundMaxY = -reader.ReadSingle();
            float boundMinY = -reader.ReadSingle();

            BoundMin = new Vector2(boundMinX, boundMinY);
            BoundMax = new Vector2(boundMaxX, boundMaxY);

            //BoundingBox = BoundingBoxExtenders.ReadFromFile(reader);
            CellSizeX = reader.ReadInt32();
            CellSizeY = reader.ReadInt32();
            Radius = reader.ReadSingle();
            Unk4 = reader.ReadInt32();
            Height = reader.ReadInt32();
            Offset = reader.ReadInt32(); //this is a potential offset;
            Grid = new int[(CellSizeX * CellSizeY)];
            Cells = new Cell[Grid.Length];

            for (int i = 0; i < Grid.Length; i++)
            {
                Grid[i] = reader.ReadInt32();
            }

            end = reader.ReadUInt32(); // end of all cells
            int Count = 0;

            for (int i = 0; i < Cells.Length; i++)
            {
                Cell cell = new Cell();
                int numSet0 = reader.ReadInt32();
                cell.Sets = new UnkSet0[numSet0];
                Cells[i] = cell;

                if (numSet0 == 0)
                {
                    continue;
                }

                //NOTE: EVERY OFFSET IN UNKSET0 BEGINS FROM MESH_POS HIGHER IN THE CODE FILE.
                cell.Offset = reader.ReadInt32(); // start offset (excluding sets)

                for (int x = 0; x < numSet0; x++)
                {
                    UnkSet0 set = new UnkSet0();
                    set.X = reader.ReadSingle();
                    set.Y = reader.ReadSingle();
                    set.Offset = reader.ReadInt32(); // end offset
                    cell.Sets[x] = set;
                }

                //NOTE: EVERY BLOCK OF DATA SEEMS TO START WITH 96, THIS IS HOWEVER UNCONFIRMED.
                for (int z = 0; z < numSet0; z++)
                {
                    long set_offset = reader.BaseStream.Position - mesh_pos;

                    UnkSet0 set = cell.Sets[z];
                    //NOTE: MOST OF THE OFFSETS BEGIN HERE, JUST AFTER THE SETS HAVE BEEN DEFINED ABOVE. 
                    set.cellUnk0 = reader.ReadInt32();

                    if (set.cellUnk0 != Unk1)
                        throw new FormatException();

                    set.cellUnk1 = reader.ReadInt32();
                    set.cellUnk2 = reader.ReadInt32();
                    set.cellUnk3 = reader.ReadInt32();
                    set.cellUnk4 = reader.ReadSingle();
                    set.cellUnk5 = reader.ReadSingle();
                    set.cellUnk6 = reader.ReadSingle();
                    set.cellUnk7 = reader.ReadSingle();
                    set.cellUnk8 = reader.ReadInt32();
                    set.cellUnk9 = reader.ReadInt32(); //-1?
                    set.NumUnk10Boxes = reader.ReadInt32(); //1;
                    set.cellUnk11 = reader.ReadInt32(); // offset is start - setpos
                    set.NumUnk12Boxes = reader.ReadInt32(); //0
                    set.cellUnk13 = reader.ReadInt32(); // offset is start - setpos
                    set.NumUn14Boxes = reader.ReadInt32(); //0
                    set.cellUnk15 = reader.ReadInt32(); // offset is start - setpos
                    set.NumEdges = reader.ReadInt32(); //8;
                    set.cellUnk17 = reader.ReadInt32(); // offset is start - setpos
                    set.NumUnk18Boxes = reader.ReadInt32(); //0;
                    set.cellUnk19 = reader.ReadInt32(); // offset is start - setpos

                    Count += set.NumUnk12Boxes;

                    //THIS BIT IS UNKNOWN, UPTO CELLUNK20
                    set.unk10Boxes = new Unk10DataSet[set.NumUnk10Boxes];
                    for (int x = 0; x < set.NumUnk10Boxes; x++)
                    {
                        Unk10DataSet unk10Set = new Unk10DataSet();
                        unk10Set.B1 = BoundingBoxExtenders.ReadFromFile(reader);
                        unk10Set.UnkOffset = reader.ReadInt32(); // offset is next set in next cell? Linked cell?
                        unk10Set.Unk20 = reader.ReadInt32(); // really unsure on what this is
                        set.unk10Boxes[x] = unk10Set;
                    }
                    //END OF CONFUSING BIT.


                    //THIS BIT IS UNKNOWN, BUT IS CELLUNK12
                    set.unk12Boxes = new Unk12DataSet[set.NumUnk12Boxes];
                    for (int x = 0; x < set.NumUnk12Boxes; x++)
                    {
                        Unk12DataSet unk12Set = new Unk12DataSet();
                        unk12Set.B1 = BoundingBoxExtenders.ReadFromFile(reader);
                        unk12Set.Unk01 = reader.ReadInt32();
                        unk12Set.Unk02 = reader.ReadInt32();
                        unk12Set.Unk03 = reader.ReadSingle();
                        unk12Set.Unk04 = reader.ReadSingle();
                        unk12Set.Unk05 = reader.ReadSingle();
                        set.unk12Boxes[x] = unk12Set;
                    }

                    //END OF CONFUSING BIT.

                    //THIS LOOPS THROUGH OFFSETS TO BBOX'S
                    set.unk14Boxes = new Unk14DataSet[set.NumUn14Boxes];
                    for (int x = 0; x < set.NumUn14Boxes; x++)
                    {
                        var dataSet = new Unk14DataSet();
                        dataSet.Offset = reader.ReadInt32();
                        set.unk14Boxes[x] = dataSet;
                    }

                    //ALWAYS A 4-BYTE INTEGER WHICH DENOTES THE END OF THE BATCH
                    if (set.NumUn14Boxes > 0)
                    {
                        set.unk14End = reader.ReadInt32();
                        var currentOffset = set.unk14Boxes[0].Offset;
                        for (int x = 0; x < set.NumUn14Boxes; x++)
                        {
                            var dataSet = set.unk14Boxes[x];
                            var offset = (x + 1 < set.NumUn14Boxes ? set.unk14Boxes[x + 1].Offset : set.unk14End);
                            var size = offset - currentOffset;
                            currentOffset = offset;

                            var count = size / 12;
                            dataSet.Points = new Vector3[count];
                            for (int f = 0; f < dataSet.Points.Length; f++)
                            {
                                dataSet.Points[f] = Vector3Extenders.ReadFromFile(reader);
                            }
                            set.unk14Boxes[x] = dataSet;
                        }
                    }

                    //CONTINUE ONTO THE NEXT BATCH
                    set.unk16Offsets = new int[set.NumEdges];
                    for (int x = 0; x < set.NumEdges; x++)
                    {
                        set.unk16Offsets[x] = reader.ReadInt32(); // bounding box position - set_pos
                    }

                    //ALWAYS A 4-BYTE INTEGER WHICH DENOTES THE END OF THE BATCH
                    if (set.NumEdges > 0)
                        set.EdgesDataEndOffset = reader.ReadInt32(); // very end of set_pos

                    set.EdgeBoxes = new BoundingBox[set.NumEdges];

                    for (int x = 0; x < set.NumEdges; x++)
                    {
                        set.EdgeBoxes[x] = BoundingBoxExtenders.ReadFromFile(reader);
                    }

                    set.unk18Set = new Unk18DataSet[set.NumUnk18Boxes];
                    if (set.NumUnk18Boxes > 0)
                    {
                        set.unk18End = reader.ReadInt32();
                        for (int x = 0; x < set.NumUnk18Boxes; x++)
                        {
                            //THIS COULD BE AN OFFSET LIST WITH SOMEKIND OF FLOAT/ROTATION DATA
                            Unk18DataSet dataSet = new Unk18DataSet();
                            dataSet.Unk0 = reader.ReadSingle();
                            dataSet.Unk1 = reader.ReadSingle();
                            dataSet.Unk2 = reader.ReadSingle();
                            dataSet.Offset = reader.ReadInt32();
                            set.unk18Set[x] = dataSet;
                        }

                        byte[] unknown_data = reader.ReadBytes(12); //usually padding

                        var currentOffset = set.unk18End;
                        for (int x = 0; x < set.NumUnk18Boxes; x++)
                        {
                            var dataSet = set.unk18Set[x];
                            var size = dataSet.Offset - currentOffset;
                            var count = size / 12;
                            dataSet.Points = new Vector3[count];

                            for(int f = 0; f < dataSet.Points.Length; f++)
                            {
                                dataSet.Points[f] = Vector3Extenders.ReadFromFile(reader);
                            }
                            currentOffset = dataSet.Offset;
                            set.unk18Set[x] = dataSet;
                        }

                    }
                    cell.Sets[z] = set;
                }
                Console.WriteLine("Completed: " + i);
                //byte[] data = reader.ReadBytes(size);
                //File.WriteAllBytes("grid_" + i + ".bin", data);
            }

            uint UnkFooter0 = reader.ReadUInt32();
            if(UnkFooter0 > 0)
            {
                Console.WriteLine("");
                // extra data..
            }

            uint SizeofMesh = reader.ReadUInt32();

            /*
            if(UnkFooter0 == 0)
            {
                throw new NotImplementedException();
            }

            uint OffsetToFooter = reader.ReadUInt32(); // should be same as reader.position - kynogon mesh start
            uint FooterPadding = reader.ReadUInt32(); // Usually zero for me.
            string FooterName = StringHelpers.ReadString(reader); // A null terminated string.
            uint FooterUnk0 = reader.ReadUInt32(); // Size of string. Goes _after_ the string has been stored.
            uint FooterMagic = reader.ReadUInt32(); // should be 0x1213F001
            Debug.Assert(FooterMagic == 0x1213F001, "Didn't reach end of file.");
            */

            //File.WriteAllLines("model.obj", data.ToArray());

        }

        public void WriteToFile(NavigationWriter writer)
        {
            long MESH_START = writer.BaseStream.Position;

            StringHelpers.WriteString(writer, "KynogonRuntimeMesh", false);
            writer.Write((ushort)0); //magic is name with two extra 00
            writer.Write(2); //version

            writer.Write(Unk0);
            writer.Write(Unk1);
            writer.Write(Unk2);
            writer.Write(Unk3);

            writer.Write(BoundMin.X);
            writer.Write(BoundMax.X);
            writer.Write(-BoundMax.Y);
            writer.Write(-BoundMin.Y);

            writer.Write(CellSizeX);
            writer.Write(CellSizeY);
            writer.Write(Radius);
            writer.Write(Unk4);
            writer.Write(Height);
            writer.Write(Offset);
            
            for (int i = 0; i < Grid.Length; i++)
            {
                writer.PushLooseObjectPtr("GRID_OFFSET_"+i, MESH_START);
            }

            writer.PushLooseObjectPtr("GRID_END", MESH_START);

            for (int i = 0; i < Grid.Length; i++)
            {
                writer.SolveLooseObjectPtr("GRID_OFFSET_" + i);

                Cell cell = Cells[i];
                writer.Write(cell.Sets.Length);

                if(cell.Sets.Length == 0)
                {
                    continue;
                }

                writer.PushLooseObjectPtr("GRID_START_OFFSET", MESH_START);

                for(int x = 0; x < cell.Sets.Length; x++)
                {
                    var set = cell.Sets[x];
                    writer.Write(set.X);
                    writer.Write(set.Y);
                    writer.PushLooseObjectPtr("SET_END_OFFSET_" + x, MESH_START);
                }

                writer.SolveLooseObjectPtr("GRID_START_OFFSET");

                for(int x = 0; x < cell.Sets.Length; x++)
                {
                    long SET_START = writer.BaseStream.Position;

                    var set = cell.Sets[x];
                    writer.Write(set.cellUnk0);
                    writer.Write(set.cellUnk1);
                    writer.Write(set.cellUnk2);
                    writer.Write(set.cellUnk3);
                    writer.Write(set.cellUnk4);
                    writer.Write(set.cellUnk5);
                    writer.Write(set.cellUnk6);
                    writer.Write(set.cellUnk7);
                    writer.Write(set.cellUnk8);
                    writer.Write(set.cellUnk9);
                    writer.Write(set.NumUnk10Boxes);
                    writer.PushLooseObjectPtr("UNK10BOXES_OFFSET", SET_START);
                    writer.Write(set.NumUnk12Boxes);
                    writer.PushLooseObjectPtr("UNK12BOXES_OFFSET", SET_START);
                    writer.Write(set.NumUn14Boxes);
                    writer.PushLooseObjectPtr("UNK14BOXES_OFFSET", SET_START);
                    writer.Write(set.NumEdges);
                    writer.PushLooseObjectPtr("EDGES_OFFSET", SET_START);
                    writer.Write(set.NumUnk18Boxes);
                    writer.PushLooseObjectPtr("UNK18BOXES_OFFSET", SET_START);

                    if (set.NumUnk10Boxes > 0)
                    {
                        writer.SolveLooseObjectPtr("UNK10BOXES_OFFSET");
                        foreach (var dataSet in set.unk10Boxes)
                        {
                            BoundingBoxExtenders.WriteToFile(dataSet.B1, writer);
                            writer.Write(dataSet.UnkOffset);
                            writer.Write(dataSet.Unk20);
                        }
                    }

                    if (set.NumUnk12Boxes > 0)
                    {
                        writer.SolveLooseObjectPtr("UNK12BOXES_OFFSET");
                        foreach (var dataSet in set.unk12Boxes)
                        {
                            BoundingBoxExtenders.WriteToFile(dataSet.B1, writer);
                            writer.Write(dataSet.Unk01);
                            writer.Write(dataSet.Unk02);
                            writer.Write(dataSet.Unk03);
                            writer.Write(dataSet.Unk04);
                            writer.Write(dataSet.Unk05);
                        }
                    }

                    if(set.NumUn14Boxes > 0)
                    {
                        // write offset of boxes
                        writer.SolveLooseObjectPtr("UNK14BOXES_OFFSET");
                        for(int z = 0; z < set.unk14Boxes.Length; z++)
                        {
                            writer.PushLooseObjectPtr("UNK14_BOX_OFFSET_" + z, SET_START);
                        }

                        // write them
                        writer.PushLooseObjectPtr("UNK14_BOX_END", SET_START);
                        for (int z = 0; z < set.unk14Boxes.Length; z++)
                        {
                            writer.SolveLooseObjectPtr("UNK14_BOX_OFFSET_" + z);
                            foreach (var point in set.unk14Boxes[z].Points)
                            {
                                Vector3Extenders.WriteToFile(point, writer);
                            }
                        }

                        // end data with ending offset
                        writer.SolveLooseObjectPtr("UNK14_BOX_END");
                    }

                    if (set.NumEdges > 0)
                    {
                        // write offset of boxes
                        writer.SolveLooseObjectPtr("EDGES_OFFSET");
                        for (int z = 0; z < set.EdgeBoxes.Length; z++)
                        {
                            writer.PushLooseObjectPtr("EDGE_OFFSET_" + z, SET_START);
                        }

                        // write them
                        writer.PushLooseObjectPtr("EDGE_END", SET_START);
                        for (int z = 0; z < set.EdgeBoxes.Length; z++)
                        {
                            writer.SolveLooseObjectPtr("EDGE_OFFSET_" + z);
                            set.EdgeBoxes[z].WriteToFile(writer);
                        }

                        // end data with ending offset
                        writer.SolveLooseObjectPtr("EDGE_END");
                    }

                    if(set.NumUnk18Boxes > 0)
                    {
                        // write offset
                        writer.SolveLooseObjectPtr("UNK18BOXES_OFFSET");
                        writer.PushLooseObjectPtr("UNK18BOXES_END", SET_START);

                        for (int z = 0; z < set.unk18Set.Length; z++)
                        {
                            writer.Write(set.unk18Set[z].Unk0);
                            writer.Write(set.unk18Set[z].Unk1);
                            writer.Write(set.unk18Set[z].Unk2);
                            writer.PushLooseObjectPtr("UNK18_SET_OFFSET_" + z, SET_START);
                        }

                        writer.Write(new byte[12]);
                        writer.SolveLooseObjectPtr("UNK18BOXES_END");

                        for (int z = 0; z < set.unk18Set.Length; z++)
                        { 
                            for (int c = 0; c < set.unk18Set[z].Points.Length; c++)
                            {
                                Vector3Extenders.WriteToFile(set.unk18Set[z].Points[c], writer);
                            }

                            writer.SolveLooseObjectPtr("UNK18_SET_OFFSET_" + z);
                        }
                    }

                    // remove any outstanding offsets
                    writer.RemoveLooseObjectPtr("UNK10BOXES_OFFSET");
                    writer.RemoveLooseObjectPtr("UNK12BOXES_OFFSET");
                    writer.RemoveLooseObjectPtr("UNK14BOXES_OFFSET");
                    writer.RemoveLooseObjectPtr("EDGES_OFFSET");
                    writer.RemoveLooseObjectPtr("UNK18BOXES_OFFSET");

                    // solve end of set
                    writer.SolveLooseObjectPtr("SET_END_OFFSET_" + x);
                }
            }

            writer.SolveLooseObjectPtr("GRID_END");
            writer.Write(0);

            // write size of mesh
            uint SizeOfMesh = (uint)(writer.BaseStream.Position - MESH_START);
            writer.Write(SizeOfMesh);
        }
    }
}
