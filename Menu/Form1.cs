using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using System.Text.RegularExpressions;
using System.Collections;


/*
 * 
 */

namespace TreeMenu
{
    public partial class Form1 : Form
    {

        private int treeSizeOffsetW = 0;
        private int treeSizeOffsetH = 0;

        //使用多线程计时器
        private System.Timers.Timer timer = new System.Timers.Timer();
        System.Windows.Forms.Timer timerLoad = new System.Windows.Forms.Timer();

        public Form1()
        {
            InitializeComponent();

            treeView1.AllowDrop = true;
            treeView1.ItemDrag += new ItemDragEventHandler(treeView1_ItemDrag);
            treeView1.DragEnter += new DragEventHandler(treeView1_DragEnter);
            treeView1.DragDrop += new DragEventHandler(treeView1_DragDrop);

            ownerAccount.TextChanged += new EventHandler(onOnwerAccountChanged);
            limitText.TextChanged += new EventHandler(OnLimitChanged);

            Resize += new EventHandler(OnFormResize);
            treeSizeOffsetW = Size.Width - treeView1.Size.Width;
            treeSizeOffsetH = Size.Height - treeView1.Size.Height;

            timer.Interval = 1000;
            timer.Elapsed += new System.Timers.ElapsedEventHandler(trySaveFile);
            timer.Enabled = true;

            timerLoad.Interval = 50;
            timerLoad.Tick += new EventHandler(tryLoadFile);
            timerLoad.Enabled = true;
        }


        #region Drag and Common
        void treeView1_DragDrop(object sender, DragEventArgs e)
        {
            TreeNode moveNode = (TreeNode)e.Data.GetData(typeof(TreeNode));

            //根据鼠标坐标确定要移动到的目标节点
            Point pt = ((TreeView)(sender)).PointToClient(new Point(e.X, e.Y));
            TreeNode targeNode = this.treeView1.GetNodeAt(pt);

            
            if (targeNode != null)
            {
                // 如果目标是当前的节点的子类或者是其自身，则不可移动
                if (targeNode.Text == moveNode.Text)
                    return;

                TreeNode parent = targeNode;
                while (parent != null && parent.GetType() == typeof(TreeNode))
                {
                    if (parent.Text == moveNode.Text)
                    {
                        MessageBox.Show("推荐人无法加入其子类");
                        return;
                    }
                    parent = parent.Parent;
                }

                TreeNode NewMoveNode = (TreeNode)moveNode.Clone();
                targeNode.Nodes.Add(NewMoveNode);
                //展开目标节点,便于显示拖放效果
                targeNode.Expand();
                treeView1.SelectedNode = NewMoveNode;
            }
            else
            {
                TreeNode NewMoveNode = (TreeNode)moveNode.Clone();
                this.treeView1.Nodes.Add(NewMoveNode);
                treeView1.SelectedNode = NewMoveNode;
            }

            //更新当前拖动的节点选择
            //treeView1.SelectedNode = NewMoveNode;
            
            //移除拖放的节点
            moveNode.Remove();

            Tools.refreshNodes(this.treeView1.Nodes);
        }


