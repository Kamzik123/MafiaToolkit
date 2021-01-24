using System;
using System.Collections.Generic;
using SharpDX;
using Gibbed.Illusion.FileFormats.Hashing;
using ResourceTypes.FrameResource;
using Utils.SharpDXExtensions;
using ResourceTypes.BufferPools;
using Utils.Types;
using ResourceTypes.ModelHelpers.ModelExporter;
using System.IO;
using Utils.Settings;
using System.Windows.Forms;

namespace Utils.Models
{
    public class ModelWrapper
    {
        
        FrameObjectSingleMesh frameMesh; //model can be either "FrameObjectSingleMesh"
        FrameObjectModel frameModel; //Or "FrameObjectModel"
        IndexBuffer[] indexBuffers; //Holds the buffer which will then be saved/replaced later
        VertexBuffer[] vertexBuffers; //Holds the buffers which will then be saved/replaced later
        MT_Object modelObject;
        private bool useSingleMesh; //False means ModelMesh, True means SingleMesh;

        public FrameObjectSingleMesh FrameMesh {
            get { return frameMesh; }
        }

        public IndexBuffer[] IndexBuffers {
            get { return indexBuffers; }
            set { indexBuffers = value; }
        }

        public VertexBuffer[] VertexBuffers {
            get { return vertexBuffers; }
            set { vertexBuffers = value; }
        }

        public MT_Object ModelObject {
            get { return modelObject; }
            set { modelObject = value; }
        }

        public ModelWrapper(FrameObjectSingleMesh frameMesh, IndexBuffer[] indexBuffers, VertexBuffer[] vertexBuffers)
        {
            this.frameMesh = frameMesh;
            this.indexBuffers = indexBuffers;
            this.vertexBuffers = vertexBuffers;
            frameGeometry = frameMesh.Geometry;
            frameMaterial = frameMesh.Material;
            modelObject = new MT_Object();
            modelObject.ObjectName = frameMesh.Name.ToString();
            //model.AOTexture = frameMesh.OMTextureHash.String; // missing support
            modelObject.BuildFromCooked(frameMesh, vertexBuffers, indexBuffers);
        }

        public ModelWrapper(FrameObjectModel frameModel, IndexBuffer[] indexBuffers, VertexBuffer[] vertexBuffers)
        {
            this.frameModel = frameModel;
            this.indexBuffers = indexBuffers;
            this.vertexBuffers = vertexBuffers;
            frameGeometry = frameModel.Geometry;
            frameMaterial = frameModel.Material;
            blendInfo = frameModel.BlendInfo;
            skeleton = frameModel.Skeleton;
            skeletonHierarchy = frameModel.SkeletonHierarchy;
            modelObject = new MT_Object();
            modelObject.ObjectName = frameMesh.Name.ToString();
            //model.AOTexture = frameMesh.OMTextureHash.String; // missing support
            modelObject.BuildFromCooked(frameMesh, vertexBuffers, indexBuffers);
        }

        /// <summary>
        /// Construct an empty model.
        /// </summary>
        public ModelWrapper()
        {
            ModelObject = new MT_Object();
        }

        public void SetFrameMesh(FrameObjectSingleMesh Mesh)
        {
            frameMesh = Mesh;
        }

        /// <summary>
        /// Update decompression offset and position.
        /// </summary>
        public void CalculateDecompression()
        {
            FrameGeometry frameGeometry = frameMesh.Geometry;

            float minFloatf = 0.000016f;
            Vector3 minFloat = new Vector3(minFloatf);

            BoundingBox bounds = new BoundingBox();
            bounds.Minimum = frameMesh.Boundings.Minimum - minFloat;
            bounds.Maximum = frameMesh.Boundings.Maximum + minFloat;
            frameGeometry.DecompressionOffset = new Vector3(bounds.Minimum.X, bounds.Minimum.Y, bounds.Minimum.Z);

            double MaxX = bounds.Maximum.X - bounds.Minimum.X + minFloatf;
            double MaxY = bounds.Maximum.Y - bounds.Minimum.Y + minFloatf;
            double MaxZ = bounds.Maximum.Z - bounds.Minimum.Z + minFloatf;

            double fMaxSize = Math.Max(MaxX, Math.Max(MaxY, MaxZ * 2.0f));
            Console.WriteLine("Decompress value before: " + fMaxSize);
            double result = Math.Log(fMaxSize) / Math.Log(2.0f);
            double pow = Math.Ceiling(result);
            double factor = Math.Pow(2.0f, pow);
            frameGeometry.DecompressionFactor = (float)(factor / 0x10000);

            Console.WriteLine("Using decompression value from: " + fMaxSize + " result is: " + frameGeometry.DecompressionFactor);
        }

