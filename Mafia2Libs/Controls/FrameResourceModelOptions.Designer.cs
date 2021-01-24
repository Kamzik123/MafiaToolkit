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
            this.ModelOptionsText = new System.Windows.Forms.Label();
            this.ImportNormalBox = new System.Windows.Forms.CheckBox();
            this.ImportUV1Box = new System.Windows.Forms.CheckBox();
            this.ImportUV2Box = new System.Windows.Forms.CheckBox();
            this.ImportAOBox = new System.Windows.Forms.CheckBox();
            this.FlipUVBox = new System.Windows.Forms.CheckBox();
            this.ImportDiffuseBox = new System.Windows.Forms.CheckBox();
            this.ImportTangentBox = new System.Windows.Forms.CheckBox();
            this.ImportColor1Box = new System.Windows.Forms.CheckBox();
            this.ImportColor0Box = new System.Windows.Forms.CheckBox();
            this.Label_BufferType = new System.Windows.Forms.Label();
            this.SplitContainer_Root = new System.Windows.Forms.SplitContainer();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.TreeView_Objects = new Utils.Extensions.MTreeView();
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
            // ModelOptionsText
            // 
            this.ModelOptionsText.Location = new System.Drawing.Point(17, 20);
            this.ModelOptionsText.Name = "ModelOptionsText";
            this.ModelOptionsText.Size = new System.Drawing.Size(290, 13);
            this.ModelOptionsText.TabIndex = 4;
            this.ModelOptionsText.Text = "$MODEL_OPTIONS_TEXT";
            // 
            // ImportNormalBox
            // 
            this.ImportNormalBox.AutoSize = true;
            this.ImportNormalBox.Location = new System.Drawing.Point(19, 47);
            this.ImportNormalBox.Name = "ImportNormalBox";
            this.ImportNormalBox.Size = new System.Drawing.Size(133, 17);
            this.ImportNormalBox.TabIndex = 6;
            this.ImportNormalBox.Text = "$IMPORT_NORMALS";
            this.ImportNormalBox.UseVisualStyleBackColor = true;
            // 
            // ImportUV1Box
            // 
            this.ImportUV1Box.AutoSize = true;
            this.ImportUV1Box.Location = new System.Drawing.Point(19, 107);
            this.ImportUV1Box.Name = "ImportUV1Box";
            this.ImportUV1Box.Size = new System.Drawing.Size(101, 17);
            this.ImportUV1Box.TabIndex = 7;
            this.ImportUV1Box.Text = "$IMPORT_UV1";
            this.ImportUV1Box.UseVisualStyleBackColor = true;
            // 
            // ImportUV2Box
            // 
            this.ImportUV2Box.AutoSize = true;
            this.ImportUV2Box.Location = new System.Drawing.Point(19, 127);
            this.ImportUV2Box.Name = "ImportUV2Box";
            this.ImportUV2Box.Size = new System.Drawing.Size(101, 17);
            this.ImportUV2Box.TabIndex = 8;
            this.ImportUV2Box.Text = "$IMPORT_UV2";
            this.ImportUV2Box.UseVisualStyleBackColor = true;
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
            // FlipUVBox
            // 
            this.FlipUVBox.AutoSize = true;
            this.FlipUVBox.Location = new System.Drawing.Point(184, 87);
            this.FlipUVBox.Name = "FlipUVBox";
            this.FlipUVBox.Size = new System.Drawing.Size(75, 17);
            this.FlipUVBox.TabIndex = 10;
            this.FlipUVBox.Text = "$FLIP_UV";
            this.FlipUVBox.UseVisualStyleBackColor = true;
            // 
            // ImportDiffuseBox
            // 
            this.ImportDiffuseBox.AutoSize = true;
            this.ImportDiffuseBox.Location = new System.Drawing.Point(19, 87);
            this.ImportDiffuseBox.Name = "ImportDiffuseBox";
            this.ImportDiffuseBox.Size = new System.Drawing.Size(125, 17);
            this.ImportDiffuseBox.TabIndex = 11;
            this.ImportDiffuseBox.Text = "$IMPORT_DIFFUSE";
            this.ImportDiffuseBox.UseVisualStyleBackColor = true;
            // 
            // ImportTangentBox
            // 
            this.ImportTangentBox.AutoSize = true;
            this.ImportTangentBox.Enabled = false;
            this.ImportTangentBox.Location = new System.Drawing.Point(19, 67);
            this.ImportTangentBox.Name = "ImportTangentBox";
            this.ImportTangentBox.Size = new System.Drawing.Size(139, 17);
            this.ImportTangentBox.TabIndex = 12;
            this.ImportTangentBox.Text = "$IMPORT_TANGENTS";
            this.ImportTangentBox.UseVisualStyleBackColor = true;
            // 
            // ImportColor1Box
            // 
            this.ImportColor1Box.AutoSize = true;
            this.ImportColor1Box.Location = new System.Drawing.Point(184, 67);
            this.ImportColor1Box.Name = "ImportColor1Box";
            this.ImportColor1Box.Size = new System.Drawing.Size(123, 17);
            this.ImportColor1Box.TabIndex = 14;
            this.ImportColor1Box.Text = "$IMPORT_COLOR1";
            this.ImportColor1Box.UseVisualStyleBackColor = true;
            // 
            // ImportColor0Box
            // 
            this.ImportColor0Box.AutoSize = true;
            this.ImportColor0Box.Location = new System.Drawing.Point(184, 47);
            this.ImportColor0Box.Name = "ImportColor0Box";
            this.ImportColor0Box.Size = new System.Drawing.Size(123, 17);
            this.ImportColor0Box.TabIndex = 13;
            this.ImportColor0Box.Text = "$IMPORT_COLOR0";
            this.ImportColor0Box.UseVisualStyleBackColor = true;
            // 
            // Label_BufferType
            // 
            this.Label_BufferType.AutoSize = true;
            this.Label_BufferType.Location = new System.Drawing.Point(18, 173);
            this.Label_BufferType.Name = "Label_BufferType";
            this.Label_BufferType.Size = new System.Drawing.Size(102, 13);
            this.Label_BufferType.TabIndex = 15;
            this.Label_BufferType.Text = "$IS_32BIT_MODEL";
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
            this.SplitContainer_Root.Panel2.Controls.Add(this.ModelOptionsText);
            this.SplitContainer_Root.Panel2.Controls.Add(this.ImportNormalBox);
            this.SplitContainer_Root.Panel2.Controls.Add(this.ImportUV1Box);
            this.SplitContainer_Root.Panel2.Controls.Add(this.ImportUV2Box);
            this.SplitContainer_Root.Panel2.Controls.Add(this.FlipUVBox);
            this.SplitContainer_Root.Panel2.Controls.Add(this.ImportDiffuseBox);
            this.SplitContainer_Root.Panel2.Controls.Add(this.ImportTangentBox);
            this.SplitContainer_Root.Panel2.Controls.Add(this.ImportColor0Box);
            this.SplitContainer_Root.Panel2.Controls.Add(this.ImportColor1Box);
            this.SplitContainer_Root.Panel2.Controls.Add(this.Label_BufferType);
            this.SplitContainer_Root.Size = new System.Drawing.Size(624, 411);
            this.SplitContainer_Root.SplitterDistance = 208;
            this.SplitContainer_Root.TabIndex = 16;
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
            // TreeView_Objects
            // 
            this.TreeView_Objects.Dock = System.Windows.Forms.DockStyle.Fill;
            this.TreeView_Objects.Location = new System.Drawing.Point(0, 0);
            this.TreeView_Objects.Name = "TreeView_Objects";
            this.TreeView_Objects.Size = new System.Drawing.Size(208, 411);
            this.TreeView_Objects.TabIndex = 0;
            this.TreeView_Objects.BeforeSelect += new System.Windows.Forms.TreeViewCancelEventHandler(this.TreeView_OnBeforeSelect);
            this.TreeView_Objects.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.TreeView_OnAfterSelect);
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
            this.ControlBox = false;
            this.Controls.Add(this.splitContainer1);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FrameResourceModelOptions";
            this.Text = "$MODEL_OPTIONS_TITLE";
            this.SplitContainer_Root.Panel1.ResumeLayout(false);
            this.SplitContainer_Root.Panel2.ResumeLayout(false);
            this.SplitContainer_Root.Panel2.PerformLayout();
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
        private System.Windows.Forms.Label ModelOptionsText;
        private System.Windows.Forms.CheckBox ImportNormalBox;
        private System.Windows.Forms.CheckBox ImportUV1Box;
        private System.Windows.Forms.CheckBox ImportUV2Box;
        private System.Windows.Forms.CheckBox ImportAOBox;
        private System.Windows.Forms.CheckBox FlipUVBox;
        private System.Windows.Forms.CheckBox ImportDiffuseBox;
        private System.Windows.Forms.CheckBox ImportTangentBox;
        private System.Windows.Forms.CheckBox ImportColor1Box;
        private System.Windows.Forms.CheckBox ImportColor0Box;
        private System.Windows.Forms.Label Label_BufferType;
        private System.Windows.Forms.SplitContainer SplitContainer_Root;
        private Utils.Extensions.MTreeView TreeView_Objects;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel Label_MessageText;
    }
}