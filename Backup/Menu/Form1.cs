using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Xml;

namespace Menu
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        #region Edit TreeView

        private void btnAddRoot_Click(object sender, EventArgs e)
        {
            TreeNode node = CreateNewNode();

            this.treeView1.Nodes.Add(node);
        }

        private TreeNode CreateNewNode()
        {
            TreeNode node = new TreeNode(this.txtTitle.Text.Trim());
            node.Tag = this.txtContent.Text.Clone();
            return node;
        }

        private void btnAddChild_Click(object sender, EventArgs e)
        {
            if (this.treeView1.SelectedNode == null)
            {
                return;
            }
            TreeNode node = CreateNewNode();

            this.treeView1.SelectedNode.Nodes.Add(node);
            this.treeView1.SelectedNode.Expand();
        }
        private void btnDelSel_Click(object sender, EventArgs e)
        {
            if (this.treeView1.SelectedNode == null)
            {
                return;
            }
            this.treeView1.Nodes.Remove(this.treeView1.SelectedNode);
        }
        #endregion

        #region TreeView 2 XML

        private void btnSave_Click(object sender, EventArgs e)
        {
            //将TreeView保存到XML文件中
            if (this.dlgSave.ShowDialog() == DialogResult.OK)
            {
                XmlDocument doc = new XmlDocument();
                doc.LoadXml("<Menu></Menu>");
                XmlNode root = doc.DocumentElement;
                doc.InsertBefore(doc.CreateXmlDeclaration("1.0", "utf-8", "yes"), root);
                TreeNode2Xml(this.treeView1.Nodes, root);
                doc.Save(dlgSave.FileName);
            }
        }

        private void TreeNode2Xml(TreeNodeCollection treeNodes, XmlNode xmlNode)
        {
            XmlDocument doc = xmlNode.OwnerDocument;
            foreach (TreeNode treeNode in treeNodes)
            {
                XmlNode element = doc.CreateNode("element", "Item", "");
                XmlAttribute attr = doc.CreateAttribute("Title");
                attr.Value = treeNode.Text;
                element.Attributes.Append(attr);
                element.AppendChild(doc.CreateCDataSection(treeNode.Tag.ToString()));
                xmlNode.AppendChild(element);

                if (treeNode.Nodes.Count > 0)
                {
                    TreeNode2Xml(treeNode.Nodes, element);
                }
            }
        }
        #endregion

        #region XML 2 TreeView
        private void btnLoad_Click(object sender, EventArgs e)
        {
            //从XML中读取数据到TreeView
            if (this.dlgOpen.ShowDialog() == DialogResult.OK)
            {
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.Load(dlgOpen.FileName);

                XmlNodeList xmlNodes = xmlDoc.DocumentElement.ChildNodes;

                this.treeView1.BeginUpdate();
                this.treeView1.Nodes.Clear();
                XmlNode2TreeNode(xmlNodes, this.treeView1.Nodes);
                this.treeView1.EndUpdate();
            }
        }

        private void XmlNode2TreeNode(XmlNodeList xmlNode, TreeNodeCollection treeNode)
        {
            foreach (XmlNode var in xmlNode)
            {
                if (var.NodeType != XmlNodeType.Element)
                {
                    continue;
                }
                TreeNode newTreeNode = new TreeNode();
                newTreeNode.Text = var.Attributes["Title"].Value;

                if (var.HasChildNodes)
                {
                    if (var.ChildNodes[0].NodeType == XmlNodeType.CDATA)
                    {
                        newTreeNode.Tag = var.ChildNodes[0].Value;
                    }

                    XmlNode2TreeNode(var.ChildNodes, newTreeNode.Nodes);
                }
                treeNode.Add(newTreeNode);
            }
        }
        #endregion

        #region TreeView 2 Menu
        private void btnRecreateMenu_Click(object sender, EventArgs e)
        {
            //根据TreeView生成层次结构的菜单
            this.contextMenuStrip1.Items.Clear();
            TreeView2Menu(this.treeView1.Nodes, this.contextMenuStrip1.Items);
        }

        private void TreeView2Menu(TreeNodeCollection nodes, ToolStripItemCollection items)
        {
            foreach (TreeNode node in nodes)
            {
                ToolStripMenuItem menu = new ToolStripMenuItem();
                menu.Text = node.Text;
                menu.Tag = node.Tag;
                items.Add(menu);
                if (node.Nodes.Count > 0)
                {
                    TreeView2Menu(node.Nodes, menu.DropDownItems);
                }
                else
                {
                    menu.Click += new EventHandler(menu_Click);
                }
            }
        }
        #endregion
        void menu_Click(object sender, EventArgs e)
        {
            MessageBox.Show((sender as ToolStripMenuItem).Text.ToString());
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            Close();
        }



    }
}