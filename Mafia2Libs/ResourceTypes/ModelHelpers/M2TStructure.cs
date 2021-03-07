using System;
using System.Collections.Generic;
using System.IO;
using SharpDX;

namespace Utils.Models
{
    public class M2TStructure
    {
        //cannot change.
        private const string fileHeader = "M2T";
        private const byte fileVersion = 2;

        //main header data of the file.
        private string name; //name of model.
        private bool isSkinned;
        private Skeleton skeleton;
        Lod[] lods; //Holds the models which can be exported, all EDM content is saved here.

        private string aoTexture;
        
        public Lod[] Lods {
            get { return lods; }
            set { lods = value; }
        }
        public bool IsSkinned {
            get { return isSkinned; }
            set { isSkinned = value; }
        }
        public Skeleton SkeletonData {
            get { return skeleton; }
            set { skeleton = value; }
        }
        public string Name {
            get { return name; }
            set { name = value; }
        }
        public string AOTexture {
            get { return aoTexture; }
            set { aoTexture = value; }
        }

        public void FlipUVs()
        {
            for (int i = 0; i != lods.Length; i++)
            {
                for (int x = 0; x != lods[i].Vertices.Length; x++)
                {
                    Vertex vert = lods[i].Vertices[x];
                    if (Lods[i].VertexDeclaration.HasFlag(VertexFlags.TexCoords0))
                    {
                        vert.UVs[0].Y = (1f - vert.UVs[0].Y);
                    }

                    if (Lods[i].VertexDeclaration.HasFlag(VertexFlags.TexCoords1))
                    {
                        vert.UVs[1].Y = (1f - vert.UVs[1].Y);
                    }

                    if (Lods[i].VertexDeclaration.HasFlag(VertexFlags.TexCoords2))
                    {
                        vert.UVs[2].Y = (1f - vert.UVs[2].Y);
                    }

                    if (Lods[i].VertexDeclaration.HasFlag(VertexFlags.ShadowTexture))
                    {
                        vert.UVs[3].Y = (1f - vert.UVs[3].Y);
                    }
                }
            }
        }

        public void ReadFromM2T(BinaryReader reader)
        {
            if (new string(reader.ReadChars(3)) != fileHeader)
            {
                return;
            }

            var version = reader.ReadByte();
            if (version > 2)
            {
                return;
            }
        }

        public bool ReadFromFbx(string file)
        {
            string m2tFile = file.Remove(file.Length - 4, 4) + ".m2t";
            int result = FBXHelper.ConvertFBX(file, m2tFile);
            using (BinaryReader reader = new BinaryReader(File.Open(m2tFile, FileMode.Open)))
            {
                ReadFromM2T(reader);
            }
            if (File.Exists(m2tFile))
            {
                File.Delete(m2tFile);
            }
            return true;
        }

        public class Joint
        {
            string name;
            byte parentIndex;
            Joint parent;
            Matrix localTransform;
            Matrix worldTransform;

            public string Name {
                get { return name; }
                set { name = value; }
            }
            public Joint Parent {
                get { return parent; }
                set { parent = value; }
            }
            public byte ParentIndex {
                get { return parentIndex; }
                set { parentIndex = value; }
            }
            public Matrix LocalTransform {
                get { return localTransform; }
                set { localTransform = value; }
            }
            public Matrix WorldTransform {
                get { return worldTransform; }
                set { worldTransform = value; }
            }

            public Vector3 GetWorldPosition(Vector3 invertedLocal)
            {
                var transposed = GetRotationFromLocalTransform(true);
                var transformed = Vector3.Transform(invertedLocal, transposed);
                return new Vector3(transformed.X, transformed.Y, transformed.Z);
            }

            public void SetWorldPosition(Vector3 position)
            {
                worldTransform.TranslationVector = position;
            }

            private Matrix GetRotationFromLocalTransform(bool transpose)
            {
                Vector3 scale, position;
                Quaternion rotation;
                localTransform.Decompose(out scale, out rotation, out position);       
                
                var matrix = Matrix.RotationQuaternion(rotation);
                if (transpose)
                {
                    matrix.Transpose();
                }

                return matrix;
            }

