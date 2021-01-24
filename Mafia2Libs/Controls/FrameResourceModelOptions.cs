using Utils.Language;
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
            Localise();

            MT_Object Model = Wrapper.ModelObject;
            TreeView_Objects.Nodes.Add(ConvertObjectToNode(Model));
        }

        private void Localise()
        {
            Label_MessageText.Text = "";
            Text = Language.GetString("$MODEL_OPTIONS_TITLE");
            ModelOptionsText.Text = Language.GetString("$MODEL_OPTIONS_TEXT");
            ImportNormalBox.Text = Language.GetString("$IMPORT_NORMAL");
            ImportTangentBox.Text = Language.GetString("$IMPORT_TANGENT");
            ImportDiffuseBox.Text = Language.GetString("$IMPORT_DIFFUSE");
            ImportUV1Box.Text = Language.GetString("$IMPORT_UV1");
            ImportUV2Box.Text = Language.GetString("$IMPORT_UV2");
            ImportAOBox.Text = Language.GetString("$IMPORT_AO");
            ImportColor0Box.Text = Language.GetString("$IMPORT_COLOR0");
            ImportColor1Box.Text = Language.GetString("$IMPORT_COLOR1");
            FlipUVBox.Text = Language.GetString("$FLIP_UV");
        }

        private void Init(VertexFlags flags, int i)
        {
            string text = string.Format("{0} LOD: {1}", Language.GetString("$MODEL_OPTIONS_TEXT"), i.ToString());
            ModelOptionsText.Text = text;

            ImportNormalBox.Enabled = ImportNormalBox.Checked = flags.HasFlag(VertexFlags.Normals);
            ImportTangentBox.Enabled = ImportTangentBox.Checked = flags.HasFlag(VertexFlags.Tangent);
            ImportDiffuseBox.Enabled = ImportDiffuseBox.Checked = flags.HasFlag(VertexFlags.TexCoords0);
            ImportUV1Box.Enabled = ImportUV1Box.Checked = flags.HasFlag(VertexFlags.TexCoords1);
            ImportUV2Box.Enabled = ImportUV2Box.Checked = flags.HasFlag(VertexFlags.TexCoords2);
            ImportAOBox.Enabled = ImportAOBox.Checked = flags.HasFlag(VertexFlags.ShadowTexture);
            ImportColor0Box.Enabled = ImportColor0Box.Checked = flags.HasFlag(VertexFlags.Color);
            ImportColor1Box.Enabled = ImportColor1Box.Checked = flags.HasFlag(VertexFlags.Color1);
            FlipUVBox.Enabled = false;
        }

        private TreeNode ConvertObjectToNode(MT_Object Object)
        {
            TreeNode Root = new TreeNode(Object.ObjectName);
            Root.Tag = Object;

            if (Object.ObjectFlags.HasFlag(MT_ObjectFlags.HasLODs))
            {
                for (int i = 0; i < Object.Lods.Length; i++)
                {
                    TreeNode LodNode = new TreeNode("LOD" + i);
                    LodNode.Tag = Object.Lods[i];
                    Root.Nodes.Add(LodNode);
                }
            }

            if (Object.ObjectFlags.HasFlag(MT_ObjectFlags.HasCollisions))
            {
                TreeNode SCollisionNode = new TreeNode("Static Collision");
                SCollisionNode.Tag = SCollisionNode;
                Root.Nodes.Add(SCollisionNode);
            }

            return Root;
        }

        private void TreeView_OnAfterSelect(object sender, TreeViewEventArgs e)
        {
            Init(0, 0);

            if (e.Node.Tag is MT_Lod)
            {
                MT_Lod LodObject = (e.Node.Tag as MT_Lod);
                Init(LodObject.VertexDeclaration, 0);
            }
        }

        private void TreeView_OnBeforeSelect(object sender, TreeViewCancelEventArgs e)
        {
            TreeNode Selected = TreeView_Objects.SelectedNode;

            if(Selected == null)
            {
                return;
            }

            if(Selected.Tag is MT_Lod)
            {
                MT_Lod LodObject = (Selected.Tag as MT_Lod);

                // Update the declaration
                VertexFlags NewDeclaration = 0;
                NewDeclaration = VertexFlags.Position;
                NewDeclaration |= (ImportNormalBox.Checked ? VertexFlags.Normals : 0);
                NewDeclaration |= (ImportTangentBox.Checked ? VertexFlags.Tangent : 0);
                NewDeclaration |= (ImportDiffuseBox.Checked ? VertexFlags.TexCoords0 : 0);
                NewDeclaration |= (ImportUV1Box.Checked ? VertexFlags.TexCoords1 : 0);
                NewDeclaration |= (ImportUV2Box.Checked ? VertexFlags.TexCoords2 : 0);
                NewDeclaration |= (ImportAOBox.Checked ? VertexFlags.ShadowTexture : 0);
                NewDeclaration |= (ImportColor0Box.Checked ? VertexFlags.Color : 0);
                NewDeclaration |= (ImportColor1Box.Checked? VertexFlags.Color1 : 0);
                LodObject.VertexDeclaration = NewDeclaration;

                string Message = string.Format("{0} - {1}", DateTime.Now.ToLongTimeString(), "Updated Vertex Flags for LOD");
                Label_MessageText.Text = Message;
            }
        }
    }
}
