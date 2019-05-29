﻿using SharpDX;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using System.Collections.Generic;
using ResourceTypes.FrameResource;
using ResourceTypes.Materials;
using ResourceTypes.BufferPools;
using Utils.Types;
using System;
using Buffer = SharpDX.Direct3D11.Buffer;
using System.IO;
using Utils.Settings;
using Utils.Models;
using Mafia2Tool;

namespace Rendering.Graphics
{
    public class RenderModel : IRenderer
    {
        public struct ModelPart
        {
            public Material Material;
            public ulong MaterialHash;
            public ShaderResourceView Texture;
            public uint StartIndex;
            public uint NumFaces;
            public BaseShader Shader;
        }

        public ShaderResourceView AOTexture { get; set; }
        public RenderBoundingBox BoundingBox { get; set; }

        public struct LOD
        {
            public ModelPart[] ModelParts { get; set; }
            public VertexLayouts.NormalLayout.Vertex[] Vertices { get; set; }
            public ushort[] Indices { get; set; }
        }

        public LOD[] LODs { get; private set; }

        public RenderModel()
        {
            DoRender = true;
            isUpdatedNeeded = false;
            Transform = Matrix.Identity;
            BoundingBox = new RenderBoundingBox();
        }

        public bool ConvertFrameToRenderModel(FrameObjectSingleMesh mesh, FrameGeometry geom, FrameMaterial mats, IndexBuffer[] indexBuffers, VertexBuffer[] vertexBuffers)
        {
            if (mesh == null || geom == null || mats == null || indexBuffers == null || vertexBuffers == null)
                return false;

            SetTransform(mesh.Matrix.Position, mesh.Matrix.Rotation);
            //DoRender = (mesh.SecondaryFlags == 4097 ? true : false);
            BoundingBox = new RenderBoundingBox();
            BoundingBox.Init(mesh.Boundings);
            BoundingBox.SetTransform(Transform);
            BoundingBox.DoRender = false;
            LODs = new LOD[geom.NumLods];

            for(int i = 0; i != geom.NumLods; i++)
            {
                LOD lod = new LOD();
                lod.Indices = indexBuffers[i].Data;
                lod.ModelParts = new ModelPart[mats.LodMatCount[i]];

                for (int z = 0; z != mats.Materials[i].Length; z++)
                {
                    lod.ModelParts[z] = new ModelPart();
                    lod.ModelParts[z].NumFaces = (uint)mats.Materials[i][z].NumFaces;
                    lod.ModelParts[z].StartIndex = (uint)mats.Materials[i][z].StartIndex;
                    lod.ModelParts[z].MaterialHash = mats.Materials[i][z].MaterialHash;
                    lod.ModelParts[z].Material = MaterialsManager.LookupMaterialByHash(lod.ModelParts[z].MaterialHash);
                }

                lod.Vertices = new VertexLayouts.NormalLayout.Vertex[geom.LOD[i].NumVertsPr];
                int vertexSize;
                Dictionary<VertexFlags, FrameLOD.VertexOffset> vertexOffsets = geom.LOD[i].GetVertexOffsets(out vertexSize);

                for (int x = 0; x != lod.Vertices.Length; x++)
                {
                    VertexLayouts.NormalLayout.Vertex vertex = new VertexLayouts.NormalLayout.Vertex();

                    if (geom.LOD[i].VertexDeclaration.HasFlag(VertexFlags.Position))
                    {
                        int startIndex = x * vertexSize + vertexOffsets[VertexFlags.Position].Offset;
                        vertex.Position = VertexTranslator.ReadPositionDataFromVB(vertexBuffers[i].Data, startIndex, geom.DecompressionFactor, geom.DecompressionOffset);
                    }

                    if (geom.LOD[i].VertexDeclaration.HasFlag(VertexFlags.Tangent))
                    {
                        //int startIndex = x * vertexSize + vertexOffsets[VertexFlags.Position].Offset;
                       // vertex.Tangent = VertexTranslator.ReadTangentDataFromVB(vertexBuffers[i].Data, startIndex);
                    }

                    if (geom.LOD[i].VertexDeclaration.HasFlag(VertexFlags.Normals))
                    {
                        int startIndex = x * vertexSize + vertexOffsets[VertexFlags.Normals].Offset;
                        vertex.Normal = VertexTranslator.ReadNormalDataFromVB(vertexBuffers[i].Data, startIndex);
                    }

                    if (geom.LOD[i].VertexDeclaration.HasFlag(VertexFlags.Skin))
                    {
                        //int startIndex = v * vertexSize + vertexOffsets[VertexFlags.BlendData].Offset;
                       // vertex.BlendWeight = VertexTranslator.ReadBlendWeightFromVB(vertexBuffer.Data, startIndex);
                       // vertex.BoneID = VertexTranslator.ReadBlendIDFromVB(vertexBuffer.Data, startIndex);
                    }

                    if (geom.LOD[i].VertexDeclaration.HasFlag(VertexFlags.Color))
                    {
                        //unknown
                    }

                    if (geom.LOD[i].VertexDeclaration.HasFlag(VertexFlags.TexCoords0))
                    {
                        int startIndex = x * vertexSize + vertexOffsets[VertexFlags.TexCoords0].Offset;
                        vertex.TexCoord0 = VertexTranslator.ReadTexcoordFromVB(vertexBuffers[i].Data, startIndex);
                    }

                    if (geom.LOD[i].VertexDeclaration.HasFlag(VertexFlags.TexCoords1))
                    {
                        //int startIndex = v * vertexSize + vertexOffsets[VertexFlags.TexCoords1].Offset;
                        //vertex.UVs[1] = VertexTranslator.ReadTexcoordFromVB(vertexBuffer.Data, startIndex);
                    }

                    if (geom.LOD[i].VertexDeclaration.HasFlag(VertexFlags.TexCoords2))
                    {
                        //int startIndex = v * vertexSize + vertexOffsets[VertexFlags.TexCoords2].Offset;
                        //vertex.UVs[2] = VertexTranslator.ReadTexcoordFromVB(vertexBuffer.Data, startIndex);
                    }

                    if (geom.LOD[i].VertexDeclaration.HasFlag(VertexFlags.ShadowTexture))
                    {
                        int startIndex = x * vertexSize + vertexOffsets[VertexFlags.ShadowTexture].Offset;
                        vertex.TexCoord7 = VertexTranslator.ReadTexcoordFromVB(vertexBuffers[i].Data, startIndex);
                    }

                    if (geom.LOD[i].VertexDeclaration.HasFlag(VertexFlags.Color1))
                    {
                        //unknown
                    }

                    if (geom.LOD[i].VertexDeclaration.HasFlag(VertexFlags.BBCoeffs))
                    {
                        //unknown
                    }

                    if (geom.LOD[i].VertexDeclaration.HasFlag(VertexFlags.DamageGroup))
                    {
                        //int startIndex = v * vertexSize + vertexOffsets[VertexFlags.DamageGroup].Offset;
                        //vertex.DamageGroup = VertexTranslator.ReadDamageGroupFromVB(vertexBuffer.Data, startIndex);
                    }

                    lod.Vertices[x] = vertex;
                }
                LODs[i] = lod;
            }

            SetupShaders();
            return true;
        }

