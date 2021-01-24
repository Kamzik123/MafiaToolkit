using System.IO;
using Utils.StringHelpers;
using Utils.Models;
using Utils.SharpDXExtensions;

namespace ResourceTypes.ModelHelpers.ModelExporter
{
    public static class MT_ObjectHandler
    {
        /** Begin Deserialize functions */
        public static MT_Object ReadObjectFromFile(string file)
        {
            using (BinaryReader reader = new BinaryReader(File.Open(file, FileMode.Open)))
            {
                MT_Object ModelObject = new MT_Object();

                // Read Header, make sure it is valid before continuing.
                string FileHeader = StringHelpers.ReadStringBuffer(reader, 3);
                byte FileVersion = reader.ReadByte();
                bool bIsHeaderValid = ModelObject.IsHeaderValid(FileHeader, FileVersion);
                if (!bIsHeaderValid)
                {
                    // Invalid header
                    return null;
                }

                // Read Meta-Data
                ModelObject.ObjectName = StringHelpers.ReadString8(reader);
                ModelObject.ObjectFlags = (MT_ObjectFlags)reader.ReadInt32();

                if (ModelObject.ObjectFlags.HasFlag(MT_ObjectFlags.HasLODs))
                {
                    uint NumLODs = reader.ReadUInt32();
                    ModelObject.Lods = new MT_Lod[NumLODs];
                    for (int i = 0; i < NumLODs; i++)
                    {
                        MT_Lod NewLOD = ReadLODFromFile(reader);
                        ModelObject.Lods[i] = NewLOD;
                    }
                }

                return ModelObject;
            }
        }

        private static MT_Lod ReadLODFromFile(BinaryReader reader)
        {
            MT_Lod NewObject = new MT_Lod();
            NewObject.VertexDeclaration = (VertexFlags)reader.ReadInt32();

            // Attempt to read Vertices
            uint NumVertices = reader.ReadUInt32();
            NewObject.Vertices = new Vertex[NumVertices];
            for(int i = 0; i < NumVertices; i++)
            {
                Vertex NewVertex = new Vertex();
                VertexFlags LocalDeclaration = NewObject.VertexDeclaration;

                // Read Vertex Information using LocalDeclaration
                #region ReadVertexInfo
                if (LocalDeclaration.HasFlag(VertexFlags.Position))
                {
                    NewVertex.Position = Vector3Extenders.ReadFromFile(reader);
                }

                if (LocalDeclaration.HasFlag(VertexFlags.Normals))
                {
                    NewVertex.Normal = Vector3Extenders.ReadFromFile(reader);
                }

                if (LocalDeclaration.HasFlag(VertexFlags.Tangent))
                {
                    NewVertex.Tangent = Vector3Extenders.ReadFromFile(reader);
                }

                if(LocalDeclaration.HasFlag(VertexFlags.Skin))
                {
                    NewVertex.BoneIDs = reader.ReadBytes(4);
                    NewVertex.BoneWeights = new float[4];
                    for(int z = 0; z < 4; z++)
                    {
                        NewVertex.BoneWeights[z] = reader.ReadSingle();
                    }
                }

                if(LocalDeclaration.HasFlag(VertexFlags.Color))
                {
                    NewVertex.Color0 = reader.ReadBytes(4);
                }

                if (LocalDeclaration.HasFlag(VertexFlags.Color1))
                {
                    NewVertex.Color1 = reader.ReadBytes(4);
                }

                if (LocalDeclaration.HasFlag(VertexFlags.TexCoords0))
                {
                    NewVertex.UVs[0] = Half2Extenders.ReadFromFile(reader);
                }
                if (LocalDeclaration.HasFlag(VertexFlags.TexCoords1))
                {
                    NewVertex.UVs[1] = Half2Extenders.ReadFromFile(reader);
                }
                if (LocalDeclaration.HasFlag(VertexFlags.TexCoords2))
                {
                    NewVertex.UVs[2] = Half2Extenders.ReadFromFile(reader);
                }
                if (LocalDeclaration.HasFlag(VertexFlags.ShadowTexture))
                {
                    NewVertex.UVs[3] = Half2Extenders.ReadFromFile(reader);
                }
                #endregion

                NewObject.Vertices[i] = NewVertex;
            }

            // Read FaceGroups
            uint NumFaceGroups = reader.ReadUInt32();
            NewObject.FaceGroups = new MT_FaceGroup[NumFaceGroups];
            for(int i = 0; i < NumFaceGroups; i++)
            {
                // Attempt to read FaceGroup
                MT_FaceGroup NewFaceGroup = new MT_FaceGroup();
                NewFaceGroup.StartIndex = reader.ReadUInt32();
                NewFaceGroup.NumFaces = reader.ReadUInt32();

                // Read FaceGroup Material
                MT_MaterialInstance NewMaterial = new MT_MaterialInstance();
                NewMaterial.MaterialFlags = (MT_MaterialInstanceFlags)reader.ReadInt32();
                NewMaterial.Name = StringHelpers.ReadString8(reader);

                if (NewMaterial.MaterialFlags.HasFlag(MT_MaterialInstanceFlags.HasDiffuse))
                {
                    NewMaterial.DiffuseTexture = StringHelpers.ReadString8(reader);
                }

                NewFaceGroup.Material = NewMaterial;
                NewObject.FaceGroups[i] = NewFaceGroup;
            }

            // Read Indices
            uint NumIndices = reader.ReadUInt32();
            NewObject.Indices = new uint[NumIndices];
            for(int i = 0; i < NumIndices; i++)
            {
                NewObject.Indices[i] = reader.ReadUInt32();
            }

            return NewObject;
        }

