﻿using SharpDX;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using System;
using Utils.Types;
using static ResourceTypes.Collisions.Collision;
using Buffer = SharpDX.Direct3D11.Buffer;

namespace Rendering.Graphics
{
    public class RenderStaticCollision : IRenderer
    {
        public RenderBoundingBox BoundingBox { get; set; }
        public VertexLayouts.BasicLayout.Vertex[] Vertices { get; private set; }
        public uint[] Indices { get; private set; }
        public BaseShader Shader;
        private uint numTriangles;

        public RenderStaticCollision()
        {
            DoRender = true;
            Transform = Matrix.Identity;
            BoundingBox = new RenderBoundingBox();
        }

        public override void InitBuffers(Device d3d)
        {
            vertexBuffer = Buffer.Create(d3d, BindFlags.VertexBuffer, Vertices);
            indexBuffer = Buffer.Create(d3d, BindFlags.IndexBuffer, Indices);
            Shader = RenderStorageSingleton.Instance.ShaderManager.shaders[1];
            BoundingBox.InitBuffers(d3d);
        }

        public void ConvertCollisionToRender(Placement placement, MeshData data)
        {
            //todo
            SetTransform(placement.Position, new Matrix33());
            DoRender = true;
            BoundingBox = new RenderBoundingBox();
            BoundingBox.Init(data.BoundingBox);

            Indices = data.Indices;
            numTriangles = Convert.ToUInt32(data.Indices.Length * 3);
            Vertices = new VertexLayouts.BasicLayout.Vertex[data.Vertices.Length];

            for (int i = 0; i != data.Vertices.Length; i++)
            {
                VertexLayouts.BasicLayout.Vertex vertex = new VertexLayouts.BasicLayout.Vertex();
                vertex.Position = data.Vertices[i];
                vertex.Colour = new Vector3(1.0f, 1.0f, 1.0f);
                Vertices[i] = vertex;
            }
        }

        public override void Render(Device device, DeviceContext deviceContext, Camera camera, LightClass light)
        {
            if (!DoRender)
                return;

            deviceContext.InputAssembler.SetVertexBuffers(0, new VertexBufferBinding(vertexBuffer, Utilities.SizeOf<VertexLayouts.BasicLayout.Vertex>(), 0));
            deviceContext.InputAssembler.SetIndexBuffer(indexBuffer, SharpDX.DXGI.Format.R32_UInt, 0);
            deviceContext.InputAssembler.PrimitiveTopology = PrimitiveTopology.TriangleList;
            Shader.SetSceneVariables(deviceContext, Transform, camera, light);
            Shader.Render(deviceContext, numTriangles, 0);
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
        }

        public override void SetTransform(Matrix matrix)
        {
            Transform = matrix;
        }

        public override void Shutdown()
        {
            Indices = null;
            Vertices = null;
            BoundingBox.Shutdown();
            BoundingBox.Shutdown();
            vertexBuffer?.Dispose();
            vertexBuffer = null;
            indexBuffer?.Dispose();
            indexBuffer = null;
        }
    }
}