        public void UpdateMaterials(FrameMaterial mats)
        {
            for (int i = 0; i != LODs.Length; i++)
            {
                for (int z = 0; z != LODs[i].ModelParts.Length; z++)
                {
                    ulong hash = mats.Materials[i][z].MaterialHash;
                    LODs[i].ModelParts[z].MaterialHash = hash;
                    LODs[i].ModelParts[z].Material = MaterialsManager.LookupMaterialByHash(hash);
                }
            }
            isUpdatedNeeded = true;
        }

        private bool InitializePartShaders(Device device)
        {
            //TextureLoader AOTextureClass = new TextureLoader();
            //ShaderResourceView m_Temp;

            //RenderStorageSingleton.Instance.TextureCache.TryGetValue(0, out m_Temp);
            //AOTexture = m_Temp;

            //if (m_Temp == null)
            //{
            //    AOTextureClass.Init(device, "texture.dds");
            //    RenderStorageSingleton.Instance.TextureCache.Add(0, AOTextureClass.TextureResource);
            //    AOTexture = AOTextureClass.TextureResource;
            //}
            return true;
        }
        private void SetupShaders()
        {
            for (int x = 0; x != LODs[0].ModelParts.Length; x++)
            {
                ModelPart part = LODs[0].ModelParts[x];
                if (part.Material == null)
                    part.Shader = RenderStorageSingleton.Instance.ShaderManager.shaders[0];
                else
                {
                    //Debug.WriteLine(LODs[0].ModelParts[x].Material.MaterialName + "\t" + LODs[0].ModelParts[x].Material.ShaderHash);
                    part.Shader = (RenderStorageSingleton.Instance.ShaderManager.shaders.ContainsKey(LODs[0].ModelParts[x].Material.ShaderHash)
                        ? RenderStorageSingleton.Instance.ShaderManager.shaders[LODs[0].ModelParts[x].Material.ShaderHash]
                        : RenderStorageSingleton.Instance.ShaderManager.shaders[0]);
                }
                LODs[0].ModelParts[x] = part;
            }
        }

