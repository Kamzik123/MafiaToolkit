﻿using SharpGLTF.Geometry;
using SharpGLTF.Geometry.VertexTypes;
using SharpGLTF.Materials;
using SharpGLTF.Scenes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using Utils.Models;
using Utils.Settings;
using Utils.StringHelpers;
using Utils.VorticeUtils;
using Vortice.Mathematics;

namespace ResourceTypes.ModelHelpers.ModelExporter
{
    using VERTEX = SharpGLTF.Geometry.VertexTypes.VertexPositionNormalTangent;
    using VERTEXS4 = VertexJoints4;
    using VERTEXRIDGEDBUILDER = VertexBuilder<VertexPositionNormalTangent, VertexTexture1, VertexEmpty>;
    using VERTEXSKINNEDBUILDER = VertexBuilder<VertexPositionNormalTangent, VertexTexture1, VertexJoints4>;
    using static Octokit.Caching.CachedResponse;

    public class MT_Lod : IValidator
    {
        public VertexFlags VertexDeclaration { get; set; }
        public Vertex[] Vertices { get; set; }
        public uint[] Indices { get; set; }
        public MT_FaceGroup[] FaceGroups { get; set; }

        public MeshBuilder<VERTEX> BuildGLTF()
        {
            MeshBuilder<VERTEX> LodMesh = new MeshBuilder<VERTEX>();

            foreach (MT_FaceGroup FaceGroup in FaceGroups)
            {
                var material1 = new MaterialBuilder(FaceGroup.Material.Name)
                    .WithDoubleSide(true)
                    .WithMetallicRoughnessShader()
                    .WithChannelParam(KnownChannel.BaseColor, KnownProperty.RGBA, new Vector4(1, 0, 0, 1));

                var CurFaceGroup = LodMesh.UsePrimitive(material1);

                uint StartIndex = FaceGroup.StartIndex;
                uint EndIndex = StartIndex + (FaceGroup.NumFaces * 3);

                for (uint Idx = StartIndex; Idx < EndIndex; Idx+=3)
                {
                    Vertex V1 = Vertices[Indices[Idx]];
                    Vertex V2 = Vertices[Indices[Idx + 1]];
                    Vertex V3 = Vertices[Indices[Idx + 2]];

                    CurFaceGroup.AddTriangle(BuildRidgedVertex(V1), BuildRidgedVertex(V2), BuildRidgedVertex(V3));
                }
            }

            return LodMesh;
        }

        public MeshBuilder<VERTEX, VertexTexture1, VertexJoints4> BuildSkinnedGLTF()
        {
            MeshBuilder<VERTEX, VertexTexture1, VertexJoints4> LodMesh = new MeshBuilder<VERTEX, VertexTexture1, VertexJoints4>();

            foreach (MT_FaceGroup FaceGroup in FaceGroups)
            {
                var material1 = new MaterialBuilder(FaceGroup.Material.Name)
                    .WithDoubleSide(true)
                    .WithMetallicRoughnessShader()
                    .WithChannelParam(KnownChannel.BaseColor, KnownProperty.RGBA, new Vector4(1, 0, 0, 1));

                var CurFaceGroup = LodMesh.UsePrimitive(material1);

                uint StartIndex = FaceGroup.StartIndex;
                uint EndIndex = StartIndex + (FaceGroup.NumFaces * 3);

                for (uint Idx = StartIndex; Idx < EndIndex; Idx += 3)
                {
                    Vertex V1 = Vertices[Indices[Idx]];
                    Vertex V2 = Vertices[Indices[Idx + 1]];
                    Vertex V3 = Vertices[Indices[Idx + 2]];

                    CurFaceGroup.AddTriangle(BuildSkinnedVertex(V1), BuildSkinnedVertex(V2), BuildSkinnedVertex(V3));
                }
            }

            return LodMesh;
        }

        private VERTEXRIDGEDBUILDER BuildRidgedVertex(Vertex GameVertex)
        {
            VERTEX VB1 = new VERTEX(GameVertex.Position, GameVertex.Normal, new Vector4(GameVertex.Tangent, 1.0f));

            VertexTexture1 TB1 = new VertexTexture1(GameVertex.UVs[0]);

            return new VERTEXRIDGEDBUILDER(VB1, TB1);
        }

        private VERTEXSKINNEDBUILDER BuildSkinnedVertex(Vertex GameVertex)
        {
            VERTEX VB1 = new VERTEX(GameVertex.Position, GameVertex.Normal, new Vector4(GameVertex.Tangent, 1.0f));

            VertexTexture1 TB1 = new VertexTexture1(GameVertex.UVs[0]);

            (int JointIndex, float Weight)[] VertexWeights = new (int JointIndex, float Weight)[4];
            VertexWeights[0] = (GameVertex.BoneIDs[0], GameVertex.BoneWeights[0]);
            VertexWeights[1] = (GameVertex.BoneIDs[1], GameVertex.BoneWeights[1]);
            VertexWeights[2] = (GameVertex.BoneIDs[2], GameVertex.BoneWeights[2]);
            VertexWeights[3] = (GameVertex.BoneIDs[3], GameVertex.BoneWeights[3]);

            return new VERTEXSKINNEDBUILDER(VB1, TB1, VertexWeights);
        }


        /** Utility Functions */
        public bool Over16BitLimit()
        {
            return (Vertices.Length > ushort.MaxValue);
        }

        public void CalculatePartBounds()
        {
            for (int i = 0; i < FaceGroups.Length; i++)
            {
                List<Vector3> partVerts = new List<Vector3>();
                for (int x = 0; x < Indices.Length; x++)
                {
                    partVerts.Add(Vertices[Indices[i]].Position);
                }

                FaceGroups[i].Bounds = BoundingBox.CreateFromPoints(partVerts.ToArray());
            }
        }

        protected override bool InternalValidate(MT_ValidationTracker TrackerObject)
        {
            bool bValidity = true;

            if(Vertices.Length == 0)
            {
               AddMessage(MT_MessageType.Error, "This LOD object has no vertices.");
                bValidity = false;
            }

            if (Indices.Length == 0)
            {
                AddMessage(MT_MessageType.Error, "This LOD object has no indices.");
                bValidity = false;
            }

            // specific game type check.
            // M2DE supports 32bit buffers, original game does not.
            if(GameStorage.IsGameType(GamesEnumerator.MafiaII))
            {
                if(Vertices.Length > ushort.MaxValue)
                {
                    AddMessage(MT_MessageType.Error,
                        "Vertex count is above {0}. Importing this model into Mafia II may prove to be unstable!", ushort.MaxValue);
                }
            }

            if (VertexDeclaration == 0)
            {
                AddMessage(MT_MessageType.Error, "This LOD object has no vertex elements.");
                bValidity = false;
            }

            if(FaceGroups.Length == 0)
            {
                AddMessage(MT_MessageType.Error, "This LOD object has no FaceGroups");
                bValidity = false;
            }

            foreach (var FaceGroup in FaceGroups)
            {
                bool bIsFaceGroupValid = FaceGroup.ValidateObject(TrackerObject);
                bValidity &= bIsFaceGroupValid;
            }

            return bValidity;
        }
    }
}
