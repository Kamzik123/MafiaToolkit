using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ResourceTypes.Navigation;
using SharpDX;
using SharpDX.Direct3D11;
using Utils.StringHelpers;
using Utils.Types;

namespace Rendering.Graphics
{
    public class SelectEntry_RenderNav : SelectEntryParams
    {
        public int VertexIndex;

        public SelectEntry_RenderNav(int InId) : base(InId) { }
    }

    public class RenderNav : IRenderer
    {
        public OBJData.ConnectionStruct[] CurrentConnections { get; set; }

        public int RefID { private set; get; }

        private OBJData RawData;
        private TreeNode NavigationNode;

        private RenderLine[] Connections;
        private RenderBoundingBox VertexBBox;
        private RenderBoundingBox[] RelatedVertices;

        private bool bHasSelectionChanged;

        public void Init(OBJData NavData)
        {
            // generate refID
            RefID = StringHelpers.GetNewRefID();
            RawData = NavData; // cache parent object

            // create root node
            TreeNode RootNode = new TreeNode();
            RootNode.Text = "NAV_OBJ_DATA";
            RootNode.Tag = this;

            // create vertex node
            for (int i = 0; i < NavData.vertices.Length; i++)
            {
                TreeNode NavPoint = new TreeNode();
                NavPoint.Name = "OBJ_DATA_VERTEX";
                NavPoint.Text = "VERTEX " + i;
                NavPoint.Tag = "OBJ_DATA_VERTEX";
                RootNode.Nodes.Add(NavPoint);
            }

            DoRender = true;

            // create vertex bbox
            VertexBBox = new RenderBoundingBox();
            VertexBBox.Init(new BoundingBox(new Vector3(-0.1f), new Vector3(0.1f)));
            VertexBBox.DoRender = false;

            NavigationNode = RootNode;
        }

        public TreeNode GetTreeNodes()
        {
            return NavigationNode;
        }

        public override void InitBuffers(Device d3d, DeviceContext deviceContext)
        {
            if(VertexBBox != null)
            {
                VertexBBox.InitBuffers(d3d, deviceContext);
            }

            if(RelatedVertices != null)
            {
                foreach (RenderBoundingBox BBox in RelatedVertices)
                {
                    BBox.InitBuffers(d3d, deviceContext);
                }
            }

            if(Connections != null)
            {
                foreach(RenderLine Connection in Connections)
                {
                    Connection.InitBuffers(d3d, deviceContext);
                }
            }
        }

        public override void Render(Device device, DeviceContext deviceContext, Camera camera)
        {
            if (DoRender)
            {
                if (VertexBBox != null)
                {
                    VertexBBox.Render(device, deviceContext, camera);
                }

                if (RelatedVertices != null)
                {
                    foreach (RenderBoundingBox BBox in RelatedVertices)
                    {
                        BBox.Render(device, deviceContext, camera);
                    }
                }

                if (Connections != null)
                {
                    foreach (RenderLine Connection in Connections)
                    {
                        Connection.Render(device, deviceContext, camera);
                    }
                }
            }
        }

        public override void Select(SelectEntryParams SelectParams)
        {
            SelectEntry_RenderNav RenderNavParams = (SelectParams as SelectEntry_RenderNav);

            // get vertex
            OBJData.VertexStruct NextVertex = RawData.vertices[RenderNavParams.VertexIndex];

            // only need to update transform after 1st time
            if(VertexBBox != null)
            {
                VertexBBox.DoRender = true;
                VertexBBox.SetTransform(Matrix.Translation(NextVertex.Position));
            }

            if(RelatedVertices == null)
            {
                var OurRelatedVerts = RawData.GetRelatedFromVertex(RenderNavParams.VertexIndex);

                RelatedVertices = new RenderBoundingBox[3];
                for(int i = 0; i < OurRelatedVerts.Length; i++)
                {
                    RenderBoundingBox NewBBox = new RenderBoundingBox();
                    NewBBox.Init(new BoundingBox(new Vector3(-0.1f), new Vector3(0.1f)));
                    NewBBox.SetTransform(Matrix.Translation(NextVertex.Position));

                    RelatedVertices[i] = NewBBox;
                }
            }

            // always need to update connections
            if(Connections == null)
            {
                var OurConnections = RawData.GetConnectionsFromVertex(RenderNavParams.VertexIndex);
                Connections = new RenderLine[OurConnections.Length];
                for (int i = 0; i < OurConnections.Length; i++)
                {
                    RenderLine NewLine = new RenderLine();

                    Vector3 OurPosition = NextVertex.Position;
                    Vector3 NextPosition = RawData.vertices[OurConnections[i].ConnectedNodeID].Position;

                    NewLine.Init(new Vector3[2] { OurPosition, NextPosition });
                    Connections[i] = NewLine;
                }

                CurrentConnections = OurConnections;
            }

            bHasSelectionChanged = true;

        }

        public override void SetTransform(Matrix matrix)
        {
            Transform = matrix;
        }

        public override void Unselect()
        {
            if(Connections != null)
            {
                CurrentConnections = null;

                foreach (RenderLine Connection in Connections)
                {
                    Connection.Shutdown();
                }

                foreach(RenderBoundingBox RelatedBox in RelatedVertices)
                {
                    RelatedBox.Shutdown();
                }
            }

            RelatedVertices = null;
            Connections = null;
        }

        public override void UpdateBuffers(Device device, DeviceContext deviceContext)
        {
            // if selection has changed, we need to reconstruct connections
            if(bHasSelectionChanged)
            {
                if (VertexBBox != null)
                {
                    VertexBBox.UpdateBuffers(device, deviceContext);
                }

                if (Connections != null)
                {
                    foreach (RenderLine Connection in Connections)
                    {
                        Connection.InitBuffers(device, deviceContext);
                    }
                }

                if (RelatedVertices != null)
                {
                    foreach (RenderBoundingBox BBox in RelatedVertices)
                    {
                        BBox.InitBuffers(device, deviceContext);
                    }
                }

                bHasSelectionChanged = false;
            }

            // if there has been an update, do it now
            if (isUpdatedNeeded)
            {
                if (VertexBBox != null)
                {
                    VertexBBox.UpdateBuffers(device, deviceContext);
                }

                if (Connections != null)
                {
                    foreach (RenderLine Connection in Connections)
                    {
                        Connection.UpdateBuffers(device, deviceContext);
                    }
                }

                isUpdatedNeeded = false;
            }
        }

        public override void Shutdown()
        {
            if (VertexBBox != null)
            {
                VertexBBox.Shutdown();
            }

            if (Connections != null)
            {
                foreach (RenderLine Connection in Connections)
                {
                    Connection.Shutdown();
                }
            }

            if (RelatedVertices != null)
            {
                foreach (RenderBoundingBox BBox in RelatedVertices)
                {
                    BBox.Shutdown();
                }
            }

            RelatedVertices = null;
            VertexBBox = null;
            Connections = null;
        }
    }
}