        private void InitTextures(Device d3d)
        {
            AOTexture = RenderStorageSingleton.Instance.TextureCache[0];
            for(int i = 0; i < LODs.Length; i++)
            {
                for(int x = 0; x < LODs[i].ModelParts.Length; x++)
                {
                    ModelPart part = LODs[i].ModelParts[x];
                    
                    if(part.Material != null)
                    {
                        ShaderParameterSampler sampler;
                        if (part.Material.Samplers.TryGetValue("S000", out sampler))
                        {

                            ShaderResourceView texture;

                            if (!RenderStorageSingleton.Instance.TextureCache.TryGetValue(sampler.TextureHash, out texture))
                            {
                                if (!string.IsNullOrEmpty(sampler.File))
                                {
                                    texture = TextureLoader.LoadTexture(d3d, Path.Combine(ToolkitSettings.TexturePath, sampler.File));
                                    RenderStorageSingleton.Instance.TextureCache.Add(sampler.TextureHash, texture);
                                }
                            }

                            part.Texture = texture;
                        }

                        if (part.Material.Samplers.TryGetValue("S011", out sampler))
                        {

                            ShaderResourceView texture;

                            if (!RenderStorageSingleton.Instance.TextureCache.TryGetValue(sampler.TextureHash, out texture))
                            {
                                if (!string.IsNullOrEmpty(sampler.File))
                                {
                                    texture = TextureLoader.LoadTexture(d3d, Path.Combine(SceneData.ScenePath, sampler.File));
                                    RenderStorageSingleton.Instance.TextureCache.Add(sampler.TextureHash, texture);
                                }
                            }

                            part.Texture = texture;
                        }
                    }
                    else
                    {
                        //blank white
                        part.Texture = RenderStorageSingleton.Instance.TextureCache[0];
                    }
                }
            }
        }

        public override void InitBuffers(Device d3d)
        {
            vertexBuffer = Buffer.Create(d3d, BindFlags.VertexBuffer, LODs[0].Vertices);
            indexBuffer = Buffer.Create(d3d, BindFlags.IndexBuffer, LODs[0].Indices);

            BoundingBox.InitBuffers(d3d);
            InitTextures(d3d);
        }

        public override void SetTransform(Vector3 position, Matrix33 rotation)
        {
            Matrix m_trans = Matrix.Identity;
            m_trans[0, 0] = rotation.M00;
            m_trans[0, 1] = rotation.M01;
            m_trans[0, 2] = rotation.M02;
            m_trans[1, 0] = rotation.M10;
            m_trans[1, 1] = rotation.M11;
            m_trans[1, 2] = rotation.M12;
            m_trans[2, 0] = rotation.M20;
            m_trans[2, 1] = rotation.M21;
            m_trans[2, 2] = rotation.M22;
            m_trans[3, 0] = position.X;
            m_trans[3, 1] = position.Y;
            m_trans[3, 2] = position.Z;
            Transform = m_trans;
            BoundingBox.SetTransform(m_trans);
        }

        public override void SetTransform(Matrix matrix)
        {
            Transform = matrix;
            BoundingBox.SetTransform(matrix);
        }

        public override void Render(Device device, DeviceContext deviceContext, Camera camera, LightClass light)
        {
            if (!DoRender)
                return;

            //if (camera.CheckBBoxFrustrum(BBox))
            //    return;

            deviceContext.InputAssembler.SetVertexBuffers(0, new VertexBufferBinding(vertexBuffer, Utilities.SizeOf<VertexLayouts.NormalLayout.Vertex>(), 0));
            deviceContext.InputAssembler.SetIndexBuffer(indexBuffer, SharpDX.DXGI.Format.R16_UInt, 0);
            deviceContext.InputAssembler.PrimitiveTopology = PrimitiveTopology.TriangleList;

            for (int i = 0; i != LODs[0].ModelParts.Length; i++)
            {
                LODs[0].ModelParts[i].Shader.SetShaderParamters(device, deviceContext, LODs[0].ModelParts[i].Material);
                LODs[0].ModelParts[i].Shader.SetSceneVariables(deviceContext, Transform, camera);
                LODs[0].ModelParts[i].Shader.Render(deviceContext, PrimitiveTopology.TriangleList, (int)(LODs[0].ModelParts[i].NumFaces * 3), LODs[0].ModelParts[i].StartIndex);
            }
            BoundingBox.Render(device, deviceContext, camera, light);
        }

        public override void Shutdown()
        {
            LODs[0].Vertices = null;
            LODs[0].Indices = null;
            BoundingBox.Shutdown();
            for (int x = 0; x != LODs[0].ModelParts.Length; x++)
            {
                LODs[0].ModelParts[x].Texture?.Dispose();
                LODs[0].ModelParts[x].Texture = null;
            }
            AOTexture?.Dispose();
            AOTexture = null;
            BoundingBox.Shutdown();
            vertexBuffer?.Dispose();
            vertexBuffer = null;
            indexBuffer?.Dispose();
            indexBuffer = null;
        }

        public override void UpdateBuffers(Device device, DeviceContext deviceContext)
        {
            BoundingBox.UpdateBuffers(device, deviceContext);

            if(isUpdatedNeeded)
            {
                SetupShaders();
                InitTextures(device);
                isUpdatedNeeded = false;
            }
        }

        public override void Select()
        {
            BoundingBox.Select();
            BoundingBox.DoRender = true;
        }

        public override void Unselect()
        {
            BoundingBox.Unselect();
            BoundingBox.DoRender = false;
        }
    }
}