        public void BuildIndexBuffer()
        {
            if (ModelObject.Lods == null)
            {
                return;
            }

            for (int i = 0; i < ModelObject.Lods.Length; i++)
            {
                MT_Lod LodObject = ModelObject.Lods[i];
                var indexFormat = (LodObject.Over16BitLimit() ? 2 : 1);
                IndexBuffers[i] = new IndexBuffer(FNV64.Hash("M2TK." + ModelObject.ObjectName + ".IB" + i));
                indexBuffers[i].SetData(LodObject.Indices);
                indexBuffers[i].SetFormat(indexFormat);
            }
        }

        /// <summary>
        /// Builds vertex buffer from the mesh data.
        /// </summary>
        public void BuildVertexBuffer()
        {
            FrameGeometry frameGeometry = frameMesh.Geometry;

            if (model.Lods == null)
            {
                return;
            }

            for (int i = 0; i != ModelObject.Lods.Length; i++)
            {
                FrameLOD frameLod = frameGeometry.LOD[i];
                int vertexSize;
                Dictionary<VertexFlags, FrameLOD.VertexOffset> vertexOffsets = frameLod.GetVertexOffsets(out vertexSize);
                byte[] vBuffer = new byte[vertexSize * frameLod.NumVerts];

                for (int v = 0; v != ModelObject.Lods[i].Vertices.Length; v++)
                {
                    Vertex vert = ModelObject.Lods[i].Vertices[v];

                    if (frameLod.VertexDeclaration.HasFlag(VertexFlags.Position))
                    {
                        int startIndex = v * vertexSize + vertexOffsets[VertexFlags.Position].Offset;
                        vert.WritePositionData(vBuffer, startIndex, frameGeometry.DecompressionFactor, frameGeometry.DecompressionOffset);
                    }

                    if (frameLod.VertexDeclaration.HasFlag(VertexFlags.Tangent))
                    {
                        int startIndex = v * vertexSize + vertexOffsets[VertexFlags.Position].Offset;
                        vert.WriteTangentData(vBuffer, startIndex);
                    }

                    if (frameLod.VertexDeclaration.HasFlag(VertexFlags.Normals))
                    {
                        int startIndex = v * vertexSize + vertexOffsets[VertexFlags.Normals].Offset;
                        vert.WriteNormalData(vBuffer, startIndex);
                    }

                    if(frameLod.VertexDeclaration.HasFlag(VertexFlags.Color))
                    {
                        int startIndex = v * vertexSize + vertexOffsets[VertexFlags.Color].Offset;
                        vert.WriteColourData(vBuffer, startIndex, 0);
                    }

                    if (frameLod.VertexDeclaration.HasFlag(VertexFlags.Color1))
                    {
                        int startIndex = v * vertexSize + vertexOffsets[VertexFlags.Color1].Offset;
                        vert.WriteColourData(vBuffer, startIndex, 1);
                    }

                    if (frameLod.VertexDeclaration.HasFlag(VertexFlags.DamageGroup))
                    {
                        int startIndex = v * vertexSize + vertexOffsets[VertexFlags.DamageGroup].Offset;
                        vert.WriteDamageGroup(vBuffer, startIndex);
                    }

                    if (frameLod.VertexDeclaration.HasFlag(VertexFlags.TexCoords0))
                    {
                        int startIndex = v * vertexSize + vertexOffsets[VertexFlags.TexCoords0].Offset;
                        vert.WriteUvData(vBuffer, startIndex, 0);
                    }

                    if (frameLod.VertexDeclaration.HasFlag(VertexFlags.TexCoords1))
                    {
                        int startIndex = v * vertexSize + vertexOffsets[VertexFlags.TexCoords1].Offset;
                        vert.WriteUvData(vBuffer, startIndex, 1);
                    }

                    if (frameLod.VertexDeclaration.HasFlag(VertexFlags.TexCoords2))
                    {
                        int startIndex = v * vertexSize + vertexOffsets[VertexFlags.TexCoords2].Offset;
                        vert.WriteUvData(vBuffer, startIndex, 2);
                    }

                    if (frameLod.VertexDeclaration.HasFlag(VertexFlags.ShadowTexture))
                    {
                        int startIndex = v * vertexSize + vertexOffsets[VertexFlags.ShadowTexture].Offset;
                        vert.WriteUvData(vBuffer, startIndex, 3);
                    }

                }

                VertexBuffers[i] = new VertexBuffer(FNV64.Hash("M2TK." + ModelObject.ObjectName + ".VB" + i));
                VertexBuffers[i].Data = vBuffer;
            }
        }

