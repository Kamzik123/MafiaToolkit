using ResourceTypes.BufferPools;
using ResourceTypes.FrameResource;
using ResourceTypes.Materials;
using SharpDX;
using System;
using System.Collections.Generic;
using System.IO;
using Utils.Models;
using Utils.SharpDXExtensions;
using Utils.StringHelpers;
using Utils.Types;

namespace ResourceTypes.ModelHelpers.ModelExporter
{
    [Flags]
    public enum MT_ObjectFlags
    {
        HasLODs = 1,
        HasSkinning = 2,
        HasCollisions = 4,
    }

    public class MT_Object : IValidator
    {
        private const string FileHeader = "MTO";
        private const byte FileVersion = 3;

        public string ObjectName { get; set; }
        public MT_ObjectFlags ObjectFlags { get; set; }
        public Vector3 Position { get; set; }
        public Vector3 Rotation { get; set; }
        public Vector3 Scale { get; set; }
        public MT_Lod[] Lods { get; set; }
        public MT_Collision Collision { get; set; }
        public MT_Skeleton Skeleton { get; set; }


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

            // TODO - Possibly add an option where we can ask to export with local transform?
            Position = Vector3.Zero;
            Rotation = Vector3.Zero;
            Scale = Vector3.One;

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
                    //if(string.IsNullOrEmpty(FaceGroups[v].MaterialName))
                    //{
                        var Material = MaterialsManager.LookupMaterialByHash(FaceGroups[v].MaterialHash);
                        FaceGroups[v].MaterialName = Material.GetMaterialName();
                        FaceGroups[v].MaterialHash = Material.GetMaterialHash();

                        // Add texture (if applicable)
                        HashName DiffuseHashName = Material.GetTextureByID("S000");
                        if(DiffuseHashName != null)
                        {
                            MaterialInstanceObject.DiffuseTexture = DiffuseHashName.String;
                            MaterialInstanceObject.MaterialFlags |= MT_MaterialInstanceFlags.HasDiffuse;
                        }
                    //}

                    MaterialInstanceObject.Name = FaceGroups[v].MaterialName;
                    FaceGroupObject.StartIndex = (uint)FaceGroups[v].StartIndex;
                    FaceGroupObject.NumFaces = (uint)FaceGroups[v].NumFaces;
                    FaceGroupObject.Material = MaterialInstanceObject;
                    LodObject.FaceGroups[v] = FaceGroupObject;
                }

