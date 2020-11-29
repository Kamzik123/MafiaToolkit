using SharpDX;
using System;
using Utils.Models;

namespace ResourceTypes.ModelHelpers.ModelExporter
{
    [Flags]
    public enum MT_MaterialInstanceFlags
    {
        IsCollision = 1,
        HasDiffuse = 2
    }

    public class MT_MaterialInstance
    {
        public MT_MaterialInstanceFlags MaterialFlags { get; set; }
        public string Name { get; set; }
        public string DiffuseTexture { get; set; }

        public MT_MaterialInstance()
        {
            Name = "";
            DiffuseTexture = "";
        }
    }

    public class MT_FaceGroup
    {
        public BoundingBox Bounds { get; set; }
        public uint StartIndex { get; set; }
        public uint NumFaces { get; set; }
        public MT_MaterialInstance Material { get; set; }
    }

    public class MT_Lod
    {
        public VertexFlags VertexDeclaration { get; set; }
        public Vertex[] Vertices { get; set; }
        public uint[] Indices { get; set; }
        public MT_FaceGroup[] FaceGroups { get; set; }

        public bool Over16BitLimit()
        {
            return (Vertices.Length > ushort.MaxValue);
        }

    }

    public class MT_Collision
    {
        public Vector3[] Vertices { get; set; }
        public uint[] Indices { get; set; }
        public ushort[] MaterialAssignments { get; set; }
    }
}
