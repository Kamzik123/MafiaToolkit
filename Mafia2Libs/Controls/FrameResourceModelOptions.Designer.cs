namespace Forms.EditorControls
{
    partial class FrameResourceModelOptions
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FrameResourceModelOptions));
            this.ImportAOBox = new System.Windows.Forms.CheckBox();
            this.SplitContainer_Root = new System.Windows.Forms.SplitContainer();
            this.TreeView_Objects = new Utils.Extensions.MTreeView();
            this.imageList1 = new System.Windows.Forms.ImageList(this.components);
            this.PropertyGrid_Test = new System.Windows.Forms.PropertyGrid();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.Label_MessageText = new System.Windows.Forms.ToolStripStatusLabel();
            ((System.ComponentModel.ISupportInitialize)(this.SplitContainer_Root)).BeginInit();
            this.SplitContainer_Root.Panel1.SuspendLayout();
            this.SplitContainer_Root.Panel2.SuspendLayout();
            this.SplitContainer_Root.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.statusStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // ImportAOBox
            // 
            this.ImportAOBox.AutoSize = true;
            this.ImportAOBox.Location = new System.Drawing.Point(19, 147);
            this.ImportAOBox.Name = "ImportAOBox";
            this.ImportAOBox.Size = new System.Drawing.Size(95, 17);
            this.ImportAOBox.TabIndex = 9;
            this.ImportAOBox.Text = "$IMPORT_AO";
            this.ImportAOBox.UseVisualStyleBackColor = true;
            // 
            // SplitContainer_Root
            // 
            this.SplitContainer_Root.Dock = System.Windows.Forms.DockStyle.Fill;
            this.SplitContainer_Root.Location = new System.Drawing.Point(0, 0);
            this.SplitContainer_Root.Name = "SplitContainer_Root";
            // 
            // SplitContainer_Root.Panel1
            // 
            this.SplitContainer_Root.Panel1.Controls.Add(this.TreeView_Objects);
            // 
            // SplitContainer_Root.Panel2
            // 
            this.SplitContainer_Root.Panel2.Controls.Add(this.PropertyGrid_Test);
            this.SplitContainer_Root.Size = new System.Drawing.Size(624, 411);
            this.SplitContainer_Root.SplitterDistance = 208;
            this.SplitContainer_Root.TabIndex = 16;
            // 
            // TreeView_Objects
            // 
            this.TreeView_Objects.Dock = System.Windows.Forms.DockStyle.Fill;
            this.TreeView_Objects.ImageIndex = 0;
            this.TreeView_Objects.ImageList = this.imageList1;
            this.TreeView_Objects.Location = new System.Drawing.Point(0, 0);
            this.TreeView_Objects.Name = "TreeView_Objects";
            this.TreeView_Objects.SelectedImageIndex = 0;
            this.TreeView_Objects.Size = new System.Drawing.Size(208, 411);
            this.TreeView_Objects.TabIndex = 0;
            this.TreeView_Objects.BeforeSelect += new System.Windows.Forms.TreeViewCancelEventHandler(this.TreeView_OnBeforeSelect);
            this.TreeView_Objects.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.TreeView_OnAfterSelect);
            // 
            // imageList1
            // 
            this.imageList1.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList1.ImageStream")));
            this.imageList1.TransparentColor = System.Drawing.Color.Transparent;
            this.imageList1.Images.SetKeyName(0, "Cross.png");
            this.imageList1.Images.SetKeyName(1, "Tick.png");
            // 
            // PropertyGrid_Test
            // 
            this.PropertyGrid_Test.Dock = System.Windows.Forms.DockStyle.Fill;
            this.PropertyGrid_Test.Location = new System.Drawing.Point(0, 0);
            this.PropertyGrid_Test.Name = "PropertyGrid_Test";
            this.PropertyGrid_Test.Size = new System.Drawing.Size(412, 411);
            this.PropertyGrid_Test.TabIndex = 16;
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Name = "splitContainer1";
            this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.SplitContainer_Root);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.statusStrip1);
            this.splitContainer1.Size = new System.Drawing.Size(624, 441);
            this.splitContainer1.SplitterDistance = 411;
            this.splitContainer1.TabIndex = 17;
            // 
            // statusStrip1
            // 
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.Label_MessageText});
            this.statusStrip1.Location = new System.Drawing.Point(0, 4);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(624, 22);
            this.statusStrip1.TabIndex = 0;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // Label_MessageText
            // 
            this.Label_MessageText.Name = "Label_MessageText";
            this.Label_MessageText.Size = new System.Drawing.Size(88, 17);
            this.Label_MessageText.Text = "MESSAGE_TEXT";
            // 
            // FrameResourceModelOptions
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.ClientSize = new System.Drawing.Size(624, 441);
            this.Controls.Add(this.splitContainer1);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FrameResourceModelOptions";
            this.Text = "$MODEL_OPTIONS_TITLE";
            this.SplitContainer_Root.Panel1.ResumeLayout(false);
            this.SplitContainer_Root.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.SplitContainer_Root)).EndInit();
            this.SplitContainer_Root.ResumeLayout(false);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            this.splitContainer1.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.CheckBox ImportAOBox;
        private System.Windows.Forms.SplitContainer SplitContainer_Root;
        private Utils.Extensions.MTreeView TreeView_Objects;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel Label_MessageText;
        private System.Windows.Forms.PropertyGrid PropertyGrid_Test;
        private System.Windows.Forms.ImageList imageList1;
    }
}