        /** End Deserialize functions */
        /** Begin Serialize Functions */

        public static void WriteObjectToFile(BinaryWriter writer, MT_Object ModelObject)
        {
            // Write Generic Header
            StringHelpers.WriteString(writer, "MTO", false);
            writer.Write((byte)3);

            // Write Meta-Data
            StringHelpers.WriteString8(writer, ModelObject.ObjectName);
            writer.Write((int)ModelObject.ObjectFlags);

            if(ModelObject.ObjectFlags.HasFlag(MT_ObjectFlags.HasLODs))
            {
                // Write LODs
                writer.Write(ModelObject.Lods.Length);

                for(int i = 0; i < ModelObject.Lods.Length; i++)
                {
                    WriteLODToFile(writer, ModelObject.Lods[i]);
                }
            }
        }

        private static void WriteLODToFile(BinaryWriter writer, MT_Lod LodObject)
        {
            writer.Write((int)LodObject.VertexDeclaration);

            // Attempt to write vertices
            writer.Write(LodObject.Vertices.Length);
            for(int i = 0; i < LodObject.Vertices.Length; i++)
            {
                Vertex Vert = LodObject.Vertices[i];
                VertexFlags LocalDeclaration = LodObject.VertexDeclaration;

                #region WriteVertexInfo
                if (LocalDeclaration.HasFlag(VertexFlags.Position))
                {
                    Vert.Position.WriteToFile(writer);
                }
                if (LocalDeclaration.HasFlag(VertexFlags.Normals))
                {
                    Vert.Normal.WriteToFile(writer);
                }
                if (LocalDeclaration.HasFlag(VertexFlags.Tangent))
                {
                    Vert.Tangent.WriteToFile(writer);
                }
                if (LocalDeclaration.HasFlag(VertexFlags.Skin))
                {
                    writer.Write(Vert.BoneIDs);
                    for (int z = 0; z < 4; z++)
                    {
                        writer.Write(Vert.BoneWeights[z]);
                    }
                }
                if (LocalDeclaration.HasFlag(VertexFlags.Color))
                {
                    writer.Write(Vert.Color0);
                }
                if (LocalDeclaration.HasFlag(VertexFlags.Color1))
                {
                    writer.Write(Vert.Color1);
                }
                if (LocalDeclaration.HasFlag(VertexFlags.TexCoords0))
                {
                    Vert.UVs[0].WriteToFile(writer);
                }
                if (LocalDeclaration.HasFlag(VertexFlags.TexCoords1))
                {
                    Vert.UVs[1].WriteToFile(writer);
                }
                if (LocalDeclaration.HasFlag(VertexFlags.TexCoords2))
                {
                    Vert.UVs[2].WriteToFile(writer);
                }
                if (LocalDeclaration.HasFlag(VertexFlags.ShadowTexture))
                {
                    Vert.UVs[3].WriteToFile(writer);
                }
                #endregion EndVertexInfo
            }

            writer.Write(LodObject.FaceGroups.Length);
            for(int i = 0; i < LodObject.FaceGroups.Length; i++)
            {
                // Write FaceGroup
                MT_FaceGroup FaceGroup = LodObject.FaceGroups[i];
                writer.Write(FaceGroup.StartIndex);
                writer.Write(FaceGroup.NumFaces);

                // Write Material Instance
                MT_MaterialInstance MaterialInstance = FaceGroup.Material;
                writer.Write((int)MaterialInstance.MaterialFlags);
                StringHelpers.WriteString8(writer, MaterialInstance.Name);

                if (MaterialInstance.MaterialFlags.HasFlag(MT_MaterialInstanceFlags.HasDiffuse))
                {
                    StringHelpers.WriteString8(writer, MaterialInstance.DiffuseTexture);
                }
            }

            writer.Write(LodObject.Indices.Length);
            for(int i = 0; i < LodObject.Indices.Length; i++)
            {
                writer.Write(LodObject.Indices[i]);
            }
        }
        /** End Serialize Functions */
    }
}