        void treeView1_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent("System.Windows.Forms.TreeNode"))
            {
                e.Effect = DragDropEffects.Move;
            }
            else
            {
                e.Effect = DragDropEffects.None;
            }
        }


        void treeView1_ItemDrag(object sender, ItemDragEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                DoDragDrop(e.Item, DragDropEffects.Move);
            }
        }


        private TreeNode CreateNewNode()
        {
            if (!checkTextBox()) return null;;

            string userName = this.userName.Text.Trim();
            string userAccount = this.userAccount.Text.Trim();

            this.userName.Text = "";
            this.userAccount.Text = "";

            NodeTag tag = new NodeTag(userName, userAccount);
            return CreateNewNode(tag);
        }


        Font font = new Font("宋体", 12, FontStyle.Regular);
        private TreeNode CreateNewNode(NodeTag tag)
        {
            TreeNode node = new TreeNode(tag.Name);
            node.Tag = tag;
            node.NodeFont = font;
            return node;
        }


        private TreeNode FindNodeByAccount(string account, TreeNodeCollection nodes = null)
        {
            TreeNode findNode = null;
            if (nodes == null)
                nodes = this.treeView1.Nodes;

            foreach (TreeNode node in nodes)
            {
                if (((NodeTag)node.Tag).Account == account)
                {
                    findNode = node;
                    break;
                }

                findNode = FindNodeByAccount(account, node.Nodes);
                if (findNode != null)
                    break;
            }

            return findNode;
        }


        private bool checkTextBox()
        {
            if (this.userName.Text.Trim() == "" || this.userAccount.Text.Trim() == "")
            {
                MessageBox.Show("输入信息有误");
                return false;
            }
            return true;
        }


        public ArrayList getNodeArrayList(TreeNodeCollection nodes)
        {
            ArrayList list = new ArrayList();

            NodeTag tag;
            foreach (TreeNode node in nodes)
            {
                tag = node.Tag as NodeTag;
                Hashtable data = tag.encode();
                list.Add(data);
                data["l"] = getNodeArrayList(node.Nodes);
            }

            return list;
        }

        #endregion


        #region Btn Click
        private void btnAddChild_Click(object sender, EventArgs e)
        {
            if(!checkTextBox())
                return;

            // 检查是否重复
            string newAccount = this.userAccount.Text.Trim();
            TreeNode node = FindNodeByAccount(newAccount);
            if(node != null)
            {
                NodeTag tag = node.Tag as NodeTag;
                MessageBox.Show("帐号[" + newAccount + "]已存在, 名字[" + tag.Name + "]");
                return;
            }

            node = CreateNewNode();
            if (node == null) return;
            string ownerAccount = this.ownerAccount.Text;
            if (ownerAccount == "")
            {
                if (this.treeView1.SelectedNode == null)
                {
                    this.treeView1.Nodes.Add(node);
                }
                else
                {
                    this.treeView1.SelectedNode.Nodes.Add(node);
                    this.treeView1.SelectedNode.Expand();
                    this.treeView1.SelectedNode = null;
                }
            }
            else
            {
                TreeNode parent = FindNodeByAccount(ownerAccount);
                if (parent == null)
                {
                    MessageBox.Show("找不到推荐人帐号:" + ownerAccount);
                    return;
                }
                else
                {
                    parent.Nodes.Add(node);
                }
            }

            Tools.refreshNodes(this.treeView1.Nodes);
        }


        private void btnDelSel_Click(object sender, EventArgs e)
        {
            if (this.treeView1.SelectedNode == null)
                return;

            this.treeView1.Nodes.Remove(this.treeView1.SelectedNode);
            Tools.refreshNodes(this.treeView1.Nodes);
        }


        private void btnRefresh_Click(object sender, EventArgs e)
        {
            TreeNode node = this.treeView1.SelectedNode;
            if (node == null) return;
            if (!checkTextBox()) return;
            NodeTag tag = node.Tag as NodeTag;
            tag.Name = this.userName.Text.Trim();
            tag.Account = this.userAccount.Text.Trim();

            node.Text = string.Format("{0} {1}({2}-{3})", tag.Name, tag.Account, tag.ChildCount, tag.TotalChileCount);

            MessageBox.Show("信息已更新");
        }


        private void btnClose_Click(object sender, EventArgs e)
        {
            Close();

            //TreeNode node = FindNodeByAccount("123");
            //if (node == null) return;
            //NodeTag tag = node.Tag as NodeTag;
            //MessageBox.Show("msg:" + tag.Name);

            //String str = userName.Text.Trim();
            //Hashtable t = new Hashtable();
            //t["name"] = str;
            //string js = JSON.JsonEncode(t);
            //MessageBox.Show("json str:" + js);
            //Hashtable t1 = JSON.DecodeMap(js);
            //MessageBox.Show("name:" + t1["name"]);
        }


        // 选择后填充为推荐人帐号
        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (this.treeView1.SelectedNode == null) return;

            NodeTag tag = this.treeView1.SelectedNode.Tag as NodeTag;
            this.ownerAccount.Text = tag.Account;
        }


        private void onOnwerAccountChanged(object sender, EventArgs e)
        {
            if(this.ownerAccount.Text.Trim() == "")
                this.treeView1.SelectedNode = null;
        }


        private void OnFormResize(object sender, EventArgs e)
        {
            this.treeView1.Size = new Size(Size.Width - treeSizeOffsetW, Size.Height - treeSizeOffsetH);
        }


        private void OnLimitChanged(object sender, EventArgs e)
        {
            string limit = limitText.Text.Trim();
            try
            {
                int value = int.Parse(limit);
                Tools.redLimit = value;
                Tools.refreshNodes(this.treeView1.Nodes);
            }
            catch (Exception e1)
            {
                MessageBox.Show( "请输入正确的阀值" );
            }
        }


        private void label1_Click(object sender, EventArgs e)
        {

        }

        #endregion


        #region TreeView and XML
        private void btnSave_Click(object sender, EventArgs e)
        {
            if (this.dlgSave.ShowDialog() == DialogResult.OK)
            {
                SaveFile(dlgSave.FileName);
            }
        }


        private void SaveFile(string fileName, bool quite = false)
        {
            if (!Tools.refreshed && quite)
                return;

            Tools.refreshed = false;

            ArrayList datas = getNodeArrayList(this.treeView1.Nodes);
            string jsonStr = JSON.JsonEncode(datas);

            if (jsonStr == null || jsonStr == "")
            {
                if(!quite)
                    MessageBox.Show("数据有误");
                return;
            }

            Hashtable cnf = new Hashtable();
            cnf["limit"] = Tools.redLimit;

            string cnfStr = JSON.JsonEncode(cnf);

            FileStream fs = null;
            StreamWriter sw = null;
            try
            {
                if (File.Exists(fileName))
                    File.Delete(fileName);
            
                fs = new FileStream(fileName, FileMode.Create);
                sw = new StreamWriter(fs, Encoding.UTF8);
                sw.WriteLine("<meta http-equiv=\"content-type\" content=\"text/html; charset=UTF-8\" />");
                sw.WriteLine("<html>");
                sw.WriteLine("<body>");
                sw.WriteLine("<customCnf data=" + cnfStr + "></customCnf>");
                sw.WriteLine("<customData data=" + jsonStr + "></customData>");

                sw.WriteLine("<div>");

                TreeNode2Html(this.treeView1.Nodes, ref sw);

                sw.WriteLine("</div>");
                sw.WriteLine("</body>");
                sw.WriteLine("</html>");

                sw.Flush();
            }
            catch (Exception e)
            {
                //MessageBox.Show("----" + e.ToString());
            }
            finally
            {
                if (sw != null)
                    sw.Close();

                if (fs != null)
                    fs.Close();
            }

            if (!quite)
                MessageBox.Show("导出成功");
        }






        const string color_black = "#000000";
        const string color_red = "#ff0000";
        private void TreeNode2Html(TreeNodeCollection treeNodes, ref StreamWriter sw)
        {
            sw.WriteLine("<ul>");

            NodeTag tag;
            string color;
            foreach (TreeNode node in treeNodes)
            {
                tag = node.Tag as NodeTag;
                if (tag.ChildCount >= Tools.redLimit)
                {
                    color = color_red;
                }
                else
                {
                    color = color_black;
                }
                
                sw.WriteLine(string.Format("<li style=\"color:{0}\">", color));
                sw.WriteLine(node.Text);
                sw.WriteLine("</li>");
                if(node.Nodes.Count > 0)
                    TreeNode2Html(node.Nodes, ref sw);
            }

            sw.WriteLine("</ul>");
        }



        private Regex regCnf = new Regex("<customCnf data=(.+)></customCnf>");
        private Regex regData = new Regex("<customData data=(.+)></customData>");
        private void btnLoad_Click(object sender, EventArgs e)
        {
            if (this.dlgOpen.ShowDialog() == DialogResult.OK)
            {
                LoadFile(dlgOpen.FileName);
            }
        }


        private void LoadFile(string fileName, bool quiet = false)
        {
            FileStream fs = null;
            StreamReader sr = null;
            bool result = false;

            try
            {
                fs = new FileStream(fileName, FileMode.Open);
            

                if (fs == null)
                {
                    if(!quiet)
                        MessageBox.Show("文件打开失败");
                    return;
                }

                sr = new StreamReader(fs, Encoding.UTF8);

                while (true)
                {
                    result = LoadCustomCnf(sr);

                    if (!result)
                        break;

                    result = LoadCustomData(sr);
                    break;
                }
            }
            catch (Exception e)
            {

            }
            finally
            {
                if (sr != null)
                    sr.Close();

                if (fs != null)
                    fs.Close();
            }

            if (!quiet)
            {
                if (result)
                {
                    MessageBox.Show("导入成功");
                }
                else
                {
                    MessageBox.Show("数据内容损坏，无法解析数据");
                }
            }
        }


        private bool LoadCustomCnf(StreamReader sr)
        {
            string lineStr = sr.ReadLine();
            while (lineStr != null && lineStr.IndexOf("<customCnf") < 0)
            {
                lineStr = sr.ReadLine();
            }

            if (lineStr != null)
            {

                Match math = regCnf.Match(lineStr);
                string jsonStr = math.Groups[1].Value;
                Hashtable datas = JSON.DecodeMap(jsonStr);
                if (datas != null)
                {
                    double value = (double)datas["limit"];
                    Tools.redLimit = (int)value;
                    limitText.Text = Tools.redLimit.ToString();
                    return true;
                }
            }

            return false;
        }


        private bool LoadCustomData(StreamReader sr)
        {
            string lineStr = sr.ReadLine();
            while (lineStr != null && lineStr.IndexOf("<customData") < 0)
            {
                lineStr = sr.ReadLine();
            }

            if (lineStr != null)
            {

                Match math = regData.Match(lineStr);
                string jsonStr = math.Groups[1].Value;
                ArrayList datas = JSON.DecodeList(jsonStr);
                if (datas != null)
                {
                    this.treeView1.BeginUpdate();
                    this.treeView1.Nodes.Clear();

                    Html2TreeNode(datas);
                    Tools.refreshNodes(this.treeView1.Nodes);

                    this.treeView1.EndUpdate();
                    return true;
                }
            }

            return false;
        }


        
        private void Html2TreeNode(ArrayList datas, TreeNodeCollection nodes = null)
        {
            if (nodes == null)
                nodes = this.treeView1.Nodes;

            Hashtable table;
            foreach (object obj in datas)
            {
                table = obj as Hashtable;
                NodeTag tag = new NodeTag();
                tag.decode(table);
                TreeNode node = CreateNewNode(tag);
                nodes.Add(node);

                ArrayList children = table["l"] as ArrayList;
                Html2TreeNode(children, node.Nodes);
            }
        }

        #endregion

        
        #region TreeView 2 Menu
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


        void menu_Click(object sender, EventArgs e)
        {
            MessageBox.Show((sender as ToolStripMenuItem).Text.ToString());
        }

        #endregion


        void trySaveFile(object sender, System.Timers.ElapsedEventArgs e)
        {
            string tempFile = System.IO.Path.GetTempPath();
            if (tempFile == null || tempFile == "")
                return;

            tempFile += "r_tree_file.tmp";
            SaveFile(tempFile, true);
        }


        void tryLoadFile(object sender, EventArgs e)
        {
            string tempFile = System.IO.Path.GetTempPath();
            if (tempFile == null || tempFile == "")
                return;

            tempFile += "r_tree_file.tmp";
            LoadFile(tempFile, true);

            timerLoad.Stop();
        }
    }
}