        public void ReadObjectFromFbx(string file)
        {
            // Change extension, pass to M2FBX
            string m2tFile = file.Remove(file.Length - 4, 4) + ".m2t";
            int result = FBXHelper.ConvertFBX(file, m2tFile);

            // Read the MT object.
            ModelObject = MT_ObjectHandler.ReadObjectFromFile(file);

            // Delete the recently-created MT file.
            if (File.Exists(m2tFile))
            {
                File.Delete(m2tFile);
            }
        }

        public void ReadObjectFromM2T(string file)
        {
            ModelObject = MT_ObjectHandler.ReadObjectFromFile(file);
        }

        public void ExportObject()
        {
            string SavePath = ToolkitSettings.ExportPath + "\\" + ModelObject.ObjectName;

            
            if (!Directory.Exists(ToolkitSettings.ExportPath))
            {
                // Ask if we can create it
                DialogResult Result = MessageBox.Show("The path does not exist. Do you want to create it?", "Toolkit", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
                if (Result == DialogResult.Yes)
                {
                    Directory.CreateDirectory(SavePath);
                }
                else
                {
                    // Can't export file with no valid directory.
                    MessageBox.Show("Cannot export a mesh with no valid directory. Please change your directory.", "Toolkit", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }

            // Export Object
            string FileName = ToolkitSettings.ExportPath + "\\" + ModelObject.ObjectName;
            ExportObjectToM2T(FileName);
            switch (ToolkitSettings.Format)
            {
                case 0:
                    ExportObjectToFbx(FileName, false);
                    break;
                case 1:
                    ExportObjectToFbx(FileName, true);
                    break;
                default:
                    break;
            }
        }

        private void ExportObjectToFbx(string File, bool bIsBinary)
        {
            FBXHelper.ConvertM2T(File + ".m2t", File + ".fbx");
        }

        private void ExportObjectToM2T(string FileToWrite)
        {
            using (BinaryWriter writer = new BinaryWriter(File.Open(FileToWrite + ".mto", FileMode.Create)))
            {
                MT_ObjectHandler.WriteObjectToFile(writer, ModelObject);
            }        
        }

        public void UpdateObjectsFromModel()
        {
            FrameGeometry frameGeometry = frameMesh.Geometry;
            FrameMaterial frameMaterial = frameMesh.Material;

            frameGeometry.NumLods = (byte)model.Lods.Length;

            if (frameGeometry.LOD == null)
            {
                frameGeometry.LOD = new FrameLOD[model.Lods.Length];
            }

            frameMaterial.NumLods = (byte)ModelObject.Lods.Length;
            frameMaterial.LodMatCount = new int[ModelObject.Lods.Length];
            frameMaterial.Materials = new List<MaterialStruct[]>();

            for (int x = 0; x < ModelObject.Lods.Length; x++)
            {
                frameMaterial.Materials.Add(new MaterialStruct[frameMaterial.LodMatCount[x]]);
            }
            for (int x = 0; x < ModelObject.Lods.Length; x++)
            {
                MT_Lod LodObject = ModelObject.Lods[x];

                var lod = new FrameLOD();
                lod.Distance = 1E+12f;
                lod.BuildNewPartition();
                lod.BuildNewMaterialSplit();
                lod.SplitInfo.NumVerts = LodObject.Vertices.Length;
                lod.NumVerts = LodObject.Vertices.Length;
                lod.SplitInfo.NumFaces = LodObject.Indices.Length / 3;
                lod.VertexDeclaration = LodObject.VertexDeclaration;

                //burst split info.
                lod.SplitInfo.IndexStride = (LodObject.Over16BitLimit() ? 4 : 2);
                lod.SplitInfo.NumMatSplit = LodObject.FaceGroups.Length;
                lod.SplitInfo.NumMatBurst = LodObject.FaceGroups.Length;
                lod.SplitInfo.MaterialSplits = new FrameLOD.MaterialSplit[LodObject.FaceGroups.Length];
                lod.SplitInfo.MaterialBursts = new FrameLOD.MaterialBurst[LodObject.FaceGroups.Length];
                frameGeometry.LOD[x] = lod;

                int faceIndex = 0;
                frameMaterial.LodMatCount[x] = LodObject.FaceGroups.Length;
                frameMaterial.Materials[x] = new MaterialStruct[LodObject.FaceGroups.Length];
                for (int i = 0; i < LodObject.FaceGroups.Length; i++)
                {
                    frameMaterial.Materials[x][i] = new MaterialStruct();
                    frameMaterial.Materials[x][i].StartIndex = (int)LodObject.FaceGroups[x].StartIndex;
                    frameMaterial.Materials[x][i].NumFaces = (int)LodObject.FaceGroups[x].NumFaces;
                    frameMaterial.Materials[x][i].Unk3 = 0;
                    frameMaterial.Materials[x][i].MaterialHash = FNV64.Hash(LodObject.FaceGroups[x].Material.Name);
                    //frameMaterial.Materials[0][i].MaterialName = model.Lods[0].Parts[i].Material;
                    faceIndex += (int)LodObject.FaceGroups[x].NumFaces;

                    frameGeometry.LOD[x].SplitInfo.MaterialBursts[i].Bounds = new short[6]
                    {
                        Convert.ToInt16(LodObject.FaceGroups[x].Bounds.Minimum.X),
                        Convert.ToInt16(LodObject.FaceGroups[x].Bounds.Minimum.Y),
                        Convert.ToInt16(LodObject.FaceGroups[x].Bounds.Minimum.Z),
                        Convert.ToInt16(LodObject.FaceGroups[x].Bounds.Maximum.X),
                        Convert.ToInt16(LodObject.FaceGroups[x].Bounds.Maximum.Y),
                        Convert.ToInt16(LodObject.FaceGroups[x].Bounds.Maximum.Z)
                    };

                    if (ModelObject.Lods[x].FaceGroups.Length == 1)
                    {
                        string MaterialName = ModelObject.Lods[0].FaceGroups[0].Material.Name;
                        frameGeometry.LOD[x].SplitInfo.Hash = FNV64.Hash(MaterialName);
                    }

                    frameGeometry.LOD[x].SplitInfo.MaterialBursts[i].FirstIndex = 0;                  
                    frameGeometry.LOD[x].SplitInfo.MaterialBursts[i].LeftIndex = -1;
                    frameGeometry.LOD[x].SplitInfo.MaterialBursts[i].RightIndex = -1;
                    frameGeometry.LOD[x].SplitInfo.MaterialBursts[i].SecondIndex =
                        Convert.ToUInt16(LodObject.FaceGroups[x].NumFaces - 1);
                    frameGeometry.LOD[x].SplitInfo.MaterialSplits[i].BaseIndex = (int)LodObject.FaceGroups[x].StartIndex;
                    frameGeometry.LOD[x].SplitInfo.MaterialSplits[i].FirstBurst = i;
                    frameGeometry.LOD[x].SplitInfo.MaterialSplits[i].NumBurst = 1;
                }
            }
        }

        /// <summary>
        /// Create objects from model. Requires FrameMesh/FrameModel to be already set and a model already read into the data.
        /// </summary>
        public void CreateObjectsFromModel()
        {
            FrameGeometry frameGeometry = frameMesh.Geometry;
            FrameMaterial frameMaterial = frameMesh.Material;

            //set lods for all data.
            indexBuffers = new IndexBuffer[ModelObject.Lods.Length];
            vertexBuffers = new VertexBuffer[ModelObject.Lods.Length];

            List<Vertex[]> vertData = new List<Vertex[]>();
            for (int i = 0; i != ModelObject.Lods.Length; i++)
            {
                vertData.Add(ModelObject.Lods[i].Vertices);
            }

            frameMesh.Boundings = BoundingBoxExtenders.CalculateBounds(vertData);
            frameMaterial.Bounds = frameMesh.Boundings;
            CalculateDecompression();
            UpdateObjectsFromModel();
            BuildIndexBuffer();
            BuildVertexBuffer();

            for(int i = 0; i < ModelObject.Lods.Length; i++)
            {
                var lod = frameGeometry.LOD[i];

                var size = 0;
                lod.GetVertexOffsets(out size);
                if (vertexBuffers[i].Data.Length != (size * lod.NumVerts)) throw new SystemException();
                lod.IndexBufferRef = new HashName("M2TK." + ModelObject.ObjectName + ".IB" + i);
                lod.VertexBufferRef = new HashName("M2TK." + ModelObject.ObjectName + ".VB" + i);
            }

            /*
            if(model.IsSkinned)
            {
                CreateSkinnedObjectsFromModel();
            }*/
        }

        public void CreateSkinnedObjectsFromModel()
        {
            FrameSkeleton skeleton = (frameMesh as FrameObjectModel).Skeleton;
            FrameSkeletonHierachy skeletonHierarchy = (frameMesh as FrameObjectModel).SkeletonHierarchy;

            int jointCount = model.SkeletonData.Joints.Length;
            skeleton.BoneNames = new HashName[jointCount];
            skeleton.NumBones = new int[4];
            skeleton.UnkLodData = new int[1];
            skeleton.BoneLODUsage = new byte[jointCount];

            skeleton.NumBlendIDs = jointCount;
            skeleton.NumUnkCount2 = jointCount;
            skeleton.UnkLodData[0] = jointCount;


            for (int i = 0; i < 4; i++)
            {
                skeleton.NumBones[i] = jointCount;
            }

            for (int i = 0; i < jointCount; i++)
            {
                HashName bone = new HashName();
                bone.Set(model.SkeletonData.Joints[i].Name);
                skeleton.BoneNames[i] = bone;

                if (model.Lods.Length == 1)
                {
                    skeleton.BoneLODUsage[i] = 1;
                }
            }

            skeletonHierarchy.ParentIndices = new byte[jointCount];
            skeletonHierarchy.LastChildIndices = new byte[jointCount];
            skeletonHierarchy.UnkData = new byte[jointCount];
            skeleton.JointTransforms = new Matrix[jointCount];

            skeletonHierarchy.UnkData[0] = (byte)(jointCount + 1);

            for (int i = 0; i < jointCount; i++)
            {
                skeletonHierarchy.ParentIndices[i] = model.SkeletonData.Joints[i].ParentIndex;
                skeletonHierarchy.UnkData[i] = (byte)(i != jointCount ? i : 0);
                skeleton.JointTransforms[i] = model.SkeletonData.Joints[i].LocalTransform;
            }*/
        }
    }
}
