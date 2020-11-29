using ResourceTypes.BufferPools;
using ResourceTypes.FrameResource;
using ResourceTypes.Materials;
using System;
using System.Collections.Generic;
using Utils.Models;

namespace ResourceTypes.ModelHelpers.ModelExporter
{
    [Flags]
    public enum MT_ObjectFlags
    {
        HasLODs = 1,
        HasSkinning = 2,
        HasCollisions = 4,
    }

    public class MT_Object
    {
        private const string FileHeader = "MTO";
        private const byte FileVersion = 3;

        public string ObjectName { get; set; }
        public MT_ObjectFlags ObjectFlags { get; set; }
        public MT_Lod[] Lods { get; set; }
        public MT_Collision Collision { get; set; }

        /** Validation Functions */
        public bool IsHeaderValid(string InHeader, byte InVersion)
        {
            // Try and validate header
            if(!InHeader.Equals(FileHeader))
            {
                return false;
            }

            // Try and validate version
            if(InVersion != FileVersion)
            {
                return false;
            }

            return true;
        }

        /** Construction Functions */
        public void BuildFromCooked(FrameObjectSingleMesh SingleMesh, VertexBuffer[] VBuffer, IndexBuffer[] IBuffer)
        {
            FrameGeometry GeometryInfo = SingleMesh.Geometry;
            FrameMaterial MaterialInfo = SingleMesh.Material;
            ObjectFlags |= MT_ObjectFlags.HasLODs;

            Lods = new MT_Lod[GeometryInfo.LOD.Length];
            for(int i = 0; i < Lods.Length; i++)
            {
                // Setup and Collect Lod Info and buffers
                FrameLOD LodInfo = GeometryInfo.LOD[i];
                MT_Lod LodObject = new MT_Lod();

                LodObject.VertexDeclaration = LodInfo.VertexDeclaration;
                IndexBuffer CurrentIBuffer = IBuffer[i];
                VertexBuffer CurrentVBuffer = VBuffer[i];

                // Get Vertex sizes and declaration
                int VertexSize = 0;
                Dictionary<VertexFlags, FrameLOD.VertexOffset> vertexOffsets = LodInfo.GetVertexOffsets(out VertexSize);
                LodObject.Vertices = new Vertex[LodInfo.NumVerts];

                if (VertexSize * LodInfo.NumVerts != CurrentVBuffer.Data.Length)
                {
                    Console.WriteLine("BIG ERROR");
                }

                // Build Vertex Array
                for (int v = 0; v < LodObject.Vertices.Length; v++)
                {
                    //declare data required and send to decompresser
                    byte[] data = new byte[LodObject.Vertices.Length];
                    Array.Copy(CurrentVBuffer.Data, (v * VertexSize), data, 0, VertexSize);
                    LodObject.Vertices[v] = VertexTranslator.DecompressVertex(data, LodInfo.VertexDeclaration, GeometryInfo.DecompressionOffset, GeometryInfo.DecompressionFactor, vertexOffsets);
                }

                // Build Indices and FaceGroups Array
                LodObject.Indices = CurrentIBuffer.GetData();
                MaterialStruct[] FaceGroups = MaterialInfo.Materials[i];
                LodObject.FaceGroups = new MT_FaceGroup[FaceGroups.Length];
                for(int v = 0; v < LodObject.FaceGroups.Length; v++)
                {
                    MT_FaceGroup FaceGroupObject = new MT_FaceGroup();
                    MT_MaterialInstance MaterialInstanceObject = new MT_MaterialInstance();

                    // TODO: Might be better to just keep this permanently.
                    if(string.IsNullOrEmpty(FaceGroups[v].MaterialName))
                    {
                        var Material = MaterialsManager.LookupMaterialByHash(FaceGroups[v].MaterialHash);
                        FaceGroups[v].MaterialName = Material.GetMaterialName();
                        FaceGroups[v].MaterialHash = Material.GetMaterialHash();
                    }

                    MaterialInstanceObject.Name = FaceGroups[v].MaterialName;
                    FaceGroupObject.StartIndex = (uint)FaceGroups[v].StartIndex;
                    FaceGroupObject.NumFaces = (uint)FaceGroups[v].NumFaces;
                    FaceGroupObject.Material = MaterialInstanceObject;
                    LodObject.FaceGroups[v] = FaceGroupObject;
                }

                Lods[i] = LodObject;
            }

        }
    }
}