                Lods[i] = LodObject;
            }
        }

        public void BuildFromCooked(FrameObjectModel RiggedModel, VertexBuffer[] VBuffer, IndexBuffer[] IBuffer)
        {
            BuildFromCooked((FrameObjectSingleMesh)RiggedModel, VBuffer, IBuffer);

            MT_Skeleton ModelSkeleton = new MT_Skeleton();

            FrameBlendInfo BlendInfo = RiggedModel.GetBlendInfoObject();
            FrameSkeleton Skeleton = RiggedModel.GetSkeletonObject();
            FrameSkeletonHierachy SkeletonHierarchy = RiggedModel.GetSkeletonHierarchyObject();

            ModelSkeleton.Joints = new MT_Joint[Skeleton.BoneNames.Length];
            for(int i = 0; i < ModelSkeleton.Joints.Length; i++)
            {
                MT_Joint JointObject = new MT_Joint();
                JointObject.Name = Skeleton.BoneNames[i].ToString();
                JointObject.ParentJointIndex = SkeletonHierarchy.ParentIndices[i];
                JointObject.UsageFlags = Skeleton.BoneLODUsage[i];

                Vector3 Scale, Position;
                Quaternion Rotation;

                Matrix JointTransform = Skeleton.JointTransforms[i];
                JointTransform.Decompose(out Scale, out Rotation, out Position);
                JointObject.Position = Position;
                JointObject.Scale = Scale;
                JointObject.Rotation = Rotation;
                ModelSkeleton.Joints[i] = JointObject;
            }

            for (int i = 0; i < BlendInfo.BoneIndexInfos.Length; i++)
            {
                var indexInfos = BlendInfo.BoneIndexInfos[i];
                var lod = Lods[i];
                bool[] remapped = new bool[lod.Vertices.Length];
                for (int x = 0; x < indexInfos.NumMaterials; x++)
                {
                    var part = lod.FaceGroups[x];
                    byte offset = 0;
                    for (int s = 0; s < indexInfos.BonesSlot[x]; s++)
                    {
                        offset += indexInfos.BonesPerPool[s];
                    }

                    for (uint z = part.StartIndex; z < part.StartIndex + (part.NumFaces * 3); z++)
                    {
                        uint index = lod.Indices[z];
                        if (!remapped[index])
                        {
                            for (uint f = 0; f < indexInfos.NumWeightsPerVertex[x]; f++)
                            {
                                var previousBoneID = lod.Vertices[index].BoneIDs[f];
                                lod.Vertices[index].BoneIDs[f] = indexInfos.IDs[offset + previousBoneID];
                            }
                            remapped[index] = true;
                        }
                    }
                }
            }

            ObjectFlags |= MT_ObjectFlags.HasSkinning;
            this.Skeleton = ModelSkeleton;
        }

        /** IO Functions */
        public bool ReadFromFile(BinaryReader reader)
        {
            // Read Header, make sure it is valid before continuing.
            string FileHeader = StringHelpers.ReadStringBuffer(reader, 3);
            byte FileVersion = reader.ReadByte();
            bool bIsHeaderValid = IsHeaderValid(FileHeader, FileVersion);
            if (!bIsHeaderValid)
            {
                // Invalid header
                return false;
            }

            // Read Meta-Data
            ObjectName = StringHelpers.ReadString8(reader);
            ObjectFlags = (MT_ObjectFlags)reader.ReadInt32();

            // Read Object Transform
            Position = Vector3Extenders.ReadFromFile(reader);
            Rotation = Vector3Extenders.ReadFromFile(reader);
            Scale = Vector3Extenders.ReadFromFile(reader);

            if (ObjectFlags.HasFlag(MT_ObjectFlags.HasLODs))
            {
                uint NumLODs = reader.ReadUInt32();
                Lods = new MT_Lod[NumLODs];
                for (int i = 0; i < NumLODs; i++)
                {
                    MT_Lod NewLOD = new MT_Lod();
                    bool bIsValid = NewLOD.ReadFromFile(reader);
                    Lods[i] = NewLOD;

                    // Failed to read LOD, return.
                    if(!bIsValid)
                    {
                        return false;
                    }
                }
            }

            if (ObjectFlags.HasFlag(MT_ObjectFlags.HasCollisions))
            {
                Collision = new MT_Collision();
                bool bIsValid = Collision.ReadFromFile(reader);

                // Failed to read Collision, return.
                if (!bIsValid)
                {
                    return false;
                }
            }

            if(ObjectFlags.HasFlag(MT_ObjectFlags.HasSkinning))
            {
                Skeleton = new MT_Skeleton();
                bool bIsValid = Skeleton.ReadFromFile(reader);

                // Failed to read Skeleton, return.
                if (!bIsValid)
                {
                    return false;
                }
            }

            return true;
        }

        public void WriteToFile(BinaryWriter writer)
        {
            // Write Generic Header
            StringHelpers.WriteString(writer, "MTO", false);
            writer.Write((byte)3);

            // Write Meta-Data
            StringHelpers.WriteString8(writer, ObjectName);
            writer.Write((int)ObjectFlags);

            // Write Transform
            Position.WriteToFile(writer);
            Rotation.WriteToFile(writer);
            Scale.WriteToFile(writer);

            // Write LODs
            if (ObjectFlags.HasFlag(MT_ObjectFlags.HasLODs))
            {
                
                writer.Write(Lods.Length);

                for (int i = 0; i < Lods.Length; i++)
                {
                    Lods[i].WriteToFile(writer);
                }
            }

            // Write collisions (if applicable)
            if (ObjectFlags.HasFlag(MT_ObjectFlags.HasCollisions))
            {
                if (Collision != null)
                {
                    Collision.WriteToFile(writer);
                }
            }

            // Write Skeleton (if applicable)
            if(ObjectFlags.HasFlag(MT_ObjectFlags.HasSkinning))
            {
                if(Skeleton != null)
                {
                    Skeleton.WriteToFile(writer);
                }
            }
        }

        public bool Validate()
        {
            bool bValidity = false;
            bValidity = !string.IsNullOrEmpty(ObjectName);
            bValidity = ObjectFlags != 0;

            if (ObjectFlags.HasFlag(MT_ObjectFlags.HasLODs))
            {
                foreach (var LodObject in Lods)
                {
                    bValidity = LodObject.Validate();
                }
            }

            if (ObjectFlags.HasFlag(MT_ObjectFlags.HasCollisions))
            {
                bValidity = Collision.Validate();
            }

            return bValidity;
        }
    }
}
