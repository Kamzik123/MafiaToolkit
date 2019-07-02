﻿using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using ResourceTypes.Misc;
using Utils.Lang;
using Utils.Settings;
using static ResourceTypes.Misc.StreamMapLoader;

namespace Mafia2Tool
{
    public partial class StreamEditor : Form
    {
        private FileInfo file;
        private StreamMapLoader stream;

        public StreamEditor(FileInfo file)
        {
            InitializeComponent();
            Localise();
            this.file = file;
            BuildData();
            Show();
            ToolkitSettings.UpdateRichPresence("Using the Stream editor.");
        }

        private void Localise()
        {
            Text = Language.GetString("$STREAM_EDITOR_TITLE");
            fileToolButton.Text = Language.GetString("$FILE");
            saveToolStripMenuItem.Text = Language.GetString("$SAVE");
            reloadToolStripMenuItem.Text = Language.GetString("$RELOAD");
            exitToolStripMenuItem.Text = Language.GetString("$EXIT");
        }

        private void Sort(List<StreamLoader> loaders)
        {
            for (int i = 0; i < loaders.Count - 1; i++)
            {
                // traverse i+1 to array length 
                for (int j = i + 1; j < loaders.Count; j++)
                {
                    // compare array element with  
                    // all next element 
                    if (loaders[i].start > loaders[j].start)
                    {

                        StreamLoader temp = loaders[i];
                        loaders[i] = loaders[j];
                        loaders[j] = temp;
                    }
                }
            }
        }

        private void UpdateStream()
        {
            List<StreamLine> lines = new List<StreamLine>();
            List<StreamLoader> loaders = new List<StreamLoader>();
            Dictionary<string, StreamLoader> currentLoaders = new Dictionary<string, StreamLoader>();
            Dictionary<string, bool> temp = new Dictionary<string, bool>();

            foreach (TreeNode node in linesTree.Nodes)
            {
                foreach (TreeNode child in node.Nodes)
                {
                    StreamLine line = (child.Tag as StreamLine);
                    line.lineID = lines.Count;
                    lines.Add(line);
                    temp = new Dictionary<string, bool>();

                    for (int i = 0; i != currentLoaders.Count; i++)
                        temp.Add(currentLoaders.ElementAt(i).Key, false);

                    foreach (var loader in currentLoaders)
                    {
                        foreach (var load in line.loadList)
                        {
                            if (loader.Value == load)
                            {
                                temp[loader.Key] = true;
                            }
                        }
                    }

                    for(int i = 0; i != temp.Count;)
                    {
                        if (temp.ElementAt(i).Value == false)
                        {
                            loaders.Add(currentLoaders[temp.ElementAt(i).Key]);
                            currentLoaders.Remove(temp.ElementAt(i).Key);
                            temp.Remove(temp.ElementAt(i).Key);
                        }
                        else i++;
                    }

                    foreach (StreamLoader loader in line.loadList)
                    {
                        if (!currentLoaders.ContainsKey(loader.Path))
                        {
                            loader.start = line.lineID;
                            loader.end = line.lineID;
                            currentLoaders.Add(loader.Path, loader);
                            temp.Add(loader.Path, true);
                        }
                        else
                        {
                            currentLoaders[loader.Path].end = line.lineID;
                        }
                    }
                }
            }
            Sort(loaders);
            Dictionary<string, List<StreamLoader>> organised = new Dictionary<string, List<StreamLoader>>();
            List<StreamGroup> groups = new List<StreamGroup>();

            foreach (TreeNode node in groupTree.Nodes)
            {
                if (!organised.ContainsKey(node.Text))
                {
                    organised.Add(node.Text, new List<StreamLoader>());
                    groups.Add((StreamGroup)node.Tag);
                }
            }

            foreach (StreamLoader pair in loaders)
            {
                if (!organised.ContainsKey(pair.Group))
                {
                    organised.Add(pair.Group, new List<StreamLoader>());
                    organised[pair.Group].Add(pair);
                }
                else
                {
                    organised[pair.Group].Add(pair);
                }
            }

            List<StreamLoader> streamLoaders = new List<StreamLoader>();
            int idx = 0;
            foreach (KeyValuePair<string, List<StreamLoader>> pair in organised)
            {
                groups[idx].startOffset = streamLoaders.Count;
                streamLoaders.AddRange(pair.Value);
                groups[idx].endOffset = streamLoaders.Count - groups[idx].startOffset;
                idx++;
            }

            stream.lines = lines.ToArray();
            stream.groups = groups.ToArray();
            stream.loaders = streamLoaders.ToArray();
        }

        private void BuildData()
        {
            stream = new StreamMapLoader(file);

            for(int i = 0; i < stream.groupHeaders.Length; i++)
            {
                TreeNode node = new TreeNode("group" + i);
                node.Text = stream.groupHeaders[i];
                linesTree.Nodes.Add(node);
            }
            for (int i = 0; i < stream.groups.Length; i++)
            {
                var line = stream.groups[i];
                TreeNode node = new TreeNode();
                node.Name = "GroupLoader" + i;
                node.Text = line.Name;
                node.Tag = line;

                for (int x = line.startOffset; x < line.startOffset + line.endOffset; x++)
                {
                    var loader = stream.loaders[x];
                    loader.Group = line.Name;
                }

                groupTree.Nodes.Add(node);
            }
            for (int i = 0; i != stream.lines.Length; i++)
            {
                var line = stream.lines[i];
                TreeNode node = new TreeNode();
                node.Name = line.Name;
                node.Text = line.Name;
                node.Tag = line;

                List<StreamLoader> list = new List<StreamLoader>();
                for (int x = 0; x < stream.loaders.Length; x++)
                {
                    var loader = stream.loaders[x];
                    if (line.lineID >= loader.start && line.lineID <= loader.end)
                    {
                        list.Add(loader);
                    }
                }
                line.loadList = list.ToArray();
                linesTree.Nodes[line.groupID].Nodes.Add(node);
            }
            for(int i = 0; i < stream.blocks.Length; i++)
            {
                var block = stream.blocks[i];
                List<ulong> hash = new List<ulong>();
                for (int x = block.startOffset; x < block.endOffset; x++)
                    hash.Add(stream.hashes[x]);
                block.Hashes = hash.ToArray();

                TreeNode node = new TreeNode();
                node.Name = "Block" + i;
                node.Text = "Block: " + i;
                node.Tag = block;
                blockView.Nodes.Add(node);
            }

        }

        public void UncheckAllNodes(TreeNodeCollection nodes)
        {
            foreach (TreeNode node in nodes)
            {
                node.Checked = false;
                CheckChildren(node, false);
            }
        }

        private void CheckChildren(TreeNode rootNode, bool isChecked)
        {
            foreach (TreeNode node in rootNode.Nodes)
            {
                CheckChildren(node, isChecked);
                node.Checked = isChecked;
            }
        }

        private void OnNodeSelectSelect(object sender, TreeViewEventArgs e)
        {
            PropertyGrid.SelectedObject = e.Node.Tag;
        }

        private void exitToolStripMenuItem_Click(object sender, System.EventArgs e)
        {
            Close();
        }

        private void reloadToolStripMenuItem_Click(object sender, System.EventArgs e)
        {

        }

        private void saveToolStripMenuItem_Click(object sender, System.EventArgs e)
        {
            UpdateStream();
            stream.WriteToFile();
        }
    }
}