using Utils.Models;
using System.Windows.Forms;
using ResourceTypes.ModelHelpers.ModelExporter;
using System;

namespace Forms.EditorControls
{
    public partial class FrameResourceModelOptions : Form
    {
        public FrameResourceModelOptions(ModelWrapper Wrapper)
        {
            InitializeComponent();

            MT_Object Model = Wrapper.ModelObject;
            TreeView_Objects.Nodes.Add(ConvertObjectToNode(Model));
        }

        public FrameResourceModelOptions(MT_ObjectBundle ObjectBundle)
        {
            InitializeComponent();

            TreeView_Objects.Nodes.Add(ConvertBundleToNode(ObjectBundle));
        }

        private TreeNode ConvertObjectToNode(MT_Object Object)
        {
            TreeNode Root = new TreeNode(Object.ObjectName);
            Root.Tag = Object;
            Root.ImageIndex = (Object.Validate() ? 1 : 0);
            Root.ImageIndex = 1;

            if (Object.ObjectFlags.HasFlag(MT_ObjectFlags.HasLODs))
            {
                for (int i = 0; i < Object.Lods.Length; i++)
                {
                    TreeNode LodNode = new TreeNode("LOD" + i);
                    LodNode.Tag = Object.Lods[i];
                    LodNode.ImageIndex = (Object.Lods[i].Validate() ? 1 : 0);
                    Root.Nodes.Add(LodNode);
                }
            }

            if (Object.ObjectFlags.HasFlag(MT_ObjectFlags.HasCollisions))
            {
                TreeNode SCollisionNode = new TreeNode("Static Collision");
                SCollisionNode.Tag = Object.Collision;
                SCollisionNode.ImageIndex = (Object.Collision.Validate() ? 1 : 0);
                Root.Nodes.Add(SCollisionNode);
            }

            return Root;
        }

        private TreeNode ConvertBundleToNode(MT_ObjectBundle Bundle)
        {
            TreeNode Root = new TreeNode("Bundle");
            Root.Tag = Bundle;

            foreach(MT_Object ObjectInfo in Bundle.Objects)
            {
                Root.Nodes.Add(ConvertObjectToNode(ObjectInfo));
            }

            return Root;
        }

        private void TreeView_OnAfterSelect(object sender, TreeViewEventArgs e)
        {
            e.Node.SelectedImageIndex = e.Node.ImageIndex;

            if (e.Node.Tag is MT_Lod)
            {
                MT_LodHelper LodHelper = new MT_LodHelper((MT_Lod)e.Node.Tag);
                LodHelper.Setup();
                PropertyGrid_Test.SelectedObject = LodHelper;
            }
            else if (e.Node.Tag is MT_Object)
            {
                MT_ObjectHelper ObjectHelper = new MT_ObjectHelper((MT_Object)e.Node.Tag);
                ObjectHelper.Setup();
                PropertyGrid_Test.SelectedObject = ObjectHelper;
            }
            else if(e.Node.Tag is MT_Collision)
            {
                MT_CollisionHelper ColHelper = new MT_CollisionHelper((MT_Collision)e.Node.Tag);
                ColHelper.Setup();
                PropertyGrid_Test.SelectedObject = ColHelper;
            }
        }

        private void TreeView_OnBeforeSelect(object sender, TreeViewCancelEventArgs e)
        {
            TreeNode Selected = TreeView_Objects.SelectedNode;

            if(Selected == null)
            {
                return;
            }

            if(Selected.Tag is MT_LodHelper)
            {
                MT_LodHelper LodHelper = (Selected.Tag as MT_LodHelper);
                LodHelper.Store();

                string Message = string.Format("{0} - {1}", DateTime.Now.ToLongTimeString(), "Updated Vertex Flags for LOD");
                Label_MessageText.Text = Message;
            }
            else if(Selected.Tag is MT_ObjectHelper)
            {
                MT_ObjectHelper ObjectHelper = (Selected.Tag as MT_ObjectHelper);
                ObjectHelper.Store();

                string Message = string.Format("{0} - Updated Object: {1}", DateTime.Now.ToLongTimeString(), ObjectHelper.ObjectName);
                Label_MessageText.Text = Message;
            }
            else if(Selected.Tag is MT_CollisionHelper)
            {
                MT_CollisionHelper CollisionHelper = (Selected.Tag as MT_CollisionHelper);
                CollisionHelper.Store();

                string Message = string.Format("{0} - {1}", DateTime.Now.ToLongTimeString(), "Updated COL.");
                Label_MessageText.Text = Message;
            }
        }
    }
}