            public void ComputeJointTransform()
            {
                var parentJoint = Parent;
                var invertedLocal = LocalTransform.TranslationVector;
                while (parentJoint != null)
                {
                    invertedLocal = parentJoint.GetWorldPosition(invertedLocal);
                    parentJoint = parentJoint.Parent;
                }
                if (Parent != null)
                {
                    Parent.ComputeJointTransform();
                    invertedLocal += Parent.GetWorldPosition(invertedLocal);
                }
                SetWorldPosition(invertedLocal);
            }
        }

        public class Skeleton
        {
            Joint[] joints;

            public Joint[] Joints {
                get { return joints; }
                set { joints = value; }
            }

            public void ComputeTransforms()
            {
                for(int i = 0; i < Joints.Length; i++)
                {
                    var currentJoint = joints[i];
                    currentJoint.ComputeJointTransform();
                }
            }

        }
        public class Lod
        {
            private VertexFlags vertexDeclaration;
            Vertex[] vertices;
            ModelPart[] parts;
            uint[] indices;

            public VertexFlags VertexDeclaration {
                get { return vertexDeclaration; }
                set { vertexDeclaration = value; }
            }

            public Vertex[] Vertices {
                get { return vertices; }
                set { vertices = value; }
            }
            public uint[] Indices {
                get { return indices; }
                set { indices = value; }
            }
            public ModelPart[] Parts {
                get { return parts; }
                set { parts = value; }
            }

            public Lod()
            {
                vertexDeclaration = 0;
            }


            public void CalculatePartBounds()
            {
                for(int i = 0; i != parts.Length; i++)
                {
                    List<Vector3> partVerts = new List<Vector3>();
                    for (int x = 0; x != indices.Length; x++)
                    {
                        partVerts.Add(vertices[indices[i]].Position);
                    }
                    BoundingBox bounds;
                    BoundingBox.FromPoints(partVerts.ToArray(), out bounds);
                    parts[i].Bounds = bounds;
                }
            }

            public int GetIndexFormat()
            {
                return (indices.Length * 3 > ushort.MaxValue ? 2 : 1);
            }

            public bool Over16BitLimit()
            {
                return (vertices.Length > ushort.MaxValue);
            }

            public void CalculateNormals()
            {
                List<Vector3> surfaceNormals = new List<Vector3>();
                Vector3[] normals = new Vector3[vertices.Length];

                for(int i = 0; i < parts.Length; i++)
                {
                    var normal = new Vector3();

                    var index = parts[i].StartIndex;
                    while(index < parts[i].StartIndex+parts[i].NumFaces*3)
                    {
                        var edge1 = vertices[indices[index]].Position - vertices[indices[index + 1]].Position;
                        var edge2 = vertices[indices[index]].Position - vertices[indices[index + 2]].Position;
                        normal = Vector3.Cross(edge1, edge2);
                        normals[indices[index]] += normal;
                        normals[indices[index+1]] += normal;
                        normals[indices[index+2]] += normal;
                        surfaceNormals.Add(normal);
                        index += 3;
                    }
                    surfaceNormals.Add(normal);
                }

                for(int i = 0; i < vertices.Length; i++)
                {
                    normals[i].Normalize();
                    vertices[i].Normal = normals[i];
                }
            }
        }

        public class ModelPart
        {
            string material;
            ulong hash;
            uint startIndex;
            uint numFaces;
            BoundingBox bounds;

            public string Material {
                get { return material; }
                set {material = value; }
            }

            public ulong Hash {
                get { return hash; }
                set { hash = value; }
            }

            public BoundingBox Bounds {
                get { return bounds; }
                set { bounds = value; }
            }

            public uint StartIndex {
                get { return startIndex; }
                set { startIndex = value; }
            }

            public uint NumFaces {
                get { return numFaces; }
                set { numFaces = value; }
            }
        }
    }